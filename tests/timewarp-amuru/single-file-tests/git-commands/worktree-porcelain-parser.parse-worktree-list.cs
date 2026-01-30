#!/usr/bin/dotnet --

#region Purpose
// Tests for WorktreePorcelainParser.ParseWorktreeList() - validates parsing of git worktree porcelain output
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: WorktreePorcelainParser (the static class for parsing worktree output)
// Action: ParseWorktreeList (the method being tested)
// Tests verify parsing of various porcelain output formats
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace WorktreePorcelainParser_
{
  [TestTag("Git")]
  public class ParseWorktreeList_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<ParseWorktreeList_Given_>();

    public static async Task MultipleWorktrees_Should_ParseAll()
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

    public static async Task EmptyInput_Should_ReturnEmptyList()
    {
      IReadOnlyList<WorktreeEntry> worktrees = WorktreePorcelainParser.ParseWorktreeList("");

      worktrees.ShouldBeEmpty();

      await Task.CompletedTask;
    }

    public static async Task WhitespaceOnlyInput_Should_ReturnEmptyList()
    {
      IReadOnlyList<WorktreeEntry> worktrees = WorktreePorcelainParser.ParseWorktreeList("   \n\t  \n");

      worktrees.ShouldBeEmpty();

      await Task.CompletedTask;
    }

    public static async Task SingleWorktree_Should_ParseCorrectly()
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
}
