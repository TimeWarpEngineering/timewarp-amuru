#!/usr/bin/dotnet --

#region Purpose
// Tests for RepoCheckVersionService - validates version checking operations
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Repo_Services
{
  [TestTag("Repo")]
  public class RepoCheckVersionService_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<RepoCheckVersionService_Given_>();

    public static async Task Constructor_WithValidDependencies_ShouldSucceed()
    {
      using TestTerminal terminal = new();
      MockNuGetPackageService nuGetService = new();
      MockRepoConfigService configService = new();

      RepoCheckVersionService service = new(terminal, nuGetService, configService);
      service.ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task CheckAsync_WhenNotInGitRepo_ShouldReturnFalse()
    {
      using TestTerminal terminal = new();
      MockNuGetPackageService nuGetService = new();
      MockRepoConfigService configService = new();

      RepoCheckVersionService service = new(terminal, nuGetService, configService);

      string tempDir = Path.Combine(Path.GetTempPath(), $"not-a-repo-{Guid.NewGuid()}");
      Directory.CreateDirectory(tempDir);

      string originalDir = Directory.GetCurrentDirectory();
      try
      {
        Directory.SetCurrentDirectory(tempDir);
        CheckVersionResult result = await service.CheckAsync();

        result.IsNewVersion.ShouldBeFalse();
        result.Version.ShouldBeEmpty();
        terminal.ErrorOutput.ShouldContain("Not in a git repository");
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDir);
        Directory.Delete(tempDir, recursive: true);
      }
    }

    public static async Task CheckAsync_WithGitTagStrategy_ShouldCompareVersions()
    {
      using TestTerminal terminal = new();
      MockNuGetPackageService nuGetService = new();
      MockRepoConfigService configService = new();

      RepoCheckVersionService service = new(terminal, nuGetService, configService);

      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      CheckVersionResult result = await service.CheckAsync(strategy: "git-tag");

      result.Version.ShouldNotBeEmpty();
    }

    private sealed class MockNuGetPackageService : INuGetPackageService
    {
      public Task<NuGetSearchResult?> SearchAsync(string packageId, CancellationToken cancellationToken = default)
      {
        return Task.FromResult<NuGetSearchResult?>(null);
      }
    }

    private sealed class MockRepoConfigService : IRepoConfigService
    {
      public string ConfigPath => "/mock/config.json";

      public Task<RepoConfig> GetConfigAsync(CancellationToken cancellationToken = default)
      {
        return Task.FromResult(new RepoConfig());
      }

      public Task SetConfigAsync(RepoConfig config, CancellationToken cancellationToken = default)
      {
        return Task.CompletedTask;
      }
    }
  }
}
