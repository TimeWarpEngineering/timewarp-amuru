#!/usr/bin/dotnet --
#:project ../../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../../../Tests/TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

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
  }

  public static async Task TestGetChildItemWithMissingDirectory()
  {
    string nonExistentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    CommandOutput result = Commands.GetChildItem(nonExistentDir);

    result.Success.ShouldBeFalse();
    result.Stderr.ShouldContain("No such file");
    result.ExitCode.ShouldBe(1);
  }
}