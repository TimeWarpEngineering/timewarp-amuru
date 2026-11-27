#region Purpose
// Parser output: options, operands, or a usage error.
#endregion

namespace TimeWarp.Amuru.Native.Text;

/// <summary>
/// Result of <see cref="TailParser.Parse"/>.
/// </summary>
/// <param name="Options">The parsed options. Defaults when <paramref name="Error"/> is set.</param>
/// <param name="Operands">File operands. Empty means stdin when there is no <paramref name="Error"/>.</param>
/// <param name="Error">Usage error text including a trailing newline, or <see langword="null"/>.</param>
public sealed record TailParseResult(TailOptions Options, IReadOnlyList<string> Operands, string? Error)
{
  /// <summary>Gets a value indicating that <see cref="Error"/> is null.</summary>
  public bool Success => Error is null;
}
