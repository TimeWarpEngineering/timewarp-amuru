#region Purpose
// Temporary git repositories for real-execution git-command tests.
#endregion

#region Design
// Repositories are created with `git init -b master` and a local user identity so commits
// do not depend on the machine default branch or global user config.
// Helpers throw when git fails so a broken fixture fails the test immediately.
// Delete clears read-only bits under .git before removing the directory.
#endregion

namespace TimeWarp.Amuru.Testing;

/// <summary>
/// Creates and deletes temporary git repositories for tests.
/// </summary>
public static class GitRepositoryFixture
{
  /// <summary>
  /// Creates a temporary repository on branch master with a local commit identity.
  /// </summary>
  public static async Task<string> CreateRepositoryAsync(string name)
  {
    string root = Directory.CreateTempSubdirectory($"amuru-git-{name}-").FullName;
    await RunGitAsync(root, "init", "-b", "master");
    await ConfigureLocalIdentityAsync(root);
    return root;
  }

  /// <summary>
  /// Writes a file and commits it in <paramref name="repositoryPath"/>.
  /// </summary>
  public static async Task CommitFileAsync(
    string repositoryPath,
    string relativePath,
    string contents,
    string message)
  {
    string fullPath = Path.Combine(repositoryPath, relativePath);
    string? directory = Path.GetDirectoryName(fullPath);
    if (!string.IsNullOrEmpty(directory))
    {
      Directory.CreateDirectory(directory);
    }

    await File.WriteAllTextAsync(fullPath, contents);
    await RunGitAsync(repositoryPath, "add", "--", relativePath);
    await RunGitAsync(repositoryPath, "-c", "commit.gpgsign=false", "commit", "-m", message);
  }

  /// <summary>
  /// Clones <paramref name="sourcePath"/> into a new temporary directory and sets a local commit identity.
  /// </summary>
  public static async Task<string> CloneAsync(string sourcePath, string name)
  {
    string destination = Directory.CreateTempSubdirectory($"amuru-git-{name}-").FullName;
    await RunGitAsync(Path.GetTempPath(), "clone", sourcePath, destination);
    await ConfigureLocalIdentityAsync(destination);
    return destination;
  }

  /// <summary>
  /// Runs <c>git rev-parse</c> and returns trimmed stdout.
  /// </summary>
  public static async Task<string> RevParseAsync(string repositoryPath, params string[] revisionArgs)
  {
    string[] arguments = ["rev-parse", .. revisionArgs];
    return (await RunGitAsync(repositoryPath, arguments)).Trim();
  }

  /// <summary>
  /// Runs git in <paramref name="repositoryPath"/> and returns stdout.
  /// </summary>
  public static Task<string> GitAsync(string repositoryPath, params string[] arguments)
    => RunGitAsync(repositoryPath, arguments);

  /// <summary>
  /// Rewrites a linked worktree's <c>.git</c> gitdir to a path relative to the worktree.
  /// </summary>
  public static void MakeGitdirRelative(string worktreePath)
  {
    string gitFile = Path.Combine(worktreePath, ".git");
    string content = File.ReadAllText(gitFile).Trim();
    const string prefix = "gitdir: ";
    if (!content.StartsWith(prefix, StringComparison.Ordinal))
    {
      throw new InvalidOperationException($"Unexpected worktree .git file: {content}");
    }

    string gitdir = content[prefix.Length..].Trim();
    if (!Path.IsPathRooted(gitdir))
    {
      return;
    }

    string relative = Path.GetRelativePath(worktreePath, gitdir).Replace('\\', '/');
    File.WriteAllText(gitFile, $"gitdir: {relative}{Environment.NewLine}");
  }

  /// <summary>
  /// Deletes a temporary repository directory when it exists.
  /// </summary>
  public static void Delete(string? path)
  {
    if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
    {
      return;
    }

    foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
    {
      File.SetAttributes(file, FileAttributes.Normal);
    }

    Directory.Delete(path, recursive: true);
  }

  private static async Task ConfigureLocalIdentityAsync(string repositoryPath)
  {
    await RunGitAsync(repositoryPath, "config", "user.email", "amuru-tests@example.com");
    await RunGitAsync(repositoryPath, "config", "user.name", "Amuru Tests");
    await RunGitAsync(repositoryPath, "config", "commit.gpgsign", "false");
  }

  private static async Task<string> RunGitAsync(string workingDirectory, params string[] arguments)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments(arguments)
      .WithWorkingDirectory(workingDirectory)
      .WithNoValidation()
      .CaptureAsync();

    if (!result.Success)
    {
      string error = string.IsNullOrWhiteSpace(result.Stderr) ? result.Stdout : result.Stderr;
      throw new InvalidOperationException($"git {string.Join(' ', arguments)} failed: {error.Trim()}");
    }

    return result.Stdout;
  }
}
