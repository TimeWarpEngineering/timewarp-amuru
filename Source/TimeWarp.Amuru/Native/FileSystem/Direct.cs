namespace TimeWarp.Amuru.Native.FileSystem;

/// <summary>
/// Direct C#-style API for file system operations.
/// These methods follow C# conventions: they can throw exceptions and return strongly-typed data.
/// Designed for LINQ composition and streaming scenarios.
/// </summary>
public static class Direct
{
  /// <summary>
  /// Reads file content as an async stream of lines.
  /// </summary>
  /// <param name="path">Path to the file to read</param>
  /// <returns>Async enumerable of lines from the file</returns>
  /// <exception cref="FileNotFoundException">When the file doesn't exist</exception>
  /// <exception cref="IOException">When there's an I/O error reading the file</exception>
  public static async IAsyncEnumerable<string> GetContent(string path)
  {
    using StreamReader reader = File.OpenText(path);
    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
      yield return line;
    }
  }

  /// <summary>
  /// Bash-style alias for GetContent.
  /// </summary>
  public static IAsyncEnumerable<string> Cat(string path) => GetContent(path);

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

  /// <summary>
  /// Gets the current working directory.
  /// </summary>
  /// <returns>The current working directory path</returns>
#pragma warning disable CA1024 // Use properties where appropriate
  public static string GetLocation() => Environment.CurrentDirectory;
#pragma warning restore CA1024 // Use properties where appropriate

  /// <summary>
  /// Bash-style alias for GetLocation.
  /// </summary>
  public static string Pwd() => GetLocation();

  /// <summary>
  /// Sets the current working directory.
  /// </summary>
  /// <param name="path">The new working directory path</param>
  /// <exception cref="DirectoryNotFoundException">When the directory doesn't exist</exception>
  public static void SetLocation(string path)
  {
    Environment.CurrentDirectory = path;
  }

  /// <summary>
  /// Bash-style alias for SetLocation.
  /// </summary>
  public static void Cd(string path) => SetLocation(path);
}