namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
  /// <summary>
  /// Sets the current working directory.
  /// </summary>
  /// <param name="path">The new working directory path</param>
  /// <returns>CommandOutput indicating success or failure</returns>
  public static CommandOutput SetLocation(string path)
  {
    try
    {
      Direct.SetLocation(path);
      return new CommandOutput(
        string.Empty,
        string.Empty,
        0
      );
    }
    catch (DirectoryNotFoundException)
    {
      return new CommandOutput(
        string.Empty,
        $"SetLocation: {path}: No such file or directory",
        1
      );
    }
    catch (UnauthorizedAccessException)
    {
      return new CommandOutput(
        string.Empty,
        $"SetLocation: {path}: Permission denied",
        1
      );
    }
    catch (Exception ex)
    {
      return new CommandOutput(
        string.Empty,
        $"SetLocation: {path}: {ex.Message}",
        1
      );
    }
  }

  /// <summary>
  /// Bash-style alias for SetLocation.
  /// </summary>
  public static CommandOutput Cd(string path) => SetLocation(path);
}