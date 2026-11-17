#region Purpose
// W3C trace context hand-off to child processes: TRACEPARENT and TRACESTATE from Activity.Current,
// so a child's spans join the caller's trace without each consumer wiring it by hand.
#endregion

#region Design
// - Capture reads Activity.Current when a command starts, not when the builder or CommandResult is built.
//   A CommandResult built before a span opens still hands the child the span active at ExecuteAsync/CaptureAsync.
// - No current activity, or a default trace id or span id, captures nothing. The child environment is then left
//   alone, so a TRACEPARENT inherited from ganda or CI still reaches the child.
// - TRACEPARENT is 00-{traceId}-{spanId}-{flags}. Flags are 01 when ActivityTraceFlags.Recorded is set, else 00.
// - TRACESTATE is written only when Activity.TraceStateString is non-empty.
// - A key the caller set through WithEnvironmentVariable(s) wins, including a null value (remove).
//   That null is the opt-out. Key matching ignores case on Windows, where environment names are case-insensitive.
// - The header values are never logged.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// The trace context resolved for one child process start.
/// </summary>
/// <param name="TraceParent">The W3C <c>traceparent</c> value.</param>
/// <param name="TraceState">The W3C <c>tracestate</c> value, or null when the activity has none.</param>
internal readonly record struct TraceContext(string TraceParent, string? TraceState);

/// <summary>
/// Resolves <see cref="Activity.Current"/> into the <c>TRACEPARENT</c> and <c>TRACESTATE</c> environment
/// variables a child process reads, without overriding a value the caller set explicitly.
/// </summary>
internal static class TraceContextPropagation
{
  /// <summary>Environment variable carrying the W3C <c>traceparent</c> header.</summary>
  internal const string TraceParentVariable = "TRACEPARENT";

  /// <summary>Environment variable carrying the W3C <c>tracestate</c> header.</summary>
  internal const string TraceStateVariable = "TRACESTATE";

  /// <summary>
  /// Reads the current activity. Returns null when there is none or its trace id or span id is default.
  /// </summary>
  internal static TraceContext? Capture()
  {
    Activity? activity = Activity.Current;
    if (activity is null || activity.TraceId == default || activity.SpanId == default)
    {
      return null;
    }

    string flags = (activity.ActivityTraceFlags & ActivityTraceFlags.Recorded) != 0 ? "01" : "00";
    string traceParent = $"00-{activity.TraceId.ToHexString()}-{activity.SpanId.ToHexString()}-{flags}";
    string? traceState = string.IsNullOrEmpty(activity.TraceStateString) ? null : activity.TraceStateString;
    return new TraceContext(traceParent, traceState);
  }

  /// <summary>
  /// The variables to add to a child environment for <paramref name="context"/>, skipping any key that
  /// <paramref name="explicitVariables"/> already sets (a null value there is an explicit removal).
  /// </summary>
  /// <param name="explicitVariables">Variables the caller configured, or null when none were configured.</param>
  /// <param name="context">The captured context, or null when there is no current activity.</param>
  internal static List<KeyValuePair<string, string>> VariablesToAdd
  (
    IReadOnlyDictionary<string, string?>? explicitVariables,
    TraceContext? context
  )
  {
    List<KeyValuePair<string, string>> variables = [];
    if (context is not { } trace)
    {
      return variables;
    }

    if (!IsExplicit(explicitVariables, TraceParentVariable))
    {
      variables.Add(new KeyValuePair<string, string>(TraceParentVariable, trace.TraceParent));
    }

    if (trace.TraceState is not null && !IsExplicit(explicitVariables, TraceStateVariable))
    {
      variables.Add(new KeyValuePair<string, string>(TraceStateVariable, trace.TraceState));
    }

    return variables;
  }

  /// <summary>
  /// Returns <paramref name="command"/> with the trace context variables added to its environment.
  /// The command's own variables are the explicit set: Amuru never bakes trace variables into it.
  /// </summary>
  internal static Command Apply(Command command, TraceContext? context)
  {
    List<KeyValuePair<string, string>> additions = VariablesToAdd(command.EnvironmentVariables, context);
    if (additions.Count == 0)
    {
      return command;
    }

    Dictionary<string, string?> merged = new(command.EnvironmentVariables);
    foreach (KeyValuePair<string, string> addition in additions)
    {
      merged[addition.Key] = addition.Value;
    }

    return command.WithEnvironmentVariables(merged);
  }

  private static bool IsExplicit(IReadOnlyDictionary<string, string?>? explicitVariables, string name)
  {
    if (explicitVariables is null)
    {
      return false;
    }

    if (!OperatingSystem.IsWindows())
    {
      return explicitVariables.ContainsKey(name);
    }

    foreach (string key in explicitVariables.Keys)
    {
      if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
      {
        return true;
      }
    }

    return false;
  }
}
