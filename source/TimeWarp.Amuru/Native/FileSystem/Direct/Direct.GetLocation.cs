namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Direct
{
  /// <summary>
  /// Gets the current working directory.
  /// </summary>
  /// <returns>The current working directory path</returns>
#pragma warning disable CA1024 // Use properties where appropriate
  public static string GetLocation() => Environment.CurrentDirectory;
#pragma warning restore CA1024 // Use properties where appropriate

  /// <summary>
  /// Bash-style alias for GetLocation.
  /// </summary>
  public static string Pwd() => GetLocation();
}