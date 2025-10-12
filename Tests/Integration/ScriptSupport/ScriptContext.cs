#!/usr/bin/dotnet --

await RunTests<ScriptContextTests>();

internal sealed class ScriptContextTests
{
  public static Task TestFromEntryPointChangesDirectory()
  {
    string originalDir = Directory.GetCurrentDirectory();
    string? expectedDir = AppContext.EntryPointFileDirectoryPath();
    
    AssertTrue(expectedDir != null, "EntryPointFileDirectoryPath should not be null");
    
    using (var context = ScriptContext.FromEntryPoint())
    {
      string currentDir = Directory.GetCurrentDirectory();
      
      AssertTrue(
        expectedDir == currentDir,
        $"ScriptContext should change to script directory: {expectedDir} != {currentDir}"
      );
      
      AssertTrue(
        context.ScriptDirectory == currentDir,
        $"ScriptDirectory property should match current directory: {context.ScriptDirectory} != {currentDir}"
      );
    }
    
    // After disposal, directory should be restored
    string restoredDir = Directory.GetCurrentDirectory();
    AssertTrue(
      originalDir == restoredDir,
      $"Directory should be restored after disposal: {originalDir} != {restoredDir}"
    );
    
    return Task.CompletedTask;
  }
  
  public static Task TestFromEntryPointWithoutDirectoryChange()
  {
    string originalDir = Directory.GetCurrentDirectory();
    
    using (var context = ScriptContext.FromEntryPoint(changeToScriptDirectory: false))
    {
      string currentDir = Directory.GetCurrentDirectory();
      
      AssertTrue(
        originalDir == currentDir,
        $"Directory should not change when changeToScriptDirectory is false: {originalDir} != {currentDir}"
      );
      
      AssertTrue(
        context.ScriptDirectory != null,
        "ScriptDirectory property should still be set"
      );
    }
    
    return Task.CompletedTask;
  }
  
  public static Task TestFromRelativePathNavigatesToParent()
  {
    string originalDir = Directory.GetCurrentDirectory();
    
    // Navigate to parent directory (one level up from script)
    using (var context = ScriptContext.FromRelativePath(".."))
    {
      string currentDir = Directory.GetCurrentDirectory();
      
      AssertTrue(
        currentDir != originalDir,
        $"Directory should change when using FromRoot: {originalDir} -> {currentDir}"
      );
      
      // Current directory should be the parent of the script directory
      string scriptDirName = Path.GetFileName(context.ScriptDirectory);
      string expectedParent = Path.GetDirectoryName(context.ScriptDirectory) ?? "";
      
      AssertTrue(
        currentDir == expectedParent,
        $"Should be in parent directory: expected {expectedParent}, got {currentDir}"
      );
    }
    
    // Verify restoration
    string restoredDir = Directory.GetCurrentDirectory();
    AssertTrue(
      originalDir == restoredDir,
      $"Directory should be restored after FromRelativePath disposal: {originalDir} != {restoredDir}"
    );
    
    return Task.CompletedTask;
  }
  
  public static async Task TestCleanupCallbackExecutes()
  {
    bool cleanupExecuted = false;
    
    using (var context = ScriptContext.FromEntryPoint(
      changeToScriptDirectory: false,
      onExit: () => cleanupExecuted = true))
    {
      // Just using the context
      await Task.Delay(1);
    }
    
    AssertTrue(
      cleanupExecuted,
      "Cleanup callback should execute on disposal"
    );
  }
  
  public static Task TestScriptFilePathIsSet()
  {
    using (var context = ScriptContext.FromEntryPoint(changeToScriptDirectory: false))
    {
      AssertTrue(
        context.ScriptFilePath != null,
        "ScriptFilePath should not be null"
      );
      
      AssertTrue(
        context.ScriptFilePath!.EndsWith(".cs", StringComparison.Ordinal),
        $"ScriptFilePath should end with .cs: {context.ScriptFilePath}"
      );
    }
    
    return Task.CompletedTask;
  }
}