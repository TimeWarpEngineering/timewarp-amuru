#region Purpose
// Sets origin/HEAD from the remote and returns the git failure text.
#endregion

#region Design
// `git remote set-head origin --auto` records which remote branch is the default.
// The result record keeps git's message when that command fails.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Represents the result of setting a remote HEAD automatically.
/// </summary>
/// <param name="Success">True if configuration succeeded, false otherwise.</param>
/// <param name="ErrorMessage">Error message if failed (null if succeeded).</param>
public record GitSetRemoteHeadResult(bool Success, string? ErrorMessage);

/// <summary>
/// Git operations - RemoteHead configuration implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Sets the remote HEAD to automatically determine the default branch.
  /// This configures the repository to automatically detect which branch
  /// is the default on the remote.
  /// </summary>
  /// <param name="repositoryPath">The path to the repository.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitSetRemoteHeadResult containing success status and any error message.</returns>
  /// <example>
  /// GitSetRemoteHeadResult configured = await Git.SetRemoteHeadAutoAsync("/path/to/repo.git");
  /// if (configured.Success)
  /// {
  ///   Console.WriteLine("Remote HEAD configured to auto-detect default branch");
  /// }
  /// </example>
  public static async Task<GitSetRemoteHeadResult> SetRemoteHeadAutoAsync(
    string repositoryPath,
    CancellationToken cancellationToken = default)
  {
    CommandOutput result = await GitBuilder(repositoryPath, "remote", "set-head", "origin", "--auto")
      .CaptureAsync(cancellationToken)
      .ConfigureAwait(false);

    if (result.Success)
    {
      return new GitSetRemoteHeadResult(true, null);
    }

    return new GitSetRemoteHeadResult(false, ErrorTextOr(result, "Failed to set remote HEAD"));
  }
}
