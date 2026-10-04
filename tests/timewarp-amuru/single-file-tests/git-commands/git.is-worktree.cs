#!/usr/bin/env -S dotnet --

#region Purpose
// Real-execution tests for Git.IsWorktree.
#endregion

#region Design
// IsWorktree is synchronous. The checklist name IsWorktreeAsync is the same member:
// a linked worktree has a .git file, and a normal repository has a .git directory.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class IsWorktree_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<IsWorktree_Given_>();

    public static async Task NormalRepository_Should_ReturnFalse()
    {
      string repository = await GitRepositoryFixture.CreateRepositoryAsync("is-worktree");
      try
      {
        Git.IsWorktree(repository).ShouldBeFalse();
      }
      finally
      {
        GitRepositoryFixture.Delete(repository);
      }

      await Task.CompletedTask;
    }

    public static async Task LinkedWorktree_Should_ReturnTrue()
    {
      string repository = await GitRepositoryFixture.CreateRepositoryAsync("is-worktree-link");
      string worktree = Path.Combine(Path.GetTempPath(), "amuru-git-wt-" + Guid.NewGuid().ToString("N"));
      try
      {
        await GitRepositoryFixture.CommitFileAsync(repository, "a.txt", "a", "A");
        await GitRepositoryFixture.GitAsync(repository, "branch", "feature");
        await GitRepositoryFixture.GitAsync(repository, "worktree", "add", worktree, "feature");

        Git.IsWorktree(worktree).ShouldBeTrue();
        Git.IsWorktree(repository).ShouldBeFalse();
      }
      finally
      {
        GitRepositoryFixture.Delete(worktree);
        GitRepositoryFixture.Delete(repository);
      }
    }

    public static async Task DirectoryThatIsNotARepository_Should_ReturnFalse()
    {
      string directory = Directory.CreateTempSubdirectory("amuru-git-not-a-repo-").FullName;
      try
      {
        Git.IsWorktree(directory).ShouldBeFalse();
      }
      finally
      {
        GitRepositoryFixture.Delete(directory);
      }

      await Task.CompletedTask;
    }
  }
}
