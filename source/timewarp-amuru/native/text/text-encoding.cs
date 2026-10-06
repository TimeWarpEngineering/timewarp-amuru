#region Purpose
// Detects a text file's BOM and newline style so ReplaceInFiles can write it back unchanged.
#endregion

#region Design
// No BOM means UTF-8 without a preamble. UTF-32 LE is detected before UTF-16 LE because
// its BOM starts with the same FF FE bytes. The Encoding stored here does not emit a BOM;
// GetBytes writes the detected preamble itself, once. Newlines are normalized to LF only
// for the match, then written in the style of the first newline. A file with zero
// replacements is not re-encoded, so mixed newlines and the original bytes stay put.
#endregion

namespace TimeWarp.Amuru.Native.Text;

internal readonly record struct NewlineStyle(string Sequence)
{
  public static NewlineStyle Lf { get; } = new("\n");

  public static NewlineStyle Crlf { get; } = new("\r\n");

  public static NewlineStyle Cr { get; } = new("\r");

  public static NewlineStyle Detect(string text)
  {
    ArgumentNullException.ThrowIfNull(text);
    int crlf = text.IndexOf("\r\n", StringComparison.Ordinal);
    int lineFeed = text.IndexOf('\n', StringComparison.Ordinal);
    int carriageReturn = text.IndexOf('\r', StringComparison.Ordinal);

    int first = -1;
    NewlineStyle style = Lf;
    if (crlf >= 0)
    {
      first = crlf;
      style = Crlf;
    }

    if (lineFeed >= 0 && (first < 0 || lineFeed < first))
    {
      first = lineFeed;
      style = Lf;
    }

    if (carriageReturn >= 0 && (first < 0 || carriageReturn < first))
    {
      style = Cr;
    }

    return style;
  }

  public static string ToLineFeed(string text)
  {
    ArgumentNullException.ThrowIfNull(text);
    return text
      .Replace("\r\n", "\n", StringComparison.Ordinal)
      .Replace("\r", "\n", StringComparison.Ordinal);
  }

  public string Apply(string lineFeedText)
  {
    ArgumentNullException.ThrowIfNull(lineFeedText);
    if (Sequence == "\n")
    {
      return lineFeedText;
    }

    return lineFeedText.Replace("\n", Sequence, StringComparison.Ordinal);
  }
}

internal readonly record struct TextFileEncoding(Encoding Encoding, byte[] Preamble)
{
  public static TextFileEncoding Detect(ReadOnlySpan<byte> bytes)
  {
    if (HasPrefix(bytes, [0xEF, 0xBB, 0xBF]))
    {
      return new TextFileEncoding(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), [0xEF, 0xBB, 0xBF]);
    }

    if (HasPrefix(bytes, [0xFF, 0xFE, 0x00, 0x00]))
    {
      return new TextFileEncoding(new UTF32Encoding(bigEndian: false, byteOrderMark: false), [0xFF, 0xFE, 0x00, 0x00]);
    }

    if (HasPrefix(bytes, [0xFF, 0xFE]))
    {
      return new TextFileEncoding(new UnicodeEncoding(bigEndian: false, byteOrderMark: false), [0xFF, 0xFE]);
    }

    if (HasPrefix(bytes, [0xFE, 0xFF]))
    {
      return new TextFileEncoding(new UnicodeEncoding(bigEndian: true, byteOrderMark: false), [0xFE, 0xFF]);
    }

    return new TextFileEncoding(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), []);
  }

  public string GetString(byte[] bytes)
  {
    ArgumentNullException.ThrowIfNull(bytes);
    int offset = Preamble.Length;
    if (bytes.Length < offset)
    {
      offset = 0;
    }

    return Encoding.GetString(bytes, offset, bytes.Length - offset);
  }

  public byte[] GetBytes(string text)
  {
    ArgumentNullException.ThrowIfNull(text);
    byte[] body = Encoding.GetBytes(text);
    if (Preamble.Length == 0)
    {
      return body;
    }

    byte[] encoded = new byte[Preamble.Length + body.Length];
    Preamble.CopyTo(encoded, 0);
    body.CopyTo(encoded, Preamble.Length);
    return encoded;
  }

  private static bool HasPrefix(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> prefix)
  {
    return bytes.Length >= prefix.Length && bytes[..prefix.Length].SequenceEqual(prefix);
  }
}
