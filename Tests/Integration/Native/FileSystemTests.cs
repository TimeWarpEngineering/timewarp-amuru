#!/usr/bin/dotnet --
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../../Tests/TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using TimeWarp.Amuru.Native.FileSystem;
using TimeWarp.Amuru.Test.Helpers;
using static TimeWarp.Amuru.Native.Aliases.Bash;
using static TimeWarp.Amuru.Test.Helpers.Asserts;

// Use the existing TestRunner from test helpers
await TestRunner.RunTests<FileSystemTests>();

internal sealed class FileSystemTests
{
  public static async Task TestGetContentWithExistingFile()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    string content = "Line 1\nLine 2\nLine 3";
    await File.WriteAllTextAsync(testFile, content);
    
    try
    {
      // Test Commands.GetContent
      CommandOutput result = Commands.GetContent(testFile);
      
      AssertTrue(
        result.Success,
        "GetContent should succeed for existing file"
      );
      
      AssertTrue(
        result.Stdout == content,
        $"Content should match. Expected: '{content}', Got: '{result.Stdout}'"
      );
      
      AssertTrue(
        result.ExitCode == 0,
        "Exit code should be 0 for success"
      );
    }
    finally
    {
      File.Delete(testFile);
    }
  }

  public static async Task TestGetContentWithMissingFile()
  {
    string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    
    CommandOutput result = Commands.GetContent(nonExistentFile);
    
    AssertFalse(
      result.Success,
      "GetContent should fail for non-existent file"
    );
    
    AssertTrue(
      result.Stderr.Contains("No such file", StringComparison.Ordinal),
      $"Error message should indicate file not found. Got: '{result.Stderr}'"
    );
    
    AssertTrue(
      result.ExitCode == 1,
      "Exit code should be 1 for failure"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestGetChildItemWithValidDirectory()
  {
    // Create a test directory with some files
    string testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(testDir);
    
    try
    {
      // Create test files
      await File.WriteAllTextAsync(Path.Combine(testDir, "file1.txt"), "content1");
      await File.WriteAllTextAsync(Path.Combine(testDir, "file2.txt"), "content2");
      Directory.CreateDirectory(Path.Combine(testDir, "subdir"));
      
      // Test Commands.GetChildItem
      CommandOutput result = Commands.GetChildItem(testDir);
      
      AssertTrue(
        result.Success,
        "GetChildItem should succeed for existing directory"
      );
      
      AssertTrue(
        result.Stdout.Contains("file1.txt", StringComparison.Ordinal),
        "Output should contain file1.txt"
      );
      
      AssertTrue(
        result.Stdout.Contains("file2.txt", StringComparison.Ordinal),
        "Output should contain file2.txt"
      );
      
      AssertTrue(
        result.Stdout.Contains("subdir", StringComparison.Ordinal),
        "Output should contain subdir"
      );
      
      AssertTrue(
        result.ExitCode == 0,
        "Exit code should be 0 for success"
      );
    }
    finally
    {
      Directory.Delete(testDir, true);
    }
  }

  public static async Task TestBashAliases()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "test content");
    
    try
    {
      // Test Cat alias
      CommandOutput catResult = Cat(testFile);
      AssertTrue(
        catResult.Success && catResult.Stdout == "test content",
        "Cat() alias should work like GetContent()"
      );
      
      // Test Ls alias
      CommandOutput lsResult = Ls(Path.GetTempPath());
      AssertTrue(
        lsResult.Success,
        "Ls() alias should work like GetChildItem()"
      );
      
      // Test Pwd alias
      CommandOutput pwdResult = Pwd();
      AssertTrue(
        pwdResult.Success && !string.IsNullOrEmpty(pwdResult.Stdout),
        "Pwd() alias should return current directory"
      );
    }
    finally
    {
      File.Delete(testFile);
    }
  }

  public static async Task TestDirectApiStreaming()
  {
    // Create a test file with multiple lines
    string testFile = Path.GetTempFileName();
    IEnumerable<string> lines = Enumerable.Range(1, 100).Select(i => $"Line {i}");
    await File.WriteAllLinesAsync(testFile, lines);
    
    try
    {
      // Test Direct.GetContent streaming
      List<string> readLines = new();
      await foreach (string line in Direct.GetContent(testFile))
      {
        readLines.Add(line);
        
        // Verify we're actually streaming (could check memory usage in real scenario)
        if (readLines.Count == 10)
        {
          // We've read 10 lines, rest should still be pending
          AssertTrue(
            readLines[9] == "Line 10",
            "Should be reading lines in order"
          );
        }
      }
      
      AssertTrue(
        readLines.Count == 100,
        $"Should read all 100 lines. Got: {readLines.Count}"
      );
    }
    finally
    {
      File.Delete(testFile);
    }
  }

  public static async Task TestBothApisCanCoexist()
  {
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "coexist test");
    
    try
    {
      // Commands API - returns CommandOutput
      CommandOutput commandResult = Commands.GetContent(testFile);
      AssertTrue(
        commandResult.Stdout == "coexist test",
        "Commands API should work"
      );
      
      // Direct API - returns IAsyncEnumerable
      List<string> lines = new();
      await foreach (string line in Direct.GetContent(testFile))
      {
        lines.Add(line);
      }
      
      AssertTrue(
        lines[0] == "coexist test",
        "Direct API should work"
      );
      
      // Both results should be the same content
      AssertTrue(
        commandResult.Stdout == string.Join("\n", lines),
        "Both APIs should return same content"
      );
    }
    finally
    {
      File.Delete(testFile);
    }
  }

  public static async Task TestGetChildItemWithMissingDirectory()
  {
    string nonExistentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    
    CommandOutput result = Commands.GetChildItem(nonExistentDir);
    
    AssertFalse(
      result.Success,
      "GetChildItem should fail for non-existent directory"
    );
    
    AssertTrue(
      result.Stderr.Contains("No such file", StringComparison.Ordinal),
      $"Error should indicate directory not found. Got: '{result.Stderr}'"
    );
    
    AssertTrue(
      result.ExitCode == 1,
      "Exit code should be 1 for failure"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestDirectApiThrowsExceptions()
  {
    string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    bool exceptionThrown = false;
    try
    {
      await foreach (string line in Direct.GetContent(nonExistentFile))
      {
        // Should not get here
      }
    }
    catch (FileNotFoundException)
    {
      exceptionThrown = true;
    }

    AssertTrue(
      exceptionThrown,
      "Direct API should throw FileNotFoundException for missing files"
    );
  }

  public static async Task TestRemoveItemWithFile()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "test content");

    AssertTrue(
      File.Exists(testFile),
      "Test file should exist before deletion"
    );

    // Test Commands.RemoveItem
    CommandOutput result = Commands.RemoveItem(testFile);

    AssertTrue(
      result.Success,
      "RemoveItem should succeed for existing file"
    );

    AssertFalse(
      File.Exists(testFile),
      "File should no longer exist after deletion"
    );

    AssertTrue(
      result.ExitCode == 0,
      "Exit code should be 0 for success"
    );
  }

  public static async Task TestRemoveItemWithDirectory()
  {
    // Create a test directory with files
    string testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(testDir);
    await File.WriteAllTextAsync(Path.Combine(testDir, "file1.txt"), "content1");
    await File.WriteAllTextAsync(Path.Combine(testDir, "file2.txt"), "content2");

    AssertTrue(
      Directory.Exists(testDir),
      "Test directory should exist before deletion"
    );

    // Test Commands.RemoveItem with recursive flag
    CommandOutput result = Commands.RemoveItem(testDir, recursive: true);

    AssertTrue(
      result.Success,
      "RemoveItem should succeed with recursive flag"
    );

    AssertFalse(
      Directory.Exists(testDir),
      "Directory should no longer exist after deletion"
    );
  }

  public static async Task TestRemoveItemWithNonEmptyDirectoryFails()
  {
    // Create a test directory with files
    string testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(testDir);
    await File.WriteAllTextAsync(Path.Combine(testDir, "file.txt"), "content");

    try
    {
      // Test Commands.RemoveItem without recursive flag
      CommandOutput result = Commands.RemoveItem(testDir, recursive: false);

      AssertFalse(
        result.Success,
        "RemoveItem should fail for non-empty directory without recursive flag"
      );

      AssertTrue(
        Directory.Exists(testDir),
        "Directory should still exist after failed deletion"
      );
    }
    finally
    {
      Directory.Delete(testDir, true);
    }
  }

  public static async Task TestRemoveItemWithReadOnlyFile()
  {
    // Create a read-only test file
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "test content");
    var fileInfo = new FileInfo(testFile);
    fileInfo.IsReadOnly = true;

    try
    {
      // Test Commands.RemoveItem with force flag
      CommandOutput result = Commands.RemoveItem(testFile, force: true);

      AssertTrue(
        result.Success,
        "RemoveItem with force should succeed for read-only file"
      );

      AssertFalse(
        File.Exists(testFile),
        "Read-only file should be deleted with force flag"
      );
    }
    finally
    {
      if (File.Exists(testFile))
      {
        fileInfo.IsReadOnly = false;
        File.Delete(testFile);
      }
    }
  }

  public static async Task TestRemoveItemWithMissingPath()
  {
    string nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    CommandOutput result = Commands.RemoveItem(nonExistentPath);

    AssertFalse(
      result.Success,
      "RemoveItem should fail for non-existent path"
    );

    AssertTrue(
      result.Stderr.Contains("No such file", StringComparison.Ordinal),
      $"Error should indicate path not found. Got: '{result.Stderr}'"
    );

    AssertTrue(
      result.ExitCode == 1,
      "Exit code should be 1 for failure"
    );

    await Task.CompletedTask;
  }

  public static async Task TestRmAlias()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "test content");

    // Test Rm alias
    CommandOutput result = Rm(testFile);

    AssertTrue(
      result.Success,
      "Rm() alias should work like RemoveItem()"
    );

    AssertFalse(
      File.Exists(testFile),
      "File should be deleted by Rm alias"
    );
  }

  public static async Task TestRmDirectAlias()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "test content");

    // Test RmDirect alias
    RmDirect(testFile);

    AssertFalse(
      File.Exists(testFile),
      "File should be deleted by RmDirect alias"
    );

    await Task.CompletedTask;
  }
}