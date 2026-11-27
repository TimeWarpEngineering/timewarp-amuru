#region Purpose
// Options record for the native Tail command.
#endregion

#region Design
// In-process callers build this value directly. The parser fills the same type from argv.
// SaveStream applies only to a non-seekable stdin read of the last N lines. Files are never
// copied into the save folder. FollowInterval is not an argv option; -s is rejected.
#endregion

namespace TimeWarp.Amuru.Native.Text;

/// <summary>
/// Options for <see cref="Commands.Tail(TailOptions, IReadOnlyList{string})"/>
/// and <see cref="Direct.Tail(TailOptions, IReadOnlyList{string}, Stream, Stream, Stream, CancellationToken)"/>.
/// </summary>
public sealed record TailOptions
{
  /// <summary>The default cap on a saved stdin stream: 64 MiB.</summary>
  public const long DefaultSaveLimit = 64L * 1024 * 1024;

  /// <summary>
  /// Gets the lines or bytes to keep. The default is the last 10 lines.
  /// </summary>
  public TailMeasure Measure { get; init; } = TailMeasure.LastLines(10);

  /// <summary>
  /// Gets when to print <c>==&gt; name &lt;==</c> headers.
  /// </summary>
  public TailHeaders Headers { get; init; } = TailHeaders.Auto;

  /// <summary>
  /// Gets a value indicating that the line delimiter is NUL (<c>-z</c>) instead of newline.
  /// </summary>
  public bool ZeroTerminated { get; init; }

  /// <summary>
  /// Gets a value indicating that file operands are polled for appended bytes (<c>-f</c>).
  /// </summary>
  public bool Follow { get; init; }

  /// <summary>
  /// Gets how long to wait between follow polls. The command line cannot set this.
  /// </summary>
  public TimeSpan FollowInterval { get; init; } = TimeSpan.FromSeconds(1);

  /// <summary>
  /// Gets a value indicating that a non-seekable stdin last-N-lines read is copied to a file.
  /// A failure to create or write that file still returns the slice.
  /// </summary>
  public bool SaveStream { get; init; } = true;

  /// <summary>
  /// Gets the directory for a saved stream.
  /// <see langword="null"/> uses <c>&lt;temp&gt;/tw-agent-streams/&lt;user&gt;</c>.
  /// </summary>
  public string? StreamDirectory { get; init; }

  /// <summary>
  /// Gets the maximum number of bytes written to a saved stream.
  /// The default is <see cref="DefaultSaveLimit"/>.
  /// </summary>
  public long SaveLimit { get; init; } = DefaultSaveLimit;

  /// <summary>
  /// Gets a value indicating that the caller asked for help. Tail does not print it.
  /// </summary>
  public bool Help { get; init; }

  /// <summary>
  /// Gets a value indicating that the caller asked for the version. Tail does not print it.
  /// </summary>
  public bool Version { get; init; }
}
