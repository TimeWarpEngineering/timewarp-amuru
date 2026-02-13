#!/usr/bin/dotnet --

#region Purpose
// Tests for Git.CloneBareAsync() - validates bare repository cloning functionality
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Git (the static class providing git operations)
// Action: CloneBare (the method being tested)
// Tests verify successful cloning and error handling using CommandMock
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class CloneBare_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<CloneBare_Given_>();

    public static async Task ValidUrl_Should_CloneSuccessfully()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "clone", "--bare", "https://github.com/user/repo.git", "/path/to/repo.git")
          .Returns("");

        GitCloneResult result = await Git.CloneBareAsync("https://github.com/user/repo.git", "/path/to/repo.git");

        result.Success.ShouldBeTrue();
        result.Path.ShouldBe("/path/to/repo.git");
        result.ErrorMessage.ShouldBeNull();
      }

      await Task.CompletedTask;
    }

    public static async Task Error_Should_ReturnFailure()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "clone", "--bare", "https://github.com/user/repo.git", "/path/to/repo.git")
          .ReturnsError("fatal: could not access repository", 128);

        GitCloneResult result = await Git.CloneBareAsync("https://github.com/user/repo.git", "/path/to/repo.git");

        result.Success.ShouldBeFalse();
        result.Path.ShouldBeNull();
        result.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
      }

      await Task.CompletedTask;
    }
  }
}
