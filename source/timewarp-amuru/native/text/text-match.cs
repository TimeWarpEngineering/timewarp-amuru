#region Purpose
// One SelectString hit: path, line, the regular-expression match, and context lines.
#endregion

#region Design
// Match is the framework Match so callers read captures without a second type.
// An inverted hit still carries a Match; its Success flag is false.
// Context lists are snapshots, so later lines do not change an earlier result.
#endregion

namespace TimeWarp.Amuru.Native.Text;

/// <summary>
/// A line selected by <c>SelectString</c>.
/// </summary>
public sealed class TextMatch
{
  /// <summary>
  /// Gets the file path, or <c>-</c> when the input is a reader or stream without a path.
  /// </summary>
  public required string Path { get; init; }

  /// <summary>
  /// Gets the 1-based line number of <see cref="Line"/>.
  /// </summary>
  public required int LineNumber { get; init; }

  /// <summary>
  /// Gets the line text without its line terminator.
  /// </summary>
  public required string Line { get; init; }

  /// <summary>
  /// Gets the regular-expression match on <see cref="Line"/>, including its captures.
  /// For an inverted hit the match is unsuccessful.
  /// </summary>
  public required Match Match { get; init; }

  /// <summary>
  /// Gets the lines immediately before <see cref="Line"/>, oldest first.
  /// </summary>
  public required IReadOnlyList<string> ContextBefore { get; init; }

  /// <summary>
  /// Gets the lines immediately after <see cref="Line"/>.
  /// </summary>
  public required IReadOnlyList<string> ContextAfter { get; init; }
}
