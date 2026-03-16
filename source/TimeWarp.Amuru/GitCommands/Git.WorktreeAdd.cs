namespace TimeWarp.Amuru;

/// <summary>
/// Represents the result of adding a git worktree.
/// </summary>
/// <param name="Success">True if worktree was added successfully, false otherwise.</param>
/// <param name="WorktreePath">The path to the created worktree (null if failed).</param>
/// <param name="ErrorMessage">Error message if failed (null if succeeded).</param>
public record GitWorktreeAddResult(bool Success, string? WorktreePath, string? ErrorMessage);

/// <summary>
/// Git operations - WorktreeAdd implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Adds a new worktree linked to an existing branch.
  /// </summary>
  /// <param name="repositoryPath">The path to the bare repository.</param>
  /// <param name="worktreePath">The path where the worktree should be created.</param>
  /// <param name="branchName">The name of the existing branch to checkout.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitWorktreeAddResult containing success status, path, and any error message.</returns>
  /// <example>
  /// GitWorktreeAddResult result = await Git.WorktreeAddAsync(
  ///   "/path/to/repo.git", 
  ///   "/path/to/worktree", 
  ///   "feature-branch");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine($"Worktree created at: {result.WorktreePath}");
  /// }
  /// </example>
  public static async Task<GitWorktreeAddResult> WorktreeAddAsync(string repositoryPath, string worktreePath, string branchName, CancellationToken cancellationToken = default)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("worktree", "add", worktreePath, branchName)
      .WithWorkingDirectory(repositoryPath)
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    if (result.Success)
    {
      return new GitWorktreeAddResult(true, worktreePath, null);
    }

    string errorMessage = !string.IsNullOrWhiteSpace(result.Stderr)
      ? result.Stderr.Trim()
      : "Failed to add worktree";

    return new GitWorktreeAddResult(false, null, errorMessage);
  }
}
