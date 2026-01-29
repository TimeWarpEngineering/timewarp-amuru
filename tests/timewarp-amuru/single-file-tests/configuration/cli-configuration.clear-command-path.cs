#!/usr/bin/dotnet --

#region Purpose
// Tests for CliConfiguration.ClearCommandPath() - validates clearing custom command paths
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// ClearCommandPath removes a previously set custom path for a command
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace CliConfiguration_
{
  [TestTag("Configuration")]
  public class ClearCommandPath_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<ClearCommandPath_Given_>();

    public static async Task ExistingPath_Should_RemoveCustomPath()
    {
      string tempFile = await CreateExecutableTempFile();

      try
      {
        CliConfiguration.SetCommandPath("git", tempFile);
        CliConfiguration.HasCustomPath("git").ShouldBeTrue();

        CliConfiguration.ClearCommandPath("git");

        CliConfiguration.HasCustomPath("git").ShouldBeFalse();
      }
      finally
      {
        CliConfiguration.Reset();
        File.Delete(tempFile);
      }
    }

    private static async Task<string> CreateExecutableTempFile()
    {
      string tempFile = Path.GetTempFileName();

      if (!OperatingSystem.IsWindows())
      {
        await Shell.Builder("chmod").WithArguments("+x", tempFile).RunAsync();
      }

      return tempFile;
    }
  }
}
