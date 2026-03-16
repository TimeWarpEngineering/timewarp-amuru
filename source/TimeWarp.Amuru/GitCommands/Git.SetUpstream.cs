#region Purpose
// TODO: Add purpose description
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Represents the result of setting upstream tracking for a branch.
/// </summary>
/// <param name="Success">True if upstream was set successfully, false otherwise.</param>
/// <param name="ErrorMessage">Error message if failed (null if succeeded).</param>
public record GitSetUpstreamResult(bool Success, string? ErrorMessage);

/// <summary>
/// Git operations - SetUpstream implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Sets the upstream tracking for a branch.
  /// This enables 'git pull' to work without specifying the remote branch.
  /// </summary>
  /// <param name="worktreePath">The path to the worktree or repository.</param>
  /// <param name="branchName">The branch name to set tracking for.</param>
  /// <param name="remote">The remote name (defaults to "origin").</param>
  /// <param name="fetchFirst">If true, fetches from remote before setting upstream (ensures remote branch ref exists).</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitSetUpstreamResult containing success status and any error message.</returns>
  /// <example>
  /// GitSetUpstreamResult result = await Git.SetUpstreamAsync("/path/to/worktree", "feature-branch");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine("Upstream tracking set successfully");
  /// }
  ///
  /// // Fetch first to ensure remote branch exists
  /// GitSetUpstreamResult fetchResult = await Git.SetUpstreamAsync(
  ///   "/path/to/worktree",
  ///   "feature-branch",
  ///   fetchFirst: true);
  /// </example>
  public static async Task<GitSetUpstreamResult> SetUpstreamAsync(
    string worktreePath,
    string branchName,
    string remote = "origin",
    bool fetchFirst = false,
    CancellationToken cancellationToken = default)
  {
    if (fetchFirst)
    {
      bool fetchResult = await FetchAsync(worktreePath, remote, cancellationToken);
      if (!fetchResult)
      {
        return new GitSetUpstreamResult(false, $"Failed to fetch from {remote} before setting upstream");
      }
    }

    CommandOutput result = await Shell.Builder("git")
      .WithArguments("branch", "--set-upstream-to", $"{remote}/{branchName}", branchName)
      .WithWorkingDirectory(worktreePath)
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    if (result.Success)
    {
      return new GitSetUpstreamResult(true, null);
    }

    string errorMessage = !string.IsNullOrWhiteSpace(result.Stderr)
      ? result.Stderr.Trim()
      : "Failed to set upstream";

    return new GitSetUpstreamResult(false, errorMessage);
  }
}
