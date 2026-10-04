#!/usr/bin/env -S dotnet --

#region Purpose
// Real-execution tests for Git.UpdateBranchAsync.
#endregion

#region Design
// The checked-out test is the case `git fetch origin <branch>:<branch>` refuses:
// origin has a new commit and the clone has that branch checked out.
// The non-checked-out test updates a different branch ref and leaves HEAD on master.
// The bare-repo test checks that a bare repository's HEAD branch is updated by refspec fetch.
// The linked-worktree test checks a branch checked out in another worktree is pulled there.
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

    public static async Task BareRepository_Should_UpdateHeadBranchByFetch()
    {
      string origin = await GitRepositoryFixture.CreateRepositoryAsync("update-bare-origin");
      string? bare = null;
      try
      {
        await GitRepositoryFixture.CommitFileAsync(origin, "a.txt", "a", "A");
        string parent = Directory.CreateTempSubdirectory("amuru-git-update-bare-").FullName;
        bare = parent;
        string bareClone = Path.Combine(parent, "bareclone");
        await GitRepositoryFixture.GitAsync(parent, "clone", "--bare", origin, bareClone);
        await GitRepositoryFixture.CommitFileAsync(origin, "b.txt", "b", "B");

        GitBranchUpdateResult result = await Git.UpdateBranchAsync("master", bareClone);

        result.Success.ShouldBeTrue(result.ErrorMessage);
        result.BranchPath.ShouldBeNull();
        string originHead = await GitRepositoryFixture.RevParseAsync(origin, "HEAD");
        string bareMaster = await GitRepositoryFixture.RevParseAsync(bareClone, "refs/heads/master");
        bareMaster.ShouldBe(originHead);
      }
      finally
      {
        GitRepositoryFixture.Delete(bare);
        GitRepositoryFixture.Delete(origin);
      }
    }

    public static async Task BranchCheckedOutInLinkedWorktree_Should_PullInThatWorktree()
    {
      string origin = await GitRepositoryFixture.CreateRepositoryAsync("update-linked-origin");
      string? clone = null;
      string? worktreeParent = null;
      try
      {
        await GitRepositoryFixture.CommitFileAsync(origin, "a.txt", "a", "A");
        await GitRepositoryFixture.GitAsync(origin, "branch", "feature");
        clone = await GitRepositoryFixture.CloneAsync(origin, "update-linked-clone");

        worktreeParent = Directory.CreateTempSubdirectory("amuru-git-update-linked-wt-").FullName;
        string worktreePath = Path.Combine(worktreeParent, "feature-wt");
        await GitRepositoryFixture.GitAsync(clone, "worktree", "add", worktreePath, "feature");

        await GitRepositoryFixture.GitAsync(origin, "checkout", "feature");
        await GitRepositoryFixture.CommitFileAsync(origin, "f.txt", "f", "F");
        await GitRepositoryFixture.GitAsync(origin, "checkout", "master");

        GitBranchUpdateResult result = await Git.UpdateBranchAsync("feature", clone);

        result.Success.ShouldBeTrue(result.ErrorMessage);
        result.BranchPath.ShouldNotBeNull();
        Path.GetFullPath(result.BranchPath!).TrimEnd('/').ShouldBe(Path.GetFullPath(worktreePath).TrimEnd('/'));
        string worktreeHead = await GitRepositoryFixture.RevParseAsync(worktreePath, "HEAD");
        string originFeature = await GitRepositoryFixture.RevParseAsync(origin, "refs/heads/feature");
        worktreeHead.ShouldBe(originFeature);
        string cloneBranch = await GitRepositoryFixture.RevParseAsync(clone, "--abbrev-ref", "HEAD");
        cloneBranch.ShouldBe("master");
      }
      finally
      {
        GitRepositoryFixture.Delete(worktreeParent);
        GitRepositoryFixture.Delete(clone);
        GitRepositoryFixture.Delete(origin);
      }
    }
  }
}
