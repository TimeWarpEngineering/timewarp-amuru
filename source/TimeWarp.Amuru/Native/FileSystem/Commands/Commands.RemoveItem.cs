namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
  /// <summary>
  /// Removes a file or directory and returns result as CommandOutput.
  /// </summary>
  /// <param name="path">Path to the file or directory to remove</param>
  /// <param name="recursive">If true, removes directories and their contents recursively</param>
  /// <param name="force">If true, removes read-only files</param>
  /// <returns>CommandOutput indicating success or failure</returns>
  public static CommandOutput RemoveItem(string path, bool recursive = false, bool force = false)
  {
    try
    {
      Direct.RemoveItem(path, recursive, force);
      return new CommandOutput(
        string.Empty,
        string.Empty,
        0
      );
    }
    catch (FileNotFoundException)
    {
      return new CommandOutput(
        string.Empty,
        $"RemoveItem: {path}: No such file or directory",
        1
      );
    }
    catch (DirectoryNotFoundException)
    {
      return new CommandOutput(
        string.Empty,
        $"RemoveItem: {path}: No such file or directory",
        1
      );
    }
    catch (UnauthorizedAccessException)
    {
      return new CommandOutput(
        string.Empty,
        $"RemoveItem: {path}: Permission denied",
        1
      );
    }
    catch (IOException ex)
    {
      return new CommandOutput(
        string.Empty,
        $"RemoveItem: {path}: {ex.Message}",
        1
      );
    }
    catch (Exception ex)
    {
      return new CommandOutput(
        string.Empty,
        $"RemoveItem: {path}: {ex.Message}",
        1
      );
    }
  }

  /// <summary>
  /// Bash-style alias for RemoveItem.
  /// </summary>
  public static CommandOutput Rm(string path, bool recursive = false, bool force = false) =>
    RemoveItem(path, recursive, force);
}