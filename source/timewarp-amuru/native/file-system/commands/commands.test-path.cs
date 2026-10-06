#region Purpose
// Commands API for TestPath: exit 0 when the path exists, exit 1 otherwise.
#endregion

#region Design
// Matches bash `test -e` / `test -f` / `test -d`: empty stdout, Success is the boolean.
// Delegates to Direct.TestPath so malformed paths are non-existent, not errors.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
  /// <summary>
  /// Tests whether a path exists. Exit code 0 means it exists; 1 means it does not.
  /// </summary>
  /// <param name="path">Path to test.</param>
  /// <returns>CommandOutput with empty stdout and exit 0 or 1.</returns>
  public static CommandOutput TestPath(string path)
  {
    bool exists = Direct.TestPath(path);
    return new CommandOutput(string.Empty, string.Empty, exists ? 0 : 1);
  }

  /// <summary>
  /// Tests whether a path exists as the specified item type.
  /// </summary>
  /// <param name="path">Path to test.</param>
  /// <param name="itemType">Required item type.</param>
  /// <returns>CommandOutput with empty stdout and exit 0 or 1.</returns>
  public static CommandOutput TestPath(string path, ItemType itemType)
  {
    bool exists = Direct.TestPath(path, itemType);
    return new CommandOutput(string.Empty, string.Empty, exists ? 0 : 1);
  }

  /// <summary>
  /// Bash-style alias for TestPath.
  /// </summary>
  public static CommandOutput Test(string path) => TestPath(path);

  /// <summary>
  /// Bash-style alias for TestPath with an item type.
  /// </summary>
  public static CommandOutput Test(string path, ItemType itemType) => TestPath(path, itemType);
}
