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
// Real-execution tests (outside CommandMock) confirm real git behavior for the exact command line.
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
        CommandMock.Setup("git", "show-ref", "--verify", "--quiet", "refs/heads/main")
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
        CommandMock.Setup("git", "show-ref", "--verify", "--quiet", "refs/heads/nonexistent")
          .ReturnsError(string.Empty, 1);

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
        CommandMock.Setup("git", "show-ref", "--verify", "--quiet", "refs/heads/main")
          .ReturnsError("fatal: not a git repository", 128);

        GitBranchExistsResult result = await Git.BranchExistsAsync("/invalid/path", "main");

        result.Success.ShouldBeFalse();
        result.Exists.ShouldBeFalse();
        result.ErrorMessage.ShouldNotBeNull();
        result.ErrorMessage!.ShouldContain("not a git repository");
      }

      await Task.CompletedTask;
    }

    public static async Task RealExistingBranch_Should_ReturnTrue()
    {
      string repository = await GitRepositoryFixture.CreateRepositoryAsync("branch-exists-real");
      try
      {
        await GitRepositoryFixture.CommitFileAsync(repository, "a.txt", "a", "A");

        GitBranchExistsResult result = await Git.BranchExistsAsync(repository, "master");

        result.Success.ShouldBeTrue(result.ErrorMessage);
        result.Exists.ShouldBeTrue();
      }
      finally
      {
        GitRepositoryFixture.Delete(repository);
      }
    }

    public static async Task RealMissingBranch_Should_ReturnFalseWithoutError()
    {
      string repository = await GitRepositoryFixture.CreateRepositoryAsync("branch-missing-real");
      try
      {
        await GitRepositoryFixture.CommitFileAsync(repository, "a.txt", "a", "A");

        GitBranchExistsResult result = await Git.BranchExistsAsync(repository, "nope");

        result.Success.ShouldBeTrue(result.ErrorMessage);
        result.Exists.ShouldBeFalse();
        result.ErrorMessage.ShouldBeNull();
      }
      finally
      {
        GitRepositoryFixture.Delete(repository);
      }
    }
  }
}
