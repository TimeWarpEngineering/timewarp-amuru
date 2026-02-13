#!/usr/bin/dotnet --

#region Purpose
// Tests for Git.BranchExistsAsync() - validates branch existence detection
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Git (the static class providing git operations)
// Action: BranchExists (the method being tested)
// Tests verify existing branches return true, non-existing return false,
// and invalid repos return false using CommandMock
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

        bool result = await Git.BranchExistsAsync("/path/to/repo", "main");

        result.ShouldBeTrue();
      }

      await Task.CompletedTask;
    }

    public static async Task NonExistingBranch_Should_ReturnFalse()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "show-ref", "--verify", "refs/heads/nonexistent")
          .ReturnsError("fatal: 'refs/heads/nonexistent' - not a valid ref", 1);

        bool result = await Git.BranchExistsAsync("/path/to/repo", "nonexistent");

        result.ShouldBeFalse();
      }

      await Task.CompletedTask;
    }

    public static async Task InvalidRepo_Should_ReturnFalse()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "show-ref", "--verify", "refs/heads/main")
          .ReturnsError("fatal: not a git repository", 128);

        bool result = await Git.BranchExistsAsync("/invalid/path", "main");

        result.ShouldBeFalse();
      }

      await Task.CompletedTask;
    }
  }
}
