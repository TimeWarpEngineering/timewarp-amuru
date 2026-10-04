#region Purpose
// Lists worktrees and keeps a failed git invocation distinct from an empty list.
#endregion

#region Design
// Success with an empty Porcelain string means git printed no worktrees.
// Failure sets Success false, Porcelain null, and ErrorMessage from git.
// Callers parse Porcelain only after Success is true.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Represents a git worktree entry.
/// </summary>
/// <param name="Path">The file system path to the worktree.</param>
/// <param name="HeadCommit">The HEAD commit hash (null if not available).</param>
/// <param name="BranchRef">The branch reference (null if detached HEAD).</param>
/// <param name="IsBare">True if this is a bare repository.</param>
public record WorktreeEntry(string Path, string? HeadCommit, string? BranchRef, bool IsBare);

/// <summary>
/// Represents the result of listing worktrees in porcelain format.
/// </summary>
/// <param name="Success">True when git produced a listing, false when the command failed.</param>
/// <param name="Porcelain">Porcelain stdout when Success is true (may be empty). Null when Success is false.</param>
/// <param name="ErrorMessage">Error message when the command failed (null when Success is true).</param>
public record GitWorktreeListResult(bool Success, string? Porcelain, string? ErrorMessage);

/// <summary>
/// Git operations - WorktreeList implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Parses the porcelain output from "git worktree list --porcelain" into a list of WorktreeEntry objects.
  /// </summary>
  /// <param name="porcelainOutput">The raw porcelain output from git.</param>
  /// <returns>A read-only list of parsed WorktreeEntry objects.</returns>
  public static IReadOnlyList<WorktreeEntry> ParseWorktreeList(string porcelainOutput) =>
    WorktreePorcelainParser.ParseWorktreeList(porcelainOutput);

  /// <summary>
  /// Lists all worktrees in a repository using porcelain output format.
  /// Returns the raw porcelain output which can be parsed using <see cref="ParseWorktreeList"/>.
  /// </summary>
  /// <param name="repositoryPath">The path to the repository.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitWorktreeListResult with porcelain stdout on success, or an error message on failure.</returns>
  /// <example>
  /// GitWorktreeListResult listed = await Git.WorktreeListPorcelainAsync("/path/to/repo.git");
  /// if (listed.Success)
  /// {
  ///   IReadOnlyList&lt;WorktreeEntry&gt; worktrees = Git.ParseWorktreeList(listed.Porcelain ?? "");
  /// }
  /// </example>
  public static async Task<GitWorktreeListResult> WorktreeListPorcelainAsync(
    string repositoryPath,
    CancellationToken cancellationToken = default)
  {
    CommandOutput result = await GitBuilder(repositoryPath, "worktree", "list", "--porcelain")
      .CaptureAsync(cancellationToken)
      .ConfigureAwait(false);

    if (!result.Success)
    {
      return new GitWorktreeListResult(false, null, ErrorTextOr(result, "Failed to list worktrees"));
    }

    return new GitWorktreeListResult(true, result.Stdout, null);
  }
}
