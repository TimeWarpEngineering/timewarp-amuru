#region Purpose
// Commands API for reading file content into a CommandOutput stdout payload.
#endregion

#region Design
// Uses synchronous File.ReadAllLines so the Commands surface stays sync and avoids
// sync-over-async. Stdout shape is lines joined with '\n', matching prior behavior.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

/// <summary>
/// Shell-style API for file system operations.
/// These methods follow shell conventions: they never throw exceptions and return CommandOutput.
/// Errors are reported via stderr and non-zero exit codes.
/// </summary>
public static partial class Commands
{
  /// <summary>
  /// Reads file content and returns it as CommandOutput.
  /// </summary>
  /// <param name="path">Path to the file to read</param>
  /// <returns>CommandOutput with file content in stdout, or error in stderr</returns>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI command wrapper: unexpected failures should return error CommandOutput, not throw."
  )]
  public static CommandOutput GetContent(string path)
  {
    try
    {
      string[] lines = File.ReadAllLines(path);

      return new CommandOutput(
        string.Join("\n", lines),
        string.Empty,
        0
      );
    }
    catch (FileNotFoundException)
    {
      return new CommandOutput(
        string.Empty,
        $"GetContent: {path}: No such file or directory",
        1
      );
    }
    catch (UnauthorizedAccessException)
    {
      return new CommandOutput(
        string.Empty,
        $"GetContent: {path}: Permission denied",
        1
      );
    }
    catch (Exception ex)
    {
      return new CommandOutput(
        string.Empty,
        $"GetContent: {path}: {ex.Message}",
        1
      );
    }
  }

  /// <summary>
  /// Bash-style alias for GetContent.
  /// </summary>
  public static CommandOutput Cat(string path) => GetContent(path);
}
