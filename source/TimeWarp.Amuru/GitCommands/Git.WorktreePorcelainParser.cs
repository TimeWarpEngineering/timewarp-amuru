namespace TimeWarp.Amuru;

/// <summary>
/// Parser for git worktree list --porcelain output.
/// </summary>
public static class WorktreePorcelainParser
{
  /// <summary>
  /// Parses the porcelain output from "git worktree list --porcelain" into a list of WorktreeEntry objects.
  /// </summary>
  /// <param name="porcelainOutput">The raw porcelain output from git.</param>
  /// <returns>A read-only list of parsed WorktreeEntry objects.</returns>
  public static IReadOnlyList<WorktreeEntry> ParseWorktreeList(string porcelainOutput)
  {
    List<WorktreeEntry> entries = new();
    
    if (string.IsNullOrWhiteSpace(porcelainOutput))
    {
      return entries;
    }

    string[] lines = porcelainOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    
    string? currentPath = null;
    string? currentHeadCommit = null;
    string? currentBranchRef = null;
    bool currentIsBare = false;

    foreach (string line in lines)
    {
      if (line.StartsWith("worktree ", StringComparison.Ordinal))
      {
        // Save previous entry if we have one
        if (currentPath != null)
        {
          entries.Add(new WorktreeEntry(currentPath, currentHeadCommit, currentBranchRef, currentIsBare));
        }
        
        // Start new entry
        currentPath = line["worktree ".Length..].Trim();
        currentHeadCommit = null;
        currentBranchRef = null;
        currentIsBare = false;
      }
      else if (line.StartsWith("HEAD ", StringComparison.Ordinal))
      {
        currentHeadCommit = line["HEAD ".Length..].Trim();
      }
      else if (line.StartsWith("branch ", StringComparison.Ordinal))
      {
        currentBranchRef = line["branch ".Length..].Trim();
      }
      else if (line == "bare")
      {
        currentIsBare = true;
      }
      else if (line == "detached")
      {
        // Detached HEAD, branch remains null
        currentBranchRef = null;
      }
      else if (string.IsNullOrWhiteSpace(line))
      {
        // Empty line indicates end of an entry block
        if (currentPath != null)
        {
          entries.Add(new WorktreeEntry(currentPath, currentHeadCommit, currentBranchRef, currentIsBare));
          currentPath = null;
          currentHeadCommit = null;
          currentBranchRef = null;
          currentIsBare = false;
        }
      }
    }

    // Don't forget the last entry
    if (currentPath != null)
    {
      entries.Add(new WorktreeEntry(currentPath, currentHeadCommit, currentBranchRef, currentIsBare));
    }

    return entries;
  }
}
