#!/usr/bin/dotnet --

#region Purpose
// Tests for RepoCheckVersionService - validates version checking operations
#endregion

using System.Xml.Linq;

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

    public static async Task CheckAsync_WhenGitTagExists_ShouldReturnIsNewVersionFalse()
    {
      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      string version = ReadVersionFromBuildProps(repoRoot);
      string expectedTag = $"v{version}";

      using TestTerminal terminal = new();
      MockNuGetPackageService nuGetService = new();
      MockRepoConfigService configService = new();

      RepoCheckVersionService service = new(terminal, nuGetService, configService);

      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "tag", "-l", expectedTag)
          .Returns(expectedTag);

        CheckVersionResult result = await service.CheckAsync(strategy: "git-tag");

        result.IsNewVersion.ShouldBeFalse();
        result.Version.ShouldBe(version);
        result.ResolvedTag.ShouldBe(expectedTag);
        CommandMock.VerifyCalled("git", "tag", "-l", expectedTag);
      }
    }

    public static async Task CheckAsync_WhenGitTagDoesNotExist_ShouldReturnIsNewVersionTrue()
    {
      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      string version = ReadVersionFromBuildProps(repoRoot);
      string expectedTag = $"v{version}";

      using TestTerminal terminal = new();
      MockNuGetPackageService nuGetService = new();
      MockRepoConfigService configService = new();

      RepoCheckVersionService service = new(terminal, nuGetService, configService);

      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "tag", "-l", expectedTag)
          .Returns("");

        CheckVersionResult result = await service.CheckAsync(strategy: "git-tag");

        result.IsNewVersion.ShouldBeTrue();
        result.Version.ShouldBe(version);
        result.ResolvedTag.ShouldBeNull();
        CommandMock.VerifyCalled("git", "tag", "-l", expectedTag);
      }
    }

    private static string ReadVersionFromBuildProps(string repoRoot)
    {
      string propsPath = Path.Combine(repoRoot, "source", "Directory.Build.props");
      var doc = XDocument.Load(propsPath);
      XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

      XElement? versionElement = doc.Descendants(ns + "Version").FirstOrDefault();
      if (versionElement == null)
      {
        versionElement = doc.Descendants("Version").FirstOrDefault();
      }

      string? version = versionElement?.Value;
      version.ShouldNotBeNull();
      version.ShouldNotBeEmpty();
      return version;
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
