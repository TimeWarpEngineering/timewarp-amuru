#region Purpose
// Entry point for the fluent shell command execution API
// Provides static methods to create command builders for generic command execution
#endregion

#region Design
// - Static class serving as the primary API entry point
// - Builder() method creates a ShellBuilder for configuring command execution
// - Used for executing arbitrary executables (not just fzf)
// - Mirrors Fzf.Builder() for consistent API design across the library
// - Returns ShellBuilder which supports RunAsync, CaptureAsync, Pipe, etc.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for generic command execution.
/// </summary>
public static class Shell
{
  /// <summary>
  /// Creates a fluent builder for executing a command.
  /// </summary>
  /// <param name="executable">The executable or command to run</param>
  /// <returns>A ShellBuilder for configuring the command</returns>
  public static ShellBuilder Builder(string executable)
  {
    return new ShellBuilder(executable);
  }

  /// <summary>
  /// Builds an executable command directly from its parts with a reusable options object.
  /// This is the supported construction path for tool-wrapper builders (e.g. TimeWarp.Amuru.Tools)
  /// that assemble arguments and options themselves; for ad-hoc use prefer <see cref="Builder"/>.
  /// Invalid input (empty executable, null options) yields a command that reports
  /// <see cref="CommandResult.NeverRanExitCode"/> instead of throwing.
  /// </summary>
  /// <param name="executable">The executable or command to run</param>
  /// <param name="arguments">The command arguments</param>
  /// <param name="options">Execution options (working directory, environment, validation)</param>
  /// <param name="standardInput">Optional text piped to the command's standard input</param>
  /// <returns>A CommandResult ready to execute</returns>
  public static CommandResult Run(string executable, string[]? arguments, CommandOptions options, string? standardInput = null)
  {
    return CommandExtensions.Run(executable, arguments, options, standardInput);
  }
}