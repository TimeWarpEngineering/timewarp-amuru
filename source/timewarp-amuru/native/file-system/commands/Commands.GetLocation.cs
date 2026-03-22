#region Purpose
// TODO: Add purpose description
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
  /// <summary>
  /// Gets the current working directory as CommandOutput.
  /// </summary>
  /// <returns>CommandOutput with current directory in stdout</returns>
  public static CommandOutput GetLocation()
  {
    return new CommandOutput(
      Direct.GetLocation(),
      string.Empty,
      0
    );
  }

  /// <summary>
  /// Bash-style alias for GetLocation.
  /// </summary>
  public static CommandOutput Pwd() => GetLocation();
}