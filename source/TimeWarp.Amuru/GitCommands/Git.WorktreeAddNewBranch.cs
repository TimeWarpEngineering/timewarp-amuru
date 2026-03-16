namespace TimeWarp.Amuru;

/// <summary>
/// Git operations - WorktreeAddNewBranch implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Adds a new worktree with a new branch created from the current HEAD.
  /// </summary>
  /// <param name="repositoryPath">The path to the bare repository.</param>
  /// <param name="worktreePath">The path where the worktree should be created.</param>
  /// <param name="newBranchName">The name of the new branch to create.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitWorktreeAddResult containing success status, path, and any error message.</returns>
  /// <example>
  /// GitWorktreeAddResult result = await Git.WorktreeAddNewBranchAsync(
  ///   "/path/to/repo.git", 
  ///   "/path/to/worktree", 
  ///   "new-feature");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine($"Worktree created with new branch at: {result.WorktreePath}");
  /// }
  /// </example>
  public static async Task<GitWorktreeAddResult> WorktreeAddNewBranchAsync(string repositoryPath, string worktreePath, string newBranchName, CancellationToken cancellationToken = default)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("worktree", "add", "-b", newBranchName, worktreePath)
      .WithWorkingDirectory(repositoryPath)
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    if (result.Success)
    {
      return new GitWorktreeAddResult(true, worktreePath, null);
    }

    string errorMessage = !string.IsNullOrWhiteSpace(result.Stderr)
      ? result.Stderr.Trim()
      : "Failed to add worktree with new branch";

    return new GitWorktreeAddResult(false, null, errorMessage);
  }
}
