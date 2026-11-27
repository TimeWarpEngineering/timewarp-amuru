#region Purpose
// How many lines or bytes Tail keeps, and whether that count is from the start.
#endregion

#region Design
// One value replaces both -n and -c. The parser writes a new measure when either
// option appears, so the last one wins, matching tail.
// A from-start count is 1-based. Zero from the start means the first byte or line,
// which is how tail treats +0.
#endregion

namespace TimeWarp.Amuru.Native.Text;

/// <summary>
/// A line or byte count for <see cref="TailOptions"/>.
/// </summary>
public readonly record struct TailMeasure(TailUnit Unit, long Count, bool FromStart)
{
  /// <summary>The last <paramref name="count"/> lines.</summary>
  public static TailMeasure LastLines(long count) => new(TailUnit.Lines, count, false);

  /// <summary>Lines starting at 1-based <paramref name="line"/>. Zero starts at the first line.</summary>
  public static TailMeasure FromLine(long line) => new(TailUnit.Lines, line, true);

  /// <summary>The last <paramref name="count"/> bytes.</summary>
  public static TailMeasure LastBytes(long count) => new(TailUnit.Bytes, count, false);

  /// <summary>Bytes starting at 1-based <paramref name="offset"/>. Zero starts at the first byte.</summary>
  public static TailMeasure FromByte(long offset) => new(TailUnit.Bytes, offset, true);
}
