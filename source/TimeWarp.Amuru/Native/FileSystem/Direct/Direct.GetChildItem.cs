namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Direct
{
  /// <summary>
  /// Lists directory contents as an async stream of file system entries.
  /// </summary>
  /// <param name="path">Path to the directory to list</param>
  /// <returns>Async enumerable of file system entries</returns>
  /// <exception cref="DirectoryNotFoundException">When the directory doesn't exist</exception>
  /// <exception cref="IOException">When there's an I/O error reading the directory</exception>
  public static async IAsyncEnumerable<FileSystemInfo> GetChildItem(string path = ".")
  {
    var directory = new DirectoryInfo(path);

    // This will throw DirectoryNotFoundException if path doesn't exist
    foreach (FileSystemInfo entry in directory.EnumerateFileSystemInfos())
    {
      yield return entry;
      await Task.Yield(); // Allow other async operations to run
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