#region Purpose
// Returns Environment.CurrentDirectory as a string.
#endregion

#region Design
// Kept as a method, with CA1024 suppressed, so the Direct surface stays verb-shaped beside SetLocation.
// Pwd is the alias and calls the same method.
#endregion

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