#region Purpose
// Shared path normalization, reparse-safe directory walk, and glob filtering.
#endregion

#region Design
// Recursive walks enumerate one directory at a time and skip reparse points so
// directory symlinks/junctions are not followed (copy/move/find/list). Hidden
// filtering is opt-out: default includeHidden=true matches shipped GetChildItem.
// Paths are normalized with Path.GetFullPath.
#endregion

namespace TimeWarp.Amuru.Native.FileSystem;

internal static class FileSystemWalk
{
  public static string NormalizePath(string path)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    return Path.GetFullPath(path);
  }

  public static string ResolveRoot(string path, out string? globPattern)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    if (GlobMatcher.IsGlob(path))
    {
      (string root, string pattern) = GlobMatcher.SplitGlobPath(path);
      globPattern = pattern;
      return NormalizePath(root);
    }

    globPattern = null;
    return NormalizePath(path);
  }

  public static bool IsReparsePoint(FileSystemInfo fileSystemInfo)
  {
    ArgumentNullException.ThrowIfNull(fileSystemInfo);
    return (fileSystemInfo.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
  }

  public static bool IsHidden(FileSystemInfo fileSystemInfo)
  {
    ArgumentNullException.ThrowIfNull(fileSystemInfo);
    if ((fileSystemInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
    {
      return true;
    }

    string name = fileSystemInfo.Name;
    return name.StartsWith('.', StringComparison.Ordinal) && name is not "." and not "..";
  }

  public static EnumerationOptions CreateEnumerationOptions()
  {
    return new EnumerationOptions
    {
      RecurseSubdirectories = false,
      IgnoreInaccessible = false,
      AttributesToSkip = FileAttributes.None,
      ReturnSpecialDirectories = false
    };
  }

  public static IEnumerable<FileSystemInfo> Enumerate(
    string rootPath,
    bool recursive,
    bool includeHidden,
    string? includePattern,
    string? excludePattern)
  {
    string root = NormalizePath(rootPath);
    DirectoryInfo directory = new(root);
    if (!directory.Exists)
    {
      throw new DirectoryNotFoundException($"Path not found: {root}");
    }

    bool matchNameOnly = includePattern is not null
      && !includePattern.Contains('/', StringComparison.Ordinal)
      && !includePattern.Contains('\\', StringComparison.Ordinal)
      && !includePattern.Contains("**", StringComparison.Ordinal);

    foreach (FileSystemInfo entry in Walk(directory, recursive, includeHidden))
    {
      if (!MatchesPattern(entry, root, includePattern, matchNameOnly, isExclude: false))
      {
        continue;
      }

      if (excludePattern is not null
        && MatchesPattern(entry, root, excludePattern, matchNameOnly: false, isExclude: true))
      {
        continue;
      }

      yield return entry;
    }
  }

  public static bool MatchesCriteria(FileSystemInfo entry, string root, FindCriteria criteria)
  {
    ArgumentNullException.ThrowIfNull(entry);
    ArgumentNullException.ThrowIfNull(criteria);

    if (criteria.Name is not null)
    {
      bool nameOnly = !criteria.Name.Contains('/', StringComparison.Ordinal)
        && !criteria.Name.Contains('\\', StringComparison.Ordinal)
        && !criteria.Name.Contains("**", StringComparison.Ordinal);
      if (!MatchesPattern(entry, root, criteria.Name, nameOnly, isExclude: false))
      {
        return false;
      }
    }

    bool sizeFilter = criteria.MinSize.HasValue || criteria.MaxSize.HasValue;
    if (sizeFilter)
    {
      if (entry is not FileInfo fileInfo)
      {
        return false;
      }

      long length = fileInfo.Length;
      if (criteria.MinSize.HasValue && length < criteria.MinSize.Value)
      {
        return false;
      }

      if (criteria.MaxSize.HasValue && length > criteria.MaxSize.Value)
      {
        return false;
      }
    }

    DateTimeOffset lastWrite = new(entry.LastWriteTime);
    if (criteria.ModifiedAfter.HasValue && lastWrite < criteria.ModifiedAfter.Value)
    {
      return false;
    }

    if (criteria.ModifiedBefore.HasValue && lastWrite > criteria.ModifiedBefore.Value)
    {
      return false;
    }

    if (criteria.Attributes.HasValue)
    {
      FileAttributes required = criteria.Attributes.Value;
      if ((entry.Attributes & required) != required)
      {
        return false;
      }
    }

    return true;
  }

  public static string RelativePath(string root, string fullName)
  {
    string relative = Path.GetRelativePath(root, fullName);
    return relative.Replace('\\', '/');
  }

  public static string ResolveFileDestination(string sourcePath, string destinationPath)
  {
    if (Directory.Exists(destinationPath))
    {
      return Path.Combine(destinationPath, Path.GetFileName(sourcePath));
    }

    return destinationPath;
  }

  public static string ResolveDirectoryDestination(string sourcePath, string destinationPath, string operation)
  {
    if (File.Exists(destinationPath))
    {
      throw new IOException($"{operation}: {destinationPath}: cannot overwrite non-directory");
    }

    if (Directory.Exists(destinationPath))
    {
      string name = Path.GetFileName(
        sourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
      return Path.Combine(destinationPath, name);
    }

    return destinationPath;
  }

  private static IEnumerable<FileSystemInfo> Walk(
    DirectoryInfo directory,
    bool recursive,
    bool includeHidden)
  {
    EnumerationOptions enumerationOptions = CreateEnumerationOptions();

    foreach (FileSystemInfo entry in directory.EnumerateFileSystemInfos("*", enumerationOptions))
    {
      if (!includeHidden && IsHidden(entry))
      {
        continue;
      }

      yield return entry;

      if (recursive
        && entry is DirectoryInfo childDirectory
        && !IsReparsePoint(entry))
      {
        foreach (FileSystemInfo child in Walk(childDirectory, recursive: true, includeHidden))
        {
          yield return child;
        }
      }
    }
  }

  private static bool MatchesPattern(
    FileSystemInfo entry,
    string root,
    string? pattern,
    bool matchNameOnly,
    bool isExclude)
  {
    if (string.IsNullOrEmpty(pattern) || pattern == "*")
    {
      return !isExclude;
    }

    string candidate = matchNameOnly
      ? entry.Name
      : RelativePath(root, entry.FullName);

    if (isExclude && !pattern.Contains('/', StringComparison.Ordinal)
      && !pattern.Contains('\\', StringComparison.Ordinal)
      && !pattern.Contains("**", StringComparison.Ordinal))
    {
      candidate = entry.Name;
    }

    return GlobMatcher.IsMatch(candidate, pattern);
  }
}
