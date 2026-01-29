#!/usr/bin/dotnet --
#:package TimeWarp.Jaribu
#:package TimeWarp.Amuru

#if !JARIBU_MULTI
return await RunAllTests();
#endif

using TimeWarp.Amuru;
using TimeWarp.Amuru.Testing;

namespace TimeWarp.Amuru.Tests;

[TestTag("Git")]
public class GitDefaultBranchTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<GitDefaultBranchTests>();

  public static async Task Should_detect_default_branch_in_real_repository()
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

  [Skip("Requires full git history - skipped in CI environments")]
  public static async Task Should_count_commits_ahead_of_default_branch()
  {
    // Skip this test in CI environments where shallow clones or limited refs may cause failures
    // The mock test verifies the actual logic
    if (Environment.GetEnvironmentVariable("CI") is not null ||
        Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null)
    {
      Console.WriteLine("Skipping real repository test in CI environment");
      return;
    }

    // Get commits ahead of default branch
    GitCommitCountResult result = await Git.GetCommitsAheadOfDefaultBranchAsync();

    result.Success.ShouldBeTrue("Should successfully count commits ahead of default branch");
    result.Count.ShouldBeGreaterThanOrEqualTo(0, "Commit count should be non-negative");
    result.ErrorMessage.ShouldBeNull("Should not have an error message on success");

    await Task.CompletedTask;
  }

  public static async Task Should_detect_default_branch_as_prerequisite_for_update()
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

  public static async Task Should_return_configured_branch_from_mock()
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

  public static async Task Should_fallback_to_show_ref_when_symbolic_ref_fails()
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

  public static async Task Should_check_master_when_main_not_found()
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

  public static async Task Should_return_error_when_no_default_branch_found()
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

  public static async Task Should_count_commits_with_mock()
  {
    using (CommandMock.Enable())
    {
      // Mock GetDefaultBranchAsync to return "main"
      CommandMock.Setup("git", "symbolic-ref", "refs/remotes/origin/HEAD", "--short")
        .Returns("origin/main");

      // Mock rev-list to return a count
      CommandMock.Setup("git", "rev-list", "--count", "main..HEAD")
        .Returns("5");

      GitCommitCountResult result = await Git.GetCommitsAheadOfDefaultBranchAsync();

      result.Success.ShouldBeTrue();
      result.Count.ShouldBe(5);
      CommandMock.VerifyCalled("git", "rev-list", "--count", "main..HEAD");
    }

    await Task.CompletedTask;
  }
}
