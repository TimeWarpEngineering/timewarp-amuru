#!/usr/bin/env -S dotnet --

#region Purpose
// Real-execution tests for Git.GetRepositoryNameAsync.
#endregion

#region Design
// The origin URL is a local git remote added in a temporary repository.
// No network remote is contacted. A repository with no origin falls back to the directory name.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Git_
{
  [TestTag("Git")]
  public class GetRepositoryName_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<GetRepositoryName_Given_>();

    public static async Task HttpsOrigin_Should_ReturnRepositoryNameWithoutGitSuffix()
    {
      string repository = await GitRepositoryFixture.CreateRepositoryAsync("repo-name");
      try
      {
        await GitRepositoryFixture.GitAsync(
          repository,
          "remote",
          "add",
          "origin",
          "https://github.com/TimeWarpEngineering/sample-repo.git");

        string? name = await Git.GetRepositoryNameAsync(repository);

        name.ShouldBe("sample-repo");
      }
      finally
      {
        GitRepositoryFixture.Delete(repository);
      }
    }

    public static async Task SshOriginWithNestedPath_Should_ReturnLastSegment()
    {
      string repository = await GitRepositoryFixture.CreateRepositoryAsync("repo-name-ssh");
      try
      {
        await GitRepositoryFixture.GitAsync(
          repository,
          "remote",
          "add",
          "origin",
          "git@github.com:TimeWarpEngineering/sample-repo.git");

        string? name = await Git.GetRepositoryNameAsync(repository);

        name.ShouldBe("sample-repo");
      }
      finally
      {
        GitRepositoryFixture.Delete(repository);
      }
    }

    public static async Task NoRemote_Should_ReturnDirectoryName()
    {
      string repository = await GitRepositoryFixture.CreateRepositoryAsync("repo-name-dir");
      try
      {
        string? name = await Git.GetRepositoryNameAsync(repository);

        name.ShouldBe(new DirectoryInfo(repository).Name);
      }
      finally
      {
        GitRepositoryFixture.Delete(repository);
      }
    }
  }
}
