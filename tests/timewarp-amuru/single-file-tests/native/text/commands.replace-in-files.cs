#!/usr/bin/env -S dotnet --

#region Purpose
// Commands.ReplaceInFiles: sed-style stdout and exit codes.
#endregion

#region Design
// Exit 0 includes the no-match case. Stdout lists only files whose text changed.
// Per-path failures go to stderr with the path and the walk continues.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace TextCommands_
{
using Text = TimeWarp.Amuru.Native.Text;

[TestTag("Native")]
public class ReplaceInFiles_Given_
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<ReplaceInFiles_Given_>();

  public static async Task Match_Should_ExitZeroAndListTheCount()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha alpha\n");
      CommandOutput result = Text.Commands.ReplaceInFiles("alpha", "beta", path);

      result.ExitCode.ShouldBe(0);
      result.Stdout.ShouldBe($"{path}: 2 replacement(s)");
      (await File.ReadAllTextAsync(path)).ShouldBe("beta beta\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task NoMatch_Should_ExitZeroWithEmptyStdout()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\n");
      CommandOutput result = Text.Commands.ReplaceInFiles("missing", "beta", path);

      result.ExitCode.ShouldBe(0);
      result.Stdout.ShouldBeEmpty();
      (await File.ReadAllTextAsync(path)).ShouldBe("alpha\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task InvalidPattern_Should_ExitOne()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\n");
      CommandOutput result = Text.Commands.ReplaceInFiles("[", "beta", path);

      result.ExitCode.ShouldBe(1);
      result.Stderr.ShouldContain("ReplaceInFiles:");
      (await File.ReadAllTextAsync(path)).ShouldBe("alpha\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task MissingPath_Should_ExitOne()
  {
    string missing = Path.Combine(Path.GetTempPath(), "amuru-text-missing-" + Guid.NewGuid().ToString("N"));
    CommandOutput result = Text.Commands.ReplaceInFiles("a", "b", missing);

    result.ExitCode.ShouldBe(1);
    result.Stderr.ShouldContain("No such file");
    await Task.CompletedTask;
  }

  public static async Task DryRun_Should_ReportTheChangeAndLeaveTheFile()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\n");
      Text.ReplaceInFilesOptions options = new() { DryRun = true };
      CommandOutput result = Text.Commands.ReplaceInFiles("alpha", "beta", path, options);

      result.ExitCode.ShouldBe(0);
      result.Stdout.ShouldBe($"{path}: 1 replacement(s)");
      (await File.ReadAllTextAsync(path)).ShouldBe("alpha\n");
      File.Exists(path + ".bak").ShouldBeFalse();
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Glob_Should_EditOnlyMatchingNames()
  {
    string directory = NewDirectory();
    try
    {
      string text = await WriteAsync(directory, "hit.txt", "alpha\n");
      string log = await WriteAsync(directory, "hit.log", "alpha\n");
      CommandOutput result = Text.Commands.ReplaceInFiles("alpha", "beta", directory, "*.txt");

      result.ExitCode.ShouldBe(0);
      result.Stdout.ShouldBe($"{text}: 1 replacement(s)");
      (await File.ReadAllTextAsync(text)).ShouldBe("beta\n");
      (await File.ReadAllTextAsync(log)).ShouldBe("alpha\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task MissingLaterPath_Should_KeepEarlierOutput()
  {
    string directory = NewDirectory();
    try
    {
      string first = await WriteAsync(directory, "a.txt", "alpha\n");
      string missing = Path.Combine(directory, "missing.txt");
      string third = await WriteAsync(directory, "c.txt", "alpha\n");
      CommandOutput result = Text.Commands.ReplaceInFiles("alpha", "beta", [first, missing, third]);

      result.ExitCode.ShouldBe(1);
      result.Stdout.ShouldBe($"{first}: 1 replacement(s)\n{third}: 1 replacement(s)");
      result.Stderr.ShouldBe($"ReplaceInFiles: {missing}: No such file or directory");
      (await File.ReadAllTextAsync(third)).ShouldBe("beta\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task InvalidText_Should_ReportThePathAndKeepWalking()
  {
    string directory = NewDirectory();
    try
    {
      string bad = Path.Combine(directory, "latin1.txt");
      byte[] original = [0x63, 0x61, 0x66, 0xE9, 0x0A, 0x66, 0x6F, 0x6F, 0x0A];
      await File.WriteAllBytesAsync(bad, original);
      string good = await WriteAsync(directory, "good.txt", "foo\n");
      CommandOutput result = Text.Commands.ReplaceInFiles("foo", "bar", [bad, good]);

      result.ExitCode.ShouldBe(1);
      result.Stdout.ShouldBe($"{good}: 1 replacement(s)");
      result.Stderr.ShouldBe($"ReplaceInFiles: {bad}: not valid utf-8 text");
      (await File.ReadAllBytesAsync(bad)).ShouldBe(original);
      (await File.ReadAllTextAsync(good)).ShouldBe("bar\n");
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
}
}
