#!/usr/bin/dotnet --

#region Purpose
// Tests for Git.ConfigureFetchRefspecAsync() - validates fetch refspec configuration
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Git (the static class providing git operations)
// Action: ConfigureFetchRefspec (the method being tested)
// Tests verify successful configuration and error handling using CommandMock
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class ConfigureFetchRefspec_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<ConfigureFetchRefspec_Given_>();

    public static async Task ValidPath_Should_ConfigureSuccessfully()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "config", "remote.origin.fetch", "+refs/heads/*:refs/remotes/origin/*")
          .Returns("");

        bool result = await Git.ConfigureFetchRefspecAsync("/path/to/repo.git");

        result.ShouldBeTrue();
      }

      await Task.CompletedTask;
    }

    public static async Task Error_Should_ReturnFalse()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "config", "remote.origin.fetch", "+refs/heads/*:refs/remotes/origin/*")
          .ReturnsError("fatal: not a git repository", 128);

        bool result = await Git.ConfigureFetchRefspecAsync("/path/to/repo.git");

        result.ShouldBeFalse();
      }

      await Task.CompletedTask;
    }
  }
}
