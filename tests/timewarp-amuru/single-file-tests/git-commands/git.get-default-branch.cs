#!/usr/bin/dotnet --

#region Purpose
// Tests for Git.GetDefaultBranchAsync() - validates default branch detection
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Git (the static class providing git operations)
// Action: GetDefaultBranch (the method being tested)
// Tests verify detection via symbolic-ref, fallback to show-ref, and error handling
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class GetDefaultBranch_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<GetDefaultBranch_Given_>();

    public static async Task RealRepository_Should_DetectDefaultBranch()
    {
      // This test runs in the actual repository, so it should detect the default branch
      GitDefaultBranchResult result = await Git.GetDefaultBranchAsync();

      result.Success.ShouldBeTrue("Should detect default branch in a valid git repository");
      result.BranchName.ShouldNotBeNullOrWhiteSpace("Branch name should not be empty");
      result.ErrorMessage.ShouldBeNull("Should not have an error message on success");

      // The branch should be one of the common names
      string[] expectedBranches = ["main", "master", "dev"];
      expectedBranches.ShouldContain(result.BranchName, $"Branch '{result.BranchName}' should be a common default branch name");

      await Task.CompletedTask;
    }

    public static async Task MockedSymbolicRef_Should_ReturnConfiguredBranch()
    {
      using (CommandMock.Enable())
      {
        // Mock the symbolic-ref command to return "origin/main"
        CommandMock.Setup("git", "symbolic-ref", "refs/remotes/origin/HEAD", "--short")
          .Returns("origin/main");

        GitDefaultBranchResult result = await Git.GetDefaultBranchAsync();

        result.Success.ShouldBeTrue();
        result.BranchName.ShouldBe("main");
        CommandMock.VerifyCalled("git", "symbolic-ref", "refs/remotes/origin/HEAD", "--short");
      }

      await Task.CompletedTask;
    }

    public static async Task SymbolicRefFailure_Should_FallbackToShowRef()
    {
      using (CommandMock.Enable())
      {
        // Mock symbolic-ref to fail
        CommandMock.Setup("git", "symbolic-ref", "refs/remotes/origin/HEAD", "--short")
          .ReturnsError("fatal: ref refs/remotes/origin/HEAD is not a symbolic ref", 128);

        // Mock show-ref for main to succeed
        CommandMock.Setup("git", "show-ref", "--verify", "--quiet", "refs/remotes/origin/main")
          .Returns("");

        GitDefaultBranchResult result = await Git.GetDefaultBranchAsync();

        result.Success.ShouldBeTrue();
        result.BranchName.ShouldBe("main");
      }

      await Task.CompletedTask;
    }

    public static async Task MainNotFound_Should_CheckMaster()
    {
      using (CommandMock.Enable())
      {
        // Mock symbolic-ref to fail
        CommandMock.Setup("git", "symbolic-ref", "refs/remotes/origin/HEAD", "--short")
          .ReturnsError("fatal: ref refs/remotes/origin/HEAD is not a symbolic ref", 128);

        // Mock show-ref for main to fail
        CommandMock.Setup("git", "show-ref", "--verify", "--quiet", "refs/remotes/origin/main")
          .ReturnsError("", 1);

        // Mock show-ref for master to succeed
        CommandMock.Setup("git", "show-ref", "--verify", "--quiet", "refs/remotes/origin/master")
          .Returns("");

        GitDefaultBranchResult result = await Git.GetDefaultBranchAsync();

        result.Success.ShouldBeTrue();
        result.BranchName.ShouldBe("master");
      }

      await Task.CompletedTask;
    }

    public static async Task NoBranchFound_Should_ReturnError()
    {
      using (CommandMock.Enable())
      {
        // Mock all commands to fail
        CommandMock.Setup("git", "symbolic-ref", "refs/remotes/origin/HEAD", "--short")
          .ReturnsError("fatal: ref refs/remotes/origin/HEAD is not a symbolic ref", 128);

        CommandMock.Setup("git", "show-ref", "--verify", "--quiet", "refs/remotes/origin/main")
          .ReturnsError("", 1);

        CommandMock.Setup("git", "show-ref", "--verify", "--quiet", "refs/remotes/origin/master")
          .ReturnsError("", 1);

        CommandMock.Setup("git", "show-ref", "--verify", "--quiet", "refs/remotes/origin/dev")
          .ReturnsError("", 1);

        GitDefaultBranchResult result = await Git.GetDefaultBranchAsync();

        result.Success.ShouldBeFalse();
        result.BranchName.ShouldBeNull();
        result.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
      }

      await Task.CompletedTask;
    }

    public static async Task RealRepository_Should_DetectAsPrerequisiteForUpdate()
    {
      // First verify we can detect the default branch (prerequisite for update)
      GitDefaultBranchResult defaultBranch = await Git.GetDefaultBranchAsync();
      defaultBranch.Success.ShouldBeTrue("Should detect default branch before attempting update");

      // Note: We don't actually run UpdateDefaultBranchAsync in tests as it modifies repository state.
      // The detection step verifies the core functionality works.
      // In a real scenario, UpdateDefaultBranchAsync would use the detected branch.

      // Verify the branch name is valid
      defaultBranch.BranchName.ShouldNotBeNullOrWhiteSpace();

      await Task.CompletedTask;
    }
  }
}
