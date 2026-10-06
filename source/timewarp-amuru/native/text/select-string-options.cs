#region Purpose
// Options for SelectString (grep): literal vs regex, invert, context, and file globs.
#endregion

#region Design
// Line numbers are always present on TextMatch, so there is no switch for them.
// MaxMatches counts matching lines per file, matching grep -m. Include and Exclude are name globs
// applied with FindCriteria / GlobMatcher, not a second glob engine.
#endregion

namespace TimeWarp.Amuru.Native.Text;

/// <summary>
/// Options for <see cref="Direct.SelectString(string, string, SelectStringOptions?, CancellationToken)"/>
/// and <see cref="Commands.SelectString(string, string, SelectStringOptions?)"/>.
/// </summary>
public sealed class SelectStringOptions
{
  /// <summary>
  /// Gets a value indicating whether <c>pattern</c> is a literal string.
  /// When <see langword="false"/>, the pattern is a regular expression.
  /// </summary>
  public bool SimpleMatch { get; init; }

  /// <summary>
  /// Gets a value indicating whether matching ignores case.
  /// </summary>
  public bool CaseInsensitive { get; init; }

  /// <summary>
  /// Gets a value indicating whether to emit lines that do not match.
  /// </summary>
  public bool InvertMatch { get; init; }

  /// <summary>
  /// Gets the number of lines before a match to include in <see cref="TextMatch.ContextBefore"/>.
  /// </summary>
  public int ContextBefore { get; init; }

  /// <summary>
  /// Gets the number of lines after a match to include in <see cref="TextMatch.ContextAfter"/>.
  /// </summary>
  public int ContextAfter { get; init; }

  /// <summary>
  /// Gets the maximum number of matching lines per file, like <c>grep -m</c>.
  /// Every hit on a counted line is still returned. <see langword="null"/> means no limit.
  /// </summary>
  public int? MaxMatches { get; init; }

  /// <summary>
  /// Gets a glob a file must match to be searched.
  /// Matched against the file name, or against the relative path when the glob contains a
  /// directory separator or <c>**</c>.
  /// </summary>
  public string? Include { get; init; }

  /// <summary>
  /// Gets a glob of files to skip.
  /// Matched against the file name, or against the relative path when the glob contains a
  /// directory separator or <c>**</c>.
  /// </summary>
  public string? Exclude { get; init; }
}
