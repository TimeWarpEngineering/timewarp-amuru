#region Purpose
// Runs Tail over file operands and stdin: headers, the slice, the stream save, and follow.
#endregion

#region Design
// File errors use GNU tail's wording and exit code 1, and the walk continues.
// A header is printed for a file that opened, including an empty file. The blank line
// between headers is written before every header after the first.
// The save runs only for a non-seekable stdin last-N-lines read. Seekable stdin is a
// redirect of a file that already exists, so it is sliced from the end and not copied.
// The save is best effort. A failure to create the store or to write the copy leaves
// stdout and the exit code alone, and a partial file is removed. The copy stream
// swallows a close that flushes again after that failed write, so this finally still
// deletes the partial file and later operands are not skipped.
// Follow polls regular files. When headers are on and the next bytes come from a
// different file, that file's header is written first. A pipe is read until it closes.
// -F is not implemented, so a replaced path is not reopened.
#endregion

namespace TimeWarp.Amuru.Native.Text;

internal static class TailEngine
{
  private const string StandardInputName = "standard input";

  public static TailRunResult Run(
    TailOptions options,
    IReadOnlyList<string> paths,
    Stream standardInput,
    Stream standardOutput,
    Stream standardError,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(options);
    ArgumentNullException.ThrowIfNull(paths);
    ArgumentNullException.ThrowIfNull(standardInput);
    ArgumentNullException.ThrowIfNull(standardOutput);
    ArgumentNullException.ThrowIfNull(standardError);
    if (options.Measure.Count < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(options), "The tail count cannot be negative.");
    }

    if (options.Help || options.Version)
    {
      return new TailRunResult(0, null);
    }

    IReadOnlyList<string> operands = paths.Count == 0 ? ["-"] : paths;
    foreach (string operand in operands)
    {
      ArgumentNullException.ThrowIfNull(operand);
    }

    bool headers = options.Headers switch
    {
      TailHeaders.Always => true,
      TailHeaders.Never => false,
      _ => operands.Count > 1
    };
    byte delimiter = options.ZeroTerminated ? (byte)0 : (byte)'\n';
    bool fileOperand = false;
    foreach (string operand in operands)
    {
      if (operand != "-")
      {
        fileOperand = true;
        break;
      }
    }

    if (options.Follow && fileOperand && !cancellationToken.CanBeCanceled)
    {
      throw new ArgumentException("Follow requires a cancellation token.", nameof(cancellationToken));
    }

    if (options.FollowInterval < TimeSpan.Zero)
    {
      throw new ArgumentOutOfRangeException(nameof(options), "The follow interval cannot be negative.");
    }

    RunState state = new(headers);
    if (!options.Follow)
    {
      foreach (string operand in operands)
      {
        if (operand == "-")
        {
          ReadStandardInput(options, standardInput, standardOutput, standardError, delimiter, state);
        }
        else
        {
          ReadFile(operand, options, standardOutput, standardError, delimiter, state, follow: null);
        }
      }

      return new TailRunResult(state.Failed ? 1 : 0, state.SavedPath);
    }

