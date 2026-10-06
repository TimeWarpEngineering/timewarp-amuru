#region Purpose
// Removes a linked worktree by resolving the main repository from its gitdir file.
#endregion

#region Design
// A linked worktree's .git file contains `gitdir: <common-git-dir>/worktrees/<name>`.
// Git 2.48+ may store that path relative to the worktree directory, so a relative gitdir
// is combined with the worktree path, not the process working directory.
// The common git directory is two parents above that per-worktree directory.
// When the common directory is named `.git`, the repository path is its parent.
// A bare repository's common directory is the repository path itself.
#endregion

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
  /// <param name="force">If true, forces removal of the worktree even if it has uncommitted changes.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitWorktreeRemoveResult containing success status and any error message.</returns>
  /// <example>
  /// GitWorktreeRemoveResult result = await Git.WorktreeRemoveAsync("/path/to/worktree");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine("Worktree removed successfully");
  /// }
  ///
  /// GitWorktreeRemoveResult forceResult = await Git.WorktreeRemoveAsync("/path/to/worktree", force: true);
  /// </example>
  public static async Task<GitWorktreeRemoveResult> WorktreeRemoveAsync(
    string worktreePath,
    bool force = false,
    CancellationToken cancellationToken = default)
  {
    string? mainRepoPath = FindMainRepositoryFromWorktree(worktreePath);

    if (string.IsNullOrWhiteSpace(mainRepoPath))
    {
      return new GitWorktreeRemoveResult(false, "Could not determine main repository path from worktree");
    }

    List<string> arguments = ["worktree", "remove"];

    if (force)
    {
      arguments.Add("--force");
    }

    arguments.Add(worktreePath);

    CommandOutput result = await GitBuilder(mainRepoPath, arguments.ToArray())
      .CaptureAsync(cancellationToken)
      .ConfigureAwait(false);

    if (result.Success)
    {
      return new GitWorktreeRemoveResult(true, null);
    }

    return new GitWorktreeRemoveResult(false, ErrorTextOr(result, "Failed to remove worktree"));
  }

  /// <summary>
  /// Resolves the main repository path from a linked worktree's <c>.git</c> file.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI utility: worktree discovery failures should return null, not throw."
  )]
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
        if (!line.StartsWith("gitdir: ", StringComparison.Ordinal))
        {
          continue;
        }

        string gitdir = line["gitdir: ".Length..].Trim();
        if (gitdir.Length == 0)
        {
          return null;
        }

        if (!Path.IsPathRooted(gitdir))
        {
          gitdir = Path.GetFullPath(Path.Combine(worktreePath, gitdir));
        }

        DirectoryInfo perWorktreeGitDir = new(gitdir);
        DirectoryInfo? commonGitDir = perWorktreeGitDir.Parent?.Parent;
        if (commonGitDir == null)
        {
          return null;
        }

        if (string.Equals(commonGitDir.Name, ".git", StringComparison.OrdinalIgnoreCase))
        {
          return commonGitDir.Parent?.FullName;
        }

        return commonGitDir.FullName;
      }
    }
    catch (Exception)
    {
      return null;
    }

    return null;
  }
}
