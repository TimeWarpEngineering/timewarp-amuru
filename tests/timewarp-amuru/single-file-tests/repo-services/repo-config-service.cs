#!/usr/bin/dotnet --

#region Purpose
// Tests for RepoConfigService - validates configuration read/write operations
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Repo_Services
{
  [TestTag("Repo")]
  public class RepoConfigService_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<RepoConfigService_Given_>();

    public static async Task Constructor_WhenNotInGitRepo_ShouldThrow()
    {
      string tempDir = Path.Combine(Path.GetTempPath(), $"not-a-repo-{Guid.NewGuid()}");
      Directory.CreateDirectory(tempDir);

      string originalDir = Directory.GetCurrentDirectory();
      try
      {
        Directory.SetCurrentDirectory(tempDir);
        InvalidOperationException? exception = null;

        try
        {
          _ = new RepoConfigService();
        }
        catch (InvalidOperationException ex)
        {
          exception = ex;
        }

        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("Not in a git repository");
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDir);
        Directory.Delete(tempDir, recursive: true);
      }

      await Task.CompletedTask;
    }

    public static async Task ConfigPath_ShouldBeInTimewarpDirectory()
    {
      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      RepoConfigService service = new();
      service.ConfigPath.ShouldBe(Path.Combine(repoRoot, ".timewarp", "ganda.jsonc"));

      await Task.CompletedTask;
    }

    public static async Task GetConfigAsync_WhenNoConfigFile_ShouldReturnDefault()
    {
      RepoConfigService service = new();
      RepoConfig config = await service.GetConfigAsync();

      config.ShouldNotBeNull();
      config.CheckVersion?.Strategy.ShouldBeNull();

      await Task.CompletedTask;
    }
  }
}
