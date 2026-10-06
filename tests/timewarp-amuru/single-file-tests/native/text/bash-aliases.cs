#!/usr/bin/env -S dotnet --

#region Purpose
// Bash.Grep, GrepDirect, Sed, and SedDirect forward to the native text commands.
#endregion

#region Design
// The static Bash using is already global in the test project, so these call the aliases directly.
// Each case uses an isolated amuru-text-<guid> directory so Sed's temp sibling stays inside it.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace TextBash_
{

[TestTag("Native")]
public class Aliases_Given_
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<Aliases_Given_>();

  public static async Task Grep_Should_UseSelectStringExitCodes()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\n");
      CommandOutput hit = Grep("alpha", path);
      CommandOutput miss = Grep("missing", path);

      hit.ExitCode.ShouldBe(0);
      hit.Stdout.ShouldBe($"{path}:1:alpha");
      miss.ExitCode.ShouldBe(1);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task Sed_Should_ReplaceAndExitZero()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\n");
      CommandOutput result = Sed("alpha", "beta", path);

      result.ExitCode.ShouldBe(0);
      result.Stdout.ShouldBe($"{path}: 1 replacement(s)");
      (await File.ReadAllTextAsync(path)).ShouldBe("beta\n");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task GrepDirect_Should_StreamTextMatches()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\nbeta\n");
      List<TimeWarp.Amuru.Native.Text.TextMatch> matches = [];
      await foreach (TimeWarp.Amuru.Native.Text.TextMatch match in GrepDirect("beta", path))
      {
        matches.Add(match);
      }

      matches.Count.ShouldBe(1);
      matches[0].LineNumber.ShouldBe(2);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  public static async Task SedDirect_Should_StreamReplaceResults()
  {
    string directory = NewDirectory();
    try
    {
      string path = await WriteAsync(directory, "notes.txt", "alpha\n");
      List<TimeWarp.Amuru.Native.Text.ReplaceResult> results = [];
      await foreach (TimeWarp.Amuru.Native.Text.ReplaceResult result in SedDirect("alpha", "beta", path))
      {
        results.Add(result);
      }

      results.Count.ShouldBe(1);
      results[0].Changed.ShouldBeTrue();
      (await File.ReadAllTextAsync(path)).ShouldBe("beta\n");
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
