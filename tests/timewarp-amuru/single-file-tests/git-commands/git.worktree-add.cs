#!/usr/bin/dotnet --

#region Purpose
// Tests for Git.WorktreeAddAsync() - validates adding worktrees for existing branches
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Git (the static class providing git operations)
// Action: WorktreeAdd (the method being tested)
// Tests verify successful worktree creation and error handling using CommandMock
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class WorktreeAdd_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WorktreeAdd_Given_>();

    public static async Task ExistingBranch_Should_AddWorktreeSuccessfully()
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

    public static async Task Error_Should_ReturnFailure()
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
  }
}
