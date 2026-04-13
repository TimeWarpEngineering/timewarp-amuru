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
  /// <returns>A RunBuilder for configuring the command</returns>
  public static ShellBuilder Builder(string executable)
  {
    return new ShellBuilder(executable);
  }
}