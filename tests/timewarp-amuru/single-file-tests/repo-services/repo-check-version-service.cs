#!/usr/bin/env -S dotnet --

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
      string version = ReadVersionFromBuildProps(repoRoot);

      using (CommandMock.Enable(MockBehavior.Loose))
      {
        CommandMock.Setup("git", "-c", "versionsort.suffix=-", "tag", "--sort=-v:refname")
          .Returns("v999.0.0\nv1.0.0");
        CommandMock.Setup("git", "tag", "-l", $"v{version}")
          .Returns("");

        GitTagCheckResult result = await service.CheckGitTagVersionAsync();

        result.Version.ShouldNotBeEmpty();
        result.LatestReleaseTag.ShouldBe("v999.0.0");
        result.IsNewVersion.ShouldBeTrue();
        CommandMock.VerifyCalled("git", "-c", "versionsort.suffix=-", "tag", "--sort=-v:refname");
        CommandMock.VerifyCalled("git", "tag", "-l", $"v{version}");
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

      using (CommandMock.Enable(MockBehavior.Loose))
      {
        CommandMock.Setup("git", "-c", "versionsort.suffix=-", "tag", "--sort=-v:refname")
          .Returns(expectedTag);
        CommandMock.Setup("git", "tag", "-l", expectedTag)
          .Returns(expectedTag);

        GitTagCheckResult result = await service.CheckGitTagVersionAsync();

        result.IsNewVersion.ShouldBeFalse();
        result.Version.ShouldBe(version);
        result.LatestReleaseTag.ShouldBe(expectedTag);
        CommandMock.VerifyCalled("git", "tag", "-l", expectedTag);
      }
    }

    public static async Task CheckGitTagVersionAsync_WhenGitTagDoesNotExist_ShouldReturnIsNewVersionTrue()
    {
      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();

      string version = ReadVersionFromBuildProps(repoRoot);
      MockNuGetPackageService nuGetService = new();

      RepoCheckVersionService service = new(nuGetPackageService: nuGetService);

      using (CommandMock.Enable(MockBehavior.Loose))
      {
        CommandMock.Setup("git", "-c", "versionsort.suffix=-", "tag", "--sort=-v:refname")
          .Returns("v0.0.1");
        CommandMock.Setup("git", "tag", "-l", $"v{version}")
          .Returns("");

        GitTagCheckResult result = await service.CheckGitTagVersionAsync();

        result.IsNewVersion.ShouldBeTrue();
        result.Version.ShouldBe(version);
        result.LatestReleaseTag.ShouldBe("v0.0.1");
        CommandMock.VerifyCalled("git", "tag", "-l", $"v{version}");
      }
    }

    public static async Task CheckGitTagVersionAsync_WhenOlderExactTagExists_ShouldReturnIsNewVersionFalse()
    {
      string tempRoot = CreateTempRepo
      (
        """
        <Project>
          <PropertyGroup>
            <Version>1.0.0</Version>
          </PropertyGroup>
        </Project>
        """
      );
      string originalDir = Directory.GetCurrentDirectory();
      try
      {
        Directory.SetCurrentDirectory(tempRoot);
        RepoCheckVersionService service = new(new MockNuGetPackageService());

        using (CommandMock.Enable(MockBehavior.Strict))
        {
          CommandMock.Setup("git", "-c", "versionsort.suffix=-", "tag", "--sort=-v:refname")
            .Returns("v1.1.0\nv1.0.0");
          CommandMock.Setup("git", "tag", "-l", "v1.0.0")
            .Returns("v1.0.0");

          GitTagCheckResult result = await service.CheckGitTagVersionAsync();

          result.IsNewVersion.ShouldBeFalse();
          result.Version.ShouldBe("1.0.0");
          result.LatestReleaseTag.ShouldBe("v1.1.0");
        }
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDir);
        Directory.Delete(tempRoot, recursive: true);
      }
    }

    public static async Task CheckGitTagVersionAsync_WhenExactTagLookupFails_ShouldNotReportNew()
    {
      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();
      string version = ReadVersionFromBuildProps(repoRoot);
      RepoCheckVersionService service = new(new MockNuGetPackageService());

      using (CommandMock.Enable(MockBehavior.Strict))
      {
        CommandMock.Setup("git", "-c", "versionsort.suffix=-", "tag", "--sort=-v:refname")
          .Returns("v0.0.1");
        CommandMock.Setup("git", "tag", "-l", $"v{version}")
          .ReturnsError("fatal: not a git repository");

        GitTagCheckResult result = await service.CheckGitTagVersionAsync();

        result.IsNewVersion.ShouldBeFalse();
        result.Version.ShouldBe(version);
        result.LatestReleaseTag.ShouldBe("v0.0.1");
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

    public static async Task CheckNuGetVersionAsync_WhenVersionIsUnlisted_ShouldTreatItAsPublished()
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
              new NuGetPackageVersion("9.9.9", Listed: false),
              new NuGetPackageVersion(version, Listed: false),
              new NuGetPackageVersion("0.1.0", Listed: true)
            }
          )
        }
      );

      RepoCheckVersionService service = new(nuGetService);
      NuGetCheckResult result = await service.CheckNuGetVersionAsync(["TestPackage"]);

      result.IsNewVersion.ShouldBeFalse();
      result.LatestNuGetVersion.ShouldBe("0.1.0");
      result.AlreadyPublishedPackages.ShouldNotBeNull();
      result.AlreadyPublishedPackages.ShouldContain("TestPackage");
    }

    public static async Task CheckNuGetVersionAsync_WhenToolsCsprojOverridesVersion_ShouldCompareThatVersion()
    {
      string tempRoot = CreateTempRepo
      (
        """
        <Project>
          <PropertyGroup>
            <Version>1.0.0</Version>
          </PropertyGroup>
        </Project>
        """,
        """
        <Project>
          <PropertyGroup>
            <PackageId>Example.Tools</PackageId>
            <Version>1.0.0-beta.2</Version>
          </PropertyGroup>
        </Project>
        """
      );
      string originalDir = Directory.GetCurrentDirectory();
      try
      {
        Directory.SetCurrentDirectory(tempRoot);
        ConfigurableMockNuGetPackageService nuGetService = new
        (
          new Dictionary<string, NuGetSearchResult?>
          {
            ["Example.Tools"] = new NuGetSearchResult
            (
              "Example.Tools",
              new List<NuGetPackageVersion>
              {
                new NuGetPackageVersion("1.0.0"),
                new NuGetPackageVersion("1.0.0-beta.2", Listed: false)
              }
            )
          }
        );

        RepoCheckVersionService service = new(nuGetService);
        NuGetCheckResult result = await service.CheckNuGetVersionAsync(["Example.Tools"]);

        result.Version.ShouldBe("1.0.0-beta.2");
        result.IsNewVersion.ShouldBeFalse();
        result.LatestNuGetVersion.ShouldBe("1.0.0");
        result.AlreadyPublishedPackages.ShouldNotBeNull();
        result.AlreadyPublishedPackages.ShouldContain("Example.Tools");
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDir);
        Directory.Delete(tempRoot, recursive: true);
      }
    }

    public static async Task CheckNuGetVersionAsync_WhenToolsHasNoCsprojVersion_ShouldUsePropsVersion()
    {
      string? repoRoot = Git.FindRoot();
      repoRoot.ShouldNotBeNull();
      string propsVersion = ReadVersionFromBuildProps(repoRoot);
      string toolsProject = Path.Combine(repoRoot, "source", "timewarp-amuru-tools", "timewarp-amuru-tools.csproj");
#pragma warning disable IDE0007
      XDocument toolsDoc = XDocument.Load(toolsProject);
#pragma warning restore IDE0007
      bool hasLiteralVersion = toolsDoc.Descendants("Version").Any(static element =>
      {
        string value = element.Value.Trim();
        return value.Length > 0 && !value.Contains("$(", StringComparison.Ordinal);
      });
      hasLiteralVersion.ShouldBeFalse();

      ConfigurableMockNuGetPackageService nuGetService = new
      (
        new Dictionary<string, NuGetSearchResult?>
        {
          ["TimeWarp.Amuru.Tools"] = new NuGetSearchResult
          (
            "TimeWarp.Amuru.Tools",
            new List<NuGetPackageVersion>
            {
              new NuGetPackageVersion("0.0.1")
            }
          )
        }
      );

      RepoCheckVersionService service = new(nuGetService);
      NuGetCheckResult result = await service.CheckNuGetVersionAsync(["TimeWarp.Amuru.Tools"]);

      result.Version.ShouldBe(propsVersion);
      result.IsNewVersion.ShouldBeTrue();
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

    private static string CreateTempRepo(string propsXml, string? toolsCsprojXml = null)
    {
      string tempRoot = Path.Combine
      (
        AppContext.BaseDirectory,
        "temp-fixtures",
        Guid.NewGuid().ToString("N")
      );
      Directory.CreateDirectory(Path.Combine(tempRoot, ".git"));
      string sourceDir = Path.Combine(tempRoot, "source");
      Directory.CreateDirectory(sourceDir);
      File.WriteAllText(Path.Combine(sourceDir, "Directory.Build.props"), propsXml);
      if (toolsCsprojXml != null)
      {
        string projectDir = Path.Combine(sourceDir, "example-tools");
        Directory.CreateDirectory(projectDir);
        File.WriteAllText(Path.Combine(projectDir, "example-tools.csproj"), toolsCsprojXml);
      }

      return tempRoot;
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
