#region Purpose
// Direct API for SelectString: streams TextMatch one line at a time.
#endregion

#region Design
// The regex is compiled before the iterator starts, so a bad pattern throws at the call.
// File bytes are read with StreamReader.ReadLineAsync; only the context window is kept.
// Multi-file input goes through Direct.FindItem and FindCriteria. Files whose first
// 8000 bytes contain NUL (no UTF-16/32 BOM) are binary and yield no matches.
#endregion

namespace TimeWarp.Amuru.Native.Text;

/// <summary>
/// Typed text operations. These methods throw on invalid input and I/O failures.
/// </summary>
public static partial class Direct
{
  /// <summary>
  /// Searches <paramref name="path"/> for <paramref name="pattern"/>, one line at a time.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="SelectStringOptions.SimpleMatch"/> is set.</param>
  /// <param name="path">File or directory. A directory is searched recursively.</param>
  /// <param name="options">Match and file-glob options. <see langword="null"/> uses defaults.</param>
  /// <param name="cancellationToken">Token used to cancel the enumeration.</param>
  /// <returns>Matches as they are found.</returns>
  /// <exception cref="ArgumentException">When <paramref name="pattern"/> or <paramref name="path"/> is invalid.</exception>
  /// <exception cref="FileNotFoundException">When <paramref name="path"/> does not exist.</exception>
  /// <exception cref="IOException">When a file cannot be read.</exception>
  /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> is canceled.</exception>
  public static IAsyncEnumerable<TextMatch> SelectString(
    string pattern,
    string path,
    SelectStringOptions? options = null,
    CancellationToken cancellationToken = default)
  {
    SelectStringOptions resolved = TextPatterns.Select(options);
    Regex regex = TextPatterns.Compile(pattern, resolved);
    return SearchAsync(regex, TextFiles.EnumerateAsync(path, TextFileQuery.From(resolved), cancellationToken), resolved, cancellationToken);
  }

  /// <summary>
  /// Searches each file or directory in <paramref name="paths"/>.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="SelectStringOptions.SimpleMatch"/> is set.</param>
  /// <param name="paths">Files and directories to search.</param>
  /// <param name="options">Match and file-glob options. <see langword="null"/> uses defaults.</param>
  /// <param name="cancellationToken">Token used to cancel the enumeration.</param>
  /// <returns>Matches as they are found.</returns>
  /// <exception cref="ArgumentNullException">When <paramref name="paths"/> is null.</exception>
  /// <exception cref="ArgumentException">When <paramref name="pattern"/> is invalid or an entry of <paramref name="paths"/> is null or blank.</exception>
  /// <exception cref="FileNotFoundException">When a path does not exist.</exception>
  /// <exception cref="IOException">When a file cannot be read.</exception>
  /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> is canceled.</exception>
  public static IAsyncEnumerable<TextMatch> SelectString(
    string pattern,
    IEnumerable<string> paths,
    SelectStringOptions? options = null,
    CancellationToken cancellationToken = default)
  {
    SelectStringOptions resolved = TextPatterns.Select(options);
    Regex regex = TextPatterns.Compile(pattern, resolved);
    ArgumentNullException.ThrowIfNull(paths);
    return SearchAsync(regex, TextFiles.EnumerateAsync(TextFiles.RequirePaths(paths), TextFileQuery.From(resolved), cancellationToken), resolved, cancellationToken);
  }

  /// <summary>
  /// Searches files under <paramref name="root"/> whose relative path matches <paramref name="globPattern"/>.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="SelectStringOptions.SimpleMatch"/> is set.</param>
  /// <param name="root">Directory to search.</param>
  /// <param name="globPattern">Glob passed to <see cref="FileSystem.FindCriteria.Name"/>.</param>
  /// <param name="options">Match and file-glob options. <see langword="null"/> uses defaults.</param>
  /// <param name="cancellationToken">Token used to cancel the enumeration.</param>
  /// <returns>Matches as they are found.</returns>
  /// <exception cref="ArgumentException">When <paramref name="pattern"/> or <paramref name="globPattern"/> is invalid.</exception>
  /// <exception cref="DirectoryNotFoundException">When <paramref name="root"/> does not exist.</exception>
  /// <exception cref="IOException">When a file cannot be read.</exception>
  /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> is canceled.</exception>
  public static IAsyncEnumerable<TextMatch> SelectString(
    string pattern,
    string root,
    string globPattern,
    SelectStringOptions? options = null,
    CancellationToken cancellationToken = default)
  {
    SelectStringOptions resolved = TextPatterns.Select(options);
    Regex regex = TextPatterns.Compile(pattern, resolved);
    var query = TextFileQuery.FromGlob(globPattern, resolved.Include, resolved.Exclude);
    return SearchAsync(regex, TextFiles.EnumerateGlobAsync(root, query, cancellationToken), resolved, cancellationToken);
  }

