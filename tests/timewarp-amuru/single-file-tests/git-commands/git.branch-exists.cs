#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for Git.BranchExistsAsync() - validates branch existence detection
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Git (the static class providing git operations)
// Action: BranchExists (the method being tested)
// Tests verify existing branches set Exists, a missing ref is Exists false with Success true,
// and an invalid repo is Success false with the git message.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class BranchExists_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<BranchExists_Given_>();

    public static async Task ExistingBranch_Should_ReturnTrue()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "show-ref", "--verify", "refs/heads/main")
          .Returns("abc123 refs/heads/main");

        GitBranchExistsResult result = await Git.BranchExistsAsync("/path/to/repo", "main");

        result.Success.ShouldBeTrue();
        result.Exists.ShouldBeTrue();
        result.ErrorMessage.ShouldBeNull();
      }

      await Task.CompletedTask;
    }

    public static async Task NonExistingBranch_Should_ReturnFalse()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "show-ref", "--verify", "refs/heads/nonexistent")
          .ReturnsError("fatal: 'refs/heads/nonexistent' - not a valid ref", 1);

        GitBranchExistsResult result = await Git.BranchExistsAsync("/path/to/repo", "nonexistent");

        result.Success.ShouldBeTrue();
        result.Exists.ShouldBeFalse();
        result.ErrorMessage.ShouldBeNull();
      }

      await Task.CompletedTask;
    }

    public static async Task InvalidRepo_Should_ReturnFalse()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "show-ref", "--verify", "refs/heads/main")
          .ReturnsError("fatal: not a git repository", 128);

        GitBranchExistsResult result = await Git.BranchExistsAsync("/invalid/path", "main");

        result.Success.ShouldBeFalse();
        result.Exists.ShouldBeFalse();
        result.ErrorMessage.ShouldNotBeNull();
        result.ErrorMessage!.ShouldContain("not a git repository");
      }

      await Task.CompletedTask;
    }
  }
}
