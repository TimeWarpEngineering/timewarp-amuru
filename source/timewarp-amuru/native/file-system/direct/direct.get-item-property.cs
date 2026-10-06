#region Purpose
// Direct API for reading file-system metadata (stat).
#endregion

#region Design
// Snapshots FileSystemInfo into ItemProperty so callers are not tied to a live handle.
// LinkTarget is taken from FileSystemInfo.LinkTarget. Missing paths throw.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Direct
{
  /// <summary>
  /// Returns metadata for a file or directory.
  /// </summary>
  /// <param name="path">Path to inspect.</param>
  /// <returns>A snapshot of size, timestamps, attributes, and symlink target.</returns>
  /// <exception cref="FileNotFoundException">When the path does not exist.</exception>
  /// <exception cref="UnauthorizedAccessException">When lacking permission to stat the path.</exception>
  /// <exception cref="ArgumentException">When <paramref name="path"/> is null or whitespace.</exception>
  public static ItemProperty GetItemProperty(string path)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    string fullPath = FileSystemWalk.NormalizePath(path);

    FileSystemInfo fileSystemInfo;
    bool isFile;
    bool isDirectory;
    if (File.Exists(fullPath))
    {
      fileSystemInfo = new FileInfo(fullPath);
      isFile = true;
      isDirectory = false;
    }
    else if (Directory.Exists(fullPath))
    {
      fileSystemInfo = new DirectoryInfo(fullPath);
      isFile = false;
      isDirectory = true;
    }
    else
    {
      throw new FileNotFoundException($"Path not found: {path}", path);
    }

    long length = 0;
    if (fileSystemInfo is FileInfo fileInfo)
    {
      length = fileInfo.Length;
    }

    return new ItemProperty
    {
      FullName = fileSystemInfo.FullName,
      Name = fileSystemInfo.Name,
      Length = length,
      CreationTime = new DateTimeOffset(fileSystemInfo.CreationTime),
      LastWriteTime = new DateTimeOffset(fileSystemInfo.LastWriteTime),
      LastAccessTime = new DateTimeOffset(fileSystemInfo.LastAccessTime),
      Attributes = fileSystemInfo.Attributes,
      Exists = true,
      IsFile = isFile,
      IsDirectory = isDirectory,
      IsReparsePoint = FileSystemWalk.IsReparsePoint(fileSystemInfo),
      LinkTarget = fileSystemInfo.LinkTarget
    };
  }

  /// <summary>
  /// Bash-style alias for GetItemProperty.
  /// </summary>
  public static ItemProperty Stat(string path) => GetItemProperty(path);
}
