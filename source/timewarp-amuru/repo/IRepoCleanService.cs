#region Purpose
// Interface for repository cleaning operations
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Result of a repository clean operation.
/// </summary>
public sealed record CleanResult
(
  int ObjDirectoriesDeleted,
  int BinDirectoriesDeleted,
  int RootBinFilesCleaned
);

/// <summary>
/// Provides operations for cleaning repository artifacts.
/// </summary>
public interface IRepoCleanService
{
  /// <summary>
  /// Cleans the repository by removing obj directories, bin directories,
  /// and selectively cleaning the root bin directory.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The result with counts of deleted items</returns>
  Task<CleanResult> CleanAsync(CancellationToken cancellationToken = default);
}
