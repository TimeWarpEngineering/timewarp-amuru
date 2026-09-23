#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Commands.GetChildItem - validates directory listing via Commands API
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Commands (the static class providing command-style file operations)
// Action: GetChildItem (the method to list directory contents)
// Tests verify directory listing with success and error cases
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Commands_
{
  [TestTag("Native")]
  public class GetChildItem_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<GetChildItem_Given_>();

    public static async Task ValidDirectory_Should_ReturnItems()
    {
      string testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(testDir);

      try
      {
        await File.WriteAllTextAsync(Path.Combine(testDir, "file1.txt"), "content1");
        await File.WriteAllTextAsync(Path.Combine(testDir, "file2.txt"), "content2");
        Directory.CreateDirectory(Path.Combine(testDir, "subdir"));

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

    public static async Task MissingDirectory_Should_Fail()
    {
      string nonExistentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

      CommandOutput result = Commands.GetChildItem(nonExistentDir);

      result.Success.ShouldBeFalse();
      result.Stderr.ShouldContain("No such file");
      result.ExitCode.ShouldBe(1);

      await Task.CompletedTask;
    }

    public static async Task GlobPattern_Should_FilterByName()
    {
      string testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(testDir);

      try
      {
        await File.WriteAllTextAsync(Path.Combine(testDir, "keep.txt"), "k");
        await File.WriteAllTextAsync(Path.Combine(testDir, "skip.log"), "s");

        CommandOutput result = Commands.GetChildItem(Path.Combine(testDir, "*.txt"));

        result.Success.ShouldBeTrue();
        result.Stdout.ShouldContain("keep.txt");
        result.Stdout.ShouldNotContain("skip.log");
      }
      finally
      {
        Directory.Delete(testDir, true);
      }
    }

    public static async Task RecursiveExcludeAndHidden_Should_Filter()
    {
      string testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(Path.Combine(testDir, "sub"));

      try
      {
        await File.WriteAllTextAsync(Path.Combine(testDir, "root.txt"), "r");
        await File.WriteAllTextAsync(Path.Combine(testDir, "sub", "nested.txt"), "n");
        await File.WriteAllTextAsync(Path.Combine(testDir, "sub", "skip.log"), "s");
        await File.WriteAllTextAsync(Path.Combine(testDir, ".secret"), "h");

        CommandOutput recursive = Commands.GetChildItem(testDir, recursive: true, pattern: "*.txt");
        recursive.Success.ShouldBeTrue();
        recursive.Stdout.ShouldContain("root.txt");
        recursive.Stdout.ShouldContain("nested.txt");
        recursive.Stdout.ShouldNotContain("skip.log");

        CommandOutput excluded = Commands.GetChildItem(testDir, recursive: true, exclude: "*.log");
        excluded.Success.ShouldBeTrue();
        excluded.Stdout.ShouldNotContain("skip.log");
        excluded.Stdout.ShouldContain("root.txt");

        CommandOutput visible = Commands.GetChildItem(testDir, includeHidden: false);
        visible.Success.ShouldBeTrue();
        visible.Stdout.ShouldNotContain(".secret");
        visible.Stdout.ShouldContain("root.txt");
      }
      finally
      {
        Directory.Delete(testDir, true);
      }
    }
  }
}
