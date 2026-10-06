#region Purpose
// Line-at-a-time matcher that yields TextMatch with bounded before/after context.
#endregion

#region Design
// Only the context window and in-flight matches are buffered, so a file is never
// loaded as one string. MaxMatches counts matching lines, like grep -m: every hit on a
// counted line is still yielded, and the read stops once the last counted line's
// after-context is filled. Each file gets its own window. A file whose first
// BinarySniffLength bytes contain NUL, without a UTF-16/32 BOM, is binary and is
// skipped with no matches (unlike grep, no "Binary file matches" line is produced).
// Reader and stream inputs are never sniffed.
#endregion

namespace TimeWarp.Amuru.Native.Text;

internal static class TextSearch
{
  private const int BinarySniffLength = 8000;

  public static IEnumerable<TextMatch> ReadFile(Regex regex, string path, SelectStringOptions options)
  {
    ArgumentNullException.ThrowIfNull(regex);
    ArgumentNullException.ThrowIfNull(options);
    using FileStream stream = Open(path, asynchronous: false);
    byte[] head = new byte[BinarySniffLength];
    int read = stream.ReadAtLeast(head, BinarySniffLength, throwOnEndOfStream: false);
    if (IsBinary(head.AsSpan(0, read)))
    {
      yield break;
    }

    stream.Position = 0;
    using StreamReader reader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
    foreach (TextMatch match in Read(regex, reader, path, options))
    {
      yield return match;
    }
  }

  public static IEnumerable<TextMatch> Read(Regex regex, TextReader reader, string path, SelectStringOptions options)
  {
    ArgumentNullException.ThrowIfNull(regex);
    ArgumentNullException.ThrowIfNull(reader);
    ArgumentNullException.ThrowIfNull(options);
    MatchWindow window = new(regex, options);
    while (reader.ReadLine() is string line)
    {
      foreach (TextMatch match in window.Push(path, line))
      {
        yield return match;
      }

      if (window.IsDone)
      {
        yield break;
      }
    }

    foreach (TextMatch match in window.Finish(path))
    {
      yield return match;
    }
  }

  public static async IAsyncEnumerable<TextMatch> ReadFileAsync(
    Regex regex,
    string path,
    SelectStringOptions options,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(regex);
    ArgumentNullException.ThrowIfNull(options);
    FileStream stream = Open(path, asynchronous: true);
    await using (stream.ConfigureAwait(false))
    {
      byte[] head = new byte[BinarySniffLength];
      int read = await stream.ReadAtLeastAsync(head, BinarySniffLength, throwOnEndOfStream: false, cancellationToken)
        .ConfigureAwait(false);
      if (IsBinary(head.AsSpan(0, read)))
      {
        yield break;
      }

      stream.Position = 0;
      using StreamReader reader = new(
        stream,
        Encoding.UTF8,
        detectEncodingFromByteOrderMarks: true,
        bufferSize: 1024,
        leaveOpen: true);
      await foreach (TextMatch match in ReadAsync(regex, reader, path, options, cancellationToken)
        .ConfigureAwait(false))
      {
        yield return match;
      }
    }
  }

  public static async IAsyncEnumerable<TextMatch> ReadAsync(
    Regex regex,
    TextReader reader,
    string path,
    SelectStringOptions options,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(regex);
    ArgumentNullException.ThrowIfNull(reader);
    ArgumentNullException.ThrowIfNull(options);
    MatchWindow window = new(regex, options);
    while (true)
    {
      cancellationToken.ThrowIfCancellationRequested();
      string? line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
      if (line is null)
      {
        break;
      }

      foreach (TextMatch match in window.Push(path, line))
      {
        yield return match;
      }

      if (window.IsDone)
      {
        yield break;
      }
    }

    foreach (TextMatch match in window.Finish(path))
    {
      yield return match;
    }
  }

  private static bool IsBinary(ReadOnlySpan<byte> head)
  {
    return TextFileEncoding.Detect(head).IsBinary(head);
  }

