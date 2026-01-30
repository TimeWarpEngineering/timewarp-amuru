#!/usr/bin/dotnet --

#region Purpose
// Tests for ScriptContext.FromEntryPoint() - validates script context creation from entry point
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: ScriptContext (the class providing script context management)
// Action: FromEntryPoint (the factory method being tested)
// Tests verify directory changes, disposal behavior, and property values
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ScriptContext_
{
  [TestTag("ScriptSupport")]
  public class FromEntryPoint_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<FromEntryPoint_Given_>();

    public static async Task Default_Should_ChangeToScriptDirectory()
    {
      string originalDir = Directory.GetCurrentDirectory();
      string? expectedDir = AppContext.EntryPointFileDirectoryPath();

      expectedDir.ShouldNotBeNull("EntryPointFileDirectoryPath should not be null");

      using (var context = ScriptContext.FromEntryPoint())
      {
        string currentDir = Directory.GetCurrentDirectory();

        currentDir.ShouldBe(expectedDir, "ScriptContext should change to script directory");
        context.ScriptDirectory.ShouldBe(currentDir, "ScriptDirectory property should match current directory");
      }

      // After disposal, directory should be restored
      string restoredDir = Directory.GetCurrentDirectory();
      restoredDir.ShouldBe(originalDir, "Directory should be restored after disposal");

      await Task.CompletedTask;
    }

    public static async Task ChangeToScriptDirectoryFalse_Should_KeepOriginalDirectory()
    {
      string originalDir = Directory.GetCurrentDirectory();

      using (var context = ScriptContext.FromEntryPoint(changeToScriptDirectory: false))
      {
        string currentDir = Directory.GetCurrentDirectory();

        currentDir.ShouldBe(originalDir, "Directory should not change when changeToScriptDirectory is false");
        context.ScriptDirectory.ShouldNotBeNull("ScriptDirectory property should still be set");
      }

      await Task.CompletedTask;
    }

    public static async Task OnExitCallback_Should_ExecuteOnDisposal()
    {
      bool cleanupExecuted = false;

      using (var context = ScriptContext.FromEntryPoint(
        changeToScriptDirectory: false,
        onExit: () => cleanupExecuted = true))
      {
        // Just using the context
        await Task.Delay(1);
      }

      cleanupExecuted.ShouldBeTrue("Cleanup callback should execute on disposal");
    }

    public static async Task Default_Should_SetScriptFilePath()
    {
      using (var context = ScriptContext.FromEntryPoint(changeToScriptDirectory: false))
      {
        context.ScriptFilePath.ShouldNotBeNull("ScriptFilePath should not be null");
        context.ScriptFilePath.ShouldEndWith(".cs");
      }

      await Task.CompletedTask;
    }
  }
}
