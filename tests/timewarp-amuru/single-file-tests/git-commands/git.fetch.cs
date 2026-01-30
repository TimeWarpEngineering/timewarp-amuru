#!/usr/bin/dotnet --

#region Purpose
// Tests for Git.FetchAsync() - validates fetching from remote repositories
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Git (the static class providing git operations)
// Action: Fetch (the method being tested)
// Tests verify fetching from default and custom remotes, and error handling using CommandMock
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class Fetch_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Fetch_Given_>();

    public static async Task DefaultRemote_Should_FetchFromOrigin()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "fetch", "origin")
          .Returns("");

        bool result = await Git.FetchAsync("/path/to/repo.git");

        result.ShouldBeTrue();
      }

      await Task.CompletedTask;
    }

    public static async Task CustomRemote_Should_FetchFromSpecifiedRemote()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "fetch", "upstream")
          .Returns("");

        bool result = await Git.FetchAsync("/path/to/repo.git", "upstream");

        result.ShouldBeTrue();
      }

      await Task.CompletedTask;
    }

    public static async Task Error_Should_ReturnFalse()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "fetch", "origin")
          .ReturnsError("fatal: not a git repository", 128);

        bool result = await Git.FetchAsync("/path/to/repo.git");

        result.ShouldBeFalse();
      }

      await Task.CompletedTask;
    }
  }
}
