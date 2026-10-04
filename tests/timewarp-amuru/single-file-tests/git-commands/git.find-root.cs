#!/usr/bin/env -S dotnet --

#region Purpose
// Real-execution tests for Git.FindRoot.
#endregion

#region Design
// Temporary repositories live outside the product worktree so a walk from /tmp cannot
// land on this repository. Each test deletes its directory in finally.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class FindRoot_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<FindRoot_Given_>();

    public static async Task NestedDirectory_Should_ReturnRepositoryRoot()
    {
      string repository = await GitRepositoryFixture.CreateRepositoryAsync("find-root");
      try
      {
        string nested = Path.Combine(repository, "sub", "dir");
        Directory.CreateDirectory(nested);

        string? root = Git.FindRoot(nested);

        root.ShouldNotBeNull();
        Path.GetFullPath(root!).ShouldBe(Path.GetFullPath(repository));
      }
      finally
      {
        GitRepositoryFixture.Delete(repository);
      }

      await Task.CompletedTask;
    }

    public static async Task OutsideRepository_Should_ReturnNull()
    {
      string directory = Directory.CreateTempSubdirectory("amuru-git-not-a-repo-").FullName;
      try
      {
        Git.FindRoot(directory).ShouldBeNull();
      }
      finally
      {
        GitRepositoryFixture.Delete(directory);
      }

      await Task.CompletedTask;
    }

    public static async Task LinkedWorktree_Should_ReturnWorktreeRoot()
    {
      string repository = await GitRepositoryFixture.CreateRepositoryAsync("find-root-wt");
      string worktree = Path.Combine(Path.GetTempPath(), "amuru-git-wt-" + Guid.NewGuid().ToString("N"));
      try
      {
        await GitRepositoryFixture.CommitFileAsync(repository, "a.txt", "a", "A");
        await GitRepositoryFixture.GitAsync(repository, "branch", "feature");
        await GitRepositoryFixture.GitAsync(repository, "worktree", "add", worktree, "feature");

        string? root = Git.FindRoot(worktree);

        root.ShouldNotBeNull();
        Path.GetFullPath(root!).ShouldBe(Path.GetFullPath(worktree));
      }
      finally
      {
        GitRepositoryFixture.Delete(worktree);
        GitRepositoryFixture.Delete(repository);
      }

      await Task.CompletedTask;
    }
  }
}
