#!/usr/bin/env -S dotnet --

#region Purpose
// Commands.SelectString: grep-format stdout and grep exit codes.
#endregion

#region Design
// SUT: Text.Commands. Each case uses a temp directory and deletes it in finally.
// Walk errors must not discard stdout already produced.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace TextCommands_
{
using Text = TimeWarp.Amuru.Native.Text;

[TestTag("Native")]
public class SelectString_Given_
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<SelectString_Given_>();

  public static async Task RegexMatch_Should_PrintPathLineTextAndExitZero()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\nbeta\nalpha\n");
      CommandOutput result = Text.Commands.SelectString("a.pha", path);

      result.ExitCode.ShouldBe(0);
      result.Stdout.ShouldBe($"{path}:1:alpha\n{path}:3:alpha");
      result.Stderr.ShouldBeEmpty();
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task LiteralMatch_Should_IgnoreRegexMetacharacters()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a.b\naxb\n");
      Text.SelectStringOptions options = new() { SimpleMatch = true };
      CommandOutput result = Text.Commands.SelectString("a.b", path, options);

      result.ExitCode.ShouldBe(0);
      result.Stdout.ShouldBe($"{path}:1:a.b");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task NoMatch_Should_ExitOne()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\n");
      CommandOutput result = Text.Commands.SelectString("missing", path);

      result.ExitCode.ShouldBe(1);
      result.Success.ShouldBeFalse();
      result.Stdout.ShouldBeEmpty();
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task InvalidPattern_Should_ExitTwo()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\n");
      CommandOutput result = Text.Commands.SelectString("[", path);

      result.ExitCode.ShouldBe(2);
      result.Stderr.ShouldContain("SelectString:");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task MissingPath_Should_ExitTwo()
  {
    string missing = Path.Combine(Path.GetTempPath(), "amuru-text-missing-" + Guid.NewGuid().ToString("N"));
    CommandOutput result = Text.Commands.SelectString("alpha", missing);

    result.ExitCode.ShouldBe(2);
    result.Stderr.ShouldContain("No such file");
    await Task.CompletedTask;
  }

  public static async Task InvertMatch_Should_EmitNonMatchingLines()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "keep\ndrop\n");
      Text.SelectStringOptions options = new() { InvertMatch = true };
      CommandOutput result = Text.Commands.SelectString("drop", path, options);

      result.ExitCode.ShouldBe(0);
      result.Stdout.ShouldBe($"{path}:1:keep");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Glob_Should_SearchOnlyMatchingNames()
  {
    string directory = NewDirectory();
    try
    {
      string text = await WriteAsync(directory, "hit.txt", "needle\n");
      await WriteAsync(directory, "hit.log", "needle\n");
      CommandOutput result = Text.Commands.SelectString("needle", directory, "*.txt");

      result.ExitCode.ShouldBe(0);
      result.Stdout.ShouldBe($"{text}:1:needle");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task SeveralPaths_Should_SearchEachFile()
  {
    string directory = NewDirectory();
    try
    {
      string first = await WriteAsync(directory, "a.txt", "one\n");
      string second = await WriteAsync(directory, "b.txt", "two\n");
      string[] paths = [first, second];
      CommandOutput result = Text.Commands.SelectString(".", paths);

      result.ExitCode.ShouldBe(0);
      result.Stdout.ShouldContain($"{first}:1:one");
      result.Stdout.ShouldContain($"{second}:1:two");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Reader_Should_LabelStandardInput()
  {
    using StringReader reader = new("alpha\nbeta\n");
    CommandOutput result = Text.Commands.SelectString("beta", reader);

    result.ExitCode.ShouldBe(0);
    result.Stdout.ShouldBe("-:2:beta");
    await Task.CompletedTask;
  }

  public static async Task TwoHitsOnOneLine_Should_PrintTheLineOnce()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "a a\nb\na\n");
      CommandOutput result = Text.Commands.SelectString("a", path);

      result.ExitCode.ShouldBe(0);
      result.Stdout.ShouldBe($"{path}:1:a a\n{path}:3:a");
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
      CommandOutput result = Text.Commands.SelectString("alpha", [first, missing, third]);

      result.ExitCode.ShouldBe(2);
      result.Stdout.ShouldBe($"{first}:1:alpha\n{third}:1:alpha");
      result.Stderr.ShouldBe($"SelectString: {missing}: No such file or directory");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task UnreadableDirectory_Should_KeepOtherHits()
  {
    if (OperatingSystem.IsWindows())
    {
      return;
    }

    string directory = NewDirectory();
    string locked = Path.Combine(directory, "locked");
    try
    {
      string path = await WriteAsync(directory, "a.txt", "alpha\n");
      Directory.CreateDirectory(locked);
      await WriteAsync(locked, "b.txt", "alpha\n");
      File.SetUnixFileMode(locked, UnixFileMode.None);
      if (CanList(locked))
      {
        await TimeWarpTerminal.Default.WriteLineAsync("Skipping: permissions are not enforced for this user.");
        return;
      }

      CommandOutput result = Text.Commands.SelectString("alpha", directory);

      result.ExitCode.ShouldBe(2);
      result.Stdout.ShouldBe($"{path}:1:alpha");
      result.Stderr.ShouldBe($"SelectString: {locked}: Permission denied");
    }
    finally
    {
      File.SetUnixFileMode(locked, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
      Directory.Delete(directory, recursive: true);
    }
  }

  private static bool CanList(string directory)
  {
    try
    {
      _ = Directory.EnumerateFileSystemEntries(directory).ToList();
      return true;
    }
    catch (UnauthorizedAccessException)
    {
      return false;
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
