#!/usr/bin/env -S dotnet --

#region Purpose
// A full save folder still prints the slice and removes the partial copy.
#endregion

#region Design
// The copy is a buffered FileStream. A full disk accepts the create, buffers the
// write, then throws from Flush and again from Dispose. The child runs inside a
// user and mount namespace so the tiny tmpfs is not this process's disk.
#endregion

#if !JARIBU_MULTI
if (Environment.GetEnvironmentVariable("AMURU_TAIL_FULL_DISK_CHILD") == "1")
{
  string? root = Environment.GetEnvironmentVariable("AMURU_TAIL_FULL_DISK_ROOT");
  if (string.IsNullOrWhiteSpace(root))
  {
    throw new InvalidOperationException("AMURU_TAIL_FULL_DISK_ROOT is not set.");
  }

  return await TextTailSave_.FullStore_Given_.RunFullDiskChild(root);
}

return await RunAllTests();
#endif

namespace TextTailSave_
{
using System.Globalization;
using System.Text;
using Text = TimeWarp.Amuru.Native.Text;

[TestTag("Native")]
public class FullStore_Given_
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<FullStore_Given_>();

  [Timeout(180000)]
  public static async Task FullStore_Should_StillPrintTheSliceAndRemoveTheFile()
  {
    if (!OperatingSystem.IsLinux())
    {
      return;
    }

    string root = NewDirectory();
    try
    {
      string notes = Path.Combine(root, "notes.txt");
      await File.WriteAllTextAsync(notes, "keep\n");
      CommandOutput child = await Shell.Builder("unshare")
        .WithArguments("--user", "--map-root-user", "--mount", "dotnet", "run", "--file", ThisFile())
        .WithEnvironmentVariable("AMURU_TAIL_FULL_DISK_CHILD", "1")
        .WithEnvironmentVariable("AMURU_TAIL_FULL_DISK_ROOT", root)
        .WithTimeout(TimeSpan.FromMinutes(2))
        .CaptureAsync();

      child.ExitCode.ShouldBe(0, child.Combined);
      File.Exists(Path.Combine(root, "full-disk-ok")).ShouldBeTrue(child.Combined);
    }
    finally
    {
      Directory.Delete(root, recursive: true);
    }
  }

  internal static async Task<int> RunFullDiskChild(string root)
  {
    string store = Path.Combine(root, "store");
    Directory.CreateDirectory(store);
    CommandOutput mounted = await Shell.Builder("mount")
      .WithArguments("-t", "tmpfs", "-o", "size=8192", "tmpfs", store)
      .CaptureAsync();
    mounted.ExitCode.ShouldBe(0, mounted.Combined);
    FillUntilFull(store);

    string notes = Path.Combine(root, "notes.txt");
    Text.TailOptions options = new()
    {
      Measure = Text.TailMeasure.LastLines(1),
      StreamDirectory = store,
      SaveStream = true,
      Headers = Text.TailHeaders.Never
    };
    using PipeStream pipe = new(Lines(20));
    Text.TailOutput result = Text.Commands.Tail(options, ["-", notes], pipe, CancellationToken.None);
    result.ExitCode.ShouldBe(0);
    result.StdoutText.ShouldBe("20\nkeep\n");
    result.StderrText.ShouldBeEmpty();
    result.SavedPath.ShouldBeNull();
    Directory.EnumerateFiles(store, "tail-*").ShouldBeEmpty();

    using MemoryStream stdout = new();
    using MemoryStream stderr = new();
    using PipeStream directPipe = new(Lines(20));
    Text.TailRunResult direct = Text.Direct.Tail(
      options,
      ["-", notes],
      directPipe,
      stdout,
      stderr,
      CancellationToken.None);
    direct.ExitCode.ShouldBe(0);
    direct.SavedPath.ShouldBeNull();
    Encoding.UTF8.GetString(stdout.ToArray()).ShouldBe("20\nkeep\n");
    Encoding.UTF8.GetString(stderr.ToArray()).ShouldBeEmpty();
    Directory.EnumerateFiles(store, "tail-*").ShouldBeEmpty();

    await File.WriteAllTextAsync(Path.Combine(root, "full-disk-ok"), "ok");
    return 0;
  }

  private static void FillUntilFull(string directory)
  {
    string fill = Path.Combine(directory, "fill");
    FileStreamOptions options = new()
    {
      Mode = FileMode.Create,
      Access = FileAccess.Write,
      Share = FileShare.None,
      BufferSize = 0
    };
    using FileStream stream = new(fill, options);
    byte[] block = new byte[4096];
    for (int blockIndex = 0; blockIndex < 8; blockIndex++)
    {
      try
      {
        stream.Write(block);
        stream.Flush();
      }
      catch (IOException)
      {
        return;
      }
    }

    throw new IOException("The save directory still accepts writes.");
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

  private static string ThisFile([CallerFilePath] string path = "") => path;

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
}
}
