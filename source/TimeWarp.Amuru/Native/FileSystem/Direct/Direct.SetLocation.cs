namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Direct
{
  /// <summary>
  /// Sets the current working directory.
  /// </summary>
  /// <param name="path">The new working directory path</param>
  /// <exception cref="DirectoryNotFoundException">When the directory doesn't exist</exception>
  public static void SetLocation(string path)
  {
    Environment.CurrentDirectory = path;
  }

  /// <summary>
  /// Bash-style alias for SetLocation.
  /// </summary>
  public static void Cd(string path) => SetLocation(path);
}