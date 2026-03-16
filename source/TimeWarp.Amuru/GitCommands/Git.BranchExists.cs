namespace TimeWarp.Amuru;

/// <summary>
/// Git operations - BranchExistsAsync implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Checks if a branch exists in the repository.
  /// Uses 'git show-ref --verify refs/heads/{branch}' to verify branch existence.
  /// </summary>
  /// <param name="repoPath">The path to the repository.</param>
  /// <param name="branchName">The branch name to check.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>True if the branch exists, false otherwise.</returns>
  public static async Task<bool> BranchExistsAsync(
    string repoPath,
    string branchName,
    CancellationToken cancellationToken = default)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("show-ref", "--verify", $"refs/heads/{branchName}")
      .WithWorkingDirectory(repoPath)
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    return result.Success;
  }
}
