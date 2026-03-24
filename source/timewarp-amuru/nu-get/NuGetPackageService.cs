#region Purpose
// Implementation of NuGet package operations using dotnet package search
#endregion

namespace TimeWarp.Amuru;

using NuGet.Versioning;

/// <summary>
/// Implementation of NuGet package operations using the dotnet CLI.
/// </summary>
public sealed class NuGetPackageService : INuGetPackageService
{
  public async Task<NuGetSearchResult?> SearchAsync(string packageId, CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

    CommandOutput result = await Shell.Builder("dotnet")
      .WithArguments("package", "search", packageId, "--exact-match", "--format", "json")
      .CaptureAsync(cancellationToken);

    if (result.ExitCode != 0)
    {
      return null;
    }

    return ParseSearchResult(packageId, result.Stdout);
  }

  public async Task<PackageVersionInfo?> GetLatestVersionsAsync(string packageId, CancellationToken cancellationToken = default)
  {
    NuGetSearchResult? result = await SearchAsync(packageId, cancellationToken);

    if (result == null || result.Versions.Count == 0)
    {
      return null;
    }

    string? stableVersion = null;
    string? prereleaseVersion = null;

    foreach (NuGetPackageVersion pkgVersion in result.Versions)
    {
      if (!NuGetVersion.TryParse(pkgVersion.Version, out NuGetVersion? parsed))
      {
        continue;
      }

      if (!parsed.IsPrerelease)
      {
        if (stableVersion == null || CompareVersions(pkgVersion.Version, stableVersion) > 0)
        {
          stableVersion = pkgVersion.Version;
        }
      }
      else
      {
        if (prereleaseVersion == null || CompareVersions(pkgVersion.Version, prereleaseVersion) > 0)
        {
          prereleaseVersion = pkgVersion.Version;
        }
      }
    }

    return new PackageVersionInfo(stableVersion, prereleaseVersion);
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

  private static NuGetSearchResult? ParseSearchResult(string packageId, string jsonOutput)
  {
    using var document = JsonDocument.Parse(jsonOutput);
    JsonElement root = document.RootElement;

    if (!root.TryGetProperty("searchResult", out JsonElement searchResults))
    {
      return null;
    }

    foreach (JsonElement searchResult in searchResults.EnumerateArray())
    {
      if (!searchResult.TryGetProperty("packages", out JsonElement packages))
      {
        continue;
      }

      foreach (JsonElement package in packages.EnumerateArray())
      {
        if (!package.TryGetProperty("id", out JsonElement idElement))
        {
          continue;
        }

        string? id = idElement.GetString();
        if (id == null || !string.Equals(id, packageId, StringComparison.OrdinalIgnoreCase))
        {
          continue;
        }

        if (!package.TryGetProperty("version", out JsonElement versionElement))
        {
          continue;
        }

        string? latestVersion = versionElement.GetString();
        if (latestVersion == null)
        {
          continue;
        }

        List<NuGetPackageVersion> versions =
        [
          new NuGetPackageVersion(latestVersion)
        ];

        return new NuGetSearchResult(id, versions);
      }
    }

    return null;
  }
}
