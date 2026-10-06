#region Purpose
// Fetches a remote and returns the git failure text when the fetch does not succeed.
#endregion

#region Design
// FetchAsync returns GitFetchResult instead of bool so a failed fetch keeps stderr.
// The remote defaults to origin. The repository path is required, matching the other
// repository-scoped git operations.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Represents the result of fetching from a git remote.
/// </summary>
/// <param name="Success">True if the fetch succeeded, false otherwise.</param>
/// <param name="ErrorMessage">Error message if failed (null if succeeded).</param>
public record GitFetchResult(bool Success, string? ErrorMessage);

/// <summary>
/// Git operations - Fetch implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Fetches updates from a remote repository.
  /// </summary>
  /// <param name="repositoryPath">The path to the repository.</param>
  /// <param name="remote">The remote name to fetch from (defaults to "origin").</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitFetchResult containing success status and any error message.</returns>
  /// <example>
  /// GitFetchResult fetched = await Git.FetchAsync("/path/to/repo.git");
  /// if (fetched.Success)
  /// {
  ///   Console.WriteLine("Fetched updates from origin");
  /// }
  /// </example>
  public static async Task<GitFetchResult> FetchAsync(
    string repositoryPath,
    string remote = "origin",
    CancellationToken cancellationToken = default)
  {
    CommandOutput result = await GitBuilder(repositoryPath, "fetch", remote)
      .CaptureAsync(cancellationToken)
      .ConfigureAwait(false);

    if (result.Success)
    {
      return new GitFetchResult(true, null);
    }

    return new GitFetchResult(false, ErrorTextOr(result, "git fetch failed"));
  }
}
