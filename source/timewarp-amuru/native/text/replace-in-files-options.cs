#region Purpose
// Options for ReplaceInFiles (sed -i): dry run, backup, limits, and regex flags.
#endregion

#region Design
// DryRun reports ReplaceResult and writes nothing, including no .bak file.
// Backup appends ".bak" beside a file only when that file's content changes.
// Multiline and Singleline map to RegexOptions and apply to the whole file.
#endregion

namespace TimeWarp.Amuru.Native.Text;

/// <summary>
/// Options for <see cref="Direct.ReplaceInFiles(string, string, string, ReplaceInFilesOptions?, CancellationToken)"/>
/// and <see cref="Commands.ReplaceInFiles(string, string, string, ReplaceInFilesOptions?)"/>.
/// </summary>
public sealed class ReplaceInFilesOptions
{
  /// <summary>
  /// Gets a value indicating whether <c>pattern</c> is a literal string.
  /// When <see langword="false"/>, the pattern is a regular expression.
  /// The replacement always honors <c>$1</c> and <c>${name}</c> substitutions.
  /// </summary>
  public bool SimpleMatch { get; init; }

  /// <summary>
  /// Gets a value indicating whether to report changes without modifying files.
  /// </summary>
  public bool DryRun { get; init; }

  /// <summary>
  /// Gets a value indicating whether to copy the original to <c>path.bak</c> before replacing it.
  /// The copy is made only when the file's content changes. Ignored when <see cref="DryRun"/> is set.
  /// </summary>
  public bool Backup { get; init; }

  /// <summary>
  /// Gets the maximum number of replacements applied in one file.
  /// <see langword="null"/> means no limit.
  /// </summary>
  public int? MaxReplacements { get; init; }

  /// <summary>
  /// Gets a value indicating whether matching ignores case.
  /// </summary>
  public bool CaseInsensitive { get; init; }

  /// <summary>
  /// Gets a value indicating whether <see cref="RegexOptions.Multiline"/> is set,
  /// so <c>^</c> and <c>$</c> match at each line boundary.
  /// </summary>
  public bool Multiline { get; init; }

  /// <summary>
  /// Gets a value indicating whether <see cref="RegexOptions.Singleline"/> is set,
  /// so <c>.</c> matches newline characters.
  /// </summary>
  public bool Singleline { get; init; }

  /// <summary>
  /// Gets a glob a file must match to be edited.
  /// </summary>
  public string? Include { get; init; }

  /// <summary>
  /// Gets a glob of files to skip.
  /// </summary>
  public string? Exclude { get; init; }
}
