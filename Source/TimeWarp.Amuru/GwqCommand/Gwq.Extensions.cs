namespace TimeWarp.Amuru;

/// <summary>
/// Extension methods for integrating gwq with CommandResult.
/// </summary>
public static class GwqExtensions
{
  /// <summary>
  /// Lists worktrees and pipes to another command.
  /// </summary>
  /// <param name="builder">The gwq builder</param>
  /// <param name="command">Command to pipe to</param>
  /// <param name="arguments">Arguments for the piped command</param>
  /// <returns>A CommandResult for the piped command</returns>
  public static CommandResult PipeTo(this GwqBuilder builder, string command, params string[] arguments)
  {
    return builder.Build().Pipe(command, arguments);
  }

  /// <summary>
  /// Lists worktrees and selects one with FZF.
  /// </summary>
  /// <param name="builder">The gwq builder configured for list</param>
  /// <param name="configureFzf">Optional FZF configuration</param>
  /// <returns>A CommandResult with the selected worktree</returns>
  public static CommandResult SelectWithFzf(this GwqBuilder builder, Action<FzfBuilder>? configureFzf = null)
  {
    FzfBuilder fzfBuilder = Fzf.Builder();
    configureFzf?.Invoke(fzfBuilder);
    
    return builder.Build().Pipe("fzf", ExtractFzfArguments(fzfBuilder));
  }

  private static string[] ExtractFzfArguments(FzfBuilder fzfBuilder)
  {
    // This would need to be implemented to extract arguments from FzfBuilder
    // For now, return empty array
    return [];
  }
}