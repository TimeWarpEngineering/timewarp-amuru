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
  public static CommandResult SelectWithFzf(this CommandResult command, Action<FzfBuilder>? configure = null)
  {
    FzfBuilder fzfBuilder = new();
    configure?.Invoke(fzfBuilder);
    
    CommandResult fzfArguments = fzfBuilder.Build();
    return command.Pipe("fzf", ExtractFzfArguments(fzfArguments));
  }

  private static string[] ExtractFzfArguments(CommandResult fzfCommand)
  {
    // This is a simplified implementation
    // In a real implementation, you'd extract the arguments from the FzfBuilder
    return [];
  }
}