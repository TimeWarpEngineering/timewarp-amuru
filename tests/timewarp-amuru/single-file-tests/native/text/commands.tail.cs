#!/usr/bin/env -S dotnet --

#region Purpose
// Commands.Tail slices lines and bytes and reports file errors the way tail does.
#endregion

#region Design
// SUT: Text.Commands. Each case uses a temp directory and deletes it in finally.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace TextTailCommands_
{
using Text = TimeWarp.Amuru.Native.Text;

[TestTag("Native")]
public class Tail_Given_
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<Tail_Given_>();

  public static async Task LastLines_Should_KeepTheTailOfAFile()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a\nb\nc\n");
      Text.TailOutput result = Text.Commands.Tail(["-n", "2", path]);

      result.ExitCode.ShouldBe(0);
      result.StdoutText.ShouldBe("b\nc\n");
      result.StderrText.ShouldBeEmpty();
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task PartialLastLine_Should_Count()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a\nb");
      Text.TailOutput result = Text.Commands.Tail(["-n", "1", path]);

      result.StdoutText.ShouldBe("b");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task BlankLastLine_Should_Count()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a\n\n");
      Text.TailOutput result = Text.Commands.Tail(["-n", "1", path]);

      result.StdoutText.ShouldBe("\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task FromLine_Should_SkipThePrefix()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a\nb\nc\n");
      Text.TailOutput result = Text.Commands.Tail(["-n", "+2", path]);

      result.StdoutText.ShouldBe("b\nc\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task FromLineZero_Should_PrintTheWholeFile()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a\nb\nc\n");
      Text.TailOutput result = Text.Commands.Tail(["-n", "+0", path]);

      result.StdoutText.ShouldBe("a\nb\nc\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task LastBytes_Should_KeepTheTail()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a\nb\nc\n");
      Text.TailOutput result = Text.Commands.Tail(["-c", "4", path]);

      result.StdoutText.ShouldBe("b\nc\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task BytesFromStart_Should_SkipThePrefix()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a\nb\nc\n");
      Text.TailOutput result = Text.Commands.Tail(["-c", "+2", path]);

      result.StdoutText.ShouldBe("\nb\nc\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task ByteSuffix_Should_Scale()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a\nb\nc\n");
      Text.TailOutput result = Text.Commands.Tail(["-c", "1K", path]);

      result.StdoutText.ShouldBe("a\nb\nc\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task ZeroLines_Should_PrintNothing()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a\nb\nc\n");
      Text.TailOutput result = Text.Commands.Tail(["-n", "0", path]);

      result.Stdout.Length.ShouldBe(0);
      result.ExitCode.ShouldBe(0);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task BinaryBytes_Should_PassThrough()
  {
    string directory = NewDirectory();
    try
    {
      byte[] payload = [0x00, 0xFF, 0xFE, 0x61];
      string path = Path.Combine(directory, "bin.dat");
      await File.WriteAllBytesAsync(path, payload);
      Text.TailOutput result = Text.Commands.Tail(["-c", "2", path]);

      result.Stdout.ToArray().ShouldBe(new byte[] { 0xFE, 0x61 });
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task ZeroTerminated_Should_SplitOnNul()
  {
    string directory = NewDirectory();
    try
    {
      string path = Path.Combine(directory, "nul.txt");
      await File.WriteAllBytesAsync(path, "a\0b\0c"u8.ToArray());
      Text.TailOutput result = Text.Commands.Tail(["-z", "-n", "1", path]);

      result.Stdout.ToArray().ShouldBe("c"u8.ToArray());
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task SeveralFiles_Should_PrintHeaders()
  {
    string directory = NewDirectory();
    try
    {
      string first = await WriteAsync(directory, "a.txt", "a\nb\nc\n");
      string second = await WriteAsync(directory, "b.txt", "d\n");
      Text.TailOutput result = Text.Commands.Tail(["-n", "1", first, second]);

      result.ExitCode.ShouldBe(0);
      result.StdoutText.ShouldBe($"==> {first} <==\nc\n\n==> {second} <==\nd\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Quiet_Should_OmitHeaders()
  {
    string directory = NewDirectory();
    try
    {
      string first = await WriteAsync(directory, "a.txt", "a\nb\nc\n");
      string second = await WriteAsync(directory, "b.txt", "d\n");
      Text.TailOutput result = Text.Commands.Tail(["-q", "-n", "1", first, second]);

      result.StdoutText.ShouldBe("c\nd\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Verbose_Should_HeadASingleFile()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "a.txt", "a\nb\nc\n");
      Text.TailOutput result = Text.Commands.Tail(["-v", "-n", "1", path]);

      result.StdoutText.ShouldBe($"==> {path} <==\nc\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task LastHeaderFlag_Should_Win()
  {
    string directory = NewDirectory();
    try
    {
      string first = await WriteAsync(directory, "a.txt", "a\nb\nc\n");
      string second = await WriteAsync(directory, "b.txt", "d\n");
      Text.TailOutput result = Text.Commands.Tail(["-v", "-q", "-n", "1", first, second]);

      result.StdoutText.ShouldBe("c\nd\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task EmptyFile_Should_StillGetAHeader()
  {
    string directory = NewDirectory();
    try
    {
      string empty = await WriteAsync(directory, "empty.txt", "");
      string notes = await WriteAsync(directory, "a.txt", "a\nb\nc\n");
      Text.TailOutput result = Text.Commands.Tail(["-n", "1", empty, notes]);

      result.StdoutText.ShouldBe($"==> {empty} <==\n\n==> {notes} <==\nc\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task MissingFile_Should_MatchTheOpenError()
  {
    string directory = NewDirectory();
    try
    {
      string missing = Path.Combine(directory, "missing.txt");
      string notes = await WriteAsync(directory, "a.txt", "a\nb\nc\n");
      Text.TailOutput result = Text.Commands.Tail(["-n", "1", missing, notes]);

      result.ExitCode.ShouldBe(1);
      result.StderrText.ShouldBe($"tail: cannot open '{missing}' for reading: No such file or directory\n");
      result.StdoutText.ShouldBe($"==> {notes} <==\nc\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Directory_Should_BeReportedAsAReadError()
  {
    string directory = NewDirectory();
    try
    {
      Text.TailOutput result = Text.Commands.Tail(["-n", "1", directory]);

      result.ExitCode.ShouldBe(1);
      result.StderrText.ShouldBe($"tail: error reading '{directory}': Is a directory\n");
      result.StdoutText.ShouldBeEmpty();
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task UnreadableFile_Should_ReportPermissionDenied()
  {
    if (OperatingSystem.IsWindows())
    {
      return;
    }

    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "secret.txt", "hidden\n");
      File.SetUnixFileMode(path, UnixFileMode.None);
      try
      {
        using FileStream probe = File.OpenRead(path);
        return;
      }
      catch (UnauthorizedAccessException)
      {
      }

      Text.TailOutput result = Text.Commands.Tail(["-n", "1", path]);
      result.ExitCode.ShouldBe(1);
      result.StderrText.ShouldBe($"tail: cannot open '{path}' for reading: Permission denied\n");
    }
    finally
    {
      foreach (string file in Directory.EnumerateFiles(directory))
      {
        File.SetUnixFileMode(file, UnixFileMode.UserRead | UnixFileMode.UserWrite);
      }

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
}
}
