#region Purpose
// Commands API for SelectString: grep-format stdout and grep exit codes.
#endregion

#region Design
// Synchronous file reads avoid sync-over-async. Exit 0 means at least one match,
// 1 means none, 2 means a regex or I/O error (grep). A missing input path, an
// unreadable directory, or an unreadable file is reported on stderr as
// "SelectString: <path>: <reason>"; the walk continues, earlier stdout is kept, and
// any error forces exit 2. A line with several hits is printed once, like grep.
#endregion

namespace TimeWarp.Amuru.Native.Text;

/// <summary>
/// Shell-style text commands. These methods do not throw; failures are stderr and a non-zero exit code.
/// </summary>
public static partial class Commands
{
  /// <summary>
  /// Searches <paramref name="path"/> for <paramref name="pattern"/>.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="SelectStringOptions.SimpleMatch"/> is set.</param>
  /// <param name="path">File or directory. A directory is searched recursively.</param>
  /// <param name="options">Match and file-glob options. <see langword="null"/> uses defaults.</param>
  /// <returns>
  /// Stdout lines of the form <c>path:line:text</c>, one per matching line.
  /// Exit code 0 when a line matches, 1 when none match, 2 on error.
  /// </returns>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI command wrapper: unexpected failures should return error CommandOutput, not throw."
  )]
  public static CommandOutput SelectString(string pattern, string path, SelectStringOptions? options = null)
  {
    try
    {
      SelectStringOptions resolved = TextPatterns.Select(options);
      Regex regex = TextPatterns.Compile(pattern, resolved);
      return Search(regex, TextFiles.Enumerate(path, TextFileQuery.From(resolved)), resolved);
    }
    catch (Exception exception)
    {
      return TextCommand.Fail("SelectString", path, exception, TextCommand.ErrorExitCode);
    }
  }

  /// <summary>
  /// Searches each file or directory in <paramref name="paths"/>.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="SelectStringOptions.SimpleMatch"/> is set.</param>
  /// <param name="paths">Files and directories to search.</param>
  /// <param name="options">Match and file-glob options. <see langword="null"/> uses defaults.</param>
  /// <returns>
  /// Stdout lines of the form <c>path:line:text</c>, one per matching line.
  /// Exit code 0 when a line matches, 1 when none match, 2 on error.
  /// </returns>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI command wrapper: unexpected failures should return error CommandOutput, not throw."
  )]
  public static CommandOutput SelectString(
    string pattern,
    IEnumerable<string> paths,
    SelectStringOptions? options = null)
  {
    try
    {
      SelectStringOptions resolved = TextPatterns.Select(options);
      Regex regex = TextPatterns.Compile(pattern, resolved);
      return Search(regex, TextFiles.Enumerate(TextFiles.RequirePaths(paths), TextFileQuery.From(resolved)), resolved);
    }
    catch (Exception exception)
    {
      return TextCommand.Fail("SelectString", null, exception, TextCommand.ErrorExitCode);
    }
  }

  /// <summary>
  /// Searches files under <paramref name="root"/> whose relative path matches <paramref name="globPattern"/>.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="SelectStringOptions.SimpleMatch"/> is set.</param>
  /// <param name="root">Directory to search.</param>
  /// <param name="globPattern">Glob passed to <see cref="FileSystem.FindCriteria.Name"/>.</param>
  /// <param name="options">Match and file-glob options. <see langword="null"/> uses defaults.</param>
  /// <returns>
  /// Stdout lines of the form <c>path:line:text</c>, one per matching line.
  /// Exit code 0 when a line matches, 1 when none match, 2 on error.
  /// </returns>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI command wrapper: unexpected failures should return error CommandOutput, not throw."
  )]
  public static CommandOutput SelectString(
    string pattern,
    string root,
    string globPattern,
    SelectStringOptions? options = null)
  {
    try
    {
      SelectStringOptions resolved = TextPatterns.Select(options);
      Regex regex = TextPatterns.Compile(pattern, resolved);
      var query = TextFileQuery.FromGlob(globPattern, resolved.Include, resolved.Exclude);
      return Search(regex, TextFiles.EnumerateGlob(root, query), resolved);
    }
    catch (Exception exception)
    {
      return TextCommand.Fail("SelectString", root, exception, TextCommand.ErrorExitCode);
    }
  }

  /// <summary>
  /// Searches lines from <paramref name="reader"/>. The reader is not disposed.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="SelectStringOptions.SimpleMatch"/> is set.</param>
  /// <param name="reader">Line source.</param>
  /// <param name="path">Path label written on each hit. <see langword="null"/> uses <c>-</c>.</param>
  /// <param name="options">Match options. File globs are ignored. <see langword="null"/> uses defaults.</param>
  /// <returns>
  /// Stdout lines of the form <c>path:line:text</c>, one per matching line.
  /// Exit code 0 when a line matches, 1 when none match, 2 on error.
  /// </returns>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI command wrapper: unexpected failures should return error CommandOutput, not throw."
  )]
  public static CommandOutput SelectString(
    string pattern,
    TextReader reader,
    string? path = null,
    SelectStringOptions? options = null)
  {
    try
    {
      ArgumentNullException.ThrowIfNull(reader);
      SelectStringOptions resolved = TextPatterns.Select(options);
      Regex regex = TextPatterns.Compile(pattern, resolved);
      string label = path ?? TextFiles.StandardInputPath;
      return Format(TextSearch.Read(regex, reader, label, resolved));
    }
    catch (Exception exception)
    {
      return TextCommand.Fail("SelectString", path, exception, TextCommand.ErrorExitCode);
    }
  }

  /// <summary>
  /// Searches lines from <paramref name="stream"/>. The stream is not disposed.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="SelectStringOptions.SimpleMatch"/> is set.</param>
  /// <param name="stream">Byte source, decoded as UTF-8 with BOM detection.</param>
  /// <param name="path">Path label written on each hit. <see langword="null"/> uses <c>-</c>.</param>
  /// <param name="options">Match options. File globs are ignored. <see langword="null"/> uses defaults.</param>
  /// <returns>
  /// Stdout lines of the form <c>path:line:text</c>, one per matching line.
  /// Exit code 0 when a line matches, 1 when none match, 2 on error.
  /// </returns>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI command wrapper: unexpected failures should return error CommandOutput, not throw."
  )]
  public static CommandOutput SelectString(
    string pattern,
    Stream stream,
    string? path = null,
    SelectStringOptions? options = null)
  {
    try
    {
      ArgumentNullException.ThrowIfNull(stream);
      using StreamReader reader = new(
        stream,
        Encoding.UTF8,
        detectEncodingFromByteOrderMarks: true,
        bufferSize: 1024,
        leaveOpen: true);
      return SelectString(pattern, reader, path, options);
    }
    catch (Exception exception)
    {
      return TextCommand.Fail("SelectString", path, exception, TextCommand.ErrorExitCode);
    }
  }

  private static CommandOutput Search(Regex regex, IEnumerable<TextFileItem> files, SelectStringOptions options)
  {
    StringBuilder stdout = new();
    StringBuilder stderr = new();
    bool any = false;
    bool error = false;
    foreach (TextFileItem file in files)
    {
      if (file.Error is not null)
      {
        error = true;
        AppendError(stderr, file.Path, file.Error);
        continue;
      }

      try
      {
        int lastLine = 0;
        foreach (TextMatch match in TextSearch.ReadFile(regex, file.Path, options))
        {
          any = true;
          if (match.LineNumber == lastLine)
          {
            continue;
          }

          lastLine = match.LineNumber;
          AppendHit(stdout, match);
        }
      }
      catch (Exception exception) when (TextCommand.IsFileError(exception))
      {
        error = true;
        AppendError(stderr, file.Path, exception);
      }
    }

    int exit = error ? TextCommand.ErrorExitCode : any ? 0 : TextCommand.NoMatchExitCode;
    return new CommandOutput(stdout.ToString(), stderr.ToString(), exit);
  }

  private static CommandOutput Format(IEnumerable<TextMatch> matches)
  {
    StringBuilder stdout = new();
    bool any = false;
    int lastLine = 0;
    foreach (TextMatch match in matches)
    {
      any = true;
      if (match.LineNumber == lastLine)
      {
        continue;
      }

      lastLine = match.LineNumber;
      AppendHit(stdout, match);
    }

    return new CommandOutput(stdout.ToString(), string.Empty, any ? 0 : TextCommand.NoMatchExitCode);
  }

  private static void AppendHit(StringBuilder stdout, TextMatch match)
  {
    if (stdout.Length > 0)
    {
      stdout.Append('\n');
    }

    stdout.Append(match.Path);
    stdout.Append(':');
    stdout.Append(match.LineNumber.ToString(CultureInfo.InvariantCulture));
    stdout.Append(':');
    stdout.Append(match.Line);
  }

  private static void AppendError(StringBuilder stderr, string file, Exception exception)
  {
    if (stderr.Length > 0)
    {
      stderr.Append('\n');
    }

    stderr.Append("SelectString: ");
    stderr.Append(file);
    stderr.Append(": ");
    stderr.Append(TextCommand.DescribeIo(exception));
  }
}