  /// <summary>
  /// Searches lines from <paramref name="reader"/>. The reader is not disposed.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="SelectStringOptions.SimpleMatch"/> is set.</param>
  /// <param name="reader">Line source.</param>
  /// <param name="path">Path stored on each <see cref="TextMatch"/>. <see langword="null"/> uses <c>-</c>.</param>
  /// <param name="options">Match options. File globs are ignored. <see langword="null"/> uses defaults.</param>
  /// <param name="cancellationToken">Token used to cancel the enumeration.</param>
  /// <returns>Matches as they are found.</returns>
  /// <exception cref="ArgumentNullException">When <paramref name="reader"/> is null.</exception>
  /// <exception cref="ArgumentException">When <paramref name="pattern"/> is invalid.</exception>
  /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> is canceled.</exception>
  public static IAsyncEnumerable<TextMatch> SelectString(
    string pattern,
    TextReader reader,
    string? path = null,
    SelectStringOptions? options = null,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(reader);
    SelectStringOptions resolved = TextPatterns.Select(options);
    Regex regex = TextPatterns.Compile(pattern, resolved);
    string label = path ?? TextFiles.StandardInputPath;
    return TextSearch.ReadAsync(regex, reader, label, resolved, cancellationToken);
  }

  /// <summary>
  /// Searches lines from <paramref name="stream"/>. The stream is not disposed.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="SelectStringOptions.SimpleMatch"/> is set.</param>
  /// <param name="stream">Byte source, decoded as UTF-8 with BOM detection.</param>
  /// <param name="path">Path stored on each <see cref="TextMatch"/>. <see langword="null"/> uses <c>-</c>.</param>
  /// <param name="options">Match options. File globs are ignored. <see langword="null"/> uses defaults.</param>
  /// <param name="cancellationToken">Token used to cancel the enumeration.</param>
  /// <returns>Matches as they are found.</returns>
  /// <exception cref="ArgumentNullException">When <paramref name="stream"/> is null.</exception>
  /// <exception cref="ArgumentException">When <paramref name="pattern"/> is invalid.</exception>
  /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> is canceled.</exception>
  public static IAsyncEnumerable<TextMatch> SelectString(
    string pattern,
    Stream stream,
    string? path = null,
    SelectStringOptions? options = null,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(stream);
    SelectStringOptions resolved = TextPatterns.Select(options);
    Regex regex = TextPatterns.Compile(pattern, resolved);
    return ReadStreamAsync(regex, stream, path ?? TextFiles.StandardInputPath, resolved, cancellationToken);
  }

  private static async IAsyncEnumerable<TextMatch> SearchAsync(
    Regex regex,
    IAsyncEnumerable<string> files,
    SelectStringOptions options,
    [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    await foreach (string file in files.ConfigureAwait(false))
    {
      cancellationToken.ThrowIfCancellationRequested();
      await foreach (TextMatch match in TextSearch.ReadFileAsync(regex, file, options, cancellationToken)
        .ConfigureAwait(false))
      {
        yield return match;
      }
    }
  }

  private static async IAsyncEnumerable<TextMatch> ReadStreamAsync(
    Regex regex,
    Stream stream,
    string path,
    SelectStringOptions options,
    [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    using StreamReader reader = new(
      stream,
      Encoding.UTF8,
      detectEncodingFromByteOrderMarks: true,
      bufferSize: 1024,
      leaveOpen: true);
    await foreach (TextMatch match in TextSearch.ReadAsync(regex, reader, path, options, cancellationToken)
      .ConfigureAwait(false))
    {
      yield return match;
    }
  }
}
