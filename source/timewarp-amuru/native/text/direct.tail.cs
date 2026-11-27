#region Purpose
// Direct API for Tail: writes the slice to caller-supplied streams.
#endregion

#region Design
// File errors are stderr and exit code 1, matching the command. Programmer errors
// (a null argument, a negative count, follow without a token) throw.
#endregion

namespace TimeWarp.Amuru.Native.Text;

public static partial class Direct
{
  /// <summary>
  /// Writes the tail of <paramref name="paths"/> to <paramref name="standardOutput"/>.
  /// </summary>
  /// <param name="options">The lines or bytes to keep, headers, follow, and the save switch.</param>
  /// <param name="paths">File operands. Empty, or an entry <c>-</c>, reads <paramref name="standardInput"/>.</param>
  /// <param name="standardInput">Stdin. Not disposed.</param>
  /// <param name="standardOutput">Where the slice is written. Not disposed.</param>
  /// <param name="standardError">Where notes and file errors are written. Not disposed.</param>
  /// <param name="cancellationToken">
  /// Ends <c>-f</c> and returns the bytes already written. Ignored for a finite read.
  /// </param>
  /// <returns>The exit code and the saved stdin path, when one was kept.</returns>
  /// <exception cref="ArgumentNullException">When a required argument is null.</exception>
  /// <exception cref="ArgumentException">When follow is set and <paramref name="cancellationToken"/> cannot be canceled.</exception>
  /// <exception cref="ArgumentOutOfRangeException">When the count, the follow interval, or the save limit is out of range.</exception>
  public static TailRunResult Tail(
    TailOptions options,
    IReadOnlyList<string> paths,
    Stream standardInput,
    Stream standardOutput,
    Stream standardError,
    CancellationToken cancellationToken = default)
  {
    return TailEngine.Run(
      options,
      paths,
      standardInput,
      standardOutput,
      standardError,
      cancellationToken);
  }
}
