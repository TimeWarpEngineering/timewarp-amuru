#!/usr/bin/env -S dotnet --

#region Purpose
// Bash.Grep and Bash.Sed forward to the native text commands.
#endregion

#region Design
// The static Bash using is already global in the test project, so these call Grep and Sed directly.
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
    string path = Path.GetTempFileName();
    try
    {
      await File.WriteAllTextAsync(path, "alpha\n");
      CommandOutput hit = Grep("alpha", path);
      CommandOutput miss = Grep("missing", path);

      hit.ExitCode.ShouldBe(0);
      hit.Stdout.ShouldBe($"{path}:1:alpha");
      miss.ExitCode.ShouldBe(1);
    }
    finally
    {
      File.Delete(path);
    }
  }

  public static async Task Sed_Should_ReplaceAndExitZero()
  {
    string path = Path.GetTempFileName();
    try
    {
      await File.WriteAllTextAsync(path, "alpha\n");
      CommandOutput result = Sed("alpha", "beta", path);

      result.ExitCode.ShouldBe(0);
      result.Stdout.ShouldBe($"{path}: 1 replacement(s)");
      (await File.ReadAllTextAsync(path)).ShouldBe("beta\n");
    }
    finally
    {
      File.Delete(path);
    }
  }
}
}
