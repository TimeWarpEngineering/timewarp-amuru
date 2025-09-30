#!/usr/bin/dotnet --
#:project ../../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../../../Tests/TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using TimeWarp.Amuru.Native.FileSystem;
using TimeWarp.Amuru.Test.Helpers;
using static TimeWarp.Amuru.Test.Helpers.Asserts;

await TestRunner.RunTests<RemoveItemTests>();

internal sealed class RemoveItemTests
{
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
}