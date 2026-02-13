#!/usr/bin/dotnet --

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
  }
}
