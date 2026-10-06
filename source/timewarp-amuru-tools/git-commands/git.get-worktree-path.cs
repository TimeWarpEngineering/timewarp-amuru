#region Purpose
// Finds the worktree directory where a branch is checked out.
#endregion

#region Design
// The branch name is required. GetDefaultWorktreePathAsync resolves the default branch
// and then calls GetWorktreePathAsync, so no method hardcodes "master".
// repositoryPath selects which repository's worktree list is read. Null uses process CWD.
// A failed `git worktree list` returns null, the same as a branch that is not checked out.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Git operations - GetWorktreePath implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Finds the worktree path where <paramref name="branchName"/> is checked out,
  /// using the process working directory as the repository.
  /// </summary>
  /// <param name="branchName">The branch name to find.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>The absolute path to the branch's worktree, or null if not found.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="branchName"/> is null or whitespace.</exception>
  public static Task<string?> GetWorktreePathAsync(
    string branchName,
    CancellationToken cancellationToken = default)
    => GetWorktreePathAsync(branchName, repositoryPath: null, cancellationToken);

  /// <summary>
  /// Finds the worktree path where the specified branch is checked out.
  /// Parses the output of <c>git worktree list --porcelain</c>.
  /// </summary>
  /// <param name="branchName">The branch name to find.</param>
  /// <param name="repositoryPath">Repository whose worktree list is read. Null uses the process working directory.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>The absolute path to the branch's worktree, or null if not found.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="branchName"/> is null or whitespace.</exception>
  /// <example>
  /// string? mainPath = await Git.GetWorktreePathAsync("main", "/path/to/repo");
  /// string? featurePath = await Git.GetWorktreePathAsync("feature-branch", "/path/to/repo");
  /// </example>
  public static async Task<string?> GetWorktreePathAsync(
    string branchName,
    string? repositoryPath,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(branchName);

    CommandOutput result = await GitBuilder(repositoryPath, "worktree", "list", "--porcelain")
      .CaptureAsync(cancellationToken)
      .ConfigureAwait(false);

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
        currentWorktreePath = line["worktree ".Length..];
      }
      else if (line == targetBranch && currentWorktreePath != null)
      {
        return currentWorktreePath;
      }
    }

    return null;
  }

  /// <summary>
  /// Finds the worktree path where the default branch is checked out,
  /// using the process working directory as the repository.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>The absolute path to the default branch worktree, or null if not found.</returns>
  public static Task<string?> GetDefaultWorktreePathAsync(CancellationToken cancellationToken = default)
    => GetDefaultWorktreePathAsync(repositoryPath: null, cancellationToken);

  /// <summary>
  /// Finds the worktree path where the default branch is checked out.
  /// Detects the default branch, then locates its worktree.
  /// </summary>
  /// <param name="repositoryPath">Repository to inspect. Null uses the process working directory.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>The absolute path to the default branch worktree, or null if not found.</returns>
  public static async Task<string?> GetDefaultWorktreePathAsync(
    string? repositoryPath,
    CancellationToken cancellationToken = default)
  {
    GitDefaultBranchResult defaultBranch = await GetDefaultBranchAsync(repositoryPath, cancellationToken)
      .ConfigureAwait(false);
    if (!defaultBranch.Success || defaultBranch.BranchName is null)
    {
      return null;
    }

    return await GetWorktreePathAsync(defaultBranch.BranchName, repositoryPath, cancellationToken)
      .ConfigureAwait(false);
  }
}
