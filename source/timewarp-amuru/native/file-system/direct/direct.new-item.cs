#region Purpose
// Direct API for creating directories (with parents) and empty files (touch).
#endregion

#region Design
// Directory: Directory.CreateDirectory (mkdir -p). Existing directory is success;
// existing file throws. File: create parent directories, then create an empty file
// or update last-write time when the file already exists (touch). Existing directory
// at a file path throws.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Direct
{
  /// <summary>
  /// Creates a file or directory.
  /// </summary>
  /// <param name="path">Path to create.</param>
  /// <param name="itemType">Whether to create a file or a directory.</param>
  /// <returns>The created or existing item.</returns>
  /// <exception cref="IOException">When the path exists as the other item type.</exception>
  /// <exception cref="UnauthorizedAccessException">When lacking permission to create.</exception>
  /// <exception cref="ArgumentException">When <paramref name="path"/> is null or whitespace.</exception>
  /// <exception cref="ArgumentOutOfRangeException">When <paramref name="itemType"/> is not a defined value.</exception>
  public static FileSystemInfo NewItem(string path, ItemType itemType)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    string fullPath = FileSystemWalk.NormalizePath(path);

    return itemType switch
    {
      ItemType.Directory => CreateDirectory(fullPath),
      ItemType.File => CreateFile(fullPath),
      _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, "Unknown item type.")
    };
  }

  /// <summary>
  /// Bash-style alias that creates a directory and any missing parents.
  /// </summary>
  public static FileSystemInfo Mkdir(string path) => NewItem(path, ItemType.Directory);

  /// <summary>
  /// Bash-style alias that creates an empty file or updates its last-write time.
  /// </summary>
  public static FileSystemInfo Touch(string path) => NewItem(path, ItemType.File);

  private static DirectoryInfo CreateDirectory(string fullPath)
  {
    if (File.Exists(fullPath))
    {
      throw new IOException($"NewItem: {fullPath}: file exists");
    }

    return Directory.CreateDirectory(fullPath);
  }

  private static FileInfo CreateFile(string fullPath)
  {
    if (Directory.Exists(fullPath))
    {
      throw new IOException($"NewItem: {fullPath}: is a directory");
    }

    string? parent = Path.GetDirectoryName(fullPath);
    if (!string.IsNullOrEmpty(parent))
    {
      Directory.CreateDirectory(parent);
    }

    if (File.Exists(fullPath))
    {
      File.SetLastWriteTime(fullPath, DateTime.Now);
      return new FileInfo(fullPath);
    }

    File.WriteAllBytes(fullPath, []);
    return new FileInfo(fullPath);
  }
}
