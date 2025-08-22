#!/usr/bin/dotnet run

await RunTests<AppContextExtensionsTests>();

internal sealed class AppContextExtensionsTests
{
  public static Task TestEntryPointFilePathReturnsCorrectPath()
  {
    string? filePath = AppContext.EntryPointFilePath();
    
    AssertTrue(
      filePath != null,
      "EntryPointFilePath should not be null for file-based apps"
    );
    
    AssertTrue(
      filePath!.EndsWith(".cs", StringComparison.Ordinal),
      $"EntryPointFilePath should end with .cs, got: {filePath}"
    );
    
    AssertTrue(
      File.Exists(filePath),
      $"EntryPointFilePath should point to an existing file: {filePath}"
    );
    
    return Task.CompletedTask;
  }
  
  public static Task TestEntryPointFileDirectoryPathReturnsCorrectPath()
  {
    string? dirPath = AppContext.EntryPointFileDirectoryPath();
    
    AssertTrue(
      dirPath != null,
      "EntryPointFileDirectoryPath should not be null for file-based apps"
    );
    
    AssertTrue(
      Directory.Exists(dirPath),
      $"EntryPointFileDirectoryPath should point to an existing directory: {dirPath}"
    );
    
    return Task.CompletedTask;
  }
  
  public static Task TestBothExtensionsReturnConsistentPaths()
  {
    string? filePath = AppContext.EntryPointFilePath();
    string? dirPath = AppContext.EntryPointFileDirectoryPath();
    
    AssertTrue(filePath != null, "FilePath should not be null");
    AssertTrue(dirPath != null, "DirPath should not be null");
    
    string? fileDir = Path.GetDirectoryName(filePath);
    
    AssertTrue(
      fileDir == dirPath,
      $"Directory from EntryPointFilePath should match EntryPointFileDirectoryPath: {fileDir} != {dirPath}"
    );
    
    return Task.CompletedTask;
  }
}