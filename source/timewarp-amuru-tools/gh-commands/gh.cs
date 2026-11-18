#region Purpose
// Entry point for the typed GitHub CLI builders. Each verb group lives in its own partial file.
#endregion

#region Design
// gh emits no OpenTelemetry of its own. Every Gh builder execution opens a span on the
// ActivitySource named ActivitySourceName, so an app that subscribes to that name sees each gh call
// as a child of its own span. TRACEPARENT reaches gh from CommandResult (task 149) and points at
// that span. Task 152.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for GitHub CLI (<c>gh</c>) commands. Each execution opens a span on
/// <see cref="ActivitySourceName"/>.
/// </summary>
public static partial class Gh
{
  /// <summary>
  /// The <see cref="System.Diagnostics.ActivitySource"/> name the gh spans are created on. Add it to
  /// the tracer provider (for example Nuru's <c>UseTelemetry</c> activity sources) to export them.
  /// </summary>
  public const string ActivitySourceName = "TimeWarp.Amuru.Gh";

  internal const string Executable = "gh";
}
