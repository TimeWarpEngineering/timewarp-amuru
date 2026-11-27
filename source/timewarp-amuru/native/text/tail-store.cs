#region Purpose
// Creates the per-user folder that holds a saved stdin stream and drops old files.
#endregion

#region Design
// The folder is <temp>/tw-agent-streams/<user>, mode 0700 on Unix, and each file is
// named with a UTC stamp, the process id, and a random suffix so two walks do not collide.
// When this process creates the shared parent, that directory is mode 01777 so other
// users can add their own folder. An existing parent is left as it is.
// Cleanup runs at most once an hour and deletes tail-* files last written more than 24 hours ago.
// The stamp is touched before the scan so overlapping processes do not both sweep.
// A failure to create, tighten, stamp, or open the file returns no stream. The caller slices without a copy.
#endregion

namespace TimeWarp.Amuru.Native.Text;

internal static class TailStore
{
  private const UnixFileMode SharedParentMode =
    UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
    | UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute
    | UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute
    | UnixFileMode.StickyBit;

  private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);
  private static readonly TimeSpan MaxAge = TimeSpan.FromHours(24);

  public static bool TryCreate(TailOptions options, out FileStream? stream, out string? path)
  {
    ArgumentNullException.ThrowIfNull(options);
    stream = null;
    path = null;
    string directory;
    if (string.IsNullOrWhiteSpace(options.StreamDirectory))
    {
      directory = DefaultDirectory();
      EnsureSharedParent();
    }
    else
    {
      directory = options.StreamDirectory;
    }

    try
    {
      Directory.CreateDirectory(directory);
      TightenDirectory(directory);
      Cleanup(directory);
      string stamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmssfff'Z'", CultureInfo.InvariantCulture);
      string name = $"tail-{stamp}-{Environment.ProcessId}-{Guid.NewGuid().ToString("N")[..8]}";
      path = Path.Combine(directory, name);
      FileStreamOptions file = new()
      {
        Mode = FileMode.CreateNew,
        Access = FileAccess.Write,
        Share = FileShare.Read
      };
      if (!OperatingSystem.IsWindows())
      {
        file.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
      }

      stream = new FileStream(path, file);
      return true;
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
      stream?.Dispose();
      stream = null;
      if (path is not null)
      {
        try
        {
          File.Delete(path);
        }
        catch (Exception deleteException) when (deleteException is IOException or UnauthorizedAccessException)
        {
        }

        path = null;
      }

      return false;
    }
  }

  public static string DefaultDirectory()
  {
    return Path.Combine(Path.GetTempPath(), "tw-agent-streams", Sanitize(Environment.UserName));
  }

  private static void EnsureSharedParent()
  {
    if (OperatingSystem.IsWindows())
    {
      return;
    }

    string parent = Path.Combine(Path.GetTempPath(), "tw-agent-streams");
    try
    {
      if (Directory.Exists(parent))
      {
        return;
      }

      Directory.CreateDirectory(parent);
      File.SetUnixFileMode(parent, SharedParentMode);
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
    }
  }

  private static void Cleanup(string directory)
  {
    string stampPath = Path.Combine(directory, ".cleanup-stamp");
    DateTime now = DateTime.UtcNow;
    try
    {
      if (File.Exists(stampPath) && now - File.GetLastWriteTimeUtc(stampPath) < CleanupInterval)
      {
        return;
      }

      File.WriteAllBytes(stampPath, []);
      File.SetLastWriteTimeUtc(stampPath, now);
      TightenFile(stampPath);
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
      return;
    }

    foreach (string file in Directory.EnumerateFiles(directory, "tail-*"))
    {
      try
      {
        if (now - File.GetLastWriteTimeUtc(file) > MaxAge)
        {
          File.Delete(file);
        }
      }
      catch (IOException)
      {
      }
      catch (UnauthorizedAccessException)
      {
      }
    }
  }

  private static string Sanitize(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return "unknown";
    }

    char[] chars = name.ToCharArray();
    for (int index = 0; index < chars.Length; index++)
    {
      if (chars[index] is '/' or '\\' or ':' or '\0')
      {
        chars[index] = '_';
      }
    }

    return new string(chars);
  }

  private static void TightenDirectory(string path)
  {
    if (OperatingSystem.IsWindows())
    {
      return;
    }

    File.SetUnixFileMode(
      path,
      UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
  }

  private static void TightenFile(string path)
  {
    if (OperatingSystem.IsWindows())
    {
      return;
    }

    File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
  }
}
