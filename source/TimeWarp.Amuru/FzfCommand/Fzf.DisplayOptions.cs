namespace TimeWarp.Amuru;

public partial class FzfBuilder
{
  // Display Options
  /// <summary>
  /// Enables processing of ANSI color codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithAnsi()
  {
    Arguments.Add("--ansi");
    return this;
  }

  /// <summary>
  /// Specifies the number of spaces for a tab character.
  /// </summary>
  /// <param name="spaces">Number of spaces per tab</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithTabstop(int spaces)
  {
    Arguments.Add($"--tabstop={spaces}");
    return this;
  }

  /// <summary>
  /// Specifies the color scheme.
  /// </summary>
  /// <param name="colors">Color specification</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithColor(string colors)
  {
    Arguments.Add($"--color={colors}");
    return this;
  }

  /// <summary>
  /// Disables bold text.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithNoBold()
  {
    Arguments.Add("--no-bold");
    return this;
  }
}