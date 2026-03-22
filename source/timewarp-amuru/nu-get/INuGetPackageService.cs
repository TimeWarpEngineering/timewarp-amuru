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
  /// Searches for a package on NuGet and returns the latest version.
  /// </summary>
  /// <param name="packageId">The package ID to search for</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The search result with package versions, or null if not found</returns>
  Task<NuGetSearchResult?> SearchAsync(string packageId, CancellationToken cancellationToken = default);
}
