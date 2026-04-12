#region Purpose
// Interface for NuGet package operations
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Provides operations for querying NuGet package information.
/// </summary>
public interface INuGetPackageService
{
  /// <summary>
  /// Searches for a package on NuGet and returns all available versions.
  /// </summary>
  /// <param name="packageId">The package ID to search for</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The search result with package versions, or null if not found</returns>
  Task<NuGetSearchResult?> SearchAsync(string packageId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the latest stable and prerelease versions for a package.
  /// </summary>
  /// <param name="packageId">The package ID to search for</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Version info with latest stable and prerelease versions, or null if not found</returns>
  Task<PackageVersionInfo?> GetLatestVersionsAsync(string packageId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Parses and normalizes a version string.
  /// </summary>
  /// <param name="version">The version string to parse (may include leading 'v')</param>
  /// <returns>The normalized version string, or null if invalid</returns>
  string? ParseVersion(string version);

  /// <summary>
  /// Compares two version strings.
  /// </summary>
  /// <param name="version1">The first version to compare</param>
  /// <param name="version2">The second version to compare</param>
  /// <returns>Less than 0 if version1 &lt; version2, 0 if equal, greater than 0 if version1 &gt; version2</returns>
  /// <exception cref="ArgumentException">Thrown when either version string is invalid</exception>
  int CompareVersions(string version1, string version2);

  /// <summary>
  /// Determines the type of update between two versions.
  /// </summary>
  /// <param name="currentVersion">The current version</param>
  /// <param name="latestVersion">The latest available version</param>
  /// <returns>One of: "major", "minor", "patch", "stable", or "none"</returns>
  /// <exception cref="ArgumentException">Thrown when either version string is invalid</exception>
  string GetUpdateType(string currentVersion, string latestVersion);
}
