#region Purpose
// Direct API for ReplaceInFiles: one ReplaceResult per file, with atomic writes.
#endregion

#region Design
// A file whose text does not change is not rewritten. DryRun fills Preview and
// skips the temp file. Backup copies to path.bak only after the temp write succeeds
// and before the move, and only when the text changes.
#endregion

namespace TimeWarp.Amuru.Native.Text;

public static partial class Direct
{
  /// <summary>
  /// Replaces <paramref name="pattern"/> in <paramref name="path"/>.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="ReplaceInFilesOptions.SimpleMatch"/> is set.</param>
  /// <param name="replacement">Replacement text. <c>$1</c> and <c>${name}</c> expand to captures.</param>
  /// <param name="path">File or directory. A directory is searched recursively.</param>
  /// <param name="options">Replacement options. <see langword="null"/> uses defaults.</param>
  /// <param name="cancellationToken">Token used to cancel the enumeration.</param>
  /// <returns>One result per file visited.</returns>
  /// <exception cref="ArgumentException">When <paramref name="pattern"/> or <paramref name="path"/> is invalid.</exception>
  /// <exception cref="ArgumentNullException">When <paramref name="replacement"/> is null.</exception>
  /// <exception cref="FileNotFoundException">When <paramref name="path"/> does not exist.</exception>
  /// <exception cref="IOException">When a file cannot be read or replaced.</exception>
  /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> is canceled.</exception>
  public static IAsyncEnumerable<ReplaceResult> ReplaceInFiles(
    string pattern,
    string replacement,
    string path,
    ReplaceInFilesOptions? options = null,
    CancellationToken cancellationToken = default)
  {
    ReplaceInFilesOptions resolved = TextPatterns.Replace(options);
    Regex regex = TextPatterns.Compile(pattern, resolved);
    ArgumentNullException.ThrowIfNull(replacement);
    return ReplaceAsync(
      regex,
      replacement,
      TextFiles.EnumerateAsync(path, TextFileQuery.From(resolved), cancellationToken),
      resolved,
      cancellationToken);
  }

  /// <summary>
  /// Replaces <paramref name="pattern"/> in each file or directory in <paramref name="paths"/>.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="ReplaceInFilesOptions.SimpleMatch"/> is set.</param>
  /// <param name="replacement">Replacement text. <c>$1</c> and <c>${name}</c> expand to captures.</param>
  /// <param name="paths">Files and directories to edit.</param>
  /// <param name="options">Replacement options. <see langword="null"/> uses defaults.</param>
  /// <param name="cancellationToken">Token used to cancel the enumeration.</param>
  /// <returns>One result per file visited.</returns>
  /// <exception cref="ArgumentNullException">When <paramref name="paths"/> or <paramref name="replacement"/> is null.</exception>
  /// <exception cref="ArgumentException">When <paramref name="pattern"/> is invalid.</exception>
  /// <exception cref="FileNotFoundException">When a path does not exist.</exception>
  /// <exception cref="IOException">When a file cannot be read or replaced.</exception>
  /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> is canceled.</exception>
  public static IAsyncEnumerable<ReplaceResult> ReplaceInFiles(
    string pattern,
    string replacement,
    IEnumerable<string> paths,
    ReplaceInFilesOptions? options = null,
    CancellationToken cancellationToken = default)
  {
    ReplaceInFilesOptions resolved = TextPatterns.Replace(options);
    Regex regex = TextPatterns.Compile(pattern, resolved);
    ArgumentNullException.ThrowIfNull(replacement);
    ArgumentNullException.ThrowIfNull(paths);
    return ReplaceAsync(
      regex,
      replacement,
      TextFiles.EnumerateAsync(paths, TextFileQuery.From(resolved), cancellationToken),
      resolved,
      cancellationToken);
  }

  /// <summary>
  /// Replaces <paramref name="pattern"/> in files under <paramref name="root"/> matching <paramref name="globPattern"/>.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="ReplaceInFilesOptions.SimpleMatch"/> is set.</param>
  /// <param name="replacement">Replacement text. <c>$1</c> and <c>${name}</c> expand to captures.</param>
  /// <param name="root">Directory to search.</param>
  /// <param name="globPattern">Glob passed to <see cref="FileSystem.FindCriteria.Name"/>.</param>
  /// <param name="options">Replacement options. <see langword="null"/> uses defaults.</param>
  /// <param name="cancellationToken">Token used to cancel the enumeration.</param>
  /// <returns>One result per file visited.</returns>
  /// <exception cref="ArgumentException">When <paramref name="pattern"/> or <paramref name="globPattern"/> is invalid.</exception>
  /// <exception cref="ArgumentNullException">When <paramref name="replacement"/> is null.</exception>
  /// <exception cref="DirectoryNotFoundException">When <paramref name="root"/> does not exist.</exception>
  /// <exception cref="IOException">When a file cannot be read or replaced.</exception>
  /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> is canceled.</exception>
  public static IAsyncEnumerable<ReplaceResult> ReplaceInFiles(
    string pattern,
    string replacement,
    string root,
    string globPattern,
    ReplaceInFilesOptions? options = null,
    CancellationToken cancellationToken = default)
  {
    ReplaceInFilesOptions resolved = TextPatterns.Replace(options);
    Regex regex = TextPatterns.Compile(pattern, resolved);
    ArgumentNullException.ThrowIfNull(replacement);
    var query = TextFileQuery.FromGlob(globPattern, resolved.Include, resolved.Exclude);
    return ReplaceAsync(
      regex,
      replacement,
      TextFiles.EnumerateGlobAsync(root, query, cancellationToken),
      resolved,
      cancellationToken);
  }

  private static async IAsyncEnumerable<ReplaceResult> ReplaceAsync(
    Regex regex,
    string replacement,
    IAsyncEnumerable<string> files,
    ReplaceInFilesOptions options,
    [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    await foreach (string file in files.ConfigureAwait(false))
    {
      cancellationToken.ThrowIfCancellationRequested();
      yield return await TextReplace.ReplaceFileAsync(regex, replacement, file, options, cancellationToken)
        .ConfigureAwait(false);
    }
  }
}
