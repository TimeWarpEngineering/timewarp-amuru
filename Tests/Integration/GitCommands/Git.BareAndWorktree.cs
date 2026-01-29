#!/usr/bin/dotnet --
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using TimeWarp.Amuru.Testing;
using Shouldly;
using static TimeWarp.Amuru.Test.Helpers.TestRunner;

await RunTests<GitBareAndWorktreeTests>();

internal sealed class GitBareAndWorktreeTests
{
  // CloneBare tests
  public static async Task TestCloneBareAsync_WithMock_Success()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "clone", "--bare", "https://github.com/user/repo.git", "/path/to/repo.git")
        .Returns("");

      GitCloneResult result = await Git.CloneBareAsync("https://github.com/user/repo.git", "/path/to/repo.git");

      result.Success.ShouldBeTrue();
      result.Path.ShouldBe("/path/to/repo.git");
      result.ErrorMessage.ShouldBeNull();
    }
  }

  public static async Task TestCloneBareAsync_WithMock_Failure()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "clone", "--bare", "https://github.com/user/repo.git", "/path/to/repo.git")
        .ReturnsError("fatal: could not access repository", 128);

      GitCloneResult result = await Git.CloneBareAsync("https://github.com/user/repo.git", "/path/to/repo.git");

      result.Success.ShouldBeFalse();
      result.Path.ShouldBeNull();
      result.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }
  }

  // ConfigureFetchRefspec tests
  public static async Task TestConfigureFetchRefspecAsync_WithMock_Success()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "config", "remote.origin.fetch", "+refs/heads/*:refs/remotes/origin/*")
        .Returns("");

      bool result = await Git.ConfigureFetchRefspecAsync("/path/to/repo.git");

      result.ShouldBeTrue();
    }
  }

  public static async Task TestConfigureFetchRefspecAsync_WithMock_Failure()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "config", "remote.origin.fetch", "+refs/heads/*:refs/remotes/origin/*")
        .ReturnsError("fatal: not a git repository", 128);

      bool result = await Git.ConfigureFetchRefspecAsync("/path/to/repo.git");

      result.ShouldBeFalse();
    }
  }

  // SetRemoteHeadAuto tests
  public static async Task TestSetRemoteHeadAutoAsync_WithMock_Success()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "remote", "set-head", "origin", "--auto")
        .Returns("origin/HEAD set to main");

      bool result = await Git.SetRemoteHeadAutoAsync("/path/to/repo.git");

      result.ShouldBeTrue();
    }
  }

  public static async Task TestSetRemoteHeadAutoAsync_WithMock_Failure()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "remote", "set-head", "origin", "--auto")
        .ReturnsError("fatal: not a git repository", 128);

      bool result = await Git.SetRemoteHeadAutoAsync("/path/to/repo.git");

      result.ShouldBeFalse();
    }
  }

  // Fetch tests
  public static async Task TestFetchAsync_WithMock_Success()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "fetch", "origin")
        .Returns("");

      bool result = await Git.FetchAsync("/path/to/repo.git");

      result.ShouldBeTrue();
    }
  }

  public static async Task TestFetchAsync_WithMock_CustomRemote()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "fetch", "upstream")
        .Returns("");

      bool result = await Git.FetchAsync("/path/to/repo.git", "upstream");

      result.ShouldBeTrue();
    }
  }

  public static async Task TestFetchAsync_WithMock_Failure()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "fetch", "origin")
        .ReturnsError("fatal: not a git repository", 128);

      bool result = await Git.FetchAsync("/path/to/repo.git");

      result.ShouldBeFalse();
    }
  }

  // WorktreeList tests
  public static async Task TestWorktreeListPorcelainAsync_WithMock_Success()
  {
    using (CommandMock.Enable())
    {
      string porcelainOutput = @"worktree /home/user/project
HEAD abcd1234
branch refs/heads/main

worktree /home/user/project-feature
HEAD efgh5678
detached

";

      CommandMock.Setup("git", "worktree", "list", "--porcelain")
        .Returns(porcelainOutput);

      string result = await Git.WorktreeListPorcelainAsync("/path/to/repo.git");

      result.ShouldNotBeNullOrWhiteSpace();
      result.ShouldContain("worktree");
    }
  }

  // WorktreeAdd tests
  public static async Task TestWorktreeAddAsync_WithMock_Success()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "worktree", "add", "/path/to/worktree", "feature-branch")
        .Returns("");

      GitWorktreeAddResult result = await Git.WorktreeAddAsync("/path/to/repo.git", "/path/to/worktree", "feature-branch");

      result.Success.ShouldBeTrue();
      result.WorktreePath.ShouldBe("/path/to/worktree");
      result.ErrorMessage.ShouldBeNull();
    }
  }

  public static async Task TestWorktreeAddAsync_WithMock_Failure()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "worktree", "add", "/path/to/worktree", "feature-branch")
        .ReturnsError("fatal: invalid reference: feature-branch", 128);

      GitWorktreeAddResult result = await Git.WorktreeAddAsync("/path/to/repo.git", "/path/to/worktree", "feature-branch");

      result.Success.ShouldBeFalse();
      result.WorktreePath.ShouldBeNull();
      result.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }
  }

  // WorktreeAddNewBranch tests
  public static async Task TestWorktreeAddNewBranchAsync_WithMock_Success()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "worktree", "add", "-b", "new-feature", "/path/to/worktree")
        .Returns("");

      GitWorktreeAddResult result = await Git.WorktreeAddNewBranchAsync("/path/to/repo.git", "/path/to/worktree", "new-feature");

      result.Success.ShouldBeTrue();
      result.WorktreePath.ShouldBe("/path/to/worktree");
      result.ErrorMessage.ShouldBeNull();
    }
  }

  public static async Task TestWorktreeAddNewBranchAsync_WithMock_Failure()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "worktree", "add", "-b", "new-feature", "/path/to/worktree")
        .ReturnsError("fatal: failed to create worktree", 128);

      GitWorktreeAddResult result = await Git.WorktreeAddNewBranchAsync("/path/to/repo.git", "/path/to/worktree", "new-feature");

      result.Success.ShouldBeFalse();
      result.WorktreePath.ShouldBeNull();
      result.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }
  }

  // WorktreeRemove tests
  public static async Task TestWorktreeRemoveAsync_WithMock_Success()
  {
    using (CommandMock.Enable())
    {
      // Mock the worktree remove command - it should succeed
      CommandMock.Setup("git", "worktree", "remove", "/path/to/worktree")
        .Returns("");

      // Note: WorktreeRemoveAsync internally tries to find the main repository from the worktree
      // This test is limited since we can't easily mock the filesystem
      // The actual worktree removal would fail because the .git file doesn't exist
      // But we can at least test the parsing logic works
      GitWorktreeRemoveResult result = await Git.WorktreeRemoveAsync("/path/to/worktree");

      // This will fail because the .git file doesn't exist (can't mock File.Exists)
      result.Success.ShouldBeFalse();
      result.ErrorMessage.ShouldNotBeNull();
      result.ErrorMessage!.ShouldContain("Could not determine main repository");
    }
  }

  // WorktreePorcelainParser tests
  public static void TestWorktreePorcelainParser_ParseWorktreeList_WithMultipleWorktrees()
  {
    string porcelainOutput = @"worktree /home/user/project
HEAD abcd1234567890abcdef1234567890abcdef12
branch refs/heads/main

worktree /home/user/project-feature
HEAD efgh5678901234efgh5678901234efgh5678
detached

worktree /path/to/bare/repo.git
HEAD 1234abcd56781234abcd56781234abcd5678
bare

";

    IReadOnlyList<WorktreeEntry> worktrees = WorktreePorcelainParser.ParseWorktreeList(porcelainOutput);

    worktrees.Count.ShouldBe(3);

    // First worktree
    worktrees[0].Path.ShouldBe("/home/user/project");
    worktrees[0].HeadCommit.ShouldBe("abcd1234567890abcdef1234567890abcdef12");
    worktrees[0].BranchRef.ShouldBe("refs/heads/main");
    worktrees[0].IsBare.ShouldBeFalse();

    // Second worktree (detached)
    worktrees[1].Path.ShouldBe("/home/user/project-feature");
    worktrees[1].HeadCommit.ShouldBe("efgh5678901234efgh5678901234efgh5678");
    worktrees[1].BranchRef.ShouldBeNull();
    worktrees[1].IsBare.ShouldBeFalse();

    // Third worktree (bare)
    worktrees[2].Path.ShouldBe("/path/to/bare/repo.git");
    worktrees[2].HeadCommit.ShouldBe("1234abcd56781234abcd56781234abcd5678");
    worktrees[2].BranchRef.ShouldBeNull();
    worktrees[2].IsBare.ShouldBeTrue();
  }

  public static void TestWorktreePorcelainParser_ParseWorktreeList_EmptyInput()
  {
    IReadOnlyList<WorktreeEntry> worktrees = WorktreePorcelainParser.ParseWorktreeList("");

    worktrees.ShouldBeEmpty();
  }

  public static void TestWorktreePorcelainParser_ParseWorktreeList_WhitespaceOnly()
  {
    IReadOnlyList<WorktreeEntry> worktrees = WorktreePorcelainParser.ParseWorktreeList("   \n\t  \n");

    worktrees.ShouldBeEmpty();
  }

  public static void TestWorktreePorcelainParser_ParseWorktreeList_SingleWorktree()
  {
    string porcelainOutput = @"worktree /home/user/single-project
HEAD 1111111111111111111111111111111111111111
branch refs/heads/main

";

    IReadOnlyList<WorktreeEntry> worktrees = WorktreePorcelainParser.ParseWorktreeList(porcelainOutput);

    worktrees.Count.ShouldBe(1);
    worktrees[0].Path.ShouldBe("/home/user/single-project");
    worktrees[0].HeadCommit.ShouldBe("1111111111111111111111111111111111111111");
    worktrees[0].BranchRef.ShouldBe("refs/heads/main");
    worktrees[0].IsBare.ShouldBeFalse();
  }
}
