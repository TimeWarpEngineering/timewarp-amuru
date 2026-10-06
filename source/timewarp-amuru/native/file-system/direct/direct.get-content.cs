#region Purpose
// Direct API for streaming file content line-by-line as an async enumerable.
#endregion

#region Design
// Streams via StreamReader so large files do not require a full in-memory buffer.
// [EnumeratorCancellation] forwards WithCancellation tokens into ReadLineAsync.
// Async method suffix is deferred: the public name shipped in 1.0.0 stays GetContent.
#endregion

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
  /// <param name="cancellationToken">Token used to cancel the enumeration</param>
  /// <returns>Async enumerable of lines from the file</returns>
  /// <exception cref="FileNotFoundException">When the file doesn't exist</exception>
  /// <exception cref="IOException">When there's an I/O error reading the file</exception>
  /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> is canceled</exception>
  public static async IAsyncEnumerable<string> GetContent(
    string path,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();
    using StreamReader reader = File.OpenText(path);
    while (true)
    {
      cancellationToken.ThrowIfCancellationRequested();
      string? line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
      if (line is null)
      {
        yield break;
      }

      yield return line;
    }
  }

  /// <summary>
  /// Bash-style alias for GetContent.
  /// </summary>
  public static IAsyncEnumerable<string> Cat(string path) => GetContent(path);
}
