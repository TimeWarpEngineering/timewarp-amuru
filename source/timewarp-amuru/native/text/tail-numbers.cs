#region Purpose
// Parses tail's line and byte counts, including the block and K/M/G suffixes.
#endregion

#region Design
// The suffix table matches GNU tail and uutils: b is 512, a bare K/M/G is 1024^n,
// KB/MB is 1000^n, and KiB/MiB is 1024^n. The unit letter may be either case.
// iB requires a lowercase i and an uppercase B. Overflow saturates, which is what
// uutils does for a count such as 999Y. A decimal that does not fit in ulong, and a
// b suffix whose product does not fit, saturate the same way.
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

    ulong magnitude = ParseMagnitude(text.AsSpan(index, digits - index));

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
      if (!TryMultiply(magnitude, 512, out scaled))
      {
        scaled = ulong.MaxValue;
      }

      return true;
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

  private static ulong ParseMagnitude(ReadOnlySpan<char> digits)
  {
    ulong magnitude = 0;
    foreach (char digit in digits)
    {
      ulong next = (ulong)(digit - '0');
      if (magnitude > (ulong.MaxValue - next) / 10)
      {
        return ulong.MaxValue;
      }

      magnitude = (magnitude * 10) + next;
    }

    return magnitude;
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
