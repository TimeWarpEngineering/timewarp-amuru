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

    int rootBinFilesCleaned = 0;
    await Task.Run(() => rootBinFilesCleaned = CleanRootBinDirectory(repoRoot), cancellationToken).ConfigureAwait(false);

    return new CleanResult(objDirectoriesDeleted, binDirectoriesDeleted, rootBinFilesCleaned);
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
  private int CleanRootBinDirectory(string repoRoot)
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
        string fileName = Path.GetFileName(file);
        if (preserveNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
        {
          continue;
        }

        try
        {
          File.Delete(file);
          Terminal.WriteLine($"Deleted: {file}");
          count++;
        }
        catch (IOException ex)
        {
          Terminal.WriteErrorLine($"Warning: Could not delete {file}: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
          Terminal.WriteErrorLine($"Warning: Could not delete {file}: {ex.Message}");
        }
      }

      foreach (string dir in Directory.GetDirectories(rootBinPath))
      {
        try
        {
          Directory.Delete(dir, recursive: true);
          Terminal.WriteLine($"Deleted: {dir}");
          count++;
        }
        catch (IOException ex)
        {
          Terminal.WriteErrorLine($"Warning: Could not delete {dir}: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
          Terminal.WriteErrorLine($"Warning: Could not delete {dir}: {ex.Message}");
        }
      }
    }
    catch (Exception ex)
    {
      Terminal.WriteErrorLine($"Warning: Error cleaning root bin directory: {ex.Message}");
    }

    return count;
  }
}
