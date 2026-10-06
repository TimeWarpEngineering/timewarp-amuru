#region Purpose
// Direct API for moving and renaming files and directories.
#endregion

#region Design
// File.Move / Directory.Move first. Cross-volume Directory.Move throws IOException;
// then copy+delete, skipping reparse children on the copy walk. Copy failure may leave a
// partial destination; delete failure after a successful copy may leave data in both places.
// Destination-is-directory matches mv: move into dest under the source name. overwrite
// replaces an existing file; an existing destination directory always throws.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Direct
{
  /// <summary>
  /// Moves or renames a file or directory.
  /// </summary>
  /// <param name="source">Source file or directory.</param>
  /// <param name="destination">Destination path. An existing directory receives the source under its original name.</param>
  /// <param name="overwrite">When true, replaces an existing file at the destination.</param>
  /// <exception cref="FileNotFoundException">When the source does not exist.</exception>
  /// <exception cref="IOException">When an existing file is not overwritten, when a destination directory already exists, or when the move fails.</exception>
  /// <exception cref="UnauthorizedAccessException">When lacking permission.</exception>
  /// <exception cref="ArgumentException">When <paramref name="source"/> or <paramref name="destination"/> is null or whitespace.</exception>
  /// <remarks>
  /// Same-volume moves use <see cref="File.Move(string, string, bool)"/> / <see cref="Directory.Move(string, string)"/>.
  /// When that throws <see cref="IOException"/> and the source still exists (typical for cross-volume moves),
  /// the fallback is copy then delete the source. A failed copy may leave a partial destination.
  /// A failed delete after a successful copy may leave data in both places.
  /// </remarks>
  public static void MoveItem(string source, string destination, bool overwrite = false)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(source);
    ArgumentException.ThrowIfNullOrWhiteSpace(destination);

    string sourcePath = FileSystemWalk.NormalizePath(source);
    string destinationPath = FileSystemWalk.NormalizePath(destination);

    if (string.Equals(sourcePath, destinationPath, OperatingSystem.IsWindows()
      ? StringComparison.OrdinalIgnoreCase
      : StringComparison.Ordinal))
    {
      return;
    }

    bool sourceIsDirectory = Directory.Exists(sourcePath);
    if (!sourceIsDirectory && !File.Exists(sourcePath))
    {
      throw new FileNotFoundException($"Path not found: {source}", source);
    }

    string finalDestination = sourceIsDirectory
      ? FileSystemWalk.ResolveDirectoryDestination(sourcePath, destinationPath, "MoveItem")
      : FileSystemWalk.ResolveFileDestination(sourcePath, destinationPath);

    if (sourceIsDirectory)
    {
      MoveDirectory(sourcePath, finalDestination, overwrite);
    }
    else
    {
      MoveFile(sourcePath, finalDestination, overwrite);
    }
  }

  /// <summary>
  /// Bash-style alias for MoveItem.
  /// </summary>
  public static void Mv(string source, string destination, bool overwrite = false) =>
    MoveItem(source, destination, overwrite);

  private static void MoveFile(string sourcePath, string destinationPath, bool overwrite)
  {
    if (Directory.Exists(destinationPath))
    {
      throw new IOException($"MoveItem: {destinationPath}: cannot overwrite directory with file");
    }

    string? parent = Path.GetDirectoryName(destinationPath);
    if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
    {
      throw new DirectoryNotFoundException($"Path not found: {parent}");
    }

    try
    {
      File.Move(sourcePath, destinationPath, overwrite);
    }
    catch (IOException) when (!File.Exists(destinationPath) || overwrite)
    {
      CopyItem(sourcePath, destinationPath, recursive: false, overwrite, preserveAttributes: true);
      RemoveItem(sourcePath, recursive: false, force: true);
    }
  }

  private static void MoveDirectory(string sourcePath, string destinationPath, bool overwrite)
  {
    if (File.Exists(destinationPath))
    {
      throw new IOException($"MoveItem: {destinationPath}: cannot overwrite non-directory");
    }

    if (Directory.Exists(destinationPath))
    {
      throw new IOException($"MoveItem: {destinationPath}: directory exists");
    }

    try
    {
      Directory.Move(sourcePath, destinationPath);
    }
    catch (IOException) when (Directory.Exists(sourcePath))
    {
      CopyItem(sourcePath, destinationPath, recursive: true, overwrite, preserveAttributes: true);
      RemoveItem(sourcePath, recursive: true, force: true);
    }
  }
}
