#region Purpose
// Direct API for recursively finding file-system entries by name, size, date, or attributes.
#endregion

#region Design
// Always recursive (find default). Skips reparse points on the walk. Streaming name is
// FindItem without an Async suffix, matching shipped GetChildItem / GetContent.
// Hidden entries are included (find default); filter with FindCriteria.Attributes if needed.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Direct
{
  /// <summary>
  /// Recursively finds file-system entries under <paramref name="path"/>.
  /// </summary>
  /// <param name="path">Directory to search.</param>
  /// <param name="criteria">Optional name, size, date, and attribute filters.</param>
  /// <param name="cancellationToken">Token used to cancel the enumeration.</param>
  /// <returns>Matching entries as they are discovered.</returns>
  /// <exception cref="DirectoryNotFoundException">When <paramref name="path"/> does not exist.</exception>
  /// <exception cref="UnauthorizedAccessException">When lacking permission to read a directory.</exception>
  /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> is canceled.</exception>
  /// <exception cref="ArgumentException">When <paramref name="path"/> is null or whitespace.</exception>
  public static async IAsyncEnumerable<FileSystemInfo> FindItem(
    string path,
    FindCriteria? criteria = null,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();
    string root = FileSystemWalk.NormalizePath(path);

    foreach (FileSystemInfo entry in FileSystemWalk.Enumerate(
      root,
      recursive: true,
      includeHidden: true,
      includePattern: null,
      excludePattern: null))
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (criteria is null || FileSystemWalk.MatchesCriteria(entry, root, criteria))
      {
        yield return entry;
        await Task.Yield();
      }
    }
  }

  /// <summary>
  /// Bash-style alias for FindItem.
  /// </summary>
  public static IAsyncEnumerable<FileSystemInfo> Find(
    string path,
    FindCriteria? criteria = null) =>
    FindItem(path, criteria);
}
