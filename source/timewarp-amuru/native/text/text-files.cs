#region Purpose
// Resolves SelectString and ReplaceInFiles inputs to files via FindItem's walker.
#endregion

#region Design
// Direct calls FileSystem.Direct.FindItem and throws on the first missing path or
// unreadable directory. Commands walk synchronously, one directory at a time, with
// the same rules as FileSystemWalk (hidden entries included, reparse-point directories
// not followed). A missing input path or an unreadable directory becomes a
// TextFileItem with an Error, so the caller reports it and the walk continues.
// Include/Exclude reuse GlobMatcher name rules: a slash or ** matches the relative
// path; otherwise the file name.
#endregion

namespace TimeWarp.Amuru.Native.Text;

using FileSystem = TimeWarp.Amuru.Native.FileSystem;

internal readonly record struct TextFileQuery(string? NameGlob, string? Include, string? Exclude)
{
  public static TextFileQuery From(SelectStringOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);
    return new TextFileQuery(null, options.Include, options.Exclude);
  }

  public static TextFileQuery From(ReplaceInFilesOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);
    return new TextFileQuery(null, options.Include, options.Exclude);
  }

  public static TextFileQuery FromGlob(string globPattern, string? include, string? exclude)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(globPattern);
    return new TextFileQuery(globPattern, include, exclude);
  }

  public FileSystem.FindCriteria? Criteria
  {
    get
    {
      string? name = NameGlob ?? Include;
      return name is null ? null : new FileSystem.FindCriteria { Name = name };
    }
  }

  public string? ExtraInclude => NameGlob is null ? null : Include;
}

/// <summary>
/// One step of a Commands walk: a file to process, or a path that could not be read.
/// </summary>
internal readonly record struct TextFileItem(string Path, Exception? Error);

internal static class TextFiles
{
  public const string StandardInputPath = "-";

  public static string RequireExisting(string path)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    string full = Path.GetFullPath(path);
    if (File.Exists(full) || Directory.Exists(full))
    {
      return full;
    }

    throw new FileNotFoundException($"Path not found: {full}", full);
  }

  public static IEnumerable<TextFileItem> Enumerate(string path, TextFileQuery query)
  {
    return Enumerate([path], query);
  }

  public static IEnumerable<TextFileItem> Enumerate(IEnumerable<string> paths, TextFileQuery query)
  {
    ArgumentNullException.ThrowIfNull(paths);
    foreach (string path in paths)
    {
      string? full = null;
      Exception? error = null;
      try
      {
        full = RequireExisting(path);
      }
      catch (FileNotFoundException exception)
      {
        error = exception;
      }

      if (full is null)
      {
        yield return new TextFileItem(path, error);
        continue;
      }

      if (File.Exists(full))
      {
        if (AcceptsFile(Path.GetDirectoryName(full) ?? full, new FileInfo(full), query.Include, query.Exclude))
        {
          yield return new TextFileItem(full, null);
        }

        continue;
      }

      foreach (TextFileItem item in WalkDirectory(full, new DirectoryInfo(full), query))
      {
        yield return item;
      }
    }
  }

  public static IEnumerable<TextFileItem> EnumerateGlob(string root, TextFileQuery query)
  {
    string full = RequireDirectory(root);
    return WalkDirectory(full, new DirectoryInfo(full), query);
  }

  public static async IAsyncEnumerable<string> EnumerateAsync(
    string path,
    TextFileQuery query,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    string full = RequireExisting(path);
    cancellationToken.ThrowIfCancellationRequested();
    if (File.Exists(full))
    {
      if (AcceptsFile(Path.GetDirectoryName(full) ?? full, new FileInfo(full), query.Include, query.Exclude))
      {
        yield return full;
      }

      yield break;
    }

    await foreach (string file in EnumerateDirectoryAsync(full, query, cancellationToken).ConfigureAwait(false))
    {
      yield return file;
    }
  }

  public static async IAsyncEnumerable<string> EnumerateAsync(
    IEnumerable<string> paths,
    TextFileQuery query,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(paths);
    foreach (string path in paths)
    {
      cancellationToken.ThrowIfCancellationRequested();
      await foreach (string file in EnumerateAsync(path, query, cancellationToken).ConfigureAwait(false))
      {
        yield return file;
      }
    }
  }

  public static IAsyncEnumerable<string> EnumerateGlobAsync(
    string root,
    TextFileQuery query,
    CancellationToken cancellationToken = default)
  {
    string full = RequireDirectory(root);
    return EnumerateDirectoryAsync(full, query, cancellationToken);
  }

  public static bool AcceptsFile(string root, FileSystemInfo entry, string? include, string? exclude)
  {
    ArgumentNullException.ThrowIfNull(entry);
    if (entry is not FileInfo)
    {
      return false;
    }

    if (include is not null && !Matches(root, entry, include))
    {
      return false;
    }

    if (exclude is not null && Matches(root, entry, exclude))
    {
      return false;
    }

    return true;
  }

  private static IEnumerable<TextFileItem> WalkDirectory(string root, DirectoryInfo directory, TextFileQuery query)
  {
    List<FileSystemInfo> entries = [];
    Exception? error = null;
    try
    {
      entries.AddRange(directory.EnumerateFileSystemInfos("*", FileSystem.FileSystemWalk.CreateEnumerationOptions()));
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
      error = exception;
    }

    if (error is not null)
    {
      yield return new TextFileItem(directory.FullName, error);
      yield break;
    }

    FileSystem.FindCriteria? criteria = query.Criteria;
    foreach (FileSystemInfo entry in entries)
    {
      if (entry is DirectoryInfo child)
      {
        if (FileSystem.FileSystemWalk.IsReparsePoint(child))
        {
          continue;
        }

        foreach (TextFileItem item in WalkDirectory(root, child, query))
        {
          yield return item;
        }

        continue;
      }

      if (criteria is not null && !FileSystem.FileSystemWalk.MatchesCriteria(entry, root, criteria))
      {
        continue;
      }

      if (!AcceptsFile(root, entry, query.ExtraInclude, query.Exclude))
      {
        continue;
      }

      yield return new TextFileItem(entry.FullName, null);
    }
  }

  private static async IAsyncEnumerable<string> EnumerateDirectoryAsync(
    string root,
    TextFileQuery query,
    [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    await foreach (FileSystemInfo entry in FileSystem.Direct.FindItem(root, query.Criteria, cancellationToken)
      .ConfigureAwait(false))
    {
      if (!AcceptsFile(root, entry, query.ExtraInclude, query.Exclude))
      {
        continue;
      }

      yield return entry.FullName;
    }
  }

  private static string RequireDirectory(string root)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(root);
    string full = Path.GetFullPath(root);
    if (!Directory.Exists(full))
    {
      throw new DirectoryNotFoundException($"Path not found: {full}");
    }

    return full;
  }

  private static bool Matches(string root, FileSystemInfo entry, string pattern)
  {
    bool nameOnly = !pattern.Contains('/', StringComparison.Ordinal)
      && !pattern.Contains('\\', StringComparison.Ordinal)
      && !pattern.Contains("**", StringComparison.Ordinal);
    string candidate = nameOnly ? entry.Name : FileSystem.FileSystemWalk.RelativePath(root, entry.FullName);
    return FileSystem.GlobMatcher.IsMatch(candidate, pattern);
  }
}
