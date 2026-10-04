#!/usr/bin/env -S dotnet --

#region Purpose
// Real-execution tests for Git.GetWorktreePathAsync.
#endregion

#region Design
// The repository path is passed explicitly so the test does not depend on process CWD.
// A missing branch returns null. The path of a linked worktree matches the directory git created.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class GetWorktreePath_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<GetWorktreePath_Given_>();

    public static async Task CheckedOutBranch_Should_ReturnWorktreeDirectory()
    {
      string repository = await GitRepositoryFixture.CreateRepositoryAsync("worktree-path");
      string worktree = Path.Combine(Path.GetTempPath(), "amuru-git-wt-" + Guid.NewGuid().ToString("N"));
      try
      {
        await GitRepositoryFixture.CommitFileAsync(repository, "a.txt", "a", "A");
        await GitRepositoryFixture.GitAsync(repository, "branch", "feature");
        await GitRepositoryFixture.GitAsync(repository, "worktree", "add", worktree, "feature");

        string? path = await Git.GetWorktreePathAsync("feature", repository);

        path.ShouldNotBeNull();
        Path.GetFullPath(path!).ShouldBe(Path.GetFullPath(worktree));
      }
      finally
      {
        GitRepositoryFixture.Delete(worktree);
        GitRepositoryFixture.Delete(repository);
      }
    }

    public static async Task MissingBranch_Should_ReturnNull()
    {
      string repository = await GitRepositoryFixture.CreateRepositoryAsync("worktree-path-missing");
      try
      {
        await GitRepositoryFixture.CommitFileAsync(repository, "a.txt", "a", "A");

        string? path = await Git.GetWorktreePathAsync("does-not-exist", repository);

        path.ShouldBeNull();
      }
      finally
      {
        GitRepositoryFixture.Delete(repository);
      }
    }
  }
}
