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