#region Purpose
// Data models for NuGet package operations
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Represents a NuGet package version from registration results.
/// </summary>
/// <param name="Version">Normalized version string.</param>
/// <param name="Description">Optional description.</param>
/// <param name="Listed">
/// False when the registration catalog entry is unlisted. Unlisted versions
/// remain on the feed and a republish of the same id and version is rejected.
/// </param>
public sealed record NuGetPackageVersion
(
  string Version,
  string? Description = null,
  bool Listed = true
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
