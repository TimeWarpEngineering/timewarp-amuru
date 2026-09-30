#region Purpose
// Implementation of repository version checking operations
#endregion

#region Design
// Git IsNewVersion is exact existence of v{version} (git tag -l), not inequality
// with the versionsort-latest tag. LatestReleaseTag stays the latest tag, or the
// caller-supplied tag, for display.
// NuGet "latest" is the highest LISTED version. Already-published includes
// UNLISTED versions, because nuget.org rejects a republish of the same id/version.
// Each package is compared to a literal <Version> on the csproj that declares its
// PackageId. When that element is absent, the version is source/Directory.Build.props.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Implementation of repository version checking operations.
/// </summary>
public sealed class RepoCheckVersionService : IRepoCheckVersionService
{
  private readonly INuGetPackageService NuGetPackageService;

  public RepoCheckVersionService(INuGetPackageService nuGetPackageService)
  {
    NuGetPackageService = nuGetPackageService;
  }

  public async Task<GitTagCheckResult> CheckGitTagVersionAsync
  (
    string? tag = null,
    CancellationToken cancellationToken = default
  )
  {
    string? repoRoot = Git.FindRoot();
    if (repoRoot == null)
    {
      return new GitTagCheckResult(false, string.Empty, null);
    }

    string? version = await GetVersionFromDirectoryBuildPropsAsync(repoRoot, cancellationToken).ConfigureAwait(false);
    if (version == null)
    {
      return new GitTagCheckResult(false, string.Empty, null);
    }

    // Environment resolution (e.g. GITHUB_REF_NAME on tag-triggered CI runs) is the
    // CALLER's job via the explicit tag parameter; peeking env vars here made the
    // service behave differently under CI than anywhere else and untestable on
    // release runs. The parameter only supplies the displayed tag.
    string? displayTag = string.IsNullOrWhiteSpace(tag) ? null : tag;
    if (displayTag == null)
    {
      displayTag = await GetLatestGitTagAsync(cancellationToken).ConfigureAwait(false);
    }

    if (string.IsNullOrWhiteSpace(displayTag))
    {
      displayTag = null;
    }

    bool exactTagExists = await ExactVersionTagExistsAsync(version, cancellationToken).ConfigureAwait(false);
    return new GitTagCheckResult(!exactTagExists, version, displayTag);
  }

  public async Task<NuGetCheckResult> CheckNuGetVersionAsync
  (
    IReadOnlyList<string> packages,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(packages);

    if (packages.Count == 0)
    {
      return new NuGetCheckResult(false, string.Empty, null, [], null);
    }

    string? repoRoot = Git.FindRoot();
    if (repoRoot == null)
    {
      return new NuGetCheckResult(false, string.Empty, null, [], null);
    }

    List<PackageVersionCheck> resolved = [];
    foreach (string pkg in packages)
    {
      string? packageVersion = await ResolvePackageVersionAsync(repoRoot, pkg, cancellationToken).ConfigureAwait(false);
      if (string.IsNullOrWhiteSpace(packageVersion))
      {
        return new NuGetCheckResult(false, string.Empty, null, [], null);
      }

      resolved.Add(new PackageVersionCheck(pkg, packageVersion));
    }

    string version = string.Join
    (
      ", ",
      resolved.Select(static check => check.Version).Distinct(StringComparer.OrdinalIgnoreCase)
    );

    List<string> checkedPackages = [];
    List<string> alreadyPublished = [];
    string? latestNuGetVersion = null;

    foreach (PackageVersionCheck check in resolved)
    {
      checkedPackages.Add(check.Package);

      NuGetSearchResult? result = await NuGetPackageService.SearchAsync(check.Package, cancellationToken).ConfigureAwait(false);
      if (result == null)
      {
        continue;
      }

      NuGetPackageVersion? latestListed = null;
      foreach (NuGetPackageVersion packageVersion in result.Versions)
      {
        if (packageVersion.Listed)
        {
          latestListed = packageVersion;
          break;
        }
      }

      if (latestListed != null)
      {
        string latestVersion = latestListed.Version;
        if
        (
          latestNuGetVersion == null ||
          NuGetPackageService.CompareVersions(latestVersion, latestNuGetVersion) > 0
        )
        {
          latestNuGetVersion = latestVersion;
        }
      }

      // "Already published" means the feed has THIS package's version anywhere,
      // including unlisted entries — a feed that hides the version still 409s
      // on republish.
      if (result.Versions.Any(v => string.Equals(check.Version, v.Version, StringComparison.OrdinalIgnoreCase)))
      {
        alreadyPublished.Add(check.Package);
      }
    }

    bool isNewVersion = alreadyPublished.Count == 0;
    IReadOnlyList<string>? alreadyPublishedPackages = alreadyPublished.Count > 0 ? alreadyPublished : null;

    return new NuGetCheckResult
    (
      isNewVersion,
      version,
      latestNuGetVersion,
      checkedPackages,
      alreadyPublishedPackages
    );
  }

