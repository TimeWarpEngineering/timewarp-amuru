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
  /// Updates a branch in its worktree by pulling from origin.
  /// This is useful when working in other worktrees and needing to sync a branch without switching directories.
  /// </summary>
  /// <param name="branchName">The branch name to update (defaults to "master").</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitWorktreeUpdateResult containing success status, branch path, and any error message.</returns>
  /// <example>
  /// GitWorktreeUpdateResult result = await Git.UpdateWorktreeAsync("master");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine($"Updated master at: {result.BranchPath}");
  /// }
  ///
  /// // Update a feature branch
  /// GitWorktreeUpdateResult featureResult = await Git.UpdateWorktreeAsync("feature-branch");
  /// </example>
  public static async Task<GitWorktreeUpdateResult> UpdateWorktreeAsync(
    string branchName = "master",
    CancellationToken cancellationToken = default)
  {
    string? branchPath = await GetWorktreePathAsync(branchName, cancellationToken);

    if (branchPath == null)
    {
      return new GitWorktreeUpdateResult(
        false,
        null,
        $"{branchName} worktree not found. Ensure {branchName} branch is checked out in a worktree.");
    }

    CommandOutput result = await Shell.Builder("git")
      .WithArguments("-C", branchPath, "pull", "origin", branchName)
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    if (result.Success)
    {
      return new GitWorktreeUpdateResult(true, branchPath, null);
    }
    else
    {
      string errorMessage = string.IsNullOrWhiteSpace(result.Stderr)
        ? result.Stdout
        : result.Stderr;
      return new GitWorktreeUpdateResult(false, branchPath, errorMessage.Trim());
    }
  }

  /// <summary>
  /// Updates the master branch in its worktree by pulling from origin.
  /// Convenience method that calls UpdateWorktreeAsync with "master".
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitWorktreeUpdateResult containing success status, branch path, and any error message.</returns>
  public static Task<GitWorktreeUpdateResult> UpdateMasterWorktreeAsync(CancellationToken cancellationToken = default)
    => UpdateWorktreeAsync("master", cancellationToken);
}
