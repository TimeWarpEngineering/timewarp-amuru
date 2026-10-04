#!/usr/bin/env -S dotnet --

#region Purpose
// Real-execution tests for Git.UpdateWorktreeAsync.
#endregion

#region Design
// A linked worktree checks out feature. Origin then moves feature.
// UpdateWorktreeAsync is given the main repository path and must pull inside the worktree.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class UpdateWorktree_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<UpdateWorktree_Given_>();

    public static async Task FeatureWorktree_Should_FastForwardFromOrigin()
    {
      string origin = await GitRepositoryFixture.CreateRepositoryAsync("update-wt-origin");
      string? clone = null;
      string worktree = Path.Combine(Path.GetTempPath(), "amuru-git-wt-" + Guid.NewGuid().ToString("N"));
      try
      {
        await GitRepositoryFixture.CommitFileAsync(origin, "a.txt", "a", "A");
        await GitRepositoryFixture.GitAsync(origin, "branch", "feature");
        await GitRepositoryFixture.GitAsync(origin, "checkout", "feature");
        await GitRepositoryFixture.CommitFileAsync(origin, "f.txt", "f1", "F1");
        await GitRepositoryFixture.GitAsync(origin, "checkout", "master");

        clone = await GitRepositoryFixture.CloneAsync(origin, "update-wt-clone");
        await GitRepositoryFixture.GitAsync(clone, "branch", "feature", "origin/feature");
        await GitRepositoryFixture.GitAsync(clone, "worktree", "add", worktree, "feature");

        await GitRepositoryFixture.GitAsync(origin, "checkout", "feature");
        await GitRepositoryFixture.CommitFileAsync(origin, "f.txt", "f2", "F2");
        await GitRepositoryFixture.GitAsync(origin, "checkout", "master");

        GitWorktreeUpdateResult result = await Git.UpdateWorktreeAsync("feature", clone);

        result.Success.ShouldBeTrue(result.ErrorMessage);
        result.BranchPath.ShouldNotBeNull();
        Path.GetFullPath(result.BranchPath!).ShouldBe(Path.GetFullPath(worktree));
        string worktreeHead = await GitRepositoryFixture.RevParseAsync(worktree, "HEAD");
        string originFeature = await GitRepositoryFixture.RevParseAsync(origin, "refs/heads/feature");
        worktreeHead.ShouldBe(originFeature);
      }
      finally
      {
        GitRepositoryFixture.Delete(worktree);
        GitRepositoryFixture.Delete(clone);
        GitRepositoryFixture.Delete(origin);
      }
    }
  }
}
