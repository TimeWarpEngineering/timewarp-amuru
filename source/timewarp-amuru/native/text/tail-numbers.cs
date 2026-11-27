#region Purpose
// Parses tail's line and byte counts, including the block and K/M/G suffixes.
#endregion

#region Design
// The suffix table matches GNU tail and uutils: b is 512, a bare K/M/G is 1024^n,
// KB/MB is 1000^n, and KiB/MiB is 1024^n. The unit letter may be either case.
// iB requires a lowercase i and an uppercase B. Overflow saturates, which is what
// uutils does for a count such as 999Y.
#endregion

namespace TimeWarp.Amuru.Native.Text;

internal static class TailNumbers
{
  public static bool TryParse(string text, out long count, out bool fromStart)
  {
    count = 0;
    fromStart = false;
    if (string.IsNullOrEmpty(text))
    {
      return false;
    }

    int index = 0;
    if (text[0] == '+')
    {
      fromStart = true;
      index = 1;
    }
    else if (text[0] == '-')
    {
      index = 1;
    }

    if (index >= text.Length || !char.IsAsciiDigit(text[index]))
    {
      return false;
    }

    int digits = index;
    while (digits < text.Length && char.IsAsciiDigit(text[digits]))
    {
      digits++;
    }

    if (!ulong.TryParse(text.AsSpan(index, digits - index), NumberStyles.None, CultureInfo.InvariantCulture, out ulong magnitude))
    {
      return false;
    }

    if (!TryApplySuffix(magnitude, text[digits..], out ulong scaled))
    {
      return false;
    }

    count = scaled > long.MaxValue ? long.MaxValue : (long)scaled;
    return true;
  }

  private static bool TryApplySuffix(ulong magnitude, string suffix, out ulong scaled)
  {
    scaled = magnitude;
    if (suffix.Length == 0)
    {
      return true;
    }

    if (suffix == "b")
    {
      return TryMultiply(magnitude, 512, out scaled);
    }

    int exponent = suffix[0] switch
    {
      'k' or 'K' => 1,
      'm' or 'M' => 2,
      'g' or 'G' => 3,
      't' or 'T' => 4,
      'p' or 'P' => 5,
      'e' or 'E' => 6,
      'z' or 'Z' => 7,
      'y' or 'Y' => 8,
      _ => 0
    };
    if (exponent == 0)
    {
      return false;
    }

    int @base = suffix[1..] switch
    {
      "" => 1024,
      "B" => 1000,
      "iB" => 1024,
      _ => 0
    };
    if (@base == 0)
    {
      return false;
    }

    ulong value = magnitude;
    for (int step = 0; step < exponent; step++)
    {
      if (!TryMultiply(value, (ulong)@base, out value))
      {
        scaled = ulong.MaxValue;
        return true;
      }
    }

    scaled = value;
    return true;
  }

  private static bool TryMultiply(ulong left, ulong right, out ulong product)
  {
    if (left != 0 && right > ulong.MaxValue / left)
    {
      product = 0;
      return false;
    }

    product = left * right;
    return true;
  }
}
