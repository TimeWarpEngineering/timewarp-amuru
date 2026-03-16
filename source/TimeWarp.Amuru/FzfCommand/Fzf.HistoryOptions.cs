namespace TimeWarp.Amuru;

public partial class FzfBuilder
{
  // History Options
  /// <summary>
  /// Specifies the history file.
  /// </summary>
  /// <param name="file">History file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithHistory(string file)
  {
    Arguments.Add($"--history={file}");
    return this;
  }

  /// <summary>
  /// Specifies the maximum number of history entries.
  /// </summary>
  /// <param name="size">Maximum history size</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithHistorySize(int size)
  {
    Arguments.Add($"--history-size={size}");
    return this;
  }
}