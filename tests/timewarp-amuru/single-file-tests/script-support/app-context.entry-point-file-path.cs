#!/usr/bin/dotnet --

#region Purpose
// Tests for AppContext.EntryPointFilePath() - validates the extension method that returns the script file path
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: AppContext (the class being extended)
// Action: EntryPointFilePath (the extension method being tested)
// Tests verify correct path retrieval and consistency with related methods
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace AppContext_
{
  [TestTag("ScriptSupport")]
  public class EntryPointFilePath_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<EntryPointFilePath_Given_>();

    public static async Task FileBasedApp_Should_ReturnCsFilePath()
    {
      string? filePath = AppContext.EntryPointFilePath();

      filePath.ShouldNotBeNull("EntryPointFilePath should not be null for file-based apps");
      filePath.ShouldEndWith(".cs");
      File.Exists(filePath).ShouldBeTrue($"EntryPointFilePath should point to an existing file: {filePath}");

      await Task.CompletedTask;
    }

    public static async Task PathsFromBothMethods_Should_BeConsistent()
    {
      string? filePath = AppContext.EntryPointFilePath();
      string? dirPath = AppContext.EntryPointFileDirectoryPath();

      filePath.ShouldNotBeNull("FilePath should not be null");
      dirPath.ShouldNotBeNull("DirPath should not be null");

      string? fileDir = Path.GetDirectoryName(filePath);

      fileDir.ShouldBe(dirPath, "Directory from EntryPointFilePath should match EntryPointFileDirectoryPath");

      await Task.CompletedTask;
    }
  }
}
