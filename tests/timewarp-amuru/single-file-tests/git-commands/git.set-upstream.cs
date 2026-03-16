#!/usr/bin/dotnet --

#region Purpose
// Tests for Git.SetUpstreamAsync() - validates setting branch tracking information
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Git (the static class providing git operations)
// Action: SetUpstream (the method being tested)
// Tests verify setting upstream with default/custom remotes, fetchFirst option, and error handling
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class SetUpstream_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<SetUpstream_Given_>();

    public static async Task DefaultRemote_Should_SetOriginTracking()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "branch", "--set-upstream-to", "origin/feature-branch", "feature-branch")
          .Returns("");

        GitSetUpstreamResult result = await Git.SetUpstreamAsync("/path/to/worktree", "feature-branch");

        result.Success.ShouldBeTrue();
      }

      await Task.CompletedTask;
    }

    public static async Task CustomRemote_Should_SetSpecifiedTracking()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "branch", "--set-upstream-to", "upstream/feature-branch", "feature-branch")
          .Returns("");

        GitSetUpstreamResult result = await Git.SetUpstreamAsync(
          "/path/to/worktree",
          "feature-branch",
          remote: "upstream");

        result.Success.ShouldBeTrue();
      }

      await Task.CompletedTask;
    }

    public static async Task FetchFirst_Should_FetchThenSetTracking()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "fetch", "origin")
          .Returns("");
        CommandMock.Setup("git", "branch", "--set-upstream-to", "origin/feature-branch", "feature-branch")
          .Returns("");

        GitSetUpstreamResult result = await Git.SetUpstreamAsync(
          "/path/to/worktree",
          "feature-branch",
          fetchFirst: true);

        result.Success.ShouldBeTrue();
      }

      await Task.CompletedTask;
    }

    public static async Task FetchFirstFails_Should_ReturnFalseWithMessage()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "fetch", "origin")
          .ReturnsError("fatal: could not read from remote", 128);

        GitSetUpstreamResult result = await Git.SetUpstreamAsync(
          "/path/to/worktree",
          "feature-branch",
          fetchFirst: true);

        result.Success.ShouldBeFalse();
        result.ErrorMessage!.ShouldContain("Failed to fetch from origin");
      }

      await Task.CompletedTask;
    }

    public static async Task Error_Should_ReturnFalseWithMessage()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "branch", "--set-upstream-to", "origin/feature-branch", "feature-branch")
          .ReturnsError("fatal: no such branch", 128);

        GitSetUpstreamResult result = await Git.SetUpstreamAsync("/path/to/worktree", "feature-branch");

        result.Success.ShouldBeFalse();
        result.ErrorMessage!.ShouldContain("fatal: no such branch");
      }

      await Task.CompletedTask;
    }
  }
}
