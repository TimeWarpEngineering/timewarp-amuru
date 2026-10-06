#region Purpose
// Direct API for testing whether a path exists, and whether it is a file or directory.
#endregion

#region Design
// File.Exists / Directory.Exists after Path.GetFullPath. Null or whitespace returns
// false rather than throwing so callers can test untrusted strings. Invalid path
// characters also return false.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Direct
{
  /// <summary>
  /// Returns whether a path exists as a file or directory.
  /// </summary>
  /// <param name="path">Path to test.</param>
  /// <returns><see langword="true"/> when the path exists.</returns>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "TestPath treats malformed paths as non-existent rather than throwing."
  )]
  public static bool TestPath(string path)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return false;
    }

    try
    {
      string fullPath = Path.GetFullPath(path);
      return File.Exists(fullPath) || Directory.Exists(fullPath);
    }
    catch (Exception)
    {
      return false;
    }
  }

  /// <summary>
  /// Returns whether a path exists as the specified item type.
  /// </summary>
  /// <param name="path">Path to test.</param>
  /// <param name="itemType">Required item type.</param>
  /// <returns><see langword="true"/> when the path exists as <paramref name="itemType"/>.</returns>
  /// <exception cref="ArgumentOutOfRangeException">When <paramref name="itemType"/> is not a defined value.</exception>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "TestPath treats malformed paths as non-existent rather than throwing."
  )]
  public static bool TestPath(string path, ItemType itemType)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return false;
    }

    if (itemType is not ItemType.File and not ItemType.Directory)
    {
      throw new ArgumentOutOfRangeException(nameof(itemType), itemType, "Unknown item type.");
    }

    try
    {
      string fullPath = Path.GetFullPath(path);
      return itemType == ItemType.File
        ? File.Exists(fullPath)
        : Directory.Exists(fullPath);
    }
    catch (Exception)
    {
      return false;
    }
  }

  /// <summary>
  /// Bash-style alias for TestPath.
  /// </summary>
  public static bool Test(string path) => TestPath(path);

  /// <summary>
  /// Bash-style alias for TestPath with an item type.
  /// </summary>
  public static bool Test(string path, ItemType itemType) => TestPath(path, itemType);
}
