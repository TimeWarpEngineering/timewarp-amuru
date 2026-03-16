namespace TimeWarp.Amuru;

/// <summary>
/// Git operations - IsWorktree implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Checks if the current repository is using git worktrees.
  /// A repository is considered to be using worktrees if .git is a file (pointing to the worktree metadata)
  /// rather than a directory.
  /// </summary>
  /// <param name="path">The path to check (defaults to current directory).</param>
  /// <returns>True if using worktrees, false otherwise.</returns>
  /// <example>
  /// if (Git.IsWorktree())
  /// {
  ///   Console.WriteLine("Using git worktrees");
  /// }
  /// else
  /// {
  ///   Console.WriteLine("Regular git repository");
  /// }
  /// </example>
  public static bool IsWorktree(string? path = null)
  {
    string? gitRoot = FindRoot(path);
    if (gitRoot == null)
    {
      return false;
    }

    string gitPath = Path.Combine(gitRoot, ".git");

    // In worktrees, .git is a file (not a directory) that points to the actual git metadata
    return File.Exists(gitPath);
  }
}
