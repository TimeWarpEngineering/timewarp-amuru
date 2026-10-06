#!/usr/bin/env -S dotnet --

#region Purpose
// Commands.ReplaceInFiles: sed-style stdout and exit codes.
#endregion

#region Design
// Exit 0 includes the no-match case. Stdout lists only files whose text changed.
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
