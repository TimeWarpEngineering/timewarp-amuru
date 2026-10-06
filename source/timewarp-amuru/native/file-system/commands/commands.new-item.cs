#region Purpose
// Commands API wrapper for NewItem (mkdir / touch) that returns CommandOutput.
#endregion

#region Design
// Delegates to Direct.NewItem. Stdout is the created path. Existing directory (mkdir)
// and existing file (touch) succeed, matching Direct.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
  /// <summary>
  /// Creates a file or directory and returns the result as CommandOutput.
  /// </summary>
  /// <param name="path">Path to create.</param>
  /// <param name="itemType">Whether to create a file or a directory.</param>
  /// <returns>CommandOutput with the created path in stdout, or error in stderr.</returns>
  public static CommandOutput NewItem(string path, ItemType itemType)
  {
    return Invoke("NewItem", path, () => Direct.NewItem(path, itemType).FullName);
  }

  /// <summary>
  /// Bash-style alias that creates a directory and any missing parents.
  /// </summary>
  public static CommandOutput Mkdir(string path) => NewItem(path, ItemType.Directory);

  /// <summary>
  /// Bash-style alias that creates an empty file or updates its last-write time.
  /// </summary>
  public static CommandOutput Touch(string path) => NewItem(path, ItemType.File);
}
