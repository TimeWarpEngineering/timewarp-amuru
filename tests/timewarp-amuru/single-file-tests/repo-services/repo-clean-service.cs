#!/usr/bin/dotnet --

#region Purpose
// Tests for RepoCleanService - validates repository cleaning operations
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Repo_Services
{
  [TestTag("Repo")]
  public class RepoCleanService_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<RepoCleanService_Given_>();

    public static async Task Constructor_WithValidTerminal_ShouldSucceed()
    {
      using TestTerminal terminal = new();
      RepoCleanService service = new(terminal);

      service.ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task CleanAsync_WhenNotInGitRepo_ShouldReturnEmptyResult()
    {
      using TestTerminal terminal = new();
      RepoCleanService service = new(terminal);

      string tempDir = Path.Combine(Path.GetTempPath(), $"not-a-repo-{Guid.NewGuid()}");
      Directory.CreateDirectory(tempDir);

      string originalDir = Directory.GetCurrentDirectory();
      try
      {
        Directory.SetCurrentDirectory(tempDir);
        CleanResult result = await service.CleanAsync();

        result.ObjDirectoriesDeleted.ShouldBe(0);
        result.BinDirectoriesDeleted.ShouldBe(0);
        result.RootBinFilesCleaned.ShouldBe(0);
        terminal.ErrorOutput.ShouldContain("Not in a git repository");
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDir);
        Directory.Delete(tempDir, recursive: true);
      }
    }

    public static async Task CleanAsync_WhenInGitRepo_ShouldDeleteObjDirectories()
    {
      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      string testDir = Path.Combine(repoRoot, "test-obj-dir");
      string objDir = Path.Combine(testDir, "obj");
      Directory.CreateDirectory(objDir);

      try
      {
        using TestTerminal terminal = new();
        RepoCleanService service = new(terminal);

        CleanResult result = await service.CleanAsync();

        result.ObjDirectoriesDeleted.ShouldBeGreaterThanOrEqualTo(1);
        Directory.Exists(objDir).ShouldBeFalse();
      }
      finally
      {
        if (Directory.Exists(testDir))
        {
          Directory.Delete(testDir, recursive: true);
        }
      }
    }
  }
}
