namespace TimeWarp.Amuru;

/// <summary>
/// Git operations - GetWorktreePath implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Finds the worktree path where the specified branch is checked out.
  /// Parses the output of 'git worktree list --porcelain' to locate the branch's worktree.
  /// </summary>
  /// <param name="branchName">The branch name to find (defaults to "master").</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>The absolute path to the branch's worktree, or null if not found.</returns>
  /// <example>
  /// string? masterPath = await Git.GetWorktreePathAsync("master");
  /// if (masterPath != null)
  /// {
  ///   Console.WriteLine($"Master worktree: {masterPath}");
  /// }
  ///
  /// string? featurePath = await Git.GetWorktreePathAsync("feature-branch");
  /// </example>
  public static async Task<string?> GetWorktreePathAsync(string branchName = "master", CancellationToken cancellationToken = default)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("worktree", "list", "--porcelain")
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    if (!result.Success)
    {
      return null;
    }

    string? currentWorktreePath = null;
    string targetBranch = $"branch refs/heads/{branchName}";

    foreach (string line in result.GetLines())
    {
      if (line.StartsWith("worktree ", StringComparison.Ordinal))
      {
        currentWorktreePath = line.Substring("worktree ".Length);
      }
      else if (line == targetBranch && currentWorktreePath != null)
      {
        return currentWorktreePath;
      }
    }

    return null;
  }

  /// <summary>
  /// Finds the worktree path where the master branch is checked out.
  /// Convenience method that calls GetWorktreePathAsync with "master".
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>The absolute path to the master worktree, or null if not found.</returns>
  public static Task<string?> GetMasterWorktreePathAsync(CancellationToken cancellationToken = default)
    => GetWorktreePathAsync("master", cancellationToken);
}
