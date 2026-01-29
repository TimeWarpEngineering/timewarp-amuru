namespace TimeWarp.Amuru;

/// <summary>
/// Represents the result of removing a git worktree.
/// </summary>
/// <param name="Success">True if worktree was removed successfully, false otherwise.</param>
/// <param name="ErrorMessage">Error message if failed (null if succeeded).</param>
public record GitWorktreeRemoveResult(bool Success, string? ErrorMessage);

/// <summary>
/// Git operations - WorktreeRemove implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Removes a git worktree from the repository.
  /// The worktree path must be a valid worktree linked to the repository.
  /// </summary>
  /// <param name="worktreePath">The path to the worktree to remove.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitWorktreeRemoveResult containing success status and any error message.</returns>
  /// <example>
  /// GitWorktreeRemoveResult result = await Git.WorktreeRemoveAsync("/path/to/worktree");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine("Worktree removed successfully");
  /// }
  /// </example>
  public static async Task<GitWorktreeRemoveResult> WorktreeRemoveAsync(string worktreePath, CancellationToken cancellationToken = default)
  {
    // We need to run the command from the main repository, not the worktree
    // First, find the main repository by looking for the git directory reference
    string? mainRepoPath = FindMainRepositoryFromWorktree(worktreePath);
    
    if (string.IsNullOrWhiteSpace(mainRepoPath))
    {
      return new GitWorktreeRemoveResult(false, "Could not determine main repository path from worktree");
    }

    CommandOutput result = await Shell.Builder("git")
      .WithArguments("worktree", "remove", worktreePath)
      .WithWorkingDirectory(mainRepoPath)
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    if (result.Success)
    {
      return new GitWorktreeRemoveResult(true, null);
    }

    string errorMessage = !string.IsNullOrWhiteSpace(result.Stderr)
      ? result.Stderr.Trim()
      : "Failed to remove worktree";

    return new GitWorktreeRemoveResult(false, errorMessage);
  }

  /// <summary>
  /// Helper method to find the main repository path from a worktree path.
  /// </summary>
  private static string? FindMainRepositoryFromWorktree(string worktreePath)
  {
    string gitFilePath = Path.Combine(worktreePath, ".git");
    
    if (!File.Exists(gitFilePath))
    {
      return null;
    }

    try
    {
      string[] lines = File.ReadAllLines(gitFilePath);
      foreach (string line in lines)
      {
        if (line.StartsWith("gitdir: ", StringComparison.Ordinal))
        {
          string gitdir = line["gitdir: ".Length..].Trim();
          // The gitdir points to something like /path/to/repo.git/worktrees/worktree-name
          // We need to go up to find the main repo
          var dir = new DirectoryInfo(gitdir);
          // Go up past worktrees and the worktree name directory
          if (dir.Parent?.Parent != null)
          {
            return dir.Parent.Parent.FullName;
          }
        }
      }
    }
    catch
    {
      return null;
    }

    return null;
  }
}
