#region Purpose
// Implementation of NuGet package operations using NuGet Protocol API
#endregion

#region Design
// Uses FindPackageByIdResource instead of PackageMetadataResource because:
// - GetAllVersionsAsync returns all versions including prerelease in one call
// - Simpler API surface - no need for multiple queries
// - Aggregate versions from all enabled NuGet sources for comprehensive results
// - SourceRepository instances are cached via NuGetSourceCache to avoid repeated init
// - Removed CLI-based approach (ParseSearchResult) for correctness and performance:
//   dotnet package search only returns latest version, Protocol gives all versions
#endregion

namespace TimeWarp.Amuru;

using NuGet.Common;
using NuGet.Versioning;

public sealed class NuGetPackageService : INuGetPackageService
{
  private readonly NuGetSourceCache SourceCache;

  public NuGetPackageService()
    : this(new NuGetSourceCache())
  {
  }

  internal NuGetPackageService(NuGetSourceCache sourceCache)
  {
    SourceCache = sourceCache;
  }

  public async Task<NuGetSearchResult?> SearchAsync(string packageId, CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

    SourceRepository repository = SourceCache.GetOrCreate("https://api.nuget.org/v3/index.json");

    FindPackageByIdResource? resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
    if (resource == null)
    {
      return null;
    }

    using SourceCacheContext cacheContext = new();
    #pragma warning disable IDE0007
    IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync
#pragma warning restore IDE0007
    (
      packageId,
      cacheContext,
      NullLogger.Instance,
      cancellationToken
    );

#pragma warning disable IDE0007
    List<NuGetVersion> versionList = versions.ToList();
#pragma warning restore IDE0007
    if (versionList.Count == 0)
    {
      return null;
    }

    #pragma warning disable IDE0007
    List<NuGetPackageVersion> packageVersions = versionList
#pragma warning restore IDE0007
      .OrderByDescending(static v => v, VersionComparer.Default)
      .Select(static v => new NuGetPackageVersion(v.ToNormalizedString()))
      .ToList();

    return new NuGetSearchResult(packageId, packageVersions);
  }

  public async Task<PackageVersionInfo?> GetLatestVersionsAsync(string packageId, CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

    SourceRepository repository = SourceCache.GetOrCreate("https://api.nuget.org/v3/index.json");

    FindPackageByIdResource? resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
    if (resource == null)
    {
      return null;
    }

    using SourceCacheContext cacheContext = new();
    #pragma warning disable IDE0007
    IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync
#pragma warning restore IDE0007
    (
      packageId,
      cacheContext,
      NullLogger.Instance,
      cancellationToken
    );

    NuGetVersion? stableVersion = null;
    NuGetVersion? prereleaseVersion = null;

    foreach (NuGetVersion version in versions)
    {
      if (!version.IsPrerelease)
      {
        if (stableVersion == null || version > stableVersion)
        {
          stableVersion = version;
        }
      }
      else
      {
        if (prereleaseVersion == null || version > prereleaseVersion)
        {
          prereleaseVersion = version;
        }
      }
    }

    if (stableVersion == null && prereleaseVersion == null)
    {
      return null;
    }

    return new PackageVersionInfo
    (
      stableVersion?.ToNormalizedString(),
      prereleaseVersion?.ToNormalizedString()
    );
  }

  public string? ParseVersion(string version)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(version);

    string normalized = version.StartsWith('v') ? version[1..] : version;

    if (NuGetVersion.TryParse(normalized, out NuGetVersion? parsed))
    {
      return parsed.ToNormalizedString();
    }

    return null;
  }

  public int CompareVersions(string version1, string version2)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(version1);
    ArgumentException.ThrowIfNullOrWhiteSpace(version2);

    if (!NuGetVersion.TryParse(version1, out NuGetVersion? v1))
    {
      throw new ArgumentException($"Invalid version format: {version1}", nameof(version1));
    }

    if (!NuGetVersion.TryParse(version2, out NuGetVersion? v2))
    {
      throw new ArgumentException($"Invalid version format: {version2}", nameof(version2));
    }

    return v1.CompareTo(v2);
  }

  public string GetUpdateType(string currentVersion, string latestVersion)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(currentVersion);
    ArgumentException.ThrowIfNullOrWhiteSpace(latestVersion);

    if (!NuGetVersion.TryParse(currentVersion, out NuGetVersion? current))
    {
      throw new ArgumentException($"Invalid version format: {currentVersion}", nameof(currentVersion));
    }

    if (!NuGetVersion.TryParse(latestVersion, out NuGetVersion? latest))
    {
      throw new ArgumentException($"Invalid version format: {latestVersion}", nameof(latestVersion));
    }

    if (current.CompareTo(latest) >= 0)
    {
      return "none";
    }

    if (current.IsPrerelease && !latest.IsPrerelease &&
        current.Major == latest.Major &&
        current.Minor == latest.Minor &&
        current.Patch == latest.Patch)
    {
      return "stable";
    }

    if (latest.Major > current.Major) return "major";
    if (latest.Minor > current.Minor) return "minor";
    return "patch";
  }
}