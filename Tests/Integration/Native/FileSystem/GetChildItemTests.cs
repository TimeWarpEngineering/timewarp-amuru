#!/usr/bin/dotnet --
#:project ../../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../../../Tests/TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using TimeWarp.Amuru.Native.FileSystem;
using TimeWarp.Amuru.Test.Helpers;
using static TimeWarp.Amuru.Test.Helpers.Asserts;

await TestRunner.RunTests<GetChildItemTests>();

internal sealed class GetChildItemTests
{
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
}