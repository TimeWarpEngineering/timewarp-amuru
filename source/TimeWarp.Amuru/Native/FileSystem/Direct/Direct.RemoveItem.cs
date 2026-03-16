namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Direct
{
  /// <summary>
  /// Removes a file or directory.
  /// </summary>
  /// <param name="path">Path to the file or directory to remove</param>
  /// <param name="recursive">If true, removes directories and their contents recursively</param>
  /// <param name="force">If true, removes read-only files</param>
  /// <exception cref="FileNotFoundException">When the file doesn't exist</exception>
  /// <exception cref="DirectoryNotFoundException">When the directory doesn't exist</exception>
  /// <exception cref="IOException">When there's an I/O error</exception>
  /// <exception cref="UnauthorizedAccessException">When lacking permission to delete</exception>
  public static void RemoveItem(string path, bool recursive = false, bool force = false)
  {
    if (File.Exists(path))
    {
      if (force)
      {
        var fileInfo = new FileInfo(path);
        if (fileInfo.IsReadOnly)
        {
          fileInfo.IsReadOnly = false;
        }
      }

      File.Delete(path);
    }
    else if (Directory.Exists(path))
    {
      if (recursive)
      {
        var directory = new DirectoryInfo(path);
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
    else
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
    foreach (FileInfo file in directory.EnumerateFiles("*", SearchOption.AllDirectories))
    {
      if (file.IsReadOnly)
      {
        file.IsReadOnly = false;
      }
    }

    foreach (DirectoryInfo subDir in directory.EnumerateDirectories("*", SearchOption.AllDirectories))
    {
      if ((subDir.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
      {
        subDir.Attributes &= ~FileAttributes.ReadOnly;
      }
    }
  }
}