#region Purpose
// Reports whether a local branch ref exists, separately from git failures.
#endregion

#region Design
// `git show-ref --verify --quiet` exits 0 when the ref exists and 1 when it does not.
// Without `--quiet`, git exits 128 ("not a valid ref") for a missing ref, so `--quiet` is required.
// Exit 1 is a completed check with Exists false, not an error.
// Any other non-zero exit (for example, not a repository) sets Success false and keeps the git message.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Represents the result of checking whether a local branch exists.
/// </summary>
/// <param name="Success">True when the check ran, false when git could not answer.</param>
/// <param name="Exists">True when the branch ref exists. False when it does not, or when Success is false.</param>
/// <param name="ErrorMessage">Error message when the check failed (null when Success is true).</param>
public record GitBranchExistsResult(bool Success, bool Exists, string? ErrorMessage);

/// <summary>
/// Git operations - BranchExistsAsync implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Checks if a branch exists in the repository.
  /// Uses <c>git show-ref --verify --quiet refs/heads/{branch}</c>.
  /// A missing ref is <see cref="GitBranchExistsResult.Exists"/> false with Success true.
  /// </summary>
  /// <param name="repoPath">The path to the repository.</param>
  /// <param name="branchName">The branch name to check.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitBranchExistsResult containing success status, existence, and any error message.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="branchName"/> is null or whitespace.</exception>
  public static async Task<GitBranchExistsResult> BranchExistsAsync(
    string repoPath,
    string branchName,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(branchName);

    CommandOutput result = await GitBuilder(repoPath, "show-ref", "--verify", "--quiet", $"refs/heads/{branchName}")
      .CaptureAsync(cancellationToken)
      .ConfigureAwait(false);

    if (result.Success)
    {
      return new GitBranchExistsResult(true, true, null);
    }

    if (result.ExitCode == 1)
    {
      return new GitBranchExistsResult(true, false, null);
    }

    return new GitBranchExistsResult(false, false, ErrorTextOr(result, "Failed to check branch"));
  }
}
