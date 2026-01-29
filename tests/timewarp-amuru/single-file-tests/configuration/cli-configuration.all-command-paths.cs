#!/usr/bin/dotnet --

#region Purpose
// Tests for CliConfiguration.AllCommandPaths - validates retrieving all custom paths
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// AllCommandPaths property returns a dictionary of all configured custom paths
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace CliConfiguration_
{
  [TestTag("Configuration")]
  public class AllCommandPaths_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<AllCommandPaths_Given_>();

    public static async Task MultiplePaths_Should_ReturnDictionary()
    {
      CliConfiguration.Reset();
      List<string> tempFiles = [];

      try
      {
        for (int i = 0; i < 2; i++)
        {
          tempFiles.Add(await CreateExecutableTempFile());
        }

        CliConfiguration.SetCommandPath("cmd1", tempFiles[0]);
        CliConfiguration.SetCommandPath("cmd2", tempFiles[1]);

        IReadOnlyDictionary<string, string> paths = CliConfiguration.AllCommandPaths;

        paths.Count.ShouldBe(2);
        paths.ShouldContainKey("cmd1");
        paths["cmd1"].ShouldBe(tempFiles[0]);
        paths.ShouldContainKey("cmd2");
        paths["cmd2"].ShouldBe(tempFiles[1]);
      }
      finally
      {
        CliConfiguration.Reset();
        foreach (string file in tempFiles)
        {
          if (File.Exists(file)) File.Delete(file);
        }
      }
    }
  }
}
