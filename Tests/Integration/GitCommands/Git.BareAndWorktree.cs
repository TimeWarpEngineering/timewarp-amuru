#!/usr/bin/dotnet --
#:package TimeWarp.Jaribu@1.0.0-beta.8
#:package TimeWarp.Amuru@1.0.0-beta.18

#if !JARIBU_MULTI
return await RunAllTests();
#endif

using TimeWarp.Amuru;
using TimeWarp.Amuru.Testing;

namespace TimeWarp.Amuru.Tests;

[TestTag("Git")]
public class GitBareAndWorktreeTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<GitBareAndWorktreeTests>();

  // ==================== CloneBareAsync Tests ====================

  public static async Task Should_clone_bare_repository_successfully()
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

    await Task.CompletedTask;
  }

  public static async Task Should_fail_clone_bare_on_error()
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

    await Task.CompletedTask;
  }

  // ==================== ConfigureFetchRefspecAsync Tests ====================

  public static async Task Should_configure_fetch_refspec_successfully()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "config", "remote.origin.fetch", "+refs/heads/*:refs/remotes/origin/*")
        .Returns("");

      bool result = await Git.ConfigureFetchRefspecAsync("/path/to/repo.git");

      result.ShouldBeTrue();
    }

    await Task.CompletedTask;
  }

  public static async Task Should_fail_configure_fetch_refspec_on_error()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "config", "remote.origin.fetch", "+refs/heads/*:refs/remotes/origin/*")
        .ReturnsError("fatal: not a git repository", 128);

      bool result = await Git.ConfigureFetchRefspecAsync("/path/to/repo.git");

      result.ShouldBeFalse();
    }

    await Task.CompletedTask;
  }

  // ==================== SetRemoteHeadAutoAsync Tests ====================

  public static async Task Should_set_remote_head_auto_successfully()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "remote", "set-head", "origin", "--auto")
        .Returns("origin/HEAD set to main");

      bool result = await Git.SetRemoteHeadAutoAsync("/path/to/repo.git");

      result.ShouldBeTrue();
    }

    await Task.CompletedTask;
  }

  public static async Task Should_fail_set_remote_head_auto_on_error()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "remote", "set-head", "origin", "--auto")
        .ReturnsError("fatal: not a git repository", 128);

      bool result = await Git.SetRemoteHeadAutoAsync("/path/to/repo.git");

      result.ShouldBeFalse();
    }

    await Task.CompletedTask;
  }

  // ==================== FetchAsync Tests ====================

  public static async Task Should_fetch_from_origin_successfully()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "fetch", "origin")
        .Returns("");

      bool result = await Git.FetchAsync("/path/to/repo.git");

      result.ShouldBeTrue();
    }

    await Task.CompletedTask;
  }

  public static async Task Should_fetch_from_custom_remote()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "fetch", "upstream")
        .Returns("");

      bool result = await Git.FetchAsync("/path/to/repo.git", "upstream");

      result.ShouldBeTrue();
    }

    await Task.CompletedTask;
  }

  public static async Task Should_fail_fetch_on_error()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "fetch", "origin")
        .ReturnsError("fatal: not a git repository", 128);

      bool result = await Git.FetchAsync("/path/to/repo.git");

      result.ShouldBeFalse();
    }

    await Task.CompletedTask;
  }

  // ==================== WorktreeListPorcelainAsync Tests ====================

  public static async Task Should_return_worktree_list_porcelain_output()
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

    await Task.CompletedTask;
  }

  // ==================== WorktreeAddAsync Tests ====================

  public static async Task Should_add_worktree_for_existing_branch()
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

    await Task.CompletedTask;
  }

  public static async Task Should_fail_worktree_add_on_error()
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

    await Task.CompletedTask;
  }

  // ==================== WorktreeAddNewBranchAsync Tests ====================

  public static async Task Should_add_worktree_with_new_branch()
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

    await Task.CompletedTask;
  }

  public static async Task Should_fail_worktree_add_new_branch_on_error()
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

    await Task.CompletedTask;
  }

  // ==================== WorktreeRemoveAsync Tests ====================

  public static async Task Should_report_error_when_cannot_find_main_repository()
  {
    // WorktreeRemoveAsync tries to find the main repository from the worktree
    // This will fail because the .git file doesn't exist
    GitWorktreeRemoveResult result = await Git.WorktreeRemoveAsync("/path/to/worktree");

    result.Success.ShouldBeFalse();
    result.ErrorMessage.ShouldNotBeNull();
    result.ErrorMessage!.ShouldContain("Could not determine main repository");

    await Task.CompletedTask;
  }

  // ==================== WorktreePorcelainParser Tests ====================

  public static async Task Should_parse_multiple_worktrees_from_porcelain()
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

    await Task.CompletedTask;
  }

  public static async Task Should_return_empty_list_for_empty_porcelain()
  {
    IReadOnlyList<WorktreeEntry> worktrees = WorktreePorcelainParser.ParseWorktreeList("");

    worktrees.ShouldBeEmpty();

    await Task.CompletedTask;
  }

  public static async Task Should_return_empty_list_for_whitespace_only_porcelain()
  {
    IReadOnlyList<WorktreeEntry> worktrees = WorktreePorcelainParser.ParseWorktreeList("   \n\t  \n");

    worktrees.ShouldBeEmpty();

    await Task.CompletedTask;
  }

  public static async Task Should_parse_single_worktree_from_porcelain()
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

    await Task.CompletedTask;
  }
}
