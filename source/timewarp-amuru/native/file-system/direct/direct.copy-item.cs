#region Purpose
// Direct API for copying files and directory trees.
#endregion

#region Design
// File.Copy for files; recursive walk for directories. Reparse points are skipped
// on the walk (the named source is copied; children that are links are not followed
// or copied). Enumeration uses AttributesToSkip=None so Hidden/System and Unix
// dotfiles are included. Destination-is-directory matches cp: copy into dest under
// the source name. preserveAttributes copies timestamps, attributes, and Unix mode.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

public static partial class Direct
{
  /// <summary>
  /// Copies a file or directory to a new location.
  /// </summary>
  /// <param name="source">Source file, directory, or glob pattern.</param>
  /// <param name="destination">Destination path. An existing directory receives the source under its original name.</param>
  /// <param name="recursive">Required when <paramref name="source"/> is a directory.</param>
  /// <param name="overwrite">When true, overwrites existing files.</param>
  /// <param name="preserveAttributes">When true, copies timestamps, attributes, and Unix mode.</param>
  /// <exception cref="FileNotFoundException">When the source does not exist and is not a glob.</exception>
  /// <exception cref="DirectoryNotFoundException">When a glob root or destination parent is missing.</exception>
  /// <exception cref="IOException">When the source is a directory and <paramref name="recursive"/> is false, or the destination cannot be written.</exception>
  /// <exception cref="UnauthorizedAccessException">When lacking permission to read or write.</exception>
  /// <exception cref="ArgumentException">When <paramref name="source"/> or <paramref name="destination"/> is null or whitespace.</exception>
  public static void CopyItem(
    string source,
    string destination,
    bool recursive = false,
    bool overwrite = false,
    bool preserveAttributes = true)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(source);
    ArgumentException.ThrowIfNullOrWhiteSpace(destination);

    if (GlobMatcher.IsGlob(source))
    {
      CopyGlob(source, destination, recursive, overwrite, preserveAttributes);
      return;
    }

    string sourcePath = FileSystemWalk.NormalizePath(source);
    string destinationPath = FileSystemWalk.NormalizePath(destination);

