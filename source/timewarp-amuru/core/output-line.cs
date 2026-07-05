#region Purpose
// Represents a single line of command output with metadata about its source stream
// Used for streaming and capturing output with line-level detail
#endregion

#region Design
// - Immutable record holding text, error flag, and timestamp (value equality)
// - IsError distinguishes stdout (false) from stderr (true)
// - Timestamp tracks when the line was produced (defaults to UtcNow)
// - Used by StreamCombinedAsync and CaptureAsync for timestamped line-by-line output
// - CommandOutput aggregates OutputLine instances for post-execution analysis
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Represents a single line of command output with metadata about its source stream.
/// </summary>
/// <param name="Text">The text content of the line</param>
/// <param name="IsError">Whether this line came from stderr (true) or stdout (false)</param>
/// <param name="Timestamp">When the line was produced</param>
public sealed record OutputLine(string Text, bool IsError, DateTimeOffset Timestamp)
{
  /// <summary>
  /// Initializes a new instance of the OutputLine record with the current timestamp.
  /// </summary>
  /// <param name="text">The text content of the line</param>
  /// <param name="isError">Whether this line is from stderr</param>
  public OutputLine(string text, bool isError) : this(text ?? string.Empty, isError, DateTimeOffset.UtcNow) { }
}
