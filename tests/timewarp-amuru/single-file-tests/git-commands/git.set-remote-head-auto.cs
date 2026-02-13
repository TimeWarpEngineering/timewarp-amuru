#!/usr/bin/dotnet --

#region Purpose
// Tests for Git.SetRemoteHeadAutoAsync() - validates automatic remote HEAD configuration
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Git (the static class providing git operations)
// Action: SetRemoteHeadAuto (the method being tested)
// Tests verify successful configuration and error handling using CommandMock
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class SetRemoteHeadAuto_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<SetRemoteHeadAuto_Given_>();

    public static async Task ValidPath_Should_SetSuccessfully()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "remote", "set-head", "origin", "--auto")
          .Returns("origin/HEAD set to main");

        bool result = await Git.SetRemoteHeadAutoAsync("/path/to/repo.git");

        result.ShouldBeTrue();
      }

      await Task.CompletedTask;
    }

    public static async Task Error_Should_ReturnFalse()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "remote", "set-head", "origin", "--auto")
          .ReturnsError("fatal: not a git repository", 128);

        bool result = await Git.SetRemoteHeadAutoAsync("/path/to/repo.git");

        result.ShouldBeFalse();
      }

      await Task.CompletedTask;
    }
  }
}
