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