#region Purpose
// Unit and header mode for the native Tail command.
#endregion

namespace TimeWarp.Amuru.Native.Text;

/// <summary>
/// What a <see cref="TailMeasure"/> counts.
/// </summary>
public enum TailUnit
{
  /// <summary>Lines ended by newline, or by NUL when zero-terminated.</summary>
  Lines = 0,

  /// <summary>Raw bytes.</summary>
  Bytes = 1
}

/// <summary>
/// When Tail prints a <c>==&gt; name &lt;==</c> header.
/// </summary>
public enum TailHeaders
{
  /// <summary>Print a header when there is more than one operand.</summary>
  Auto = 0,

  /// <summary>Never print headers (<c>-q</c>).</summary>
  Never = 1,

  /// <summary>Print a header for every operand (<c>-v</c>).</summary>
  Always = 2
}
