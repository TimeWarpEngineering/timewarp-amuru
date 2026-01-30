#!/usr/bin/dotnet --

#region Purpose
// Tests for Commands.RemoveItem - validates file/directory removal via Commands API
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Commands (the static class providing command-style file operations)
// Action: RemoveItem (the method to delete files/directories)
// Tests verify removal with various options (recursive, force) and error cases
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Commands_
{
  [TestTag("Native")]
  public class RemoveItem_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<RemoveItem_Given_>();

    public static async Task File_Should_BeRemoved()
    {
      string testFile = Path.GetTempFileName();
      await File.WriteAllTextAsync(testFile, "test content");

      File.Exists(testFile).ShouldBeTrue();

      CommandOutput result = Commands.RemoveItem(testFile);

      result.Success.ShouldBeTrue();
      File.Exists(testFile).ShouldBeFalse();
      result.ExitCode.ShouldBe(0);
    }

    public static async Task DirectoryWithRecursive_Should_BeRemoved()
    {
      string testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(testDir);
      await File.WriteAllTextAsync(Path.Combine(testDir, "file1.txt"), "content1");
      await File.WriteAllTextAsync(Path.Combine(testDir, "file2.txt"), "content2");

      Directory.Exists(testDir).ShouldBeTrue();

      CommandOutput result = Commands.RemoveItem(testDir, recursive: true);

      result.Success.ShouldBeTrue();
      Directory.Exists(testDir).ShouldBeFalse();
    }

    public static async Task NonEmptyDirectoryWithoutRecursive_Should_Fail()
    {
      string testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(testDir);
      await File.WriteAllTextAsync(Path.Combine(testDir, "file.txt"), "content");

      try
      {
        CommandOutput result = Commands.RemoveItem(testDir, recursive: false);

        result.Success.ShouldBeFalse();
        Directory.Exists(testDir).ShouldBeTrue();
      }
      finally
      {
        Directory.Delete(testDir, true);
      }
    }

    public static async Task ReadOnlyFileWithForce_Should_BeRemoved()
    {
      string testFile = Path.GetTempFileName();
      await File.WriteAllTextAsync(testFile, "test content");
      var fileInfo = new FileInfo(testFile);
      fileInfo.IsReadOnly = true;

      try
      {
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

    public static async Task MissingPath_Should_Fail()
    {
      string nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

      CommandOutput result = Commands.RemoveItem(nonExistentPath);

      result.Success.ShouldBeFalse();
      result.Stderr.ShouldContain("No such file");
      result.ExitCode.ShouldBe(1);

      await Task.CompletedTask;
    }
  }
}
