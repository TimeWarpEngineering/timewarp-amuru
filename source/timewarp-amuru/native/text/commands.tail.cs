#region Purpose
// Commands API for Tail: parse argv or take a TailOptions value and return the bytes.
#endregion

#region Design
// The result keeps the stdout bytes unchanged, including a trailing newline and
// non-UTF-8 input. CommandOutput cannot do that, so Tail does not return one.
// Help and version are flags on the result. This method does not print either.
#endregion

namespace TimeWarp.Amuru.Native.Text;

public static partial class Commands
{
  /// <summary>
  /// Runs tail from <paramref name="arguments"/>. An operand that reads stdin requires the overload that takes a stream.
  /// </summary>
  /// <param name="arguments">The argument vector, without the program name.</param>
  /// <returns>The slice, the stderr text, and the exit code.</returns>
  /// <exception cref="ArgumentNullException">
  /// When <paramref name="arguments"/> is null, or an operand reads stdin and no stream was passed.
  /// </exception>
  public static TailOutput Tail(IReadOnlyList<string> arguments)
  {
    ArgumentNullException.ThrowIfNull(arguments);
    return Tail(arguments, null, CancellationToken.None);
  }

  /// <summary>
  /// Runs tail from <paramref name="arguments"/>.
  /// </summary>
  /// <param name="arguments">The argument vector, without the program name.</param>
  /// <param name="standardInput">
  /// Stdin bytes. <see langword="null"/> is empty input, and throws when an operand reads stdin.
  /// </param>
  /// <param name="cancellationToken">
  /// Ends <c>-f</c> and returns the bytes written so far. Ignored for a finite read.
  /// </param>
  /// <returns>The slice, the stderr text, and the exit code.</returns>
  /// <exception cref="ArgumentNullException">
  /// When <paramref name="arguments"/> is null, or an operand reads stdin and <paramref name="standardInput"/> is null.
  /// </exception>
  public static TailOutput Tail(
    IReadOnlyList<string> arguments,
    Stream? standardInput,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(arguments);
    TailParseResult parsed = TailParser.Parse(arguments);
    if (parsed.Error is not null)
    {
      return new TailOutput(ReadOnlyMemory<byte>.Empty, Encoding.UTF8.GetBytes(parsed.Error), 1, null, false, false);
    }

    return Capture(parsed.Options, parsed.Operands, standardInput, cancellationToken);
  }

  /// <summary>
  /// Runs tail from an options value built by the caller.
  /// An operand that reads stdin requires the overload that takes a stream.
  /// </summary>
  /// <param name="options">The lines or bytes to keep, headers, follow, and the save switch.</param>
  /// <param name="paths">File operands. Empty, or an entry <c>-</c>, reads stdin.</param>
  /// <returns>The slice, the stderr text, and the exit code.</returns>
  /// <exception cref="ArgumentNullException">
  /// When <paramref name="options"/> or <paramref name="paths"/> is null, or an operand reads stdin and no stream was passed.
  /// </exception>
  public static TailOutput Tail(TailOptions options, IReadOnlyList<string> paths)
  {
    ArgumentNullException.ThrowIfNull(options);
    ArgumentNullException.ThrowIfNull(paths);
    return Tail(options, paths, null, CancellationToken.None);
  }

  /// <summary>
  /// Runs tail from an options value built by the caller.
  /// </summary>
  /// <param name="options">The lines or bytes to keep, headers, follow, and the save switch.</param>
  /// <param name="paths">File operands. Empty, or an entry <c>-</c>, reads <paramref name="standardInput"/>.</param>
  /// <param name="standardInput">
  /// Stdin bytes. <see langword="null"/> is empty input, and throws when an operand reads stdin.
  /// </param>
  /// <param name="cancellationToken">
  /// Ends <c>-f</c> and returns the bytes written so far. Ignored for a finite read.
  /// </param>
  /// <returns>The slice, the stderr text, and the exit code.</returns>
  /// <exception cref="ArgumentNullException">
  /// When <paramref name="options"/> or <paramref name="paths"/> is null, or an operand reads stdin and <paramref name="standardInput"/> is null.
  /// </exception>
  public static TailOutput Tail(
    TailOptions options,
    IReadOnlyList<string> paths,
    Stream? standardInput,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(options);
    ArgumentNullException.ThrowIfNull(paths);
    return Capture(options, paths, standardInput, cancellationToken);
  }

  private static TailOutput Capture(
    TailOptions options,
    IReadOnlyList<string> paths,
    Stream? standardInput,
    CancellationToken cancellationToken)
  {
    if (options.Help || options.Version)
    {
      return new TailOutput(
        ReadOnlyMemory<byte>.Empty,
        ReadOnlyMemory<byte>.Empty,
        0,
        null,
        options.Help,
        options.Version);
    }

    bool needsInput = paths.Count == 0 || ContainsStandardInput(paths);
    if (needsInput && standardInput is null)
    {
      throw new ArgumentNullException(
        nameof(standardInput),
        "Pass standard input. Tail does not open the process standard input.");
    }

    Stream input = standardInput ?? Stream.Null;
    using MemoryStream stdout = new();
    using MemoryStream stderr = new();
    try
    {
      TailRunResult result = TailEngine.Run(options, paths, input, stdout, stderr, cancellationToken);
      return new TailOutput(stdout.ToArray(), stderr.ToArray(), result.ExitCode, result.SavedPath, false, false);
    }
    catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException)
    {
      byte[] message = Encoding.UTF8.GetBytes($"tail: {exception.Message}\n");
      return new TailOutput(stdout.ToArray(), message, 1, null, false, false);
    }
  }

  private static bool ContainsStandardInput(IReadOnlyList<string> paths)
  {
    foreach (string path in paths)
    {
      if (path == "-")
      {
        return true;
      }
    }

    return false;
  }
}
