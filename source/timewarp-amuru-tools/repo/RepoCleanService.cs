#region Purpose
// Implementation of repository cleaning operations
#endregion

#region Design
// Safety rules for a destructive operation:
// - Enumeration never follows directory symlinks/reparse points, so a link inside the
//   repo can never cause deletion (or traversal) outside the repo.
// - A directory containing git-TRACKED files is skipped with a warning: "bin"/"obj" are
//   build-output conventions, but nothing stops a repo from tracking sources under those
//   names (e.g. tools/bin/*.sh), and tracked content must never be deleted by a cleaner.
// - Root bin children (files and subdirectories) use the same reparse and tracked-file
//   guards. dev and dev.exe are preserved by name.
// - CleanLocalFeedAsync removes local nupkgs and package-id folders under
//   artifacts/packages. Enumeration does not follow directory reparse points, so a
//   link under the feed cannot delete files outside the repo. It is separate from
//   CleanAsync so a bin/obj clean does not discard a just-built package set.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Implementation of repository cleaning operations.
/// </summary>
public sealed class RepoCleanService : IRepoCleanService
{
  private readonly ITerminal Terminal;

  public RepoCleanService(ITerminal terminal)
  {
    Terminal = terminal;
  }

  public async Task<CleanResult> CleanAsync(CancellationToken cancellationToken = default)
  {
    string? repoRoot = Git.FindRoot();
    if (repoRoot == null)
    {
      await Terminal.WriteErrorLineAsync("Not in a git repository").ConfigureAwait(false);
      return new CleanResult(0, 0, 0);
    }

    string rootBinPath = Path.Combine(repoRoot, "bin");

    List<string> objDirectories = FindDirectories(repoRoot, "obj");
    var binDirectories = FindDirectories(repoRoot, "bin")
      .Where(dir => !string.Equals(dir, rootBinPath, StringComparison.Ordinal))
      .ToList();

    int objDirectoriesDeleted = await DeleteDirectoriesAsync(repoRoot, objDirectories, cancellationToken).ConfigureAwait(false);
    int binDirectoriesDeleted = await DeleteDirectoriesAsync(repoRoot, binDirectories, cancellationToken).ConfigureAwait(false);

    int rootBinFilesCleaned = await CleanRootBinDirectoryAsync(repoRoot, cancellationToken).ConfigureAwait(false);

    return new CleanResult(objDirectoriesDeleted, binDirectoriesDeleted, rootBinFilesCleaned);
  }

  public async Task<int> CleanLocalFeedAsync(CancellationToken cancellationToken = default)
  {
    string? repoRoot = Git.FindRoot();
    if (repoRoot == null)
    {
      return 0;
    }

    string localFeedPath = Path.Combine(repoRoot, "artifacts", "packages");
    if (!Directory.Exists(localFeedPath))
    {
      return 0;
    }

    if (IsReparsePoint(localFeedPath))
    {
      await Terminal.WriteErrorLineAsync($"Skipped (reparse point): {localFeedPath}").ConfigureAwait(false);
      return 0;
    }

    return await DeleteLocalFeedEntriesAsync(repoRoot, localFeedPath, cancellationToken).ConfigureAwait(false);
  }

  private async Task<int> DeleteLocalFeedEntriesAsync
  (
    string repoRoot,
    string directory,
    CancellationToken cancellationToken
  )
  {
    int count = 0;
    foreach (string file in Directory.GetFiles(directory, "TimeWarp.Amuru.*.nupkg"))
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (await TryDeletePathAsync(repoRoot, file, isDirectory: false, cancellationToken).ConfigureAwait(false))
      {
        count++;
      }
    }

    foreach (string child in Directory.GetDirectories(directory))
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (IsReparsePoint(child))
      {
        await Terminal.WriteErrorLineAsync($"Skipped (reparse point): {child}").ConfigureAwait(false);
        continue;
      }

      count += await DeleteLocalFeedEntriesAsync(repoRoot, child, cancellationToken).ConfigureAwait(false);

      if (!Directory.Exists(child) || !IsLocalFeedPackageDirectory(Path.GetFileName(child)))
      {
        continue;
      }

