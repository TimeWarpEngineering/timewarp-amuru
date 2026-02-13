#!/usr/bin/dotnet --

#region Purpose
// Tests for Git.WorktreeAddNewBranchAsync() - validates adding worktrees with new branches
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Git (the static class providing git operations)
// Action: WorktreeAddNewBranch (the method being tested)
// Tests verify successful worktree creation with new branch and error handling using CommandMock
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class WorktreeAddNewBranch_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WorktreeAddNewBranch_Given_>();

    public static async Task NewBranch_Should_CreateWorktreeWithBranch()
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

    public static async Task Error_Should_ReturnFailure()
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
  }
}
