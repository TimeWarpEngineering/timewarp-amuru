#region Purpose
// Configures origin's fetch refspec and returns the git failure text.
#endregion

#region Design
// Bare clones need `+refs/heads/*:refs/remotes/origin/*` before worktree operations can see every branch.
// The result record keeps git's message when `git config` fails.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Represents the result of configuring the origin fetch refspec.
/// </summary>
/// <param name="Success">True if configuration succeeded, false otherwise.</param>
/// <param name="ErrorMessage">Error message if failed (null if succeeded).</param>
public record GitConfigureFetchRefspecResult(bool Success, string? ErrorMessage);

/// <summary>
/// Git operations - FetchRefspec configuration implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Configures the fetch refspec to fetch all branches from the remote.
  /// This is typically needed after cloning a bare repository to ensure
  /// all remote branches are available for worktree operations.
  /// </summary>
  /// <param name="repositoryPath">The path to the bare repository.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitConfigureFetchRefspecResult containing success status and any error message.</returns>
  /// <example>
  /// GitConfigureFetchRefspecResult configured = await Git.ConfigureFetchRefspecAsync("/path/to/repo.git");
  /// if (configured.Success)
  /// {
  ///   Console.WriteLine("Fetch refspec configured successfully");
  /// }
  /// </example>
  public static async Task<GitConfigureFetchRefspecResult> ConfigureFetchRefspecAsync(
    string repositoryPath,
    CancellationToken cancellationToken = default)
  {
    CommandOutput result = await GitBuilder(
        repositoryPath,
        "config",
        "remote.origin.fetch",
        "+refs/heads/*:refs/remotes/origin/*")
      .CaptureAsync(cancellationToken)
      .ConfigureAwait(false);

    if (result.Success)
    {
      return new GitConfigureFetchRefspecResult(true, null);
    }

    return new GitConfigureFetchRefspecResult(false, ErrorTextOr(result, "Failed to configure fetch refspec"));
  }
}
