#!/usr/bin/dotnet --

#region Purpose
// Tests for AppContext.EntryPointFileDirectoryPath() - validates the extension method that returns the script directory path
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: AppContext (the class being extended)
// Action: EntryPointFileDirectoryPath (the extension method being tested)
// Tests verify correct directory path retrieval
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace AppContext_
{
  [TestTag("ScriptSupport")]
  public class EntryPointFileDirectoryPath_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<EntryPointFileDirectoryPath_Given_>();

    public static async Task FileBasedApp_Should_ReturnExistingDirectory()
    {
      string? dirPath = AppContext.EntryPointFileDirectoryPath();

      dirPath.ShouldNotBeNull("EntryPointFileDirectoryPath should not be null for file-based apps");
      Directory.Exists(dirPath).ShouldBeTrue($"EntryPointFileDirectoryPath should point to an existing directory: {dirPath}");

      await Task.CompletedTask;
    }
  }
}
