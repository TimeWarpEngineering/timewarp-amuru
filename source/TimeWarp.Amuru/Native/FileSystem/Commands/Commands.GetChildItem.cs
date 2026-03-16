namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
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
}