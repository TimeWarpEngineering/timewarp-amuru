#region Purpose
// Implementation of repository cleaning operations
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
      await Terminal.WriteErrorLineAsync("Not in a git repository");
      return new CleanResult(0, 0, 0);
    }

    int objDirectoriesDeleted = 0;
    int binDirectoriesDeleted = 0;
    int rootBinFilesCleaned = 0;

    await Task.Run
    (
      () =>
      {
        objDirectoriesDeleted = DeleteDirectories(repoRoot, "obj");
        binDirectoriesDeleted = DeleteBinDirectories(repoRoot);
        rootBinFilesCleaned = CleanRootBinDirectory(repoRoot);
      },
      cancellationToken
    );

    return new CleanResult(objDirectoriesDeleted, binDirectoriesDeleted, rootBinFilesCleaned);
  }

  private int DeleteDirectories(string repoRoot, string directoryName)
  {
    int count = 0;

    try
    {
      string[] directories = Directory.GetDirectories(repoRoot, directoryName, SearchOption.AllDirectories);
      foreach (string dir in directories)
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
      Terminal.WriteErrorLine($"Warning: Error searching for {directoryName} directories: {ex.Message}");
    }

    return count;
  }

  private int DeleteBinDirectories(string repoRoot)
  {
    int count = 0;
    string rootBinPath = Path.Combine(repoRoot, "bin");

    try
    {
      string[] directories = Directory.GetDirectories(repoRoot, "bin", SearchOption.AllDirectories);
      foreach (string dir in directories)
      {
        if (string.Equals(dir, rootBinPath, StringComparison.Ordinal))
        {
          continue;
        }

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
      Terminal.WriteErrorLine($"Warning: Error searching for bin directories: {ex.Message}");
    }

    return count;
  }

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
