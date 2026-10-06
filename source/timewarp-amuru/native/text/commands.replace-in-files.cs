#region Purpose
// Commands API for ReplaceInFiles: one stdout line per changed file, sed exit codes.
#endregion

#region Design
// Exit 0 when every file was readable, including when nothing matched.
// Exit 1 when a regex or I/O error occurs. Changed files already written stay written.
// Stdout lists only files whose text changed, including dry-run hits.
#endregion

namespace TimeWarp.Amuru.Native.Text;

public static partial class Commands
{
  /// <summary>
  /// Replaces <paramref name="pattern"/> in <paramref name="path"/>.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="ReplaceInFilesOptions.SimpleMatch"/> is set.</param>
  /// <param name="replacement">Replacement text. <c>$1</c> and <c>${name}</c> expand to captures.</param>
  /// <param name="path">File or directory. A directory is searched recursively.</param>
  /// <param name="options">Replacement options. <see langword="null"/> uses defaults.</param>
  /// <returns>
  /// One stdout line <c>path: N replacement(s)</c> per changed file.
  /// Exit code 0 when the walk finishes, 1 on error.
  /// </returns>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI command wrapper: unexpected failures should return error CommandOutput, not throw."
  )]
  public static CommandOutput ReplaceInFiles(
    string pattern,
    string replacement,
    string path,
    ReplaceInFilesOptions? options = null)
  {
    try
    {
      ReplaceInFilesOptions resolved = TextPatterns.Replace(options);
      Regex regex = TextPatterns.Compile(pattern, resolved);
      ArgumentNullException.ThrowIfNull(replacement);
      return Replace(regex, replacement, TextFiles.Enumerate(path, TextFileQuery.From(resolved)), resolved);
    }
    catch (Exception exception)
    {
      return TextCommand.Fail("ReplaceInFiles", path, exception, TextCommand.NoMatchExitCode);
    }
  }

  /// <summary>
  /// Replaces <paramref name="pattern"/> in each file or directory in <paramref name="paths"/>.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="ReplaceInFilesOptions.SimpleMatch"/> is set.</param>
  /// <param name="replacement">Replacement text. <c>$1</c> and <c>${name}</c> expand to captures.</param>
  /// <param name="paths">Files and directories to edit.</param>
  /// <param name="options">Replacement options. <see langword="null"/> uses defaults.</param>
  /// <returns>
  /// One stdout line <c>path: N replacement(s)</c> per changed file.
  /// Exit code 0 when the walk finishes, 1 on error.
  /// </returns>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI command wrapper: unexpected failures should return error CommandOutput, not throw."
  )]
  public static CommandOutput ReplaceInFiles(
    string pattern,
    string replacement,
    IEnumerable<string> paths,
    ReplaceInFilesOptions? options = null)
  {
    try
    {
      ReplaceInFilesOptions resolved = TextPatterns.Replace(options);
      Regex regex = TextPatterns.Compile(pattern, resolved);
      ArgumentNullException.ThrowIfNull(replacement);
      return Replace(regex, replacement, TextFiles.Enumerate(paths, TextFileQuery.From(resolved)), resolved);
    }
    catch (Exception exception)
    {
      return TextCommand.Fail("ReplaceInFiles", null, exception, TextCommand.NoMatchExitCode);
    }
  }

  /// <summary>
  /// Replaces <paramref name="pattern"/> in files under <paramref name="root"/> matching <paramref name="globPattern"/>.
  /// </summary>
  /// <param name="pattern">Regular expression, or a literal when <see cref="ReplaceInFilesOptions.SimpleMatch"/> is set.</param>
  /// <param name="replacement">Replacement text. <c>$1</c> and <c>${name}</c> expand to captures.</param>
  /// <param name="root">Directory to search.</param>
  /// <param name="globPattern">Glob passed to <see cref="FileSystem.FindCriteria.Name"/>.</param>
  /// <param name="options">Replacement options. <see langword="null"/> uses defaults.</param>
  /// <returns>
  /// One stdout line <c>path: N replacement(s)</c> per changed file.
  /// Exit code 0 when the walk finishes, 1 on error.
  /// </returns>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI command wrapper: unexpected failures should return error CommandOutput, not throw."
  )]
  public static CommandOutput ReplaceInFiles(
    string pattern,
    string replacement,
    string root,
    string globPattern,
    ReplaceInFilesOptions? options = null)
  {
    try
    {
      ReplaceInFilesOptions resolved = TextPatterns.Replace(options);
      Regex regex = TextPatterns.Compile(pattern, resolved);
      ArgumentNullException.ThrowIfNull(replacement);
      var query = TextFileQuery.FromGlob(globPattern, resolved.Include, resolved.Exclude);
      return Replace(regex, replacement, TextFiles.EnumerateGlob(root, query), resolved);
    }
    catch (Exception exception)
    {
      return TextCommand.Fail("ReplaceInFiles", root, exception, TextCommand.NoMatchExitCode);
    }
  }

  private static CommandOutput Replace(
    Regex regex,
    string replacement,
    IEnumerable<string> files,
    ReplaceInFilesOptions options)
  {
    StringBuilder stdout = new();
    StringBuilder stderr = new();
    bool error = false;
    foreach (string file in files)
    {
      try
      {
        ReplaceResult result = TextReplace.ReplaceFile(regex, replacement, file, options);
        if (!result.Changed)
        {
          continue;
        }

        if (stdout.Length > 0)
        {
          stdout.Append('\n');
        }

        stdout.Append(result.Path);
        stdout.Append(": ");
        stdout.Append(result.ReplacementCount.ToString(CultureInfo.InvariantCulture));
        stdout.Append(" replacement(s)");
      }
      catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
      {
        error = true;
        if (stderr.Length > 0)
        {
          stderr.Append('\n');
        }

        stderr.Append("ReplaceInFiles: ");
        stderr.Append(file);
        stderr.Append(": ");
        stderr.Append(TextCommand.DescribeIo(exception));
      }
    }

    return new CommandOutput(stdout.ToString(), stderr.ToString(), error ? TextCommand.NoMatchExitCode : 0);
  }
}
