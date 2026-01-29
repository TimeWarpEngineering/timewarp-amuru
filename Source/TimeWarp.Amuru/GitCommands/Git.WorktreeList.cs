namespace TimeWarp.Amuru;

/// <summary>
/// Represents a git worktree entry.
/// </summary>
/// <param name="Path">The file system path to the worktree.</param>
/// <param name="HeadCommit">The HEAD commit hash (null if not available).</param>
/// <param name="BranchRef">The branch reference (null if detached HEAD).</param>
/// <param name="IsBare">True if this is a bare repository.</param>
public record WorktreeEntry(string Path, string? HeadCommit, string? BranchRef, bool IsBare);

/// <summary>
/// Git operations - WorktreeList implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Lists all worktrees in a repository using porcelain output format.
  /// Returns the raw porcelain output which can be parsed using WorktreePorcelainParser.
  /// </summary>
  /// <param name="repositoryPath">The path to the repository.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>The raw porcelain output from git worktree list.</returns>
  /// <example>
  /// string porcelain = await Git.WorktreeListPorcelainAsync("/path/to/repo.git");
  /// List&lt;WorktreeEntry&gt; worktrees = WorktreePorcelainParser.ParseWorktreeList(porcelain);
  /// foreach (WorktreeEntry wt in worktrees)
  /// {
  ///   Console.WriteLine($"Worktree: {wt.Path} - Branch: {wt.BranchRef}");
  /// }
  /// </example>
  public static async Task<string> WorktreeListPorcelainAsync(string repositoryPath, CancellationToken cancellationToken = default)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("worktree", "list", "--porcelain")
      .WithWorkingDirectory(repositoryPath)
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    return result.Success ? result.Stdout : string.Empty;
  }
}
