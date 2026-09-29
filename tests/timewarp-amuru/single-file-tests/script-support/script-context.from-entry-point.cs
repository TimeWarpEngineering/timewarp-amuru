#!/usr/bin/env -S dotnet --

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

    public static async Task NestedInnerContext_Should_RestoreEachOriginalDirectory()
    {
      string originalDir = Directory.GetCurrentDirectory();

      try
      {
        using (var outer = ScriptContext.FromEntryPoint())
        {
          string scriptDir = Directory.GetCurrentDirectory();
          using (var inner = ScriptContext.FromRelativePath(".."))
          {
            Directory.GetCurrentDirectory().ShouldNotBe(scriptDir);
            inner.ScriptDirectory.ShouldBe(scriptDir);
          }

          Directory.GetCurrentDirectory().ShouldBe(scriptDir);
          outer.ScriptDirectory.ShouldBe(scriptDir);
        }

        Directory.GetCurrentDirectory().ShouldBe(originalDir);
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDir);
      }

      await Task.CompletedTask;
    }

    public static async Task OnExit_Should_ObserveRestoredDirectory()
    {
      string originalDir = Directory.GetCurrentDirectory();
      string elsewhere = Path.Combine(Path.GetTempPath(), "script-context-" + Guid.NewGuid().ToString("N"));
      string? directoryDuringOnExit = null;
      Directory.CreateDirectory(elsewhere);

      try
      {
        using (ScriptContext.FromEntryPoint(
          changeToScriptDirectory: false,
          onExit: () => directoryDuringOnExit = Directory.GetCurrentDirectory()))
        {
          Directory.SetCurrentDirectory(elsewhere);
          Directory.GetCurrentDirectory().ShouldBe(elsewhere);
        }
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDir);
        if (Directory.Exists(elsewhere))
        {
          Directory.Delete(elsewhere);
        }
      }

      directoryDuringOnExit.ShouldBe(originalDir);
      await Task.CompletedTask;
    }

    [Timeout(5000)]
    public static async Task OnExitNestedContext_Should_NotDeadlock()
    {
      string originalDir = Directory.GetCurrentDirectory();
      bool nestedCreated = false;

      var work = Task.Run(() =>
      {
        using var outer = ScriptContext.FromEntryPoint(
          changeToScriptDirectory: false,
          onExit: () =>
          {
            using var nested = ScriptContext.FromEntryPoint(changeToScriptDirectory: false);
            nestedCreated = true;
          });

        outer.ScriptDirectory.ShouldNotBeNull();
      });

      try
      {
        await work.WaitAsync(TimeSpan.FromSeconds(5));
      }
      catch (TimeoutException)
      {
        throw new TimeoutException("Nested ScriptContext inside onExit deadlocked on SyncLock");
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDir);
      }

      nestedCreated.ShouldBeTrue();
      await Task.CompletedTask;
    }
  }
}
