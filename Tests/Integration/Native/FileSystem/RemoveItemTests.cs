#!/usr/bin/dotnet --
#:package TimeWarp.Jaribu@1.0.0-beta.8
#:package TimeWarp.Amuru@1.0.0-beta.18

#if !JARIBU_MULTI
return await RunAllTests();
#endif

using TimeWarp.Amuru;

namespace TimeWarp.Amuru.Tests;

[TestTag("Native")]
public class RemoveItemTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<RemoveItemTests>();

  public static async Task Should_remove_file()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "test content");

    File.Exists(testFile).ShouldBeTrue();

    // Test Commands.RemoveItem
    CommandOutput result = Commands.RemoveItem(testFile);

    result.Success.ShouldBeTrue();
    File.Exists(testFile).ShouldBeFalse();
    result.ExitCode.ShouldBe(0);

    await Task.CompletedTask;
  }

  public static async Task Should_remove_directory_recursively()
  {
    // Create a test directory with files
    string testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(testDir);
    await File.WriteAllTextAsync(Path.Combine(testDir, "file1.txt"), "content1");
    await File.WriteAllTextAsync(Path.Combine(testDir, "file2.txt"), "content2");

    Directory.Exists(testDir).ShouldBeTrue();

    // Test Commands.RemoveItem with recursive flag
    CommandOutput result = Commands.RemoveItem(testDir, recursive: true);

    result.Success.ShouldBeTrue();
    Directory.Exists(testDir).ShouldBeFalse();

    await Task.CompletedTask;
  }

  public static async Task Should_fail_removing_non_empty_directory_without_recursive()
  {
    // Create a test directory with files
    string testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(testDir);
    await File.WriteAllTextAsync(Path.Combine(testDir, "file.txt"), "content");

    try
    {
      // Test Commands.RemoveItem without recursive flag
      CommandOutput result = Commands.RemoveItem(testDir, recursive: false);

      result.Success.ShouldBeFalse();
      Directory.Exists(testDir).ShouldBeTrue();
    }
    finally
    {
      Directory.Delete(testDir, true);
    }

    await Task.CompletedTask;
  }

  public static async Task Should_remove_read_only_file_with_force()
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

      result.Success.ShouldBeTrue();
      File.Exists(testFile).ShouldBeFalse();
    }
    finally
    {
      if (File.Exists(testFile))
      {
        fileInfo.IsReadOnly = false;
        File.Delete(testFile);
      }
    }

    await Task.CompletedTask;
  }

  public static async Task Should_fail_removing_missing_path()
  {
    string nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    CommandOutput result = Commands.RemoveItem(nonExistentPath);

    result.Success.ShouldBeFalse();
    result.Stderr.ShouldContain("No such file");
    result.ExitCode.ShouldBe(1);

    await Task.CompletedTask;
  }
}
