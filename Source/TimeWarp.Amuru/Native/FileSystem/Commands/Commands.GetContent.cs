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
  public static CommandOutput GetContent(string path)
  {
    try
    {
      var lines = new List<string>();

      // Use Direct API internally and collect results
      var task = Task.Run(async () =>
      {
        await foreach (string line in Direct.GetContent(path))
        {
          lines.Add(line);
        }
      });
      task.GetAwaiter().GetResult();

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