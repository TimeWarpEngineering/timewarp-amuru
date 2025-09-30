#!/usr/bin/dotnet --
#:project ../../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../../../Tests/TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

await TestRunner.RunTests<RemoveItemTests>();

internal sealed class RemoveItemTests
{
  public static async Task TestRemoveItemWithFile()
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
  }

  public static async Task TestRemoveItemWithDirectory()
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

      result.Success.ShouldBeFalse();
      Directory.Exists(testDir).ShouldBeTrue();
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
  }

  public static async Task TestRemoveItemWithMissingPath()
  {
    string nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    CommandOutput result = Commands.RemoveItem(nonExistentPath);

    result.Success.ShouldBeFalse();
    result.Stderr.ShouldContain("No such file");
    result.ExitCode.ShouldBe(1);

    await Task.CompletedTask;
  }
}