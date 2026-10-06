#region Purpose
// Cross-platform PATH lookup for executables.
#endregion

#region Design
// Unix matches require an execute bit (user, group, or other). That rejects non-executable
// names the way `which` does, without an access(2) check of the caller's credentials.
// Windows matches PATHEXT (then a bare name) and does not consult Unix mode.
// A name that already contains a separator is a direct path, not a PATH search.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Provides cross-platform PATH resolution for finding executables.
/// On Unix, a match must exist and have an execute bit, approximating <c>which</c>.
/// On Windows, a match is an existing file selected with PATHEXT, approximating <c>where</c>.
/// </summary>
public static class PathResolver
{
  private static readonly char PathSeparator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
  private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

  /// <summary>
  /// Finds the first instance of an executable in PATH.
  /// Returns null if not found.
  /// </summary>
  /// <param name="name">The executable name (without extension on Windows).</param>
  /// <returns>Full path to the executable, or null if not found.</returns>
  /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
  /// <example>
  /// string? gitPath = PathResolver.ResolveExecutable("git");
  /// if (gitPath != null)
  /// {
  ///   Console.WriteLine($"Git found at: {gitPath}");
  /// }
  /// </example>
  public static string? ResolveExecutable(string name)
  {
    ValidateName(name);

    // If name contains path separators, check if it exists directly
    if (ContainsPathSeparator(name))
    {
      return IsExecutableFile(name) ? Path.GetFullPath(name) : null;
    }

    string? pathEnv = Environment.GetEnvironmentVariable("PATH");
    if (string.IsNullOrEmpty(pathEnv))
    {
      return null;
    }

    string[] pathDirs = pathEnv.Split(PathSeparator);
    string[] extensions = GetExecutableExtensions();

    foreach (string dir in pathDirs)
    {
      string? result = SearchDirectory(dir, name, extensions);
      if (result != null)
      {
        return result;
      }
    }

    return null;
  }

  /// <summary>
  /// Finds all instances of an executable in PATH, in order of precedence.
  /// </summary>
  /// <param name="name">The executable name (without extension on Windows).</param>
  /// <returns>List of full paths to all matching executables.</returns>
  /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
  /// <example>
  /// IReadOnlyList&lt;string&gt; allDevs = PathResolver.ResolveAllExecutables("dev");
  /// foreach (string path in allDevs)
  /// {
  ///   Console.WriteLine($"Found: {path}");
  /// }
  /// </example>
  public static IReadOnlyList<string> ResolveAllExecutables(string name)
  {
    ValidateName(name);

    List<string> results = new();

    // If name contains path separators, check if it exists directly
    if (ContainsPathSeparator(name))
    {
      if (IsExecutableFile(name))
      {
        results.Add(Path.GetFullPath(name));
      }

      return results;
    }

    string? pathEnv = Environment.GetEnvironmentVariable("PATH");
    if (string.IsNullOrEmpty(pathEnv))
    {
      return results;
    }

    string[] pathDirs = pathEnv.Split(PathSeparator);
    string[] extensions = GetExecutableExtensions();

    foreach (string dir in pathDirs)
    {
      string? result = SearchDirectory(dir, name, extensions);
      if (result != null)
      {
        results.Add(result);
      }
    }

    return results;
  }

  private static void ValidateName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("Executable name cannot be null or whitespace.", nameof(name));
    }
  }

  private static bool ContainsPathSeparator(string name)
  {
    return name.Contains('/', StringComparison.Ordinal) || name.Contains('\\', StringComparison.Ordinal);
  }

  private static string[] GetExecutableExtensions()
  {
    if (!IsWindows)
    {
      return Array.Empty<string>();
    }

    string? pathExt = Environment.GetEnvironmentVariable("PATHEXT");
    if (string.IsNullOrEmpty(pathExt))
    {
      return new[] { ".exe", ".cmd", ".bat", ".com" };
    }

    return pathExt.Split(';')
      .Where(static ext => !string.IsNullOrEmpty(ext))
      .ToArray();
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "PATH probing should be resilient; malformed or inaccessible path entries are intentionally skipped during executable resolution."
  )]
  private static string? SearchDirectory(string directory, string name, string[] extensions)
  {
    // Handle relative paths
    string fullPath;
    try
    {
      fullPath = Path.GetFullPath(directory);
    }
    catch
    {
      return null;
    }

    if (!Directory.Exists(fullPath))
    {
      return null;
    }

    // On Unix, just check the name directly
    if (!IsWindows)
    {
      string filePath = Path.Combine(fullPath, name);
      if (IsExecutableFile(filePath))
      {
        return filePath;
      }

      return null;
    }

    // On Windows, try with each extension
    foreach (string ext in extensions)
    {
      string filePath = Path.Combine(fullPath, name + ext);
      if (File.Exists(filePath))
      {
        return filePath;
      }
    }

    // Also try without extension (in case name already has one)
    string barePath = Path.Combine(fullPath, name);
    if (File.Exists(barePath))
    {
      return barePath;
    }

    return null;
  }

  private static bool IsExecutableFile(string filePath)
  {
    if (!File.Exists(filePath))
    {
      return false;
    }

    if (OperatingSystem.IsWindows())
    {
      return true;
    }

    try
    {
      UnixFileMode mode = File.GetUnixFileMode(filePath);
      const UnixFileMode executeBits =
        UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
      return (mode & executeBits) != 0;
    }
    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
    {
      return false;
    }
  }
}
