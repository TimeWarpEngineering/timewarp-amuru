namespace TimeWarp.Amuru;

/// <summary>
/// Represents the result of a git branch update operation.
/// </summary>
/// <param name="Success">True if the update succeeded, false otherwise.</param>
/// <param name="BranchPath">The path to the branch's worktree (null if not using worktrees).</param>
/// <param name="ErrorMessage">Error message if failed (null if succeeded).</param>
public record GitBranchUpdateResult(bool Success, string? BranchPath, string? ErrorMessage);

/// <summary>
/// Git operations - UpdateBranch implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Updates a branch from origin, handling both worktree and regular repository configurations.
  /// If the branch is in a worktree, updates it there. Otherwise, uses fetch to update the local branch ref.
  /// </summary>
  /// <param name="branchName">The branch name to update (defaults to "master").</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitBranchUpdateResult containing success status, branch path, and any error message.</returns>
  /// <example>
  /// GitBranchUpdateResult result = await Git.UpdateBranchAsync("master");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine(result.BranchPath != null
  ///     ? $"Updated master at: {result.BranchPath}"
  ///     : "Updated master");
  /// }
  ///
  /// // Update a feature branch
  /// GitBranchUpdateResult featureResult = await Git.UpdateBranchAsync("feature-branch");
  /// </example>
  public static async Task<GitBranchUpdateResult> UpdateBranchAsync(
    string branchName = "master",
    CancellationToken cancellationToken = default)
  {
    // Check if using worktrees
    if (IsWorktree())
    {
      // Using worktrees - delegate to worktree-specific method
      GitWorktreeUpdateResult worktreeResult = await UpdateWorktreeAsync(branchName, cancellationToken);
      return new GitBranchUpdateResult(worktreeResult.Success, worktreeResult.BranchPath, worktreeResult.ErrorMessage);
    }
    else
    {
      // Not using worktrees - use fetch to update branch ref
      CommandOutput result = await Shell.Builder("git")
        .WithArguments("fetch", "origin", $"{branchName}:{branchName}")
        .WithNoValidation()
        .CaptureAsync(cancellationToken);

      if (result.Success)
      {
        return new GitBranchUpdateResult(true, null, null);
      }
      else
      {
        string errorMessage = string.IsNullOrWhiteSpace(result.Stderr)
          ? result.Stdout
          : result.Stderr;
        return new GitBranchUpdateResult(false, null, errorMessage.Trim());
      }
    }
  }

  /// <summary>
  /// Updates the master branch from origin.
  /// Convenience method that calls UpdateBranchAsync with "master".
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitBranchUpdateResult containing success status, branch path, and any error message.</returns>
  public static Task<GitBranchUpdateResult> UpdateMasterAsync(CancellationToken cancellationToken = default)
    => UpdateBranchAsync("master", cancellationToken);
}
