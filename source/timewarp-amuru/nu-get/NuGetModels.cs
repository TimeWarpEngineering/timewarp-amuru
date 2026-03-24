#region Purpose
// Data models for NuGet package operations
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Represents a NuGet package version from search results.
/// </summary>
public sealed record NuGetPackageVersion
(
  string Version,
  string? Description = null
);

/// <summary>
/// Represents the result of a NuGet package search.
/// </summary>
public sealed record NuGetSearchResult
(
  string PackageId,
  IReadOnlyList<NuGetPackageVersion> Versions
);

/// <summary>
/// Contains the latest stable and prerelease versions for a package.
/// </summary>
/// <param name="StableVersion">The latest stable version, or null if none exists.</param>
/// <param name="PrereleaseVersion">The latest prerelease version, or null if none exists.</param>
public sealed record PackageVersionInfo
(
  string? StableVersion,
  string? PrereleaseVersion
);
