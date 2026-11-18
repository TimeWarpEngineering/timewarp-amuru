#region Purpose
// Fluent builder for `gh pr checks`. Task 152.
#endregion

#region Design
// gh pr checks exits 8 while checks are pending and 1 when one failed. Those are reported through
// the exit code under the default validation, and the span's gh.exit_code shows which.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for <c>gh pr checks</c>.
/// </summary>
public sealed class GhPrChecksBuilder : GhJsonCommandBuilder<GhPrChecksBuilder>
{
  private readonly string? Selector;
  private bool Required;
  private bool Watch;
  private bool FailFast;
  private int? Interval;

  internal GhPrChecksBuilder(string command, string? selector) : base(command)
  {
    Selector = selector;
  }

  /// <summary>
  /// Shows only required checks with <c>--required</c>.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrChecksBuilder WithRequired()
  {
    Required = true;
    return this;
  }

  /// <summary>
  /// Waits until the checks finish with <c>--watch</c>.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrChecksBuilder WithWatch()
  {
    Watch = true;
    return this;
  }

  /// <summary>
  /// With <see cref="WithWatch"/>, stops at the first failure (<c>--fail-fast</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrChecksBuilder WithFailFast()
  {
    FailFast = true;
    return this;
  }

  /// <summary>
  /// With <see cref="WithWatch"/>, sets the refresh interval in seconds (<c>--interval</c>).
  /// </summary>
  /// <param name="seconds">A positive number of seconds</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrChecksBuilder WithInterval(int seconds)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seconds);
    Interval = seconds;
    return this;
  }

  private protected override void AppendVerbArguments(List<string> arguments)
  {
    if (!string.IsNullOrWhiteSpace(Selector))
    {
      arguments.Add(Selector);
    }

    AddFlag(arguments, "--required", Required);
    AddFlag(arguments, "--watch", Watch);
    AddFlag(arguments, "--fail-fast", FailFast);
    AddOption(arguments, "--interval", Interval?.ToString(CultureInfo.InvariantCulture));
  }
}