  private static FileStream Open(string path, bool asynchronous)
  {
    FileOptions fileOptions = FileOptions.SequentialScan;
    if (asynchronous)
    {
      fileOptions |= FileOptions.Asynchronous;
    }

    return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, fileOptions);
  }

  private sealed class MatchWindow
  {
    private readonly Regex Pattern;
    private readonly SelectStringOptions Options;
    private readonly Queue<string> Before = new();
    private readonly List<PendingMatch> Pending = [];
    private int LineNumber;
    private int Started;

    public MatchWindow(Regex pattern, SelectStringOptions options)
    {
      Pattern = pattern;
      Options = options;
    }

    public bool IsDone => IsSaturated && Pending.Count == 0;

    public IEnumerable<TextMatch> Push(string path, string line)
    {
      LineNumber++;
      foreach (TextMatch completed in CompletePending(path, line))
      {
        yield return completed;
      }

      if (!IsSaturated)
      {
        foreach (TextMatch immediate in Start(path, line))
        {
          yield return immediate;
        }
      }

      Remember(line);
    }

    public IEnumerable<TextMatch> Finish(string path)
    {
      foreach (PendingMatch pending in Pending)
      {
        yield return pending.ToTextMatch(path);
      }

      Pending.Clear();
    }

    private bool IsSaturated => Options.MaxMatches is int max && Started >= max;

    private IEnumerable<TextMatch> CompletePending(string path, string line)
    {
      if (Options.ContextAfter <= 0 || Pending.Count == 0)
      {
        yield break;
      }

      List<TextMatch> completed = [];
      for (int index = 0; index < Pending.Count;)
      {
        PendingMatch pending = Pending[index];
        pending.ContextAfter.Add(line);
        if (pending.ContextAfter.Count >= Options.ContextAfter)
        {
          completed.Add(pending.ToTextMatch(path));
          Pending.RemoveAt(index);
          continue;
        }

        index++;
      }

      foreach (TextMatch match in completed)
      {
        yield return match;
      }
    }

    private IEnumerable<TextMatch> Start(string path, string line)
    {
      if (Options.InvertMatch)
      {
        if (Pattern.IsMatch(line))
        {
          yield break;
        }

        Started++;
        foreach (TextMatch match in Add(path, line, Pattern.Match(line)))
        {
          yield return match;
        }

        yield break;
      }

      bool counted = false;
      for (Match found = Pattern.Match(line); found.Success; found = found.NextMatch())
      {
        if (!counted)
        {
          Started++;
          counted = true;
        }

        foreach (TextMatch match in Add(path, line, found))
        {
          yield return match;
        }
      }
    }

    private IEnumerable<TextMatch> Add(string path, string line, Match match)
    {
      string[] contextBefore = Options.ContextBefore <= 0 ? [] : Before.ToArray();
      PendingMatch pending = new()
      {
        LineNumber = LineNumber,
        Line = line,
        Match = match,
        ContextBefore = contextBefore
      };

      if (Options.ContextAfter <= 0)
      {
        yield return pending.ToTextMatch(path);
        yield break;
      }

      Pending.Add(pending);
    }

    private void Remember(string line)
    {
      if (Options.ContextBefore <= 0)
      {
        return;
      }

      Before.Enqueue(line);
      while (Before.Count > Options.ContextBefore)
      {
        Before.Dequeue();
      }
    }
  }

  private sealed class PendingMatch
  {
    public required int LineNumber { get; init; }

    public required string Line { get; init; }

    public required Match Match { get; init; }

    public required string[] ContextBefore { get; init; }

    public List<string> ContextAfter { get; } = [];

    public TextMatch ToTextMatch(string path)
    {
      return new TextMatch
      {
        Path = path,
        LineNumber = LineNumber,
        Line = Line,
        Match = Match,
        ContextBefore = ContextBefore,
        ContextAfter = ContextAfter.ToArray()
      };
    }
  }
}
