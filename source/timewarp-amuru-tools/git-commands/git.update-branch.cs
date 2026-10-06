#region Purpose
// Updates a local branch from origin, including the branch that is checked out.
#endregion

#region Design
// `git fetch origin <branch>:<branch>` refuses to update a branch that is checked out in any work tree.
// GetWorktreePathAsync (porcelain worktree list) finds the work tree holding the branch, whether it is the
// main work tree of repositoryPath, a different linked worktree, or none.
// A bare repository is listed as `bare` with no branch line, so its HEAD branch is not treated as checked out.
// Checked out: `git -C <worktreePath> -c pull.rebase=false pull --ff-only origin <branch>` (PullFastForwardAsync),
// so the ref and the work tree move together. pull.rebase is forced off so a machine-level rebase setting
// cannot rewrite commits; a non-fast-forward still fails, matching the refspec update.
// Not checked out: the refspec fetch in repositoryPath, which updates the ref without touching any work tree.
// repositoryPath selects the repository; null uses the process working directory.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Represents the result of a git branch update operation.
/// </summary>
/// <param name="Success">True if the update succeeded, false otherwise.</param>
/// <param name="BranchPath">The path of the work tree where the branch is checked out (null when it is not checked out and the ref was updated by fetch).</param>
/// <param name="ErrorMessage">Error message if failed (null if succeeded).</param>
public record GitBranchUpdateResult(bool Success, string? BranchPath, string? ErrorMessage);

/// <summary>
/// Git operations - UpdateBranch implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Updates <paramref name="branchName"/> from origin in the process working directory.
  /// </summary>
  /// <param name="branchName">The branch name to update.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitBranchUpdateResult containing success status, branch path, and any error message.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="branchName"/> is null or whitespace.</exception>
  public static Task<GitBranchUpdateResult> UpdateBranchAsync(
    string branchName,
    CancellationToken cancellationToken = default)
    => UpdateBranchAsync(branchName, repositoryPath: null, cancellationToken);

  /// <summary>
  /// Updates a branch from origin, handling worktree, regular and bare repository configurations.
  /// A branch checked out in any work tree (main or linked) is updated with <c>git pull --ff-only</c> in that
  /// work tree. Any other branch is updated with <c>git fetch origin &lt;branch&gt;:&lt;branch&gt;</c>.
  /// </summary>
  /// <param name="branchName">The branch name to update.</param>
  /// <param name="repositoryPath">Repository to update. Null uses the process working directory.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitBranchUpdateResult containing success status, branch path, and any error message.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="branchName"/> is null or whitespace.</exception>
  /// <example>
  /// GitBranchUpdateResult result = await Git.UpdateBranchAsync("main", "/path/to/repo");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine(result.BranchPath != null
  ///     ? $"Updated main at: {result.BranchPath}"
  ///     : "Updated main");
  /// }
  ///
  /// GitBranchUpdateResult featureResult = await Git.UpdateBranchAsync("feature-branch", "/path/to/repo");
  /// </example>
  public static async Task<GitBranchUpdateResult> UpdateBranchAsync(
    string branchName,
    string? repositoryPath,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(branchName);

    string? worktreePath = await GetWorktreePathAsync(branchName, repositoryPath, cancellationToken)
      .ConfigureAwait(false);

    if (worktreePath is not null)
    {
      CommandOutput pullResult = await PullFastForwardAsync(worktreePath, branchName, cancellationToken)
        .ConfigureAwait(false);

      return pullResult.Success
        ? new GitBranchUpdateResult(true, worktreePath, null)
        : new GitBranchUpdateResult(false, worktreePath, ErrorTextOr(pullResult, "Failed to update branch"));
    }

    CommandOutput fetchResult = await GitBuilder(
        repositoryPath,
        "fetch",
        "origin",
        $"{branchName}:{branchName}")
      .CaptureAsync(cancellationToken)
      .ConfigureAwait(false);

    return FromBranchCommand(fetchResult);
  }

  /// <summary>
  /// Updates the default branch from origin in the process working directory.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitBranchUpdateResult containing success status, branch path, and any error message.</returns>
  public static Task<GitBranchUpdateResult> UpdateDefaultBranchAsync(CancellationToken cancellationToken = default)
    => UpdateDefaultBranchAsync(repositoryPath: null, cancellationToken);

  /// <summary>
  /// Updates the default branch (main/master/dev) from origin.
  /// Auto-detects the default branch using GetDefaultBranchAsync, then updates it.
  /// </summary>
  /// <param name="repositoryPath">Repository to update. Null uses the process working directory.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitBranchUpdateResult containing success status, branch path, and any error message.</returns>
  /// <example>
  /// GitBranchUpdateResult result = await Git.UpdateDefaultBranchAsync("/path/to/repo");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine(result.BranchPath != null
  ///     ? $"Updated default branch at: {result.BranchPath}"
  ///     : "Updated default branch");
  /// }
  /// </example>
  public static async Task<GitBranchUpdateResult> UpdateDefaultBranchAsync(
    string? repositoryPath,
    CancellationToken cancellationToken = default)
  {
    GitDefaultBranchResult defaultBranchResult = await GetDefaultBranchAsync(repositoryPath, cancellationToken)
      .ConfigureAwait(false);
    if (!defaultBranchResult.Success || defaultBranchResult.BranchName is null)
    {
      return new GitBranchUpdateResult(false, null, defaultBranchResult.ErrorMessage ?? "Failed to detect default branch");
    }

    return await UpdateBranchAsync(defaultBranchResult.BranchName, repositoryPath, cancellationToken)
      .ConfigureAwait(false);
  }

  private static GitBranchUpdateResult FromBranchCommand(CommandOutput result)
  {
    if (result.Success)
    {
      return new GitBranchUpdateResult(true, null, null);
    }

    return new GitBranchUpdateResult(false, null, ErrorTextOr(result, "Failed to update branch"));
  }

  private static Task<CommandOutput> PullFastForwardAsync(
    string worktreePath,
    string branchName,
    CancellationToken cancellationToken)
    => GitBuilder(
        null,
        "-C",
        worktreePath,
        "-c",
        "pull.rebase=false",
        "pull",
        "--ff-only",
        "origin",
        branchName)
      .CaptureAsync(cancellationToken);
}