    List<FollowedFile> followed = [];
    FollowedFile? opened = null;
    try
    {
      foreach (string operand in operands)
      {
        if (operand == "-")
        {
          ReadStandardInput(options, standardInput, standardOutput, standardError, delimiter, state);
          continue;
        }

        opened = ReadFile(operand, options, standardOutput, standardError, delimiter, state, follow: followed);
        if (opened is not null)
        {
          followed.Add(opened);
          opened = null;
        }
      }

      if (fileOperand && followed.Count == 0)
      {
        WriteText(standardError, "tail: no files remaining\n");
        state.Failed = true;
        return new TailRunResult(1, state.SavedPath);
      }

      if (followed.Count == 0)
      {
        return new TailRunResult(state.Failed ? 1 : 0, state.SavedPath);
      }

      Poll(followed, standardOutput, standardError, state, options.FollowInterval, cancellationToken);
      return new TailRunResult(state.Failed ? 1 : 0, state.SavedPath);
    }
    finally
    {
      opened?.Dispose();
      foreach (FollowedFile followedFile in followed)
      {
        followedFile.Dispose();
      }
    }
  }

  private static void ReadStandardInput(
    TailOptions options,
    Stream standardInput,
    Stream standardOutput,
    Stream standardError,
    byte delimiter,
    RunState state)
  {
    bool save = options.SaveStream
      && !options.Follow
      && !options.Measure.FromStart
      && options.Measure.Unit == TailUnit.Lines
      && !standardInput.CanSeek;
    if (save && options.SaveLimit <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(options), "The save limit must be positive.");
    }

    FileStream? saved = null;
    TailCaptureStream? capture = null;
    string? path = null;
    bool keep = false;
    Stream source = standardInput;
    try
    {
      if (save
        && TailStore.TryCreate(options, out FileStream? created, out string? createdPath)
        && created is not null
        && createdPath is not null)
      {
        saved = created;
        path = createdPath;
        capture = new TailCaptureStream(standardInput, saved, options.SaveLimit);
        source = capture;
      }
      else
      {
        save = false;
      }

      WriteHeader(standardOutput, StandardInputName, state);
      long seen = TailSlice.Copy(source, standardOutput, options.Measure, delimiter);
      if (!save || path is null || saved is null || capture is null)
      {
        return;
      }

      if (capture.Failed || !TryFlush(saved))
      {
        return;
      }

      bool cut = seen > options.Measure.Count;
      if (!cut && !capture.Capped)
      {
        return;
      }

      string limitNote = capture.Capped ? LimitNote(options.SaveLimit) : string.Empty;
      string count = options.Measure.Count.ToString(CultureInfo.InvariantCulture);
      string total = seen.ToString(CultureInfo.InvariantCulture);
      WriteText(
        standardError,
        $"tail: showed last {count} of {total} lines; full output: {path}{limitNote}\n");
      keep = true;
      state.SavedPath = path;
    }
    catch (IOException exception)
    {
      state.Failed = true;
      WriteText(standardError, $"tail: error reading '{StandardInputName}': {exception.Message}\n");
    }
    finally
    {
      // CopyStream swallows a failed flush on close so the delete below still runs.
      capture?.Dispose();
      saved?.Dispose();
      if (save && !keep && path is not null)
      {
        try
        {
          File.Delete(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
        }
      }
    }
  }

  private static FollowedFile? ReadFile(
    string path,
    TailOptions options,
    Stream standardOutput,
    Stream standardError,
    byte delimiter,
    RunState state,
    List<FollowedFile>? follow)
  {
    if (Directory.Exists(path))
    {
      state.Failed = true;
      WriteText(standardError, $"tail: error reading '{path}': Is a directory\n");
      return null;
    }

    FileStream? stream = null;
    bool opened = false;
    try
    {
      stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
      opened = true;
      WriteHeader(standardOutput, path, state);
      long marker = stream.Length;
      TailSlice.Copy(stream, standardOutput, options.Measure, delimiter);
      if (follow is null)
      {
        return null;
      }

      long position = stream.Position < marker ? marker : stream.Position;
      FollowedFile owned = new(stream, path, position);
      stream = null;
      return owned;
    }
    catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException or IOException)
    {
      state.Failed = true;
      WriteText(
        standardError,
        opened
          ? $"tail: error reading '{path}': {exception.Message}\n"
          : OpenError(path, exception));
      return null;
    }
    finally
    {
      stream?.Dispose();
    }
  }

  private static void Poll(
    List<FollowedFile> followed,
    Stream standardOutput,
    Stream standardError,
    RunState state,
    TimeSpan interval,
    CancellationToken cancellationToken)
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      foreach (FollowedFile file in followed)
      {
        try
        {
          long length = file.Stream.Length;
          if (length < file.Position)
          {
            WriteText(standardError, $"tail: {file.Path}: file truncated\n");
            file.Stream.Position = 0;
            WriteFollowHeader(standardOutput, file.Path, state);
            CopyAmount(file.Stream, standardOutput, length);
            file.Position = file.Stream.Position;
          }
          else if (length > file.Position)
          {
            file.Stream.Position = file.Position;
            WriteFollowHeader(standardOutput, file.Path, state);
            CopyAmount(file.Stream, standardOutput, length - file.Position);
            file.Position = file.Stream.Position;
          }
        }
        catch (IOException)
        {
        }
      }

      if (Wait(interval, cancellationToken))
      {
        return;
      }
    }
  }

  private static void CopyAmount(Stream input, Stream output, long count)
  {
    byte[] buffer = new byte[8192];
    while (count > 0)
    {
      int asked = (int)Math.Min(buffer.Length, count);
      int read = input.Read(buffer, 0, asked);
      if (read == 0)
      {
        return;
      }

      output.Write(buffer, 0, read);
      count -= read;
    }
  }

  private static bool Wait(TimeSpan interval, CancellationToken cancellationToken)
  {
    if (interval <= TimeSpan.Zero)
    {
      return cancellationToken.IsCancellationRequested;
    }

    return cancellationToken.WaitHandle.WaitOne(interval);
  }

  private static void WriteFollowHeader(Stream output, string name, RunState state)
  {
    if (!state.Headers || state.LastName == name)
    {
      return;
    }

    WriteHeader(output, name, state);
  }

  private static bool TryFlush(FileStream stream)
  {
    try
    {
      stream.Flush();
      return true;
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
      return false;
    }
  }

  private static void WriteHeader(Stream output, string name, RunState state)
  {
    if (!state.Headers)
    {
      return;
    }

    if (state.Started)
    {
      WriteText(output, "\n");
    }

    state.Started = true;
    state.LastName = name;
    WriteText(output, $"==> {name} <==\n");
  }

  private static string OpenError(string path, Exception exception)
  {
    if (exception is FileNotFoundException or DirectoryNotFoundException)
    {
      return $"tail: cannot open '{path}' for reading: No such file or directory\n";
    }

    if (exception is UnauthorizedAccessException)
    {
      return $"tail: cannot open '{path}' for reading: Permission denied\n";
    }

    if (exception.Message.Contains("denied", StringComparison.OrdinalIgnoreCase))
    {
      return $"tail: cannot open '{path}' for reading: Permission denied\n";
    }

    if (exception.Message.Contains("directory", StringComparison.OrdinalIgnoreCase))
    {
      return $"tail: error reading '{path}': Is a directory\n";
    }

    return $"tail: error reading '{path}': {exception.Message}\n";
  }

  private static string LimitNote(long limit)
  {
    if (limit == TailOptions.DefaultSaveLimit)
    {
      return " (stopped at 64 MiB)";
    }

    return " (stopped at " + limit.ToString(CultureInfo.InvariantCulture) + " bytes)";
  }

  private static void WriteText(Stream stream, string text)
  {
    byte[] bytes = Encoding.UTF8.GetBytes(text);
    stream.Write(bytes);
  }

  private sealed class RunState
  {
    public RunState(bool headers)
    {
      Headers = headers;
    }

    public bool Headers { get; }

    public bool Started { get; set; }

    public string? LastName { get; set; }

    public bool Failed { get; set; }

    public string? SavedPath { get; set; }
  }

  private sealed class FollowedFile : IDisposable
  {
    public FollowedFile(FileStream stream, string path, long position)
    {
      Stream = stream;
      Path = path;
      Position = position;
    }

    public FileStream Stream { get; }

    public string Path { get; }

    public long Position { get; set; }

    public void Dispose() => Stream.Dispose();
  }
}
