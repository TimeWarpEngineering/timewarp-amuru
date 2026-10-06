#region Purpose
// History file and history size options for the fzf builder.
#endregion

#region Design
// The history path is passed through as `--history=` with no extra quoting. The caller supplies the path fzf opens.
#endregion

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