#region Purpose
// Direct API for changing the process working directory.
#endregion

#region Design
// Assigns Environment.CurrentDirectory, which is process-global. Concurrent callers
// (parallel tests or tasks) that resolve relative paths race on this shared value.
// Behavior is intentional and unchanged; prefer ScriptContext for scoped directory work.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Direct
{
  /// <summary>
  /// Sets the current working directory.
  /// </summary>
  /// <param name="path">The new working directory path</param>
  /// <exception cref="DirectoryNotFoundException">When the directory doesn't exist</exception>
  /// <remarks>
  /// Mutates process-global <see cref="Environment.CurrentDirectory"/>. Concurrent callers
  /// that resolve relative paths race on this process-wide value.
  /// </remarks>
  public static void SetLocation(string path)
  {
    Environment.CurrentDirectory = path;
  }

  /// <summary>
  /// Bash-style alias for SetLocation.
  /// </summary>
  public static void Cd(string path) => SetLocation(path);
}
