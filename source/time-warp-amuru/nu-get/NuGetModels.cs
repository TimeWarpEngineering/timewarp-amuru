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
