#region Purpose
// Applies a replacement to one file and atomically writes it when the text changes.
#endregion

#region Design
// The match runs on LF-normalized text so ^, $, and . see logical lines. Zero
// replacements skip encoding and the write, which keeps mtime and original bytes.
// The temp file lives in the same directory so the final move stays on one volume.
// DryRun fills Preview and does not create a temp file or a .bak.
#endregion

namespace TimeWarp.Amuru.Native.Text;

internal static class TextReplace
{
  public static ReplaceResult ReplaceFile(
    Regex regex,
    string replacement,
    string path,
    ReplaceInFilesOptions options)
  {
    ArgumentNullException.ThrowIfNull(regex);
    ArgumentNullException.ThrowIfNull(replacement);
    ArgumentNullException.ThrowIfNull(options);
    byte[] bytes = File.ReadAllBytes(path);
    ReplacePlan plan = Plan(regex, replacement, path, bytes, options);
    if (!plan.Changed || options.DryRun)
    {
      return ToResult(plan, backupPath: null);
    }

    string? backupPath = Commit(path, plan.Encoded, options.Backup);
    return ToResult(plan, backupPath);
  }

  public static async Task<ReplaceResult> ReplaceFileAsync(
    Regex regex,
    string replacement,
    string path,
    ReplaceInFilesOptions options,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(regex);
    ArgumentNullException.ThrowIfNull(replacement);
    ArgumentNullException.ThrowIfNull(options);
    byte[] bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
    ReplacePlan plan = Plan(regex, replacement, path, bytes, options);
    if (!plan.Changed || options.DryRun)
    {
      return ToResult(plan, backupPath: null);
    }

    string? backupPath = await CommitAsync(path, plan.Encoded, options.Backup, cancellationToken)
      .ConfigureAwait(false);
    return ToResult(plan, backupPath);
  }

  private static ReplacePlan Plan(
    Regex regex,
    string replacement,
    string path,
    byte[] bytes,
    ReplaceInFilesOptions options)
  {
    var encoding = TextFileEncoding.Detect(bytes);
    string original = encoding.GetString(bytes);
    var newlineStyle = NewlineStyle.Detect(original);
    string normalized = NewlineStyle.ToLineFeed(original);
    int count = 0;
    string replaced = regex.Replace(
      normalized,
      match =>
      {
        if (options.MaxReplacements is int limit && count >= limit)
        {
          return match.Value;
        }

        count++;
        return match.Result(replacement);
      });

    string updated = newlineStyle.Apply(replaced);
    bool changed = count > 0 && !string.Equals(updated, original, StringComparison.Ordinal);
    IReadOnlyList<string> preview = options.DryRun && changed
      ? TextDiff.Unified(path, original, updated)
      : [];
    byte[] encoded = changed && !options.DryRun ? encoding.GetBytes(updated) : [];
    return new ReplacePlan(path, count, changed, preview, encoded);
  }

  private static ReplaceResult ToResult(ReplacePlan plan, string? backupPath)
  {
    return new ReplaceResult
    {
      Path = plan.Path,
      ReplacementCount = plan.Count,
      Changed = plan.Changed,
      Preview = plan.Preview,
      BackupPath = backupPath
    };
  }

  private static string? Commit(string path, byte[] contents, bool backup)
  {
    string temp = CreateTempPath(path);
    try
    {
      File.WriteAllBytes(temp, contents);
      CopyUnixMode(path, temp);
      string? backupPath = WriteBackup(path, backup);
      File.Move(temp, path, overwrite: true);
      return backupPath;
    }
    finally
    {
      DeleteIfExists(temp);
    }
  }

  private static async Task<string?> CommitAsync(
    string path,
    byte[] contents,
    bool backup,
    CancellationToken cancellationToken)
  {
    string temp = CreateTempPath(path);
    try
    {
      await File.WriteAllBytesAsync(temp, contents, cancellationToken).ConfigureAwait(false);
      CopyUnixMode(path, temp);
      string? backupPath = WriteBackup(path, backup);
      File.Move(temp, path, overwrite: true);
      return backupPath;
    }
    finally
    {
      DeleteIfExists(temp);
    }
  }

  private static string CreateTempPath(string path)
  {
    string? directory = Path.GetDirectoryName(path);
    if (string.IsNullOrEmpty(directory))
    {
      throw new IOException($"ReplaceInFiles: {path}: cannot determine directory");
    }

    return Path.Combine(
      directory,
      "." + Path.GetFileName(path) + "." + Guid.NewGuid().ToString("N") + ".tmp");
  }

  private static void CopyUnixMode(string source, string destination)
  {
    if (OperatingSystem.IsWindows())
    {
      return;
    }

    File.SetUnixFileMode(destination, File.GetUnixFileMode(source));
  }

  private static string? WriteBackup(string path, bool backup)
  {
    if (!backup)
    {
      return null;
    }

    string backupPath = path + ".bak";
    File.Copy(path, backupPath, overwrite: true);
    return backupPath;
  }

  private static void DeleteIfExists(string path)
  {
    if (!File.Exists(path))
    {
      return;
    }

    try
    {
      File.Delete(path);
    }
    catch (IOException)
    {
    }
    catch (UnauthorizedAccessException)
    {
    }
  }

  private readonly record struct ReplacePlan(
    string Path,
    int Count,
    bool Changed,
    IReadOnlyList<string> Preview,
    byte[] Encoded);
}
