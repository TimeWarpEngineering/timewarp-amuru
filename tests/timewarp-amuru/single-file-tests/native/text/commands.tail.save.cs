#!/usr/bin/env -S dotnet --

#region Purpose
// A cut pipe is saved. An uncut pipe is not. The save stays private and ages out.
#endregion

#region Design
// Pipe cases use a non-seekable stream so the save runs. StreamDirectory keeps those
// files out of the real per-user folder. The default-folder case touches .cleanup-stamp
// first so the hourly sweep does not delete anyone else's files.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace TextTailSave_
{
using System.Globalization;
using System.Text;
using Text = TimeWarp.Amuru.Native.Text;

[TestTag("Native")]
public class Save_Given_
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<Save_Given_>();

  public static async Task PipeCapture_Should_KeepOnlyTheSliceOnStdout()
  {
    HoldDefaultCleanup();
    string? saved = null;
    try
    {
      using PipeStream pipe = new(Lines(100));
      Text.TailOutput result = Text.Commands.Tail(["-1"], pipe, CancellationToken.None);
      saved = result.SavedPath;

      result.ExitCode.ShouldBe(0);
      result.StdoutText.ShouldBe("100\n");
      result.StdoutText.ShouldNotContain("full output");
      result.StderrText.ShouldContain("tail: showed last 1 of 100 lines; full output: ");
      result.SavedPath.ShouldNotBeNull();
      File.Exists(result.SavedPath).ShouldBeTrue();
      Path.GetFileName(result.SavedPath).ShouldMatch(@"^tail-\d{8}T\d{9}Z-\d+-[0-9a-f]{8}$");
      result.SavedPath.ShouldStartWith(DefaultStreamDirectory());
    }
    finally
    {
      if (saved is not null && File.Exists(saved))
      {
        File.Delete(saved);
      }
    }
  }

  public static async Task UncutPipe_Should_DeleteTheSavedFile()
  {
    string directory = NewDirectory();
    try
    {
      using PipeStream pipe = new("a\nb\nc\n"u8.ToArray());
      Text.TailOptions options = Options(directory, Text.TailMeasure.LastLines(10));
      Text.TailOutput result = Text.Commands.Tail(options, [], pipe, CancellationToken.None);

      result.ExitCode.ShouldBe(0);
      result.StdoutText.ShouldBe("a\nb\nc\n");
      result.StderrText.ShouldBeEmpty();
      result.SavedPath.ShouldBeNull();
      Directory.EnumerateFiles(directory, "tail-*").ShouldBeEmpty();
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task SaveDisabled_Should_DropTheNoteAndTheFile()
  {
    string directory = NewDirectory();
    try
    {
      using PipeStream pipe = new(Lines(20));
      Text.TailOptions options = Options(directory, Text.TailMeasure.LastLines(1)) with { SaveStream = false };
      Text.TailOutput result = Text.Commands.Tail(options, [], pipe, CancellationToken.None);

      result.StdoutText.ShouldBe("20\n");
      result.StderrText.ShouldBeEmpty();
      result.SavedPath.ShouldBeNull();
      Directory.EnumerateFiles(directory, "tail-*").ShouldBeEmpty();
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task SaveCap_Should_StopAndSaySo()
  {
    string directory = NewDirectory();
    try
    {
      using PipeStream pipe = new(Lines(40));
      Text.TailOptions options = Options(directory, Text.TailMeasure.LastLines(1)) with { SaveLimit = 32 };
      Text.TailOutput result = Text.Commands.Tail(options, [], pipe, CancellationToken.None);

      result.StdoutText.ShouldBe("40\n");
      result.StderrText.ShouldContain("tail: showed last 1 of 40 lines; full output: ");
      result.StderrText.ShouldContain(" (stopped at 32 bytes)\n");
      result.SavedPath.ShouldNotBeNull();
      new FileInfo(result.SavedPath).Length.ShouldBe(32);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task SavedNames_Should_Differ()
  {
    string directory = NewDirectory();
    try
    {
      using PipeStream first = new(Lines(5));
      using PipeStream second = new(Lines(5));
      Text.TailOptions options = Options(directory, Text.TailMeasure.LastLines(1));
      Text.TailOutput left = Text.Commands.Tail(options, [], first, CancellationToken.None);
      Text.TailOutput right = Text.Commands.Tail(options, [], second, CancellationToken.None);

      left.SavedPath.ShouldNotBeNull();
      right.SavedPath.ShouldNotBeNull();
      left.SavedPath.ShouldNotBe(right.SavedPath);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task UnixModes_Should_BePrivate()
  {
    if (OperatingSystem.IsWindows())
    {
      return;
    }

    string directory = NewDirectory();
    try
    {
      using PipeStream pipe = new(Lines(5));
      Text.TailOptions options = Options(directory, Text.TailMeasure.LastLines(1));
      Text.TailOutput result = Text.Commands.Tail(options, [], pipe, CancellationToken.None);

      result.SavedPath.ShouldNotBeNull();
      File.GetUnixFileMode(directory).ShouldBe(
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
      File.GetUnixFileMode(result.SavedPath).ShouldBe(UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task OldSavedFiles_Should_BeDeletedWhenTheStampIsDue()
  {
    string directory = NewDirectory();
    try
    {
      string stale = Path.Combine(directory, "tail-stale");
      await File.WriteAllBytesAsync(stale, "old"u8.ToArray());
      File.SetLastWriteTimeUtc(stale, DateTime.UtcNow.AddHours(-25));
      string stamp = Path.Combine(directory, ".cleanup-stamp");
      await File.WriteAllBytesAsync(stamp, []);
      File.SetLastWriteTimeUtc(stamp, DateTime.UtcNow.AddHours(-2));

      using PipeStream pipe = new(Lines(5));
      Text.TailOptions options = Options(directory, Text.TailMeasure.LastLines(1));
      Text.TailOutput result = Text.Commands.Tail(options, [], pipe, CancellationToken.None);

      result.SavedPath.ShouldNotBeNull();
      File.Exists(stale).ShouldBeFalse();
      File.Exists(result.SavedPath).ShouldBeTrue();
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task RecentStamp_Should_SkipCleanup()
  {
    string directory = NewDirectory();
    try
    {
      string stale = Path.Combine(directory, "tail-stale");
      await File.WriteAllBytesAsync(stale, "old"u8.ToArray());
      File.SetLastWriteTimeUtc(stale, DateTime.UtcNow.AddHours(-25));
      string stamp = Path.Combine(directory, ".cleanup-stamp");
      await File.WriteAllBytesAsync(stamp, []);
      File.SetLastWriteTimeUtc(stamp, DateTime.UtcNow.AddMinutes(-10));

      using PipeStream pipe = new(Lines(5));
      Text.TailOptions options = Options(directory, Text.TailMeasure.LastLines(1));
      Text.Commands.Tail(options, [], pipe, CancellationToken.None);

      File.Exists(stale).ShouldBeTrue();
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task SeekableInput_Should_ReadFromTheEnd()
  {
    byte[] payload = Lines(30_000);
    using CountingStream input = new(payload);
    Text.TailOptions options = new()
    {
      Measure = Text.TailMeasure.LastLines(2),
      SaveStream = false
    };
    Text.TailOutput result = Text.Commands.Tail(options, [], input, CancellationToken.None);

    result.StdoutText.ShouldBe("29999\n30000\n");
    input.PrefixReads.ShouldBe(0);
    await Task.CompletedTask;
  }

  public static async Task HelpAndVersion_Should_PrintNothing()
  {
    Text.TailOutput help = Text.Commands.Tail(["--help", "-x"]);
    help.ExitCode.ShouldBe(0);
    help.HelpRequested.ShouldBeTrue();
    help.Stdout.Length.ShouldBe(0);
    help.Stderr.Length.ShouldBe(0);

    Text.TailOutput version = Text.Commands.Tail(["--version"]);
    version.ExitCode.ShouldBe(0);
    version.VersionRequested.ShouldBeTrue();
    version.Stdout.Length.ShouldBe(0);

    Text.TailOutput unknown = Text.Commands.Tail(["-x"]);
    unknown.ExitCode.ShouldBe(1);
    unknown.StderrText.ShouldContain("Supported forms:");
    await Task.CompletedTask;
  }

  private static Text.TailOptions Options(string directory, Text.TailMeasure measure)
  {
    return new Text.TailOptions
    {
      Measure = measure,
      StreamDirectory = directory,
      SaveStream = true
    };
  }

  private static byte[] Lines(int count)
  {
    using MemoryStream stream = new();
    for (int line = 1; line <= count; line++)
    {
      stream.Write(Encoding.UTF8.GetBytes(line.ToString(CultureInfo.InvariantCulture)));
      stream.WriteByte((byte)'\n');
    }

    return stream.ToArray();
  }

  private static string NewDirectory()
  {
    string path = Path.Combine(Path.GetTempPath(), "amuru-text-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(path);
    return path;
  }

  private static string DefaultStreamDirectory()
  {
    return Path.Combine(Path.GetTempPath(), "tw-agent-streams", Sanitize(Environment.UserName));
  }

  private static void HoldDefaultCleanup()
  {
    string directory = DefaultStreamDirectory();
    Directory.CreateDirectory(directory);
    string stamp = Path.Combine(directory, ".cleanup-stamp");
    File.WriteAllBytes(stamp, []);
    File.SetLastWriteTimeUtc(stamp, DateTime.UtcNow);
  }

  private static string Sanitize(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return "unknown";
    }

    char[] chars = name.ToCharArray();
    for (int index = 0; index < chars.Length; index++)
    {
      if (chars[index] is '/' or '\\' or ':' or '\0')
      {
        chars[index] = '_';
      }
    }

    return new string(chars);
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

  private sealed class CountingStream : MemoryStream
  {
    public CountingStream(byte[] data)
      : base(data, writable: false)
    {
    }

    public int PrefixReads { get; private set; }

    public override int Read(byte[] buffer, int offset, int count)
    {
      Note();
      return base.Read(buffer, offset, count);
    }

    public override int Read(Span<byte> buffer)
    {
      Note();
      return base.Read(buffer);
    }

    private void Note()
    {
      if (Position < Length / 2)
      {
        PrefixReads++;
      }
    }
  }
}
}
