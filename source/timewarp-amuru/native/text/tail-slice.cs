#region Purpose
// Copies the last N lines or bytes, or the tail of a file starting at a 1-based offset.
#endregion

#region Design
// The slice is bytes. A trailing partial line counts as a line. Newline and NUL are
// the only delimiters; CR is just a byte.
// A seekable stream is sliced over [Position, Length). A last-N request seeks from
// the end of that range so a large file is not loaded whole. A pipe is read once
// from the start and only the kept window is stored.
// A pipe window that does not fit in memory spills to a 0600 file removed when the read ends.
// A saturated count (long.MaxValue) is the whole stream: a suffix such as 999Y overflows
// to that, and tail then prints every byte.
#endregion

namespace TimeWarp.Amuru.Native.Text;

internal static class TailSlice
{
  private const int BlockSize = 8192;
  private const long ByteWindowLimit = 8L * 1024 * 1024;
  private const long LineWindowLimit = 1_000_000;

  public static long Copy(Stream input, Stream output, TailMeasure measure, byte delimiter)
  {
    ArgumentNullException.ThrowIfNull(input);
    ArgumentNullException.ThrowIfNull(output);
    if (measure.Count < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(measure), "The tail count cannot be negative.");
    }

    if (measure.FromStart)
    {
      if (measure.Unit == TailUnit.Bytes)
      {
        CopyFromByte(input, output, measure.Count);
      }
      else
      {
        CopyFromLine(input, output, measure.Count, delimiter);
      }

      return 0;
    }

    if (measure.Unit == TailUnit.Bytes)
    {
      CopyLastBytes(input, output, measure.Count);
      return 0;
    }

