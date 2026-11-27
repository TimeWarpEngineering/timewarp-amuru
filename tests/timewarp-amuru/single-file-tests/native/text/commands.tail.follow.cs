#!/usr/bin/env -S dotnet --

#region Purpose
// Tail -f polls a file for appended bytes and notices truncation.
#endregion

#region Design
// Direct writes while it polls, so the test can watch the streams. Commands buffers
// until the call returns, which hides the follow. A file follow without a cancellation
// token fails instead of blocking.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace TextTailFollow_
{
using System.Text;
using Text = TimeWarp.Amuru.Native.Text;

[TestTag("Native")]
public class Follow_Given_
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<Follow_Given_>();

  public static async Task FollowWithoutAToken_Should_Fail()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "log.txt", "a\n");
      Text.TailOutput result = Text.Commands.Tail(["-f", path]);
      result.ExitCode.ShouldBe(1);
      result.StderrText.ShouldContain("Follow requires a cancellation token.");

      Text.TailOptions options = new() { Follow = true };
      ArgumentException error = Should.Throw<ArgumentException>(() =>
        Text.Direct.Tail(options, [path], Stream.Null, Stream.Null, Stream.Null, CancellationToken.None));
      error.Message.ShouldContain("Follow requires a cancellation token.");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Follow_Should_PrintAppendedBytesAndTruncation()
  {
    string directory = NewDirectory();
    using CancellationTokenSource cancel = new();
    using CapturedOutput stdout = new();
    using CapturedOutput stderr = new();
    Task<Text.TailRunResult>? running = null;
    try
    {
      string path = await WriteAsync(directory, "log.txt", "a\nb\nc\n");
      Text.TailOptions options = new()
      {
        Measure = Text.TailMeasure.LastLines(2),
        Follow = true,
        FollowInterval = TimeSpan.FromMilliseconds(20),
        SaveStream = false
      };
      running = Task.Run(() =>
        Text.Direct.Tail(options, [path], Stream.Null, stdout, stderr, cancel.Token));

      await WaitFor(() => TextOf(stdout).Contains("b\nc\n", StringComparison.Ordinal), stdout, stderr, "initial slice");
      await File.AppendAllTextAsync(path, "d\n");
      await WaitFor(() => TextOf(stdout).Contains("b\nc\nd\n", StringComparison.Ordinal), stdout, stderr, "appended line");
      await File.WriteAllTextAsync(path, "Z\n");
      await WaitFor(
        () => TextOf(stderr).Contains("file truncated\n", StringComparison.Ordinal)
          && TextOf(stdout).Contains("Z\n", StringComparison.Ordinal),
        stdout,
        stderr,
        "truncation");
    }
    finally
    {
      await cancel.CancelAsync();
      if (running is not null)
      {
        Text.TailRunResult result = await running.WaitAsync(TimeSpan.FromSeconds(3));
        result.ExitCode.ShouldBe(0);
      }

      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task FollowFromZero_Should_PrintOnlyNewBytes()
  {
    string directory = NewDirectory();
    using CancellationTokenSource cancel = new();
    using CapturedOutput stdout = new();
    using CapturedOutput stderr = new();
    Task? running = null;
    try
    {
      string path = await WriteAsync(directory, "log.txt", "a\nb\nc\n");
      Text.TailOptions options = new()
      {
        Measure = Text.TailMeasure.LastLines(0),
        Follow = true,
        FollowInterval = TimeSpan.FromMilliseconds(20),
        SaveStream = false
      };
      running = Task.Run(() =>
        Text.Direct.Tail(options, [path], Stream.Null, stdout, stderr, cancel.Token));

      await Task.Delay(100);
      await File.AppendAllTextAsync(path, "d\n");
      try
      {
        await WaitFor(() => TextOf(stdout).Contains("d\n", StringComparison.Ordinal), stdout, stderr, "appended line");
      }
      catch (TimeoutException)
      {
        await File.AppendAllTextAsync(path, "e\n");
        await WaitFor(() => TextOf(stdout).Contains("e\n", StringComparison.Ordinal), stdout, stderr, "later appended line");
      }

      TextOf(stdout).ShouldNotContain("a\n");
    }
    finally
    {
      await cancel.CancelAsync();
      if (running is not null)
      {
        await running.WaitAsync(TimeSpan.FromSeconds(3));
      }

      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task FollowOnAPipe_Should_ReadUntilEof()
  {
    using PipeStream pipe = new("a\nb\nc\n"u8.ToArray());
    Text.TailOutput result = Text.Commands.Tail(["-fn", "1"], pipe, CancellationToken.None);

    result.ExitCode.ShouldBe(0);
    result.StdoutText.ShouldBe("c\n");
    result.SavedPath.ShouldBeNull();
    result.StderrText.ShouldBeEmpty();
    await Task.CompletedTask;
  }

  public static async Task MissingFollowFile_Should_ReportNoFilesRemaining()
  {
    string directory = NewDirectory();
    try
    {
      string missing = Path.Combine(directory, "gone.txt");
      using CancellationTokenSource cancel = new();
      Text.TailOptions options = new() { Follow = true, FollowInterval = TimeSpan.FromMilliseconds(20) };
      Text.TailOutput result = Text.Commands.Tail(options, [missing], Stream.Null, cancel.Token);

      result.ExitCode.ShouldBe(1);
      result.StderrText.ShouldBe(
        $"tail: cannot open '{missing}' for reading: No such file or directory\ntail: no files remaining\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  private static string NewDirectory()
  {
    string path = Path.Combine(Path.GetTempPath(), "amuru-text-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(path);
    return path;
  }

  private static async Task<string> WriteAsync(string directory, string name, string content)
  {
    string path = Path.Combine(directory, name);
    await File.WriteAllTextAsync(path, content);
    return path;
  }

  private static string TextOf(CapturedOutput output)
  {
    return Encoding.UTF8.GetString(output.Snapshot());
  }

  private static async Task WaitFor(Func<bool> done, CapturedOutput stdout, CapturedOutput stderr, string what)
  {
    DateTime deadline = DateTime.UtcNow.AddSeconds(3);
    while (!done())
    {
      if (DateTime.UtcNow >= deadline)
      {
        throw new TimeoutException(
          what + " stdout=[" + TextOf(stdout) + "] stderr=[" + TextOf(stderr) + "]");
      }

      await Task.Delay(20);
    }
  }

  private sealed class PipeStream : Stream
  {
    private readonly MemoryStream Inner;

    public PipeStream(byte[] data)
    {
      Inner = new MemoryStream(data);
    }

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

    public override int Read(byte[] buffer, int offset, int count) => Inner.Read(buffer, offset, count);

    public override int Read(Span<byte> buffer) => Inner.Read(buffer);

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        Inner.Dispose();
      }

      base.Dispose(disposing);
    }
  }

  private sealed class CapturedOutput : Stream
  {
    private readonly MemoryStream Inner = new();
    private readonly object Gate = new();

    public byte[] Snapshot()
    {
      lock (Gate)
      {
        return Inner.ToArray();
      }
    }

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
      lock (Gate)
      {
        Inner.Write(buffer, offset, count);
      }
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
      lock (Gate)
      {
        Inner.Write(buffer);
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        Inner.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
}
