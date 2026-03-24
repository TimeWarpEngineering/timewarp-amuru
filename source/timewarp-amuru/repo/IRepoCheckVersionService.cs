#region Purpose
// Interface for repository version checking operations
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Result of a version check operation.
/// </summary>
public sealed record CheckVersionResult
(
  bool IsNewVersion,
  string Version,
  string Strategy,
  string? LatestReleaseTag,
  string? LatestNuGetVersion,
  IReadOnlyList<string>? CheckedPackages,
  IReadOnlyList<string>? AlreadyPublishedPackages
);

/// <summary>
/// Provides operations for checking if a new version can be published.
/// </summary>
public interface IRepoCheckVersionService
{
  /// <summary>
  /// Checks if the current version is new and can be published.
  /// </summary>
  /// <param name="strategy">The version check strategy (git-tag or nuget-search)</param>
  /// <param name="package">Specific package to check (for nuget-search strategy)</param>
  /// <param name="tag">Specific tag to check (for git-tag strategy)</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The result of the version check</returns>
  Task<CheckVersionResult> CheckAsync
  (
    string? strategy = null,
    string? package = null,
    string? tag = null,
    CancellationToken cancellationToken = default
  );
}
