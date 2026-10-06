#region Purpose
// Counts commits the checked-out branch is ahead of another branch.
#endregion

#region Design
// `git rev-list --count branch..HEAD` is the count. The branch argument still defaults to
// master on the working-directory overload; GetCommitsAheadOfDefaultBranchAsync is the
// entry point that resolves the default branch first.
// repositoryPath selects the repository on the three-argument overload.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Represents the result of getting the commit count between branches.
/// </summary>
/// <param name="Success">True if the count succeeded, false otherwise.</param>
/// <param name="Count">The number of commits ahead (0 if failed or equal).</param>
/// <param name="ErrorMessage">Error message if failed (null if succeeded).</param>
public record GitCommitCountResult(bool Success, int Count, string? ErrorMessage);

/// <summary>
/// Git operations - GetCommitsAhead implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Gets the number of commits the current branch is ahead of <paramref name="branchName"/>
  /// in the process working directory.
  /// </summary>
  /// <param name="branchName">The branch to compare against (defaults to "master").</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitCommitCountResult containing success status, commit count, and any error message.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="branchName"/> is null or whitespace.</exception>
  public static Task<GitCommitCountResult> GetCommitsAheadAsync(
    string branchName = "master",
    CancellationToken cancellationToken = default)
    => GetCommitsAheadAsync(branchName, repositoryPath: null, cancellationToken);

  /// <summary>
  /// Gets the number of commits the current branch is ahead of the specified branch.
  /// Uses <c>git rev-list --count branch..HEAD</c>.
  /// </summary>
  /// <param name="branchName">The branch to compare against.</param>
  /// <param name="repositoryPath">Repository to inspect. Null uses the process working directory.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitCommitCountResult containing success status, commit count, and any error message.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="branchName"/> is null or whitespace.</exception>
  /// <example>
  /// GitCommitCountResult result = await Git.GetCommitsAheadAsync("main", "/path/to/repo");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine($"Commits ahead of main: {result.Count}");
  /// }
  /// </example>
  public static async Task<GitCommitCountResult> GetCommitsAheadAsync(
    string branchName,
    string? repositoryPath,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(branchName);

    CommandOutput result = await GitBuilder(repositoryPath, "rev-list", "--count", $"{branchName}..HEAD")
      .CaptureAsync(cancellationToken)
      .ConfigureAwait(false);

    if (!result.Success)
    {
      return new GitCommitCountResult(false, 0, ErrorTextOr(result, "Failed to count commits"));
    }

    string output = result.Stdout.Trim();
    if (int.TryParse(output, out int count))
    {
      return new GitCommitCountResult(true, count, null);
    }

    return new GitCommitCountResult(false, 0, $"Failed to parse commit count: '{output}'");
  }

  /// <summary>
  /// Gets the number of commits the current branch is ahead of the default branch
  /// in the process working directory.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitCommitCountResult containing success status, commit count, and any error message.</returns>
  public static Task<GitCommitCountResult> GetCommitsAheadOfDefaultBranchAsync(
    CancellationToken cancellationToken = default)
    => GetCommitsAheadOfDefaultBranchAsync(repositoryPath: null, cancellationToken);

  /// <summary>
  /// Gets the number of commits the current branch is ahead of the default branch (main/master/dev).
  /// Auto-detects the default branch using GetDefaultBranchAsync, then counts commits.
  /// </summary>
  /// <param name="repositoryPath">Repository to inspect. Null uses the process working directory.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitCommitCountResult containing success status, commit count, and any error message.</returns>
  /// <example>
  /// GitCommitCountResult result = await Git.GetCommitsAheadOfDefaultBranchAsync("/path/to/repo");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine($"Commits ahead of default branch: {result.Count}");
  /// }
  /// </example>
  public static async Task<GitCommitCountResult> GetCommitsAheadOfDefaultBranchAsync(
    string? repositoryPath,
    CancellationToken cancellationToken = default)
  {
    GitDefaultBranchResult defaultBranchResult = await GetDefaultBranchAsync(repositoryPath, cancellationToken)
      .ConfigureAwait(false);
    if (!defaultBranchResult.Success || defaultBranchResult.BranchName is null)
    {
      return new GitCommitCountResult(false, 0, defaultBranchResult.ErrorMessage ?? "Failed to detect default branch");
    }

    return await GetCommitsAheadAsync(defaultBranchResult.BranchName, repositoryPath, cancellationToken)
      .ConfigureAwait(false);
  }
}