    return CopyLastLines(input, output, measure.Count, delimiter);
  }

  private static void CopyFromByte(Stream input, Stream output, long count)
  {
    long skip = count == 0 ? 0 : count - 1;
    if (input.CanSeek)
    {
      long start = input.Position;
      long length = input.Length;
      if (start >= length || skip >= length - start)
      {
        return;
      }

      input.Position = start + skip;
      input.CopyTo(output);
      return;
    }

    byte[] buffer = new byte[BlockSize];
    while (skip > 0)
    {
      int asked = (int)Math.Min(buffer.Length, skip);
      int read = input.Read(buffer, 0, asked);
      if (read == 0)
      {
        return;
      }

      skip -= read;
    }

    input.CopyTo(output);
  }

  private static void CopyFromLine(Stream input, Stream output, long count, byte delimiter)
  {
    long skip = count == 0 ? 0 : count - 1;
    if (skip == 0)
    {
      input.CopyTo(output);
      return;
    }

    byte[] buffer = new byte[BlockSize];
    long seen = 0;
    while (true)
    {
      int read = input.Read(buffer, 0, buffer.Length);
      if (read == 0)
      {
        return;
      }

      for (int index = 0; index < read; index++)
      {
        if (buffer[index] != delimiter)
        {
          continue;
        }

        seen++;
        if (seen == skip)
        {
          int from = index + 1;
          if (from < read)
          {
            output.Write(buffer, from, read - from);
          }

          input.CopyTo(output);
          return;
        }
      }
    }
  }

  private static void CopyLastBytes(Stream input, Stream output, long count)
  {
    if (count == 0)
    {
      if (!input.CanSeek)
      {
        Drain(input);
      }

      return;
    }

    if (count == long.MaxValue)
    {
      input.CopyTo(output);
      return;
    }

    if (input.CanSeek)
    {
      long start = input.Position;
      long length = input.Length;
      if (start >= length)
      {
        return;
      }

      input.Position = Math.Max(start, length - count);
      input.CopyTo(output);
      return;
    }

    if (count > ByteWindowLimit)
    {
      Spill(input, stream => CopyLastBytes(stream, output, count));
      return;
    }

    int window = (int)count;
    byte[] ring = new byte[window];
    byte[] buffer = new byte[BlockSize];
    int filled = 0;
    int next = 0;
    while (true)
    {
      int read = input.Read(buffer, 0, buffer.Length);
      if (read == 0)
      {
        break;
      }

      for (int index = 0; index < read; index++)
      {
        ring[next] = buffer[index];
        next++;
        if (next == window)
        {
          next = 0;
        }

        if (filled < window)
        {
          filled++;
        }
      }
    }

    if (filled < window)
    {
      output.Write(ring, 0, filled);
      return;
    }

    output.Write(ring, next, window - next);
    if (next > 0)
    {
      output.Write(ring, 0, next);
    }
  }

  private static long CopyLastLines(Stream input, Stream output, long count, byte delimiter)
  {
    if (count == long.MaxValue)
    {
      return CopyAllCounting(input, output, delimiter);
    }

    if (input.CanSeek)
    {
      CopyLastLinesSeekable(input, output, count, delimiter);
      return 0;
    }

    if (count > LineWindowLimit)
    {
      long seen = 0;
      Spill(input, stream =>
      {
        seen = CountLines(stream, delimiter);
        stream.Position = 0;
        CopyLastLinesSeekable(stream, output, count, delimiter);
      });
      return seen;
    }

    return CopyLastLinesForward(input, output, count, delimiter);
  }

  private static void CopyLastLinesSeekable(Stream input, Stream output, long count, byte delimiter)
  {
    if (count == 0)
    {
      return;
    }

    long length = input.Length;
    long start = input.Position;
    if (length <= start)
    {
      return;
    }

    byte[] block = new byte[BlockSize];
    long position = length;
    bool skipTerminator = ReadByteAt(input, length - 1) == delimiter;
    long found = 0;
    while (position > start)
    {
      int size = (int)Math.Min(block.Length, position - start);
      position -= size;
      input.Position = position;
      ReadExactly(input, block, size);
      for (int index = size - 1; index >= 0; index--)
      {
        if (block[index] != delimiter)
        {
          continue;
        }

        if (skipTerminator)
        {
          skipTerminator = false;
          continue;
        }

        found++;
        if (found == count)
        {
          input.Position = position + index + 1;
          input.CopyTo(output);
          return;
        }
      }
    }

    input.Position = start;
    input.CopyTo(output);
  }

  private static long CopyLastLinesForward(Stream input, Stream output, long count, byte delimiter)
  {
    Queue<byte[]> kept = new();
    using MemoryStream current = new();
    byte[] buffer = new byte[BlockSize];
    long seen = 0;
    while (true)
    {
      int read = input.Read(buffer, 0, buffer.Length);
      if (read == 0)
      {
        break;
      }

      for (int index = 0; index < read; index++)
      {
        current.WriteByte(buffer[index]);
        if (buffer[index] != delimiter)
        {
          continue;
        }

        seen++;
        Keep(kept, current.ToArray(), count);
        current.SetLength(0);
      }
    }

    if (current.Length > 0)
    {
      seen++;
      Keep(kept, current.ToArray(), count);
    }

    foreach (byte[] line in kept)
    {
      output.Write(line);
    }

    return seen;
  }

  private static void Keep(Queue<byte[]> kept, byte[] line, long count)
  {
    if (count == 0)
    {
      return;
    }

    kept.Enqueue(line);
    while (kept.Count > count)
    {
      kept.Dequeue();
    }
  }

  private static long CopyAllCounting(Stream input, Stream output, byte delimiter)
  {
    byte[] buffer = new byte[BlockSize];
    long seen = 0;
    bool any = false;
    bool endedOnDelimiter = false;
    while (true)
    {
      int read = input.Read(buffer, 0, buffer.Length);
      if (read == 0)
      {
        break;
      }

      output.Write(buffer, 0, read);
      for (int index = 0; index < read; index++)
      {
        any = true;
        if (buffer[index] == delimiter)
        {
          seen++;
          endedOnDelimiter = true;
        }
        else
        {
          endedOnDelimiter = false;
        }
      }
    }

    if (any && !endedOnDelimiter)
    {
      seen++;
    }

    return seen;
  }

  private static long CountLines(Stream input, byte delimiter)
  {
    byte[] buffer = new byte[BlockSize];
    long seen = 0;
    bool any = false;
    bool endedOnDelimiter = false;
    while (true)
    {
      int read = input.Read(buffer, 0, buffer.Length);
      if (read == 0)
      {
        break;
      }

      for (int index = 0; index < read; index++)
      {
        any = true;
        if (buffer[index] == delimiter)
        {
          seen++;
          endedOnDelimiter = true;
        }
        else
        {
          endedOnDelimiter = false;
        }
      }
    }

    if (any && !endedOnDelimiter)
    {
      seen++;
    }

    return seen;
  }

  private static void Spill(Stream input, Action<Stream> copy)
  {
    string path = Path.Combine(Path.GetTempPath(), "amuru-tail-spill-" + Guid.NewGuid().ToString("N"));
    try
    {
      FileStreamOptions options = new()
      {
        Mode = FileMode.CreateNew,
        Access = FileAccess.ReadWrite,
        Share = FileShare.None,
        Options = FileOptions.DeleteOnClose
      };
      if (!OperatingSystem.IsWindows())
      {
        options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
      }

      using FileStream spill = new(path, options);
      input.CopyTo(spill);
      spill.Position = 0;
      copy(spill);
    }
    finally
    {
      try
      {
        File.Delete(path);
      }
      catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
      {
      }
    }
  }

  private static void Drain(Stream input)
  {
    byte[] buffer = new byte[BlockSize];
    while (input.Read(buffer, 0, buffer.Length) > 0)
    {
    }
  }

  private static byte ReadByteAt(Stream stream, long position)
  {
    stream.Position = position;
    int value = stream.ReadByte();
    if (value < 0)
    {
      throw new EndOfStreamException("The file ended while tail was reading it.");
    }

    return (byte)value;
  }

  private static void ReadExactly(Stream stream, byte[] buffer, int count)
  {
    int offset = 0;
    while (offset < count)
    {
      int read = stream.Read(buffer, offset, count - offset);
      if (read == 0)
      {
        throw new EndOfStreamException("The file ended while tail was reading it.");
      }

      offset += read;
    }
  }
}
