#!/usr/bin/dotnet --

#region Purpose
// Tests for Git.WorktreeListPorcelainAsync() - validates worktree list in porcelain format
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Git (the static class providing git operations)
// Action: WorktreeListPorcelain (the method being tested)
// Tests verify porcelain output retrieval using CommandMock
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class WorktreeListPorcelain_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WorktreeListPorcelain_Given_>();

    public static async Task ValidRepo_Should_ReturnPorcelainOutput()
    {
      using (CommandMock.Enable())
      {
        string porcelainOutput = @"worktree /home/user/project
HEAD abcd1234
branch refs/heads/main

worktree /home/user/project-feature
HEAD efgh5678
detached

";

        CommandMock.Setup("git", "worktree", "list", "--porcelain")
          .Returns(porcelainOutput);

        string result = await Git.WorktreeListPorcelainAsync("/path/to/repo.git");

        result.ShouldNotBeNullOrWhiteSpace();
        result.ShouldContain("worktree");
      }

      await Task.CompletedTask;
    }
  }
}
