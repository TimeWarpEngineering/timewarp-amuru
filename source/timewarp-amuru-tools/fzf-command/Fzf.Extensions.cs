#region Purpose
// Pipes an existing command into fzf using the builder's option flags.
#endregion

#region Design
// ExtractFzfArguments reads the builder's argument list. Re-parsing a built CommandResult
// would split CliWrap's escaped argument string and drop flags that contain spaces.
// Input methods on the builder are not applied: the upstream command already owns stdin.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Extension methods for integrating Fzf with CommandResult.
/// </summary>
public static class FzfExtensions
{
  /// <summary>
  /// Pipes the command output to Fzf for interactive selection.
  /// </summary>
  /// <param name="command">The command to pipe from</param>
  /// <param name="configure">Optional Fzf configuration</param>
  /// <returns>A CommandResult with Fzf selection</returns>
  /// <remarks>
  /// Only option flags from <paramref name="configure"/> are passed to fzf.
  /// Input methods on the builder are ignored because <paramref name="command"/> supplies stdin.
  /// </remarks>
  public static CommandResult SelectWithFzf(this CommandResult command, Action<FzfBuilder>? configure = null)
  {
    ArgumentNullException.ThrowIfNull(command);
    FzfBuilder fzfBuilder = new();
    configure?.Invoke(fzfBuilder);
    return command.Pipe("fzf", ExtractFzfArguments(fzfBuilder));
  }

  private static string[] ExtractFzfArguments(FzfBuilder fzfBuilder)
  {
    return fzfBuilder.CopyArguments();
  }
}