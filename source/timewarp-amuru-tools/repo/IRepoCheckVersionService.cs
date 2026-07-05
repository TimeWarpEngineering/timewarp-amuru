#region Purpose
// Interface for repository version checking operations
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Result of a git tag version check operation.
/// </summary>
public sealed record GitTagCheckResult
(
  bool IsNewVersion,
  string Version,
  string? LatestReleaseTag
);

/// <summary>
/// Result of a NuGet version check operation.
/// </summary>
public sealed record NuGetCheckResult
(
  bool IsNewVersion,
  string Version,
  string? LatestNuGetVersion,
  IReadOnlyList<string> CheckedPackages,
  IReadOnlyList<string>? AlreadyPublishedPackages
);

/// <summary>
/// Provides operations for checking if a new version can be published.
/// </summary>
public interface IRepoCheckVersionService
{
  /// <summary>
  /// Checks if the current version is new compared to git tags.
  /// </summary>
  /// <param name="tag">Specific tag to check (optional, uses GITHUB_REF_NAME or latest git tag if not provided)</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The result of the git tag version check</returns>
  Task<GitTagCheckResult> CheckGitTagVersionAsync(string? tag = null, CancellationToken cancellationToken = default);

  /// <summary>
  /// Checks if the current version is new compared to NuGet packages.
  /// </summary>
  /// <param name="packages">List of packages to check</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The result of the NuGet version check</returns>
  Task<NuGetCheckResult> CheckNuGetVersionAsync(IReadOnlyList<string> packages, CancellationToken cancellationToken = default);
}
