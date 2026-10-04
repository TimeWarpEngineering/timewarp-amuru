#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Git.WorktreeRemoveAsync() - validates worktree removal functionality
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Git (the static class providing git operations)
// Action: WorktreeRemove (the method being tested)
// Tests verify error handling when main repository cannot be found
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class WorktreeRemove_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WorktreeRemove_Given_>();

    public static async Task MissingMainRepo_Should_ReturnError()
    {
      // WorktreeRemoveAsync tries to find the main repository from the worktree
      // This will fail because the .git file doesn't exist
      GitWorktreeRemoveResult result = await Git.WorktreeRemoveAsync("/path/to/worktree");

      result.Success.ShouldBeFalse();
      result.ErrorMessage.ShouldNotBeNull();
      result.ErrorMessage!.ShouldContain("Could not determine main repository");

      await Task.CompletedTask;
    }

    public static async Task RelativeGitdirAndForeignWorkingDirectory_Should_RemoveWorktree()
    {
      string repository = await GitRepositoryFixture.CreateRepositoryAsync("worktree-remove");
      string worktree = Path.Combine(Path.GetTempPath(), "amuru-git-wt-" + Guid.NewGuid().ToString("N"));
      string unrelated = Directory.CreateTempSubdirectory("amuru-git-cwd-").FullName;
      string originalDirectory = Directory.GetCurrentDirectory();
      try
      {
        await GitRepositoryFixture.CommitFileAsync(repository, "a.txt", "a", "A");
        await GitRepositoryFixture.GitAsync(repository, "branch", "feature");
        await GitRepositoryFixture.GitAsync(repository, "worktree", "add", worktree, "feature");
        GitRepositoryFixture.MakeGitdirRelative(worktree);

        Directory.SetCurrentDirectory(unrelated);
        GitWorktreeRemoveResult result = await Git.WorktreeRemoveAsync(worktree);

        result.Success.ShouldBeTrue(result.ErrorMessage);
        Directory.Exists(worktree).ShouldBeFalse();
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDirectory);
        GitRepositoryFixture.Delete(worktree);
        GitRepositoryFixture.Delete(repository);
        GitRepositoryFixture.Delete(unrelated);
      }
    }
  }
}
