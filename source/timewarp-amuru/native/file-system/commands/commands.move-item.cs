#region Purpose
// Commands API wrapper for MoveItem that returns CommandOutput instead of throwing.
#endregion

#region Design
// Delegates to Direct.MoveItem and maps exceptions to stderr + non-zero exit codes.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
  /// <summary>
  /// Moves or renames a file or directory and returns the result as CommandOutput.
  /// </summary>
  /// <param name="source">Source file or directory.</param>
  /// <param name="destination">Destination path.</param>
  /// <param name="overwrite">When true, replaces an existing file at the destination.</param>
  /// <returns>CommandOutput indicating success or failure.</returns>
  public static CommandOutput MoveItem(string source, string destination, bool overwrite = false)
  {
    return Invoke("MoveItem", source, () => Direct.MoveItem(source, destination, overwrite));
  }

  /// <summary>
  /// Bash-style alias for MoveItem.
  /// </summary>
  public static CommandOutput Mv(string source, string destination, bool overwrite = false) =>
    MoveItem(source, destination, overwrite);
}
