#region Purpose
// Represents a single line of command output with metadata about its source stream
// Used for streaming and capturing output with line-level detail
#endregion

#region Design
// - Immutable value-like class holding text, error flag, and timestamp
// - IsError distinguishes stdout (false) from stderr (true)
// - Timestamp tracks when the line was produced (defaults to UtcNow)
// - Two constructors: one with explicit timestamp, one with automatic UtcNow
// - Used by StreamCombinedAsync and CaptureAsync for timestamped line-by-line output
// - CommandOutput aggregates OutputLine instances for post-execution analysis
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Represents a single line of command output with metadata about its source stream.
/// </summary>
public class OutputLine
{
  /// <summary>
  /// Gets the text content of the output line.
  /// </summary>
  public string Text { get; }

  /// <summary>
  /// Gets whether this line came from stderr (true) or stdout (false).
  /// </summary>
  public bool IsError { get; }

  /// <summary>
  /// Gets the timestamp when this line was produced.
  /// </summary>
  public DateTime Timestamp { get; }

  /// <summary>
  /// Initializes a new instance of the OutputLine class.
  /// </summary>
  /// <param name="text">The text content of the line</param>
  /// <param name="isError">Whether this line is from stderr</param>
  /// <param name="timestamp">When the line was produced</param>
  public OutputLine(string text, bool isError, DateTime timestamp)
  {
    Text = text ?? string.Empty;
    IsError = isError;
    Timestamp = timestamp;
  }

  /// <summary>
  /// Initializes a new instance of the OutputLine class with the current timestamp.
  /// </summary>
  /// <param name="text">The text content of the line</param>
  /// <param name="isError">Whether this line is from stderr</param>
  public OutputLine(string text, bool isError) : this(text, isError, DateTime.UtcNow)
  {
  }
}