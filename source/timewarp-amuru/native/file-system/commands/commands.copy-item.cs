#region Purpose
// Commands API wrapper for CopyItem that returns CommandOutput instead of throwing.
#endregion

#region Design
// Delegates to Direct.CopyItem and maps exceptions to stderr + non-zero exit codes.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Commands
{
  /// <summary>
  /// Copies a file or directory and returns the result as CommandOutput.
  /// </summary>
  /// <param name="source">Source file, directory, or glob pattern.</param>
  /// <param name="destination">Destination path.</param>
  /// <param name="recursive">Required when <paramref name="source"/> is a directory.</param>
  /// <param name="overwrite">When true, overwrites existing files.</param>
  /// <param name="preserveAttributes">When true, copies timestamps, attributes, and Unix mode.</param>
  /// <returns>CommandOutput indicating success or failure.</returns>
  public static CommandOutput CopyItem(
    string source,
    string destination,
    bool recursive = false,
    bool overwrite = false,
    bool preserveAttributes = true)
  {
    return Invoke("CopyItem", source, () =>
      Direct.CopyItem(source, destination, recursive, overwrite, preserveAttributes));
  }

  /// <summary>
  /// Bash-style alias for CopyItem.
  /// </summary>
  public static CommandOutput Cp(
    string source,
    string destination,
    bool recursive = false,
    bool overwrite = false,
    bool preserveAttributes = true) =>
    CopyItem(source, destination, recursive, overwrite, preserveAttributes);
}
