#region Purpose
// Pulls a branch inside the worktree where that branch is checked out.
#endregion

#region Design
// The branch name is required. UpdateDefaultWorktreeAsync resolves the default branch first,
// so no method hardcodes "master".
// repositoryPath selects which repository's worktree list is searched. The pull itself runs
// in the worktree directory (`git -C`), because that directory holds the checked-out branch.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Represents the result of a git branch update operation in a worktree.
/// </summary>
/// <param name="Success">True if the update succeeded, false otherwise.</param>
/// <param name="BranchPath">The path to the branch's worktree (null if not found).</param>
/// <param name="ErrorMessage">Error message if failed (null if succeeded).</param>
public record GitWorktreeUpdateResult(bool Success, string? BranchPath, string? ErrorMessage);

/// <summary>
/// Git operations - UpdateWorktree implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Updates <paramref name="branchName"/> in its worktree, searching from the process working directory.
  /// </summary>
  /// <param name="branchName">The branch name to update.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitWorktreeUpdateResult containing success status, branch path, and any error message.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="branchName"/> is null or whitespace.</exception>
  public static Task<GitWorktreeUpdateResult> UpdateWorktreeAsync(
    string branchName,
    CancellationToken cancellationToken = default)
    => UpdateWorktreeAsync(branchName, repositoryPath: null, cancellationToken);

  /// <summary>
  /// Updates a branch in its worktree by pulling from origin.
  /// This is useful when working in other worktrees and needing to sync a branch without switching directories.
  /// </summary>
  /// <param name="branchName">The branch name to update.</param>
  /// <param name="repositoryPath">Repository whose worktree list is searched. Null uses the process working directory.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitWorktreeUpdateResult containing success status, branch path, and any error message.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="branchName"/> is null or whitespace.</exception>
  /// <example>
  /// GitWorktreeUpdateResult result = await Git.UpdateWorktreeAsync("main", "/path/to/repo");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine($"Updated main at: {result.BranchPath}");
  /// }
  /// </example>
  public static async Task<GitWorktreeUpdateResult> UpdateWorktreeAsync(
    string branchName,
    string? repositoryPath,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(branchName);

    string? branchPath = await GetWorktreePathAsync(branchName, repositoryPath, cancellationToken)
      .ConfigureAwait(false);

    if (branchPath == null)
    {
      return new GitWorktreeUpdateResult(
        false,
        null,
        $"{branchName} worktree not found. Ensure {branchName} branch is checked out in a worktree.");
    }

    CommandOutput result = await GitBuilder(null, "-C", branchPath, "pull", "origin", branchName)
      .CaptureAsync(cancellationToken)
      .ConfigureAwait(false);

    if (result.Success)
    {
      return new GitWorktreeUpdateResult(true, branchPath, null);
    }

    return new GitWorktreeUpdateResult(false, branchPath, ErrorTextOr(result, "Failed to update worktree"));
  }

  /// <summary>
  /// Updates the default branch in its worktree, searching from the process working directory.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitWorktreeUpdateResult containing success status, branch path, and any error message.</returns>
  public static Task<GitWorktreeUpdateResult> UpdateDefaultWorktreeAsync(CancellationToken cancellationToken = default)
    => UpdateDefaultWorktreeAsync(repositoryPath: null, cancellationToken);

  /// <summary>
  /// Updates the default branch in its worktree by pulling from origin.
  /// Detects the default branch, then updates that branch's worktree.
  /// </summary>
  /// <param name="repositoryPath">Repository whose worktree list is searched. Null uses the process working directory.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitWorktreeUpdateResult containing success status, branch path, and any error message.</returns>
  public static async Task<GitWorktreeUpdateResult> UpdateDefaultWorktreeAsync(
    string? repositoryPath,
    CancellationToken cancellationToken = default)
  {
    GitDefaultBranchResult defaultBranch = await GetDefaultBranchAsync(repositoryPath, cancellationToken)
      .ConfigureAwait(false);
    if (!defaultBranch.Success || defaultBranch.BranchName is null)
    {
      return new GitWorktreeUpdateResult(false, null, defaultBranch.ErrorMessage ?? "Failed to detect default branch");
    }

    return await UpdateWorktreeAsync(defaultBranch.BranchName, repositoryPath, cancellationToken)
      .ConfigureAwait(false);
  }
}
