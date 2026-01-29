#!/usr/bin/dotnet --
#:package TimeWarp.Jaribu

#if !JARIBU_MULTI
return await RunAllTests();
#endif

using TimeWarp.Amuru;

namespace TimeWarp.Amuru.Tests;

[TestTag("Native")]
public class GetChildItemTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<GetChildItemTests>();

  public static async Task Should_get_child_items_with_valid_directory()
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

      result.Success.ShouldBeTrue();
      result.Stdout.ShouldContain("file1.txt");
      result.Stdout.ShouldContain("file2.txt");
      result.Stdout.ShouldContain("subdir");
      result.ExitCode.ShouldBe(0);
    }
    finally
    {
      Directory.Delete(testDir, true);
    }

    await Task.CompletedTask;
  }

  public static async Task Should_fail_get_child_items_with_missing_directory()
  {
    string nonExistentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    CommandOutput result = Commands.GetChildItem(nonExistentDir);

    result.Success.ShouldBeFalse();
    result.Stderr.ShouldContain("No such file");
    result.ExitCode.ShouldBe(1);

    await Task.CompletedTask;
  }
}
