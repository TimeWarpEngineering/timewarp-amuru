namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
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
}