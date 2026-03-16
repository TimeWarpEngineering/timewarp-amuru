namespace TimeWarp.Amuru;

public partial class FzfBuilder
{
  // Input Methods
  /// <summary>
  /// Specifies input items as a string array.
  /// </summary>
  /// <param name="items">Items to select from</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder FromInput(params string[] items)
  {
    InputItems.AddRange(items);
    return this;
  }

  /// <summary>
  /// Specifies input items as a collection.
  /// </summary>
  /// <param name="items">Items to select from</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder FromInput(IEnumerable<string> items)
  {
    InputItems.AddRange(items);
    return this;
  }

  /// <summary>
  /// Specifies input from file glob patterns.
  /// </summary>
  /// <param name="pattern">File glob pattern</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder FromFiles(string pattern)
  {
    InputGlob = pattern;
    return this;
  }

  /// <summary>
  /// Specifies input from a command.
  /// </summary>
  /// <param name="command">Command to execute for input</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder FromCommand(string command)
  {
    InputCommand = command;
    return this;
  }

  /// <summary>
  /// Uses standard input as the input source.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder FromStdin()
  {
    UseStdin = true;
    return this;
  }
}