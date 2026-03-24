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
      MockNuGetPackageService nuGetService = new();
      MockRepoConfigService configService = new();

      RepoCheckVersionService service = new(nuGetService, configService);
      service.ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task CheckAsync_WhenNotInGitRepo_ShouldReturnFalse()
    {
      MockNuGetPackageService nuGetService = new();
      MockRepoConfigService configService = new();

      RepoCheckVersionService service = new(nuGetService, configService);

      string tempDir = Path.Combine(Path.GetTempPath(), $"not-a-repo-{Guid.NewGuid()}");
      Directory.CreateDirectory(tempDir);

      string originalDir = Directory.GetCurrentDirectory();
      try
      {
        Directory.SetCurrentDirectory(tempDir);
        CheckVersionResult result = await service.CheckAsync();

        result.IsNewVersion.ShouldBeFalse();
        result.Version.ShouldBeEmpty();
        result.Strategy.ShouldBeEmpty();
        result.LatestReleaseTag.ShouldBeNull();
        result.LatestNuGetVersion.ShouldBeNull();
        result.CheckedPackages.ShouldBeNull();
        result.AlreadyPublishedPackages.ShouldBeNull();
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDir);
        Directory.Delete(tempDir, recursive: true);
      }
    }

    public static async Task CheckAsync_WithGitTagStrategy_ShouldCompareVersions()
    {
      MockNuGetPackageService nuGetService = new();
      MockRepoConfigService configService = new();

      RepoCheckVersionService service = new(nuGetService, configService);

      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "tag", "--sort=-v:refname")
          .Returns("v999.0.0\nv1.0.0");

        CheckVersionResult result = await service.CheckAsync(strategy: "git-tag");

        result.Version.ShouldNotBeEmpty();
        result.Strategy.ShouldBe("git-tag");
        result.LatestReleaseTag.ShouldBe("v999.0.0");
        CommandMock.VerifyCalled("git", "tag", "--sort=-v:refname");
      }
    }

    public static async Task CheckAsync_WhenGitTagExists_ShouldReturnIsNewVersionFalse()
    {
      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      string version = ReadVersionFromBuildProps(repoRoot);
      string expectedTag = $"v{version}";

      MockNuGetPackageService nuGetService = new();
      MockRepoConfigService configService = new();

      RepoCheckVersionService service = new(nuGetService, configService);

      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "tag", "--sort=-v:refname")
          .Returns(expectedTag);

        CheckVersionResult result = await service.CheckAsync(strategy: "git-tag");

        result.IsNewVersion.ShouldBeFalse();
        result.Version.ShouldBe(version);
        result.Strategy.ShouldBe("git-tag");
        result.LatestReleaseTag.ShouldBe(expectedTag);
        result.LatestNuGetVersion.ShouldBeNull();
        result.CheckedPackages.ShouldBeNull();
        result.AlreadyPublishedPackages.ShouldBeNull();
        CommandMock.VerifyCalled("git", "tag", "--sort=-v:refname");
      }
    }

    public static async Task CheckAsync_WhenGitTagDoesNotExist_ShouldReturnIsNewVersionTrue()
    {
      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      string version = ReadVersionFromBuildProps(repoRoot);
      MockNuGetPackageService nuGetService = new();
      MockRepoConfigService configService = new();

      RepoCheckVersionService service = new(nuGetService, configService);

      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "tag", "--sort=-v:refname")
          .Returns("v0.0.1");

        CheckVersionResult result = await service.CheckAsync(strategy: "git-tag");

        result.IsNewVersion.ShouldBeTrue();
        result.Version.ShouldBe(version);
        result.Strategy.ShouldBe("git-tag");
        result.LatestReleaseTag.ShouldBe("v0.0.1");
        result.LatestNuGetVersion.ShouldBeNull();
        result.CheckedPackages.ShouldBeNull();
        result.AlreadyPublishedPackages.ShouldBeNull();
        CommandMock.VerifyCalled("git", "tag", "--sort=-v:refname");
      }
    }

    public static async Task CheckAsync_WithNuGetSearchStrategy_WhenVersionIsNew_ShouldReturnTrue()
    {
      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      string version = ReadVersionFromBuildProps(repoRoot);
      version.ShouldNotBe("1.0.0-beta.1");

      ConfigurableMockNuGetPackageService nuGetService = new
      (
        new Dictionary<string, NuGetSearchResult?>
        {
          ["TestPackage"] = new NuGetSearchResult
          (
            "TestPackage",
            new List<NuGetPackageVersion>
            {
              new NuGetPackageVersion("1.0.0-beta.1")
            }
          )
        }
      );

      RepoConfig repoConfig = new()
      {
        CheckVersion = new RepoCheckVersionConfig
        {
          Strategy = "nuget-search",
          Packages = "TestPackage"
        }
      };

      MockRepoConfigService configService = new(repoConfig);
      RepoCheckVersionService service = new(nuGetService, configService);

      CheckVersionResult result = await service.CheckAsync();

      result.IsNewVersion.ShouldBeTrue();
      result.Strategy.ShouldBe("nuget-search");
      result.LatestNuGetVersion.ShouldBe("1.0.0-beta.1");
      result.CheckedPackages.ShouldNotBeNull();

      bool checkedPackagesContainsTestPackage = result.CheckedPackages.Any
      (
        package => string.Equals(package, "TestPackage", StringComparison.Ordinal)
      );
      checkedPackagesContainsTestPackage.ShouldBeTrue();

      result.AlreadyPublishedPackages.ShouldBeNull();
    }

    public static async Task CheckAsync_WithNuGetSearchStrategy_WhenVersionExists_ShouldReturnFalse()
    {
      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      string version = ReadVersionFromBuildProps(repoRoot);

      ConfigurableMockNuGetPackageService nuGetService = new
      (
        new Dictionary<string, NuGetSearchResult?>
        {
          ["TestPackage"] = new NuGetSearchResult
          (
            "TestPackage",
            new List<NuGetPackageVersion>
            {
              new NuGetPackageVersion(version)
            }
          )
        }
      );

      RepoConfig repoConfig = new()
      {
        CheckVersion = new RepoCheckVersionConfig
        {
          Strategy = "nuget-search",
          Packages = "TestPackage"
        }
      };

      MockRepoConfigService configService = new(repoConfig);
      RepoCheckVersionService service = new(nuGetService, configService);

      CheckVersionResult result = await service.CheckAsync();

      result.IsNewVersion.ShouldBeFalse();
      result.Strategy.ShouldBe("nuget-search");
      result.LatestNuGetVersion.ShouldBe(version);
      result.AlreadyPublishedPackages.ShouldNotBeNull();

      bool alreadyPublishedContainsTestPackage = result.AlreadyPublishedPackages.Any
      (
        package => string.Equals(package, "TestPackage", StringComparison.Ordinal)
      );
      alreadyPublishedContainsTestPackage.ShouldBeTrue();
    }

    private static string ReadVersionFromBuildProps(string repoRoot)
    {
      string propsPath = Path.Combine(repoRoot, "source", "Directory.Build.props");
#pragma warning disable IDE0007
      XDocument doc = XDocument.Load(propsPath);
#pragma warning restore IDE0007
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

    private sealed class ConfigurableMockNuGetPackageService : INuGetPackageService
    {
      private readonly IReadOnlyDictionary<string, NuGetSearchResult?> ResultsByPackageId;

      public ConfigurableMockNuGetPackageService(IReadOnlyDictionary<string, NuGetSearchResult?> resultsByPackageId)
      {
        ResultsByPackageId = resultsByPackageId;
      }

      public Task<NuGetSearchResult?> SearchAsync(string packageId, CancellationToken cancellationToken = default)
      {
        ResultsByPackageId.TryGetValue(packageId, out NuGetSearchResult? result);
        return Task.FromResult(result);
      }
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
      private readonly RepoConfig RepoConfig;

      public string ConfigPath => "/mock/config.json";

      public MockRepoConfigService()
      {
        RepoConfig = new RepoConfig();
      }

      public MockRepoConfigService(RepoConfig repoConfig)
      {
        RepoConfig = repoConfig;
      }

      public Task<RepoConfig> GetConfigAsync(CancellationToken cancellationToken = default)
      {
        return Task.FromResult(RepoConfig);
      }

      public Task SetConfigAsync(RepoConfig config, CancellationToken cancellationToken = default)
      {
        return Task.CompletedTask;
      }
    }
  }
}
