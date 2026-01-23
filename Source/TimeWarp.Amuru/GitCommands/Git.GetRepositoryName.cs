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
      string remoteUrl = result.Stdout.Trim();
      string? repoName = ParseRepositoryNameFromUrl(remoteUrl);
      if (repoName != null)
      {
        return repoName;
      }
    }

    // Fallback to directory name
    return new DirectoryInfo(gitRoot).Name;
  }

  /// <summary>
  /// Parses the repository name from a git remote URL.
  /// Supports various URL formats:
  /// - SSH: git@host:user/repo.git or git@host:repo.git
  /// - HTTPS: https://host/user/repo.git or https://host/user/repo
  /// - Git protocol: git://host/user/repo.git
  /// </summary>
  /// <param name="remoteUrl">The git remote URL to parse.</param>
  /// <returns>The repository name, or null if it cannot be parsed.</returns>
  private static string? ParseRepositoryNameFromUrl(string remoteUrl)
  {
    if (string.IsNullOrWhiteSpace(remoteUrl))
    {
      return null;
    }

    string repoName;

    // Handle SSH format: git@host:path/to/repo.git or git@host:repo.git
    if (remoteUrl.Contains('@', StringComparison.Ordinal) && remoteUrl.Contains(':', StringComparison.Ordinal) && !remoteUrl.Contains("://", StringComparison.Ordinal))
    {
      // Extract the path part after the colon
      int colonIndex = remoteUrl.IndexOf(':', StringComparison.Ordinal);
      string path = remoteUrl[(colonIndex + 1)..];
      repoName = path.Split('/').Last();
    }
    // Handle HTTPS/Git protocol: https://host/path/to/repo.git or git://host/path/to/repo.git
    else if (remoteUrl.Contains("://", StringComparison.Ordinal))
    {
      repoName = remoteUrl.Split('/').Last();
    }
    else
    {
      // Unknown format, try to get the last path segment
      repoName = remoteUrl.Split('/', '\\').Last();
    }

    // Remove .git suffix if present
    if (repoName.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
    {
      repoName = repoName[..^4];
    }

    return string.IsNullOrWhiteSpace(repoName) ? null : repoName;
  }
}
