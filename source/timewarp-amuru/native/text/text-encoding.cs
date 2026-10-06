#region Purpose
// Detects a text file's BOM and newline style so ReplaceInFiles can write it back unchanged.
#endregion

#region Design
// No BOM means UTF-8 without a preamble. UTF-32 LE is detected before UTF-16 LE because
// its BOM starts with the same FF FE bytes. Decoders throw on invalid bytes, so a file
// that is not valid text in its detected encoding fails instead of being rewritten with
// U+FFFD. A UTF-8 or no-BOM file that contains a NUL byte is binary, which includes
// UTF-16/32 files without a BOM: they are skipped, not decoded. The Encoding stored
// here does not emit a BOM; GetBytes writes the detected preamble itself, once.
// Newline style: CRLF when the first \n follows a \r, else LF. CR only when the file has
// no \n at all. Only \r\n is folded to \n for the match, so a lone \r in an LF or CRLF
// file stays a literal character.
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
    int lineFeed = text.IndexOf('\n', StringComparison.Ordinal);
    if (lineFeed < 0)
    {
      return text.Contains('\r', StringComparison.Ordinal) ? Cr : Lf;
    }

    return lineFeed > 0 && text[lineFeed - 1] == '\r' ? Crlf : Lf;
  }

  public string ToLineFeed(string text)
  {
    ArgumentNullException.ThrowIfNull(text);
    return Sequence == "\r"
      ? text.Replace("\r", "\n", StringComparison.Ordinal)
      : text.Replace("\r\n", "\n", StringComparison.Ordinal);
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
      return new TextFileEncoding(Utf8(), [0xEF, 0xBB, 0xBF]);
    }

    if (HasPrefix(bytes, [0xFF, 0xFE, 0x00, 0x00]))
    {
      return new TextFileEncoding(new UTF32Encoding(bigEndian: false, byteOrderMark: false, throwOnInvalidCharacters: true), [0xFF, 0xFE, 0x00, 0x00]);
    }

    if (HasPrefix(bytes, [0xFF, 0xFE]))
    {
      return new TextFileEncoding(new UnicodeEncoding(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: true), [0xFF, 0xFE]);
    }

    if (HasPrefix(bytes, [0xFE, 0xFF]))
    {
      return new TextFileEncoding(new UnicodeEncoding(bigEndian: true, byteOrderMark: false, throwOnInvalidBytes: true), [0xFE, 0xFF]);
    }

    return new TextFileEncoding(Utf8(), []);
  }

  public bool IsBinary(ReadOnlySpan<byte> bytes)
  {
    return Encoding is UTF8Encoding && bytes.Contains((byte)0);
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

  private static UTF8Encoding Utf8()
  {
    return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
  }

  private static bool HasPrefix(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> prefix)
  {
    return bytes.Length >= prefix.Length && bytes[..prefix.Length].SequenceEqual(prefix);
  }
}
