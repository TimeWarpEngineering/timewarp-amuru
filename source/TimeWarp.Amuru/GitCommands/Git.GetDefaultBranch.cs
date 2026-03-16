namespace TimeWarp.Amuru;

/// <summary>
/// Represents the result of detecting the default branch.
/// </summary>
/// <param name="Success">True if detection succeeded, false otherwise.</param>
/// <param name="BranchName">The detected branch name (null if failed).</param>
/// <param name="ErrorMessage">Error message if failed (null if succeeded).</param>
public record GitDefaultBranchResult(bool Success, string? BranchName, string? ErrorMessage);

/// <summary>
/// Git operations - GetDefaultBranch implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Auto-detects the default branch of the repository.
  /// First tries to read the symbolic ref for origin/HEAD, then falls back to checking
  /// for common branch names (main, master, dev).
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitDefaultBranchResult containing success status, branch name, and any error message.</returns>
  /// <example>
  /// GitDefaultBranchResult result = await Git.GetDefaultBranchAsync();
  /// if (result.Success)
  /// {
  ///   Console.WriteLine($"Default branch: {result.BranchName}");
  /// }
  /// </example>
  public static async Task<GitDefaultBranchResult> GetDefaultBranchAsync(CancellationToken cancellationToken = default)
  {
    // Option 1: Check symbolic ref for origin/HEAD
    CommandOutput symbolicRefResult = await Shell.Builder("git")
      .WithArguments("symbolic-ref", "refs/remotes/origin/HEAD", "--short")
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    if (symbolicRefResult.Success)
    {
      string branchName = symbolicRefResult.Stdout.Trim().Replace("origin/", "", StringComparison.Ordinal);
      if (!string.IsNullOrWhiteSpace(branchName))
      {
        return new GitDefaultBranchResult(true, branchName, null);
      }
    }

    // Option 2: Check common branch names
    string[] commonBranches = ["main", "master", "dev"];
    foreach (string branch in commonBranches)
    {
      CommandOutput existsResult = await Shell.Builder("git")
        .WithArguments("show-ref", "--verify", "--quiet", $"refs/remotes/origin/{branch}")
        .WithNoValidation()
        .CaptureAsync(cancellationToken);

      if (existsResult.Success)
      {
        return new GitDefaultBranchResult(true, branch, null);
      }
    }

    return new GitDefaultBranchResult(false, null, "Could not detect default branch. No origin/HEAD and no common branch names (main, master, dev) found.");
  }
}
