#region Purpose
// Copies bytes read from a pipe into the saved stream, up to a fixed limit.
#endregion

#region Design
// The slice still sees every byte. The file stops at the limit so a huge pipe cannot
// fill the disk, and Capped tells the caller to say so in the stderr note.
// A write that fails sets Failed and later bytes are not copied. The caller drops
// the partial file and still finishes the slice.
// The caller owns both streams. They sit on a nested value so this stream does not
// dispose them when it is disposed.
#endregion

namespace TimeWarp.Amuru.Native.Text;

internal sealed class TailCaptureStream : Stream
{
  private readonly Borrowed BorrowedStreams;

  private sealed class Borrowed
  {
    public Borrowed(Stream inner, Stream copy)
    {
      Inner = inner;
      Copy = copy;
    }

    public Stream Inner { get; }

    public Stream Copy { get; }
  }

  private readonly long Limit;
  private long Written;

  public TailCaptureStream(Stream inner, Stream copy, long limit)
  {
    ArgumentNullException.ThrowIfNull(inner);
    ArgumentNullException.ThrowIfNull(copy);
    BorrowedStreams = new Borrowed(inner, copy);
    Limit = limit;
  }

  public bool Capped { get; private set; }

  public bool Failed { get; private set; }

  public override bool CanRead => true;

  public override bool CanSeek => false;

  public override bool CanWrite => false;

  public override long Length => throw new NotSupportedException();

  public override long Position
  {
    get => throw new NotSupportedException();
    set => throw new NotSupportedException();
  }

  public override void Flush()
  {
  }

  public override int Read(byte[] buffer, int offset, int count)
  {
    ArgumentNullException.ThrowIfNull(buffer);
    return Read(buffer.AsSpan(offset, count));
  }

  public override int Read(Span<byte> buffer)
  {
    int read = BorrowedStreams.Inner.Read(buffer);
    if (read > 0)
    {
      Capture(buffer[..read]);
    }

    return read;
  }

  public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

  public override void SetLength(long value) => throw new NotSupportedException();

  public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

  private void Capture(ReadOnlySpan<byte> data)
  {
    if (Capped || Failed)
    {
      return;
    }

    long room = Limit - Written;
    if (room <= 0)
    {
      Capped = true;
      return;
    }

    int take = (int)Math.Min(data.Length, room);
    try
    {
      BorrowedStreams.Copy.Write(data[..take]);
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
      Failed = true;
      return;
    }

    Written += take;
    if (take < data.Length)
    {
      Capped = true;
    }
  }
}
