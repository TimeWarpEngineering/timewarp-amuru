namespace TimeWarp.Amuru;

/// <summary>
/// Git operations - GetRepositoryName implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Gets the repository name by parsing the git remote URL or falling back to the directory name.
  /// Attempts to extract the repository name from the 'origin' remote URL, removing .git suffix if present.
  /// </summary>
  /// <param name="gitRoot">The git repository root directory. If null, attempts to find it automatically.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>The repository name, or null if not in a git repository.</returns>
  /// <example>
  /// string? repoName = await Git.GetRepositoryNameAsync();
  /// Console.WriteLine($"Repository: {repoName}");
  /// </example>
  public static async Task<string?> GetRepositoryNameAsync(string? gitRoot = null, CancellationToken cancellationToken = default)
  {
    gitRoot ??= FindRoot();
    if (gitRoot == null)
    {
      return null;
    }

    // Try to get from git remote
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("remote", "get-url", "origin")
      .WithWorkingDirectory(gitRoot)
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    if (result.Success && !string.IsNullOrWhiteSpace(result.Stdout))
    {
      string output = result.Stdout.Trim();

      // Extract repo name from URL
      if (output.Contains("github.com", StringComparison.OrdinalIgnoreCase) ||
          output.Contains("gitlab.com", StringComparison.OrdinalIgnoreCase) ||
          output.Contains("bitbucket.org", StringComparison.OrdinalIgnoreCase))
      {
        string repoName = output.Split('/').Last();
        if (repoName.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
          repoName = repoName[..^4]; // Remove ".git" suffix
        }

        return repoName;
      }
    }

    // Fallback to directory name
    return new DirectoryInfo(gitRoot).Name;
  }
}
