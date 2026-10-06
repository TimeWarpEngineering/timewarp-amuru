#region Purpose
// Direct API for removing files and directories with optional recursive and force semantics.
#endregion

#region Design
// Force clears read-only attributes before delete so callers can remove protected trees.
// Attribute clearing walks one directory level at a time and skips reparse points so
// directory symlinks/junctions are not followed into foreign trees (and cyclic links
// cannot hang the walk). File symlinks are deleted as links without mutating the target.
// Force on a missing path matches bash `rm -f` and returns without throwing.
// Twin bool parameters are retained for 1.0 compatibility; a flags enum is deferred.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Direct
{
  /// <summary>
  /// Removes a file or directory.
  /// </summary>
  /// <param name="path">Path to the file or directory to remove</param>
  /// <param name="recursive">If true, removes directories and their contents recursively</param>
  /// <param name="force">
  /// If true, removes read-only files and treats a missing path as success (bash <c>rm -f</c>).
  /// </param>
  /// <exception cref="FileNotFoundException">When the path does not exist and <paramref name="force"/> is false</exception>
  /// <exception cref="DirectoryNotFoundException">When the directory doesn't exist</exception>
  /// <exception cref="IOException">When there's an I/O error</exception>
  /// <exception cref="UnauthorizedAccessException">When lacking permission to delete</exception>
  public static void RemoveItem(string path, bool recursive = false, bool force = false)
  {
    if (File.Exists(path))
    {
      FileInfo fileInfo = new(path);
      if (force && !FileSystemWalk.IsReparsePoint(fileInfo) && fileInfo.IsReadOnly)
      {
        fileInfo.IsReadOnly = false;
      }

      File.Delete(path);
    }
    else if (Directory.Exists(path))
    {
      if (recursive)
      {
        DirectoryInfo directory = new(path);
        if (force)
        {
          RemoveReadOnlyAttribute(directory);
        }

        Directory.Delete(path, recursive: true);
      }
      else
      {
        Directory.Delete(path);
      }
    }
    else if (!force)
    {
      throw new FileNotFoundException($"Path not found: {path}", path);
    }
  }

  /// <summary>
  /// Bash-style alias for RemoveItem.
  /// </summary>
  public static void Rm(string path, bool recursive = false, bool force = false) =>
    RemoveItem(path, recursive, force);

  private static void RemoveReadOnlyAttribute(DirectoryInfo directory)
  {
    if (FileSystemWalk.IsReparsePoint(directory))
    {
      return;
    }

    foreach (FileInfo file in directory.EnumerateFiles())
    {
      if (FileSystemWalk.IsReparsePoint(file))
      {
        continue;
      }

      if (file.IsReadOnly)
      {
        file.IsReadOnly = false;
      }
    }

    foreach (DirectoryInfo subDir in directory.EnumerateDirectories())
    {
      if (FileSystemWalk.IsReparsePoint(subDir))
      {
        continue;
      }

      RemoveReadOnlyAttribute(subDir);
    }

    directory.Refresh();
    if ((directory.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
    {
      directory.Attributes &= ~FileAttributes.ReadOnly;
    }
  }
}
