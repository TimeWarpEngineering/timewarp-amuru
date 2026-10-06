#region Purpose
// Resolves SelectString and ReplaceInFiles inputs to files via FindItem's walker.
#endregion

#region Design
// Direct calls FileSystem.Direct.FindItem. Commands use FileSystemWalk, the same
// walker, so they stay synchronous. Include/Exclude reuse GlobMatcher name rules:
// a slash or ** matches the relative path; otherwise the file name.
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

  public static IEnumerable<string> Enumerate(string path, TextFileQuery query)
  {
    string full = RequireExisting(path);
    if (File.Exists(full))
    {
      if (AcceptsFile(Path.GetDirectoryName(full) ?? full, new FileInfo(full), query.Include, query.Exclude))
      {
        yield return full;
      }

      yield break;
    }

    foreach (string file in EnumerateDirectory(full, query))
    {
      yield return file;
    }
  }

  public static IEnumerable<string> Enumerate(IEnumerable<string> paths, TextFileQuery query)
  {
    ArgumentNullException.ThrowIfNull(paths);
    foreach (string path in paths)
    {
      foreach (string file in Enumerate(path, query))
      {
        yield return file;
      }
    }
  }

  public static IEnumerable<string> EnumerateGlob(string root, TextFileQuery query)
  {
    string full = RequireDirectory(root);
    foreach (string file in EnumerateDirectory(full, query))
    {
      yield return file;
    }
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

  private static IEnumerable<string> EnumerateDirectory(string root, TextFileQuery query)
  {
    FileSystem.FindCriteria? criteria = query.Criteria;
    foreach (FileSystemInfo entry in FileSystem.FileSystemWalk.Enumerate(
      root,
      recursive: true,
      includeHidden: true,
      includePattern: null,
      excludePattern: null))
    {
      if (criteria is not null && !FileSystem.FileSystemWalk.MatchesCriteria(entry, root, criteria))
      {
        continue;
      }

      if (!AcceptsFile(root, entry, query.ExtraInclude, query.Exclude))
      {
        continue;
      }

      yield return entry.FullName;
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
