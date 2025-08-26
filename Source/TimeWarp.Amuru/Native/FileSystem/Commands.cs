namespace TimeWarp.Amuru.Native.FileSystem;

/// <summary>
/// Shell-style API for file system operations.
/// These methods follow shell conventions: they never throw exceptions and return CommandOutput.
/// Errors are reported via stderr and non-zero exit codes.
/// </summary>
public static class Commands
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

  /// <summary>
  /// Lists directory contents and returns them as CommandOutput.
  /// </summary>
  /// <param name="path">Path to the directory to list</param>
  /// <returns>CommandOutput with directory listing in stdout, or error in stderr</returns>
  public static CommandOutput GetChildItem(string path = ".")
  {
    try
    {
      var entries = new List<string>();
      
      // Use Direct API internally and collect results
      var task = Task.Run(async () =>
      {
        await foreach (FileSystemInfo entry in Direct.GetChildItem(path))
        {
          // Format similar to ls -la
          string type = entry is DirectoryInfo ? "d" : "-";
          string size = entry is FileInfo file ? file.Length.ToString(CultureInfo.InvariantCulture).PadLeft(10) : "<DIR>".PadLeft(10);
          string name = entry.Name;
          
          entries.Add($"{type}  {size}  {entry.LastWriteTime:yyyy-MM-dd HH:mm}  {name}");
        }
      });
      task.GetAwaiter().GetResult();
      
      return new CommandOutput(
        string.Join("\n", entries),
        string.Empty,
        0
      );
    }
    catch (DirectoryNotFoundException)
    {
      return new CommandOutput(
        string.Empty,
        $"GetChildItem: {path}: No such file or directory",
        1
      );
    }
    catch (UnauthorizedAccessException)
    {
      return new CommandOutput(
        string.Empty,
        $"GetChildItem: {path}: Permission denied",
        1
      );
    }
    catch (Exception ex)
    {
      return new CommandOutput(
        string.Empty,
        $"GetChildItem: {path}: {ex.Message}",
        1
      );
    }
  }

  /// <summary>
  /// Bash-style alias for GetChildItem.
  /// </summary>
  public static CommandOutput Ls(string path = ".") => GetChildItem(path);

  /// <summary>
  /// DOS-style alias for GetChildItem.
  /// </summary>
  public static CommandOutput Dir(string path = ".") => GetChildItem(path);

  /// <summary>
  /// Gets the current working directory as CommandOutput.
  /// </summary>
  /// <returns>CommandOutput with current directory in stdout</returns>
  public static CommandOutput GetLocation()
  {
    try
    {
      return new CommandOutput(
        Direct.GetLocation(),
        string.Empty,
        0
      );
    }
    catch (Exception ex)
    {
      return new CommandOutput(
        string.Empty,
        $"GetLocation: {ex.Message}",
        1
      );
    }
  }

  /// <summary>
  /// Bash-style alias for GetLocation.
  /// </summary>
  public static CommandOutput Pwd() => GetLocation();

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