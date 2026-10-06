#region Purpose
// Direct API for listing directory entries as an async enumerable of FileSystemInfo.
#endregion

#region Design
// Enumerates via FileSystemWalk so globbing, recursion, include/exclude, and hidden
// filtering share one walk with FindItem. Reparse points are yielded but not descended.
// Basic listing (path only) keeps shipped behavior: non-recursive, includes hidden.
// Async method suffix is deferred: the public name shipped in 1.0.0 stays GetChildItem.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Direct
{
  /// <summary>
  /// Lists directory contents as an async stream of file system entries.
  /// </summary>
  /// <param name="path">Path to the directory to list, or a glob pattern.</param>
  /// <param name="cancellationToken">Token used to cancel the enumeration.</param>
  /// <returns>Async enumerable of file system entries</returns>
  /// <exception cref="DirectoryNotFoundException">When the directory doesn't exist</exception>
  /// <exception cref="IOException">When there's an I/O error reading the directory</exception>
  /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> is canceled</exception>
  public static IAsyncEnumerable<FileSystemInfo> GetChildItem(
    string path = ".",
    CancellationToken cancellationToken = default) =>
    GetChildItem(path, recursive: false, pattern: null, exclude: null, includeHidden: true, cancellationToken);

  /// <summary>
  /// Lists directory contents with globbing, recursion, and hidden-file control.
  /// </summary>
  /// <param name="path">Path to the directory to list, or a glob pattern.</param>
  /// <param name="recursive">When true, lists descendants. Implied when the pattern contains <c>**</c>.</param>
  /// <param name="pattern">Optional include glob. Combined with a glob in <paramref name="path"/>.</param>
  /// <param name="exclude">Optional exclude glob.</param>
  /// <param name="includeHidden">When false, skips hidden and dot-prefixed names.</param>
  /// <param name="cancellationToken">Token used to cancel the enumeration.</param>
  /// <returns>Matching file-system entries.</returns>
  /// <exception cref="DirectoryNotFoundException">When the directory doesn't exist.</exception>
  /// <exception cref="IOException">When there's an I/O error reading the directory.</exception>
  /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> is canceled.</exception>
  public static async IAsyncEnumerable<FileSystemInfo> GetChildItem(
    string path,
    bool recursive,
    string? pattern = null,
    string? exclude = null,
    bool includeHidden = true,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();
    string root = FileSystemWalk.ResolveRoot(path, out string? pathPattern);
    string? includePattern = pattern ?? pathPattern;
    bool effectiveRecursive = recursive || GlobMatcher.PatternImpliesRecursive(includePattern);

    foreach (FileSystemInfo entry in FileSystemWalk.Enumerate(
      root,
      effectiveRecursive,
      includeHidden,
      includePattern,
      exclude))
    {
      cancellationToken.ThrowIfCancellationRequested();
      yield return entry;
      await Task.Yield();
    }
  }

  /// <summary>
  /// Bash-style alias for GetChildItem.
  /// </summary>
  public static IAsyncEnumerable<FileSystemInfo> Ls(string path = ".") => GetChildItem(path);

  /// <summary>
  /// DOS-style alias for GetChildItem.
  /// </summary>
  public static IAsyncEnumerable<FileSystemInfo> Dir(string path = ".") => GetChildItem(path);
}
