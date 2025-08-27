namespace TimeWarp.Amuru;

using System.Threading;

/// <summary>
/// Global configuration for TimeWarp.Cli, including command path overrides for testing.
/// </summary>
public static class CliConfiguration
{
  private static readonly Dictionary<string, string> CommandPaths = new();
  private static readonly Lock Lock = new();
  
  /// <summary>
  /// Sets a custom path for a command executable. Useful for testing with mock executables.
  /// </summary>
  /// <param name="command">The command name (e.g., "fzf", "git")</param>
  /// <param name="path">The full path to the executable</param>
  /// <exception cref="ArgumentNullException">Thrown when command or path is null</exception>
  /// <exception cref="FileNotFoundException">Thrown when the specified path does not exist</exception>
  /// <exception cref="UnauthorizedAccessException">Thrown when the file exists but is not executable</exception>
  /// <example>
  /// CliConfiguration.SetCommandPath("fzf", "/tmp/mock-bin/fzf");
  /// </example>
  public static void SetCommandPath(string command, string path)
  {
    ArgumentNullException.ThrowIfNull(command);
    ArgumentNullException.ThrowIfNull(path);
    
    // Validate that the file exists
    if (!File.Exists(path))
    {
      throw new FileNotFoundException($"The specified executable path does not exist: {path}", path);
    }
    
    // On Unix-like systems, check if the file is executable
    if (!OperatingSystem.IsWindows())
    {
      try
      {
        // Try to get file attributes to ensure we have access
        FileAttributes attributes = File.GetAttributes(path);
        
        // Check if it's a directory (not an executable file)
        if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
        {
          throw new ArgumentException($"The specified path is a directory, not an executable file: {path}", nameof(path));
        }
        
        // For Unix systems, we could check execute permission but File.GetAttributes doesn't provide that
        // The actual execute check will happen when CliWrap tries to run it
      }
      catch (UnauthorizedAccessException)
      {
        throw new UnauthorizedAccessException($"Cannot access the specified executable: {path}");
      }
    }
    
    lock (Lock)
    {
      CommandPaths[command] = path;
    }
  }
  
  /// <summary>
  /// Clears a custom command path, reverting to the default behavior.
  /// </summary>
  /// <param name="command">The command name to clear</param>
  public static void ClearCommandPath(string command)
  {
    ArgumentNullException.ThrowIfNull(command);
    
    lock (Lock)
    {
      CommandPaths.Remove(command);
    }
  }
  
  /// <summary>
  /// Clears all custom command paths, reverting all commands to default behavior.
  /// </summary>
  public static void Reset()
  {
    lock (Lock)
    {
      CommandPaths.Clear();
    }
  }
  
  /// <summary>
  /// Gets the configured path for a command, or returns the command itself if no custom path is set.
  /// </summary>
  /// <param name="command">The command to look up</param>
  /// <returns>The configured path if set, otherwise the original command</returns>
  internal static string GetCommandPath(string command)
  {
    lock (Lock)
    {
      return CommandPaths.TryGetValue(command, out string? path) ? path : command;
    }
  }
  
  /// <summary>
  /// Checks if a command has a custom path configured.
  /// </summary>
  /// <param name="command">The command to check</param>
  /// <returns>True if a custom path is configured, false otherwise</returns>
  public static bool HasCustomPath(string command)
  {
    ArgumentNullException.ThrowIfNull(command);
    
    lock (Lock)
    {
      return CommandPaths.ContainsKey(command);
    }
  }
  
  /// <summary>
  /// Gets all currently configured command paths.
  /// </summary>
  /// <returns>A read-only dictionary of command to path mappings</returns>
  public static IReadOnlyDictionary<string, string> AllCommandPaths
  {
    get
    {
      lock (Lock)
      {
        return new Dictionary<string, string>(CommandPaths);
      }
    }
  }
}