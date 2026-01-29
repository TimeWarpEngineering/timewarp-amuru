namespace TimeWarp.Amuru;

/// <summary>
/// Represents the result of a git clone operation.
/// </summary>
/// <param name="Success">True if clone succeeded, false otherwise.</param>
/// <param name="Path">The path where the repository was cloned (null if failed).</param>
/// <param name="ErrorMessage">Error message if failed (null if succeeded).</param>
public record GitCloneResult(bool Success, string? Path, string? ErrorMessage);

/// <summary>
/// Git operations - CloneBare implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Clones a git repository as a bare repository (without a working directory).
  /// This is useful for creating a central repository that can host worktrees.
  /// </summary>
  /// <param name="repositoryUrl">The URL of the repository to clone.</param>
  /// <param name="targetPath">The local path where the bare repository should be created.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>GitCloneResult containing success status, path, and any error message.</returns>
  /// <example>
  /// GitCloneResult result = await Git.CloneBareAsync("https://github.com/user/repo.git", "/path/to/repo.git");
  /// if (result.Success)
  /// {
  ///   Console.WriteLine($"Cloned bare repository to: {result.Path}");
  /// }
  /// </example>
#pragma warning disable CA1054 // URI parameters should not be strings
  public static async Task<GitCloneResult> CloneBareAsync(string repositoryUrl, string targetPath, CancellationToken cancellationToken = default)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("clone", "--bare", repositoryUrl, targetPath)
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    if (result.Success)
    {
      return new GitCloneResult(true, targetPath, null);
    }

    string errorMessage = !string.IsNullOrWhiteSpace(result.Stderr)
      ? result.Stderr.Trim()
      : "Failed to clone bare repository";

    return new GitCloneResult(false, null, errorMessage);
  }
#pragma warning restore CA1054 // URI parameters should not be strings
}
