#!/usr/bin/dotnet --

#region Purpose
// Tests for CliConfiguration.Reset() - validates resetting all configuration
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Reset clears all custom command paths
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace CliConfiguration_
{
  [TestTag("Configuration")]
  public class Reset_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Reset_Given_>();

    public static async Task MultiplePaths_Should_ClearAll()
    {
      List<string> tempFiles = [];

      try
      {
        for (int i = 0; i < 3; i++)
        {
          tempFiles.Add(await CreateExecutableTempFile());
        }

        CliConfiguration.SetCommandPath("fzf", tempFiles[0]);
        CliConfiguration.SetCommandPath("git", tempFiles[1]);
        CliConfiguration.SetCommandPath("gh", tempFiles[2]);

        CliConfiguration.AllCommandPaths.Count.ShouldBeGreaterThanOrEqualTo(3);

        CliConfiguration.Reset();

        CliConfiguration.AllCommandPaths.Count.ShouldBe(0);
      }
      finally
      {
        foreach (string file in tempFiles)
        {
          if (File.Exists(file)) File.Delete(file);
        }
      }
    }
  }
}