      if (await TryDeletePathAsync(repoRoot, child, isDirectory: true, cancellationToken).ConfigureAwait(false))
      {
        count++;
      }
    }

    return count;
  }

  private static bool IsLocalFeedPackageDirectory(string name)
  {
    return string.Equals(name, "timewarp.amuru", StringComparison.Ordinal)
      || string.Equals(name, "timewarp.amuru.tools", StringComparison.Ordinal);
  }

  /// <summary>
  /// Recursively finds directories with the given name, never descending into
  /// (or returning) directory symlinks/reparse points.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI cleanup operation: directory enumeration failures should be reported as warnings and allow cleanup to continue."
  )]
  private List<string> FindDirectories(string root, string directoryName)
  {
    List<string> results = [];
    Stack<string> pending = new();
    pending.Push(root);

    while (pending.Count > 0)
    {
      string current = pending.Pop();

      try
      {
        foreach (string dir in Directory.GetDirectories(current))
        {
          FileAttributes attributes = File.GetAttributes(dir);
          if ((attributes & FileAttributes.ReparsePoint) != 0)
          {
            continue;
          }

          if (string.Equals(Path.GetFileName(dir), directoryName, StringComparison.Ordinal))
          {
            results.Add(dir);
            continue;
          }

          pending.Push(dir);
        }
      }
      catch (Exception ex)
      {
        Terminal.WriteErrorLine($"Warning: Error searching under {current}: {ex.Message}");
      }
    }

    return results;
  }

  private async Task<int> DeleteDirectoriesAsync(string repoRoot, IReadOnlyList<string> directories, CancellationToken cancellationToken)
  {
    int count = 0;

    foreach (string dir in directories)
    {
      if (await HasTrackedFilesAsync(repoRoot, dir, cancellationToken).ConfigureAwait(false))
      {
        await Terminal.WriteErrorLineAsync($"Skipped (contains git-tracked files): {dir}").ConfigureAwait(false);
        continue;
      }

      try
      {
        Directory.Delete(dir, recursive: true);
        await Terminal.WriteLineAsync($"Deleted: {dir}").ConfigureAwait(false);
        count++;
      }
      catch (IOException ex)
      {
        await Terminal.WriteErrorLineAsync($"Warning: Could not delete {dir}: {ex.Message}").ConfigureAwait(false);
      }
      catch (UnauthorizedAccessException ex)
      {
        await Terminal.WriteErrorLineAsync($"Warning: Could not delete {dir}: {ex.Message}").ConfigureAwait(false);
      }
    }

    return count;
  }

  private static async Task<bool> HasTrackedFilesAsync(string repoRoot, string directory, CancellationToken cancellationToken)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("-C", repoRoot, "ls-files", "--", directory)
      .CaptureAsync(cancellationToken).ConfigureAwait(false);

    // Fail safe: if git itself failed we cannot prove the directory is untracked — skip it.
    return !result.Success || !string.IsNullOrWhiteSpace(result.Stdout);
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI cleanup operation: file/directory enumeration failures should be reported as warnings and allow cleanup to continue."
  )]
  private async Task<int> CleanRootBinDirectoryAsync(string repoRoot, CancellationToken cancellationToken)
  {
    string rootBinPath = Path.Combine(repoRoot, "bin");
    if (!Directory.Exists(rootBinPath))
    {
      return 0;
    }

    int count = 0;
    string[] preserveNames = ["dev", "dev.exe"];

    try
    {
      foreach (string file in Directory.GetFiles(rootBinPath))
      {
        cancellationToken.ThrowIfCancellationRequested();
        string fileName = Path.GetFileName(file);
        if (preserveNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
        {
          continue;
        }

        if (await TryDeletePathAsync(repoRoot, file, isDirectory: false, cancellationToken).ConfigureAwait(false))
        {
          count++;
        }
      }

      foreach (string dir in Directory.GetDirectories(rootBinPath))
      {
        cancellationToken.ThrowIfCancellationRequested();
        if (await TryDeletePathAsync(repoRoot, dir, isDirectory: true, cancellationToken).ConfigureAwait(false))
        {
          count++;
        }
      }
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      await Terminal.WriteErrorLineAsync($"Warning: Error cleaning root bin directory: {ex.Message}").ConfigureAwait(false);
    }

    return count;
  }

  private async Task<bool> TryDeletePathAsync
  (
    string repoRoot,
    string path,
    bool isDirectory,
    CancellationToken cancellationToken
  )
  {
    if (IsReparsePoint(path))
    {
      await Terminal.WriteErrorLineAsync($"Skipped (reparse point): {path}").ConfigureAwait(false);
      return false;
    }

    if (await HasTrackedFilesAsync(repoRoot, path, cancellationToken).ConfigureAwait(false))
    {
      string reason = isDirectory ? "contains git-tracked files" : "git-tracked file";
      await Terminal.WriteErrorLineAsync($"Skipped ({reason}): {path}").ConfigureAwait(false);
      return false;
    }

    try
    {
      if (isDirectory)
      {
        Directory.Delete(path, recursive: true);
      }
      else
      {
        File.Delete(path);
      }

      await Terminal.WriteLineAsync($"Deleted: {path}").ConfigureAwait(false);
      return true;
    }
    catch (IOException ex)
    {
      await Terminal.WriteErrorLineAsync($"Warning: Could not delete {path}: {ex.Message}").ConfigureAwait(false);
      return false;
    }
    catch (UnauthorizedAccessException ex)
    {
      await Terminal.WriteErrorLineAsync($"Warning: Could not delete {path}: {ex.Message}").ConfigureAwait(false);
      return false;
    }
  }

  private static bool IsReparsePoint(string path)
  {
    // LinkTarget does not follow the link. GetAttributes is the fallback for
    // other reparse points that are not symlinks.
    if (new FileInfo(path).LinkTarget != null || new DirectoryInfo(path).LinkTarget != null)
    {
      return true;
    }

    return (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
  }
}
