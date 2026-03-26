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

      RepoCheckVersionService service = new(nuGetService);
      service.ShouldNotBeNull();

      await Task.CompletedTask;
    }

    public static async Task CheckGitTagVersionAsync_WhenNotInGitRepo_ShouldReturnFalse()
    {
      MockNuGetPackageService nuGetService = new();

      RepoCheckVersionService service = new(nuGetPackageService: nuGetService);

      string tempDir = Path.Combine(Path.GetTempPath(), $"not-a-repo-{Guid.NewGuid()}");
      Directory.CreateDirectory(tempDir);

      string originalDir = Directory.GetCurrentDirectory();
      try
      {
        Directory.SetCurrentDirectory(tempDir);
        GitTagCheckResult result = await service.CheckGitTagVersionAsync();

        result.IsNewVersion.ShouldBeFalse();
        result.Version.ShouldBeEmpty();
        result.LatestReleaseTag.ShouldBeNull();
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDir);
        Directory.Delete(tempDir, recursive: true);
      }
    }

    public static async Task CheckGitTagVersionAsync_ShouldCompareVersions()
    {
      MockNuGetPackageService nuGetService = new();

      RepoCheckVersionService service = new(nuGetPackageService: nuGetService);

      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "tag", "--sort=-v:refname")
          .Returns("v999.0.0\nv1.0.0");

        GitTagCheckResult result = await service.CheckGitTagVersionAsync();

        result.Version.ShouldNotBeEmpty();
        result.LatestReleaseTag.ShouldBe("v999.0.0");
        CommandMock.VerifyCalled("git", "tag", "--sort=-v:refname");
      }
    }

    public static async Task CheckGitTagVersionAsync_WhenGitTagExists_ShouldReturnIsNewVersionFalse()
    {
      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      string version = ReadVersionFromBuildProps(repoRoot);
      string expectedTag = $"v{version}";

      MockNuGetPackageService nuGetService = new();

      RepoCheckVersionService service = new(nuGetPackageService: nuGetService);

      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "tag", "--sort=-v:refname")
          .Returns(expectedTag);

        GitTagCheckResult result = await service.CheckGitTagVersionAsync();

        result.IsNewVersion.ShouldBeFalse();
        result.Version.ShouldBe(version);
        result.LatestReleaseTag.ShouldBe(expectedTag);
        CommandMock.VerifyCalled("git", "tag", "--sort=-v:refname");
      }
    }

    public static async Task CheckGitTagVersionAsync_WhenGitTagDoesNotExist_ShouldReturnIsNewVersionTrue()
    {
      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      string version = ReadVersionFromBuildProps(repoRoot);
      MockNuGetPackageService nuGetService = new();

      RepoCheckVersionService service = new(nuGetPackageService: nuGetService);

      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "tag", "--sort=-v:refname")
          .Returns("v0.0.1");

        GitTagCheckResult result = await service.CheckGitTagVersionAsync();

        result.IsNewVersion.ShouldBeTrue();
        result.Version.ShouldBe(version);
        result.LatestReleaseTag.ShouldBe("v0.0.1");
        CommandMock.VerifyCalled("git", "tag", "--sort=-v:refname");
      }
    }

    public static async Task CheckNuGetVersionAsync_WhenVersionIsNew_ShouldReturnTrue()
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

      RepoCheckVersionService service = new(nuGetPackageService: nuGetService);

      NuGetCheckResult result = await service.CheckNuGetVersionAsync(["TestPackage"]);

      result.IsNewVersion.ShouldBeTrue();
      result.LatestNuGetVersion.ShouldBe("1.0.0-beta.1");
      result.CheckedPackages.ShouldNotBeNull();
      result.CheckedPackages.Count.ShouldBe(1);
      result.CheckedPackages[0].ShouldBe("TestPackage");
      result.AlreadyPublishedPackages.ShouldBeNull();
    }

    public static async Task CheckNuGetVersionAsync_WhenVersionExists_ShouldReturnFalse()
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

      RepoCheckVersionService service = new(nuGetPackageService: nuGetService);

      NuGetCheckResult result = await service.CheckNuGetVersionAsync(["TestPackage"]);

      result.IsNewVersion.ShouldBeFalse();
      result.LatestNuGetVersion.ShouldBe(version);
      result.AlreadyPublishedPackages.ShouldNotBeNull();
      result.AlreadyPublishedPackages.Count.ShouldBe(1);
      result.AlreadyPublishedPackages[0].ShouldBe("TestPackage");
    }

    public static async Task CheckNuGetVersionAsync_WithEmptyPackages_ShouldReturnFalse()
    {
      MockNuGetPackageService nuGetService = new();

      RepoCheckVersionService service = new(nuGetPackageService: nuGetService);

      NuGetCheckResult result = await service.CheckNuGetVersionAsync([]);

      result.IsNewVersion.ShouldBeFalse();
      result.Version.ShouldBeEmpty();
      result.CheckedPackages.Count.ShouldBe(0);
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

      public Task<PackageVersionInfo?> GetLatestVersionsAsync(string packageId, CancellationToken cancellationToken = default)
      {
        return Task.FromResult<PackageVersionInfo?>(null);
      }

      public string? ParseVersion(string version) => version;

      public int CompareVersions(string version1, string version2)
      {
        return string.Compare(version1, version2, StringComparison.OrdinalIgnoreCase);
      }

      public string GetUpdateType(string currentVersion, string latestVersion) => "none";
    }

    private sealed class MockNuGetPackageService : INuGetPackageService
    {
      public Task<NuGetSearchResult?> SearchAsync(string packageId, CancellationToken cancellationToken = default)
      {
        return Task.FromResult<NuGetSearchResult?>(null);
      }

      public Task<PackageVersionInfo?> GetLatestVersionsAsync(string packageId, CancellationToken cancellationToken = default)
      {
        return Task.FromResult<PackageVersionInfo?>(null);
      }

      public string? ParseVersion(string version) => version;

      public int CompareVersions(string version1, string version2)
      {
        return string.Compare(version1, version2, StringComparison.OrdinalIgnoreCase);
      }

      public string GetUpdateType(string currentVersion, string latestVersion) => "none";
    }
  }
}
