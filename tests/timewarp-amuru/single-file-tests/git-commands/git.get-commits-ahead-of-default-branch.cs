#!/usr/bin/dotnet --

#region Purpose
// Tests for Git.GetCommitsAheadOfDefaultBranchAsync() - validates commit counting ahead of default branch
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Git (the static class providing git operations)
// Action: GetCommitsAheadOfDefaultBranch (the method being tested)
// Tests verify commit counting using mocked commands and real repository
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class GetCommitsAheadOfDefaultBranch_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<GetCommitsAheadOfDefaultBranch_Given_>();

    [Skip("Requires full git history - skipped in CI environments")]
    public static async Task RealRepository_Should_CountCommits()
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

    public static async Task MockedCommands_Should_ReturnCorrectCount()
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
}
