#region Purpose
// Commands API for listing directory contents as ls-style CommandOutput text.
#endregion

#region Design
// Uses FileSystemWalk so globbing, recursion, include/exclude, and hidden filtering
// stay sync (no sync-over-async). Default listing is non-recursive and includes hidden
// entries, matching the shipped 1.0 output shape.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
  /// <summary>
  /// Lists directory contents and returns them as CommandOutput.
  /// </summary>
  /// <param name="path">Path to the directory to list, or a glob pattern.</param>
  /// <param name="recursive">When true, lists descendants. Implied when the pattern contains <c>**</c>.</param>
  /// <param name="pattern">Optional include glob. Combined with a glob in <paramref name="path"/>.</param>
  /// <param name="exclude">Optional exclude glob.</param>
  /// <param name="includeHidden">When false, skips hidden and dot-prefixed names.</param>
  /// <returns>CommandOutput with directory listing in stdout, or error in stderr.</returns>
  public static CommandOutput GetChildItem(
    string path = ".",
    bool recursive = false,
    string? pattern = null,
    string? exclude = null,
    bool includeHidden = true)
  {
    return Invoke("GetChildItem", path, () =>
    {
      string root = FileSystemWalk.ResolveRoot(path, out string? pathPattern);
      string? includePattern = pattern ?? pathPattern;
      bool effectiveRecursive = recursive || GlobMatcher.PatternImpliesRecursive(includePattern);
      List<string> entries = [];

      foreach (FileSystemInfo entry in FileSystemWalk.Enumerate(
        root,
        effectiveRecursive,
        includeHidden,
        includePattern,
        exclude))
      {
        string displayName = effectiveRecursive
          ? FileSystemWalk.RelativePath(root, entry.FullName)
          : entry.Name;
        entries.Add(FormatChildItem(entry, displayName));
      }

      return string.Join("\n", entries);
    });
  }

  /// <summary>
  /// Bash-style alias for GetChildItem.
  /// </summary>
  public static CommandOutput Ls(
    string path = ".",
    bool recursive = false,
    string? pattern = null,
    string? exclude = null,
    bool includeHidden = true) =>
    GetChildItem(path, recursive, pattern, exclude, includeHidden);

  /// <summary>
  /// DOS-style alias for GetChildItem.
  /// </summary>
  public static CommandOutput Dir(
    string path = ".",
    bool recursive = false,
    string? pattern = null,
    string? exclude = null,
    bool includeHidden = true) =>
    GetChildItem(path, recursive, pattern, exclude, includeHidden);
}
