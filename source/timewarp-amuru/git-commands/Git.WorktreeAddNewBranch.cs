#region Purpose
// TODO: Add purpose description
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Git operations - WorktreeAddNewBranch implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Adds a new worktree with a new branch.
  /// When <paramref name="startPoint"/> is provided, the new branch is created from that
  /// commit-ish (e.g., a branch name, remote tracking branch, tag, or commit SHA).
  /// When omitted, the new branch is created from the current HEAD.
  /// </summary>
  /// <param name="repositoryPath">The path to the bare repository.</param>
  /// <param name="worktreePath">The path where the worktree should be created.</param>
  /// <param name="newBranchName">The name of the new branch to create.</param>
  /// <param name="startPoint">
  /// Optional commit-ish to start the new branch from. Valid values include remote
  /// tracking branches (e.g., <c>origin/develop</c>), local branches (e.g., <c>develop</c>,
  /// <c>main</c>), commit SHAs (e.g., <c>abc1234</c>), and revision ranges (e.g., <c>HEAD~3</c>).
  /// When <c>null</c>, the branch is created from HEAD.
  /// </param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitWorktreeAddResult containing success status, path, and any error message.</returns>
  /// <example>
  /// // Create from HEAD (existing behavior):
  /// GitWorktreeAddResult result = await Git.WorktreeAddNewBranchAsync(
  ///   "/path/to/repo.git",
  ///   "/path/to/worktree",
  ///   "new-feature");
  ///
  /// // Create from a specific start point:
  /// GitWorktreeAddResult result = await Git.WorktreeAddNewBranchAsync(
  ///   "/path/to/repo.git",
  ///   "/path/to/worktree",
  ///   "new-feature",
  ///   startPoint: "origin/develop");
  ///
  /// if (result.Success)
  /// {
  ///   Console.WriteLine($"Worktree created with new branch at: {result.WorktreePath}");
  /// }
  /// </example>
  public static async Task<GitWorktreeAddResult> WorktreeAddNewBranchAsync(string repositoryPath, string worktreePath, string newBranchName, string? startPoint = null, CancellationToken cancellationToken = default)
  {
    List<string> arguments = ["worktree", "add", "-b", newBranchName, worktreePath];

    if (startPoint is not null)
    {
      arguments.Add(startPoint);
    }

    CommandOutput result = await Shell.Builder("git")
      .WithArguments(arguments.ToArray())
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
