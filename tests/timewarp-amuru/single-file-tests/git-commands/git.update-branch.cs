#!/usr/bin/env -S dotnet --

#region Purpose
// Real-execution tests for Git.UpdateBranchAsync.
#endregion

#region Design
// The checked-out test is the case `git fetch origin <branch>:<branch>` refuses:
// origin has a new commit and the clone has that branch checked out.
// The non-checked-out test updates a different branch ref and leaves HEAD on master.
// Both pass an explicit repository path so process CWD is not the repository under test.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class UpdateBranch_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<UpdateBranch_Given_>();

    public static async Task CheckedOutBranch_Should_FastForwardFromOrigin()
    {
      string origin = await GitRepositoryFixture.CreateRepositoryAsync("update-checked-out-origin");
      string? clone = null;
      try
      {
        await GitRepositoryFixture.CommitFileAsync(origin, "a.txt", "a", "A");
        clone = await GitRepositoryFixture.CloneAsync(origin, "update-checked-out-clone");
        await GitRepositoryFixture.CommitFileAsync(origin, "b.txt", "b", "B");

        GitBranchUpdateResult result = await Git.UpdateBranchAsync("master", clone);

        result.Success.ShouldBeTrue(result.ErrorMessage);
        string originHead = await GitRepositoryFixture.RevParseAsync(origin, "HEAD");
        string cloneHead = await GitRepositoryFixture.RevParseAsync(clone, "HEAD");
        cloneHead.ShouldBe(originHead);
        string branch = await GitRepositoryFixture.RevParseAsync(clone, "--abbrev-ref", "HEAD");
        branch.ShouldBe("master");
      }
      finally
      {
        GitRepositoryFixture.Delete(clone);
        GitRepositoryFixture.Delete(origin);
      }
    }

    public static async Task OtherBranch_Should_UpdateRefWithoutLeavingMaster()
    {
      string origin = await GitRepositoryFixture.CreateRepositoryAsync("update-other-origin");
      string? clone = null;
      try
      {
        await GitRepositoryFixture.CommitFileAsync(origin, "a.txt", "a", "A");
        await GitRepositoryFixture.GitAsync(origin, "branch", "feature");
        await GitRepositoryFixture.GitAsync(origin, "checkout", "feature");
        await GitRepositoryFixture.CommitFileAsync(origin, "f.txt", "f1", "F1");
        await GitRepositoryFixture.GitAsync(origin, "checkout", "master");

        clone = await GitRepositoryFixture.CloneAsync(origin, "update-other-clone");

        await GitRepositoryFixture.GitAsync(origin, "checkout", "feature");
        await GitRepositoryFixture.CommitFileAsync(origin, "f.txt", "f2", "F2");
        await GitRepositoryFixture.GitAsync(origin, "checkout", "master");

        GitBranchUpdateResult result = await Git.UpdateBranchAsync("feature", clone);

        result.Success.ShouldBeTrue(result.ErrorMessage);
        string branch = await GitRepositoryFixture.RevParseAsync(clone, "--abbrev-ref", "HEAD");
        branch.ShouldBe("master");
        string cloneFeature = await GitRepositoryFixture.RevParseAsync(clone, "refs/heads/feature");
        string originFeature = await GitRepositoryFixture.RevParseAsync(origin, "refs/heads/feature");
        cloneFeature.ShouldBe(originFeature);
      }
      finally
      {
        GitRepositoryFixture.Delete(clone);
        GitRepositoryFixture.Delete(origin);
      }
    }
  }
}
