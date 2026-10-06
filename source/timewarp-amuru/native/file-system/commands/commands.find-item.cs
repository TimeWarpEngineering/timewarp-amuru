#region Purpose
// Commands API for FindItem: recursive search with optional criteria, paths on stdout.
#endregion

#region Design
// Sync walk (no sync-over-async). One full path per stdout line. Delegates filtering
// to FileSystemWalk so Direct and Commands share match rules.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
  /// <summary>
  /// Recursively finds file-system entries and returns their paths as CommandOutput.
  /// </summary>
  /// <param name="path">Directory to search.</param>
  /// <param name="criteria">Optional name, size, date, and attribute filters.</param>
  /// <returns>CommandOutput with one path per stdout line, or error in stderr.</returns>
  public static CommandOutput FindItem(string path, FindCriteria? criteria = null)
  {
    return Invoke("FindItem", path, () =>
    {
      string root = FileSystemWalk.NormalizePath(path);
      List<string> matches = [];
      foreach (FileSystemInfo entry in FileSystemWalk.Enumerate(
        root,
        recursive: true,
        includeHidden: true,
        includePattern: null,
        excludePattern: null))
      {
        if (criteria is null || FileSystemWalk.MatchesCriteria(entry, root, criteria))
        {
          matches.Add(entry.FullName);
        }
      }

      return string.Join("\n", matches);
    });
  }

  /// <summary>
  /// Bash-style alias for FindItem.
  /// </summary>
  public static CommandOutput Find(string path, FindCriteria? criteria = null) =>
    FindItem(path, criteria);
}
