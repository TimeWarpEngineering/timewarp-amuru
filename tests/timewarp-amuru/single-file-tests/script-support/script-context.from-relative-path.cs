#!/usr/bin/dotnet --

#region Purpose
// Tests for ScriptContext.FromRelativePath() - validates script context creation with relative path navigation
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: ScriptContext (the class providing script context management)
// Action: FromRelativePath (the factory method being tested)
// Tests verify relative path navigation and directory restoration
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ScriptContext_
{
  [TestTag("ScriptSupport")]
  public class FromRelativePath_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<FromRelativePath_Given_>();

    public static async Task ParentPath_Should_NavigateToParentDirectory()
    {
      string originalDir = Directory.GetCurrentDirectory();

      // Navigate to parent directory (one level up from script)
      using (var context = ScriptContext.FromRelativePath(".."))
      {
        string currentDir = Directory.GetCurrentDirectory();

        currentDir.ShouldNotBe(originalDir, "Directory should change when using FromRelativePath");

        // Current directory should be the parent of the script directory
        string expectedParent = Path.GetDirectoryName(context.ScriptDirectory) ?? "";

        currentDir.ShouldBe(expectedParent, "Should be in parent directory");
      }

      // Verify restoration
      string restoredDir = Directory.GetCurrentDirectory();
      restoredDir.ShouldBe(originalDir, "Directory should be restored after FromRelativePath disposal");

      await Task.CompletedTask;
    }
  }
}
