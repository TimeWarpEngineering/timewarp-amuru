#region Purpose
// Detects the repository default branch from origin/HEAD or common branch names.
#endregion

#region Design
// symbolic-ref prints `origin/<branch>`. Only a leading `origin/` is removed, so a branch
// whose name contains `origin/` later (for example `feature/origin/topic`) stays intact.
// repositoryPath selects the repository. The CancellationToken overload uses process CWD.
#endregion

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
  /// Auto-detects the default branch of the repository at the process working directory.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitDefaultBranchResult containing success status, branch name, and any error message.</returns>
  public static Task<GitDefaultBranchResult> GetDefaultBranchAsync(CancellationToken cancellationToken = default)
    => GetDefaultBranchAsync(repositoryPath: null, cancellationToken);

  /// <summary>
  /// Auto-detects the default branch of the repository.
  /// First tries to read the symbolic ref for origin/HEAD, then falls back to checking
  /// for common branch names (main, master, dev).
  /// </summary>
  /// <param name="repositoryPath">Repository to inspect. Null uses the process working directory.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitDefaultBranchResult containing success status, branch name, and any error message.</returns>
  /// <example>
  /// GitDefaultBranchResult result = await Git.GetDefaultBranchAsync("/path/to/repo");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine($"Default branch: {result.BranchName}");
  /// }
  /// </example>
  public static async Task<GitDefaultBranchResult> GetDefaultBranchAsync(
    string? repositoryPath,
    CancellationToken cancellationToken = default)
  {
    CommandOutput symbolicRefResult = await GitBuilder(
        repositoryPath,
        "symbolic-ref",
        "refs/remotes/origin/HEAD",
        "--short")
      .CaptureAsync(cancellationToken)
      .ConfigureAwait(false);

    if (symbolicRefResult.Success)
    {
      string? branchName = BranchFromOriginHead(symbolicRefResult.Stdout);
      if (branchName is not null)
      {
        return new GitDefaultBranchResult(true, branchName, null);
      }
    }

    string[] commonBranches = ["main", "master", "dev"];
    foreach (string branch in commonBranches)
    {
      CommandOutput existsResult = await GitBuilder(
          repositoryPath,
          "show-ref",
          "--verify",
          "--quiet",
          $"refs/remotes/origin/{branch}")
        .CaptureAsync(cancellationToken)
        .ConfigureAwait(false);

      if (existsResult.Success)
      {
        return new GitDefaultBranchResult(true, branch, null);
      }
    }

    return new GitDefaultBranchResult(
      false,
      null,
      "Could not detect default branch. No origin/HEAD and no common branch names (main, master, dev) found.");
  }

  /// <summary>
  /// Strips one leading <c>origin/</c> from <c>git symbolic-ref --short</c> output.
  /// </summary>
  private static string? BranchFromOriginHead(string symbolicRefOutput)
  {
    string trimmed = symbolicRefOutput.Trim();
    const string prefix = "origin/";
    if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
    {
      trimmed = trimmed[prefix.Length..];
    }

    return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
  }
}