  private readonly record struct PackageVersionCheck(string Package, string Version);

  private static async Task<string?> ResolvePackageVersionAsync
  (
    string repoRoot,
    string packageId,
    CancellationToken cancellationToken
  )
  {
    string? overrideVersion = TryReadCsprojVersionOverride(repoRoot, packageId);
    if (!string.IsNullOrWhiteSpace(overrideVersion))
    {
      return overrideVersion;
    }

    return await GetVersionFromDirectoryBuildPropsAsync(repoRoot, cancellationToken).ConfigureAwait(false);
  }

  private static string? TryReadCsprojVersionOverride(string repoRoot, string packageId)
  {
    string sourceDir = Path.Combine(repoRoot, "source");
    if (!Directory.Exists(sourceDir))
    {
      return null;
    }

    foreach (string projectPath in Directory.EnumerateFiles(sourceDir, "*.csproj", SearchOption.AllDirectories))
    {
      if (IsUnderBuildOutput(sourceDir, projectPath))
      {
        continue;
      }

#pragma warning disable IDE0007
      XDocument doc = XDocument.Load(projectPath);
#pragma warning restore IDE0007
      string? declaredPackageId = FindElementValue(doc, "PackageId");
      if (!string.Equals(declaredPackageId, packageId, StringComparison.OrdinalIgnoreCase))
      {
        continue;
      }

      string? version = FindElementValue(doc, "Version");
      if (version == null || version.Contains("$(", StringComparison.Ordinal))
      {
        return null;
      }

      return version;
    }

    return null;
  }

  private static bool IsUnderBuildOutput(string sourceDir, string path)
  {
    string relative = Path.GetRelativePath(sourceDir, path).Replace('\\', '/');
    return relative.Contains("/bin/", StringComparison.Ordinal)
      || relative.Contains("/obj/", StringComparison.Ordinal)
      || relative.StartsWith("bin/", StringComparison.Ordinal)
      || relative.StartsWith("obj/", StringComparison.Ordinal);
  }

  private static string? FindElementValue(XDocument doc, string localName)
  {
    XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
    XElement? element = doc.Descendants(ns + localName).FirstOrDefault();
    if (element == null)
    {
      element = doc.Descendants(localName).FirstOrDefault();
    }

    string? value = element?.Value.Trim();
    return string.IsNullOrWhiteSpace(value) ? null : value;
  }

  private static async Task<string?> GetVersionFromDirectoryBuildPropsAsync(string repoRoot, CancellationToken cancellationToken)
  {
    string sourceDir = Path.Combine(repoRoot, "source");
    if (!Directory.Exists(sourceDir))
    {
      return null;
    }

    string[] buildPropsFiles = Directory.GetFiles(sourceDir, "Directory.Build.props", SearchOption.TopDirectoryOnly);
    if (buildPropsFiles.Length == 0)
    {
      return null;
    }

    string buildPropsPath = buildPropsFiles[0];
    string xml = await File.ReadAllTextAsync(buildPropsPath, cancellationToken).ConfigureAwait(false);

#pragma warning disable IDE0007
    XDocument doc = XDocument.Parse(xml);
#pragma warning restore IDE0007
    return FindElementValue(doc, "Version");
  }

  private static async Task<bool> ExactVersionTagExistsAsync(string version, CancellationToken cancellationToken)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("tag", "-l", $"v{version}")
      .CaptureAsync(cancellationToken).ConfigureAwait(false);

    // Fail safe: a git failure cannot prove the tag is absent, so do not
    // report the version as new.
    if (!result.Success)
    {
      return true;
    }

    return !string.IsNullOrWhiteSpace(result.Stdout);
  }

  private static async Task<string?> GetLatestGitTagAsync(CancellationToken cancellationToken)
  {
    // versionsort.suffix=- makes pre-release tags (v1.0.0-beta.34) sort BEFORE their
    // release (v1.0.0); without it the latest tag can be a pre-release older than the release.
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("-c", "versionsort.suffix=-", "tag", "--sort=-v:refname")
      .CaptureAsync(cancellationToken).ConfigureAwait(false);

    if (result.ExitCode != 0)
    {
      return null;
    }

    string tagOutput = result.Stdout.Trim();
    if (string.IsNullOrWhiteSpace(tagOutput))
    {
      return null;
    }

    string normalizedOutput = tagOutput.Replace("\r\n", "\n", StringComparison.Ordinal);
    string[] lines = normalizedOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (lines.Length == 0)
    {
      return null;
    }

    return lines[0];
  }
}
