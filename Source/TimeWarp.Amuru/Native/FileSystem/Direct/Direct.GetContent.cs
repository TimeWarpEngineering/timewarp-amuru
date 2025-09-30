namespace TimeWarp.Amuru.Native.FileSystem;

/// <summary>
/// Direct C#-style API for file system operations.
/// These methods follow C# conventions: they can throw exceptions and return strongly-typed data.
/// Designed for LINQ composition and streaming scenarios.
/// </summary>
public static partial class Direct
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
}