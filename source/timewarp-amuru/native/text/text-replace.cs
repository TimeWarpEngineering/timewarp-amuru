#region Purpose
// Applies a replacement to one file and atomically writes it when the text changes.
#endregion

#region Design
// The match runs on text with \r\n folded to \n so ^, $, and . see logical lines.
// When the text changes, the whole file is written back in the detected newline style,
// so a mixed-newline file comes out uniform. Zero replacements, or replacements that
// leave the text identical, skip encoding and the write, which keeps mtime and the
// original bytes. Binary files (NUL in a UTF-8 or no-BOM file) are skipped with zero
// replacements. Undecodable bytes, or a replacement that leaves text the encoding
// cannot write (such as a split surrogate pair), throw InvalidDataException naming
// the path instead of writing U+FFFD. Encoding also runs on DryRun so it fails the
// same way. A symlink is resolved to its final target and the target is rewritten,
// so the link stays a link (sed --follow-symlinks). The .bak goes beside the link
// path that was passed in, not beside the target. Hard links are still split by the
// move. The temp file lives in the target's directory so the final move stays on one
// volume. DryRun fills Preview and does not create a temp file or a .bak.
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

    string? backupPath = Commit(path, ResolveTarget(path), plan.Encoded, options.Backup);
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

    string? backupPath = await CommitAsync(path, ResolveTarget(path), plan.Encoded, options.Backup, cancellationToken)
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
    if (encoding.IsBinary(bytes))
    {
      return new ReplacePlan(path, 0, Changed: false, [], []);
    }

    string original = Decode(encoding, bytes, path);
    var newlineStyle = NewlineStyle.Detect(original);
    string normalized = newlineStyle.ToLineFeed(original);
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
    byte[] encoded = changed ? Encode(encoding, updated, path) : [];
    return new ReplacePlan(path, count, changed, preview, encoded);
  }

  private static string Decode(TextFileEncoding encoding, byte[] bytes, string path)
  {
    try
    {
      return encoding.GetString(bytes);
    }
    catch (DecoderFallbackException exception)
    {
      string reason = $"not valid {encoding.Encoding.WebName} text";
      InvalidDataException invalid = new($"ReplaceInFiles: {path}: {reason}", exception);
      invalid.Data[TextCommand.ReasonKey] = reason;
      throw invalid;
    }
  }

  private static byte[] Encode(TextFileEncoding encoding, string text, string path)
  {
    try
    {
      return encoding.GetBytes(text);
    }
    catch (EncoderFallbackException exception)
    {
      string reason = $"replacement produced invalid {encoding.Encoding.WebName} text";
      InvalidDataException invalid = new($"ReplaceInFiles: {path}: {reason}", exception);
      invalid.Data[TextCommand.ReasonKey] = reason;
      throw invalid;
    }
  }

  private static string ResolveTarget(string path)
  {
    FileInfo file = new(path);
    if (file.LinkTarget is null)
    {
      return path;
    }

    FileSystemInfo? target = file.ResolveLinkTarget(returnFinalTarget: true);
    return target?.FullName ?? path;
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

  private static string? Commit(string path, string target, byte[] contents, bool backup)
  {
    string temp = CreateTempPath(target);
    try
    {
      File.WriteAllBytes(temp, contents);
      CopyUnixMode(target, temp);
      string? backupPath = WriteBackup(path, backup);
      File.Move(temp, target, overwrite: true);
      return backupPath;
    }
    finally
    {
      DeleteIfExists(temp);
    }
  }

  private static async Task<string?> CommitAsync(
    string path,
    string target,
    byte[] contents,
    bool backup,
    CancellationToken cancellationToken)
  {
    string temp = CreateTempPath(target);
    try
    {
      await File.WriteAllBytesAsync(temp, contents, cancellationToken).ConfigureAwait(false);
      CopyUnixMode(target, temp);
      string? backupPath = WriteBackup(path, backup);
      File.Move(temp, target, overwrite: true);
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