    if (Directory.Exists(sourcePath))
    {
      if (!recursive)
      {
        throw new IOException($"CopyItem: {source}: is a directory");
      }

      string destDirectory = FileSystemWalk.ResolveDirectoryDestination(sourcePath, destinationPath, "CopyItem");
      CopyDirectory(sourcePath, destDirectory, overwrite, preserveAttributes);
    }
    else if (File.Exists(sourcePath))
    {
      string destFile = FileSystemWalk.ResolveFileDestination(sourcePath, destinationPath);
      CopyFile(sourcePath, destFile, overwrite, preserveAttributes);
    }
    else
    {
      throw new FileNotFoundException($"Path not found: {source}", source);
    }
  }

  /// <summary>
  /// Bash-style alias for CopyItem.
  /// </summary>
  public static void Cp(
    string source,
    string destination,
    bool recursive = false,
    bool overwrite = false,
    bool preserveAttributes = true) =>
    CopyItem(source, destination, recursive, overwrite, preserveAttributes);

  private static void CopyGlob(
    string source,
    string destination,
    bool recursive,
    bool overwrite,
    bool preserveAttributes)
  {
    string root = FileSystemWalk.ResolveRoot(source, out string? globPattern);
    bool effectiveRecursive = recursive || GlobMatcher.PatternImpliesRecursive(globPattern);
    List<FileSystemInfo> matches = [.. FileSystemWalk.Enumerate(root, effectiveRecursive, includeHidden: true, globPattern, excludePattern: null)];
    if (matches.Count == 0)
    {
      throw new FileNotFoundException($"Path not found: {source}", source);
    }

    string destinationPath = FileSystemWalk.NormalizePath(destination);
    bool destinationIsDirectory = Directory.Exists(destinationPath);

    if (matches.Count > 1 && !destinationIsDirectory)
    {
      throw new IOException($"CopyItem: {destination}: target is not a directory");
    }

    foreach (FileSystemInfo match in matches)
    {
      if (match is DirectoryInfo)
      {
        if (!effectiveRecursive)
        {
          continue;
        }

        string destDirectory = destinationIsDirectory
          ? Path.Combine(destinationPath, match.Name)
          : destinationPath;
        CopyDirectory(match.FullName, destDirectory, overwrite, preserveAttributes);
      }
      else
      {
        string destFile = destinationIsDirectory
          ? Path.Combine(destinationPath, match.Name)
          : destinationPath;
        CopyFile(match.FullName, destFile, overwrite, preserveAttributes);
      }
    }
  }

  private static void CopyDirectory(
    string sourcePath,
    string destinationPath,
    bool overwrite,
    bool preserveAttributes)
  {
    Directory.CreateDirectory(destinationPath);
    DirectoryInfo sourceDirectory = new(sourcePath);
    EnumerationOptions enumerationOptions = FileSystemWalk.CreateEnumerationOptions();

    foreach (FileInfo file in sourceDirectory.EnumerateFiles("*", enumerationOptions))
    {
      if (FileSystemWalk.IsReparsePoint(file))
      {
        continue;
      }

      CopyFile(file.FullName, Path.Combine(destinationPath, file.Name), overwrite, preserveAttributes);
    }

    foreach (DirectoryInfo child in sourceDirectory.EnumerateDirectories("*", enumerationOptions))
    {
      if (FileSystemWalk.IsReparsePoint(child))
      {
        continue;
      }

      CopyDirectory(child.FullName, Path.Combine(destinationPath, child.Name), overwrite, preserveAttributes);
    }

    if (preserveAttributes)
    {
      CopyDirectoryAttributes(sourceDirectory, new DirectoryInfo(destinationPath));
    }
  }

  private static void CopyFile(
    string sourcePath,
    string destinationPath,
    bool overwrite,
    bool preserveAttributes)
  {
    string? parent = Path.GetDirectoryName(destinationPath);
    if (!string.IsNullOrEmpty(parent))
    {
      Directory.CreateDirectory(parent);
    }

    if (overwrite && File.Exists(destinationPath))
    {
      FileInfo existing = new(destinationPath);
      if (existing.IsReadOnly)
      {
        existing.IsReadOnly = false;
      }
    }

    File.Copy(sourcePath, destinationPath, overwrite);

    if (preserveAttributes)
    {
      CopyFileAttributes(new FileInfo(sourcePath), new FileInfo(destinationPath));
    }
  }

  private static void CopyFileAttributes(FileInfo source, FileInfo destination)
  {
    destination.CreationTime = source.CreationTime;
    destination.LastWriteTime = source.LastWriteTime;
    destination.LastAccessTime = source.LastAccessTime;
    destination.Attributes = source.Attributes & ~(FileAttributes.Directory | FileAttributes.ReparsePoint);
    CopyUnixFileMode(source.FullName, destination.FullName);
  }

  private static void CopyDirectoryAttributes(DirectoryInfo source, DirectoryInfo destination)
  {
    destination.CreationTime = source.CreationTime;
    destination.LastWriteTime = source.LastWriteTime;
    destination.LastAccessTime = source.LastAccessTime;
    FileAttributes attributes = source.Attributes & ~FileAttributes.ReparsePoint;
    destination.Attributes = attributes;
    CopyUnixFileMode(source.FullName, destination.FullName);
  }

  private static void CopyUnixFileMode(string sourcePath, string destinationPath)
  {
    if (OperatingSystem.IsWindows())
    {
      return;
    }

    try
    {
      UnixFileMode unixFileMode = File.GetUnixFileMode(sourcePath);
      File.SetUnixFileMode(destinationPath, unixFileMode);
    }
    catch (PlatformNotSupportedException)
    {
    }
  }
}
