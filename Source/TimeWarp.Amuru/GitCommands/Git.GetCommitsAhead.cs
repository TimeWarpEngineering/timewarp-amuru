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
  /// Gets the number of commits the current branch is ahead of the specified branch.
  /// Uses 'git rev-list --count branch..HEAD' to count commits.
  /// </summary>
  /// <param name="branchName">The branch to compare against (defaults to "master").</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitCommitCountResult containing success status, commit count, and any error message.</returns>
  /// <example>
  /// GitCommitCountResult result = await Git.GetCommitsAheadAsync("master");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine($"Commits ahead of master: {result.Count}");
  /// }
  ///
  /// // Compare against a different branch
  /// GitCommitCountResult devResult = await Git.GetCommitsAheadAsync("develop");
  /// </example>
  public static async Task<GitCommitCountResult> GetCommitsAheadAsync(
    string branchName = "master",
    CancellationToken cancellationToken = default)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("rev-list", "--count", $"{branchName}..HEAD")
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    if (!result.Success)
    {
      string errorMessage = string.IsNullOrWhiteSpace(result.Stderr)
        ? result.Stdout
        : result.Stderr;
      return new GitCommitCountResult(false, 0, errorMessage.Trim());
    }

    string output = result.Stdout.Trim();
    if (int.TryParse(output, out int count))
    {
      return new GitCommitCountResult(true, count, null);
    }
    else
    {
      return new GitCommitCountResult(false, 0, $"Failed to parse commit count: '{output}'");
    }
  }

  /// <summary>
  /// Gets the number of commits the current branch is ahead of master.
  /// Convenience method that calls GetCommitsAheadAsync with "master".
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitCommitCountResult containing success status, commit count, and any error message.</returns>
  public static Task<GitCommitCountResult> GetCommitsAheadOfMasterAsync(CancellationToken cancellationToken = default)
    => GetCommitsAheadAsync("master", cancellationToken);
}
