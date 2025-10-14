namespace TimeWarp.Amuru;

/// <summary>
/// Git operations - FindRoot implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Finds the root directory of the git repository by searching for .git directory.
  /// Walks up the directory tree from the start path until it finds a .git directory or file (for worktrees).
  /// </summary>
  /// <param name="startPath">The directory to start searching from. Defaults to current directory.</param>
  /// <returns>The absolute path to the git repository root, or null if not in a git repository.</returns>
  /// <example>
  /// string? gitRoot = Git.FindRoot();
  /// if (gitRoot != null)
  /// {
  ///   Console.WriteLine($"Git root: {gitRoot}");
  /// }
  /// </example>
  public static string? FindRoot(string? startPath = null)
  {
    string currentPath = startPath ?? Directory.GetCurrentDirectory();

    while (!string.IsNullOrEmpty(currentPath))
    {
      string gitPath = Path.Combine(currentPath, ".git");

      // Check if .git exists (either as directory or file for worktrees)
      if (Directory.Exists(gitPath) || File.Exists(gitPath))
      {
        return currentPath;
      }

      DirectoryInfo? parent = Directory.GetParent(currentPath);
      if (parent == null)
      {
        break;
      }

      currentPath = parent.FullName;
    }

    return null;
  }
}
