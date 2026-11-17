#region Purpose
// Start-time trace context for CommandResult: the command handed to CliWrap carries TRACEPARENT and
// TRACESTATE from the Activity current when the run starts.
#endregion

#region Design
// - InternalCommand never holds trace variables. Its environment is exactly what the caller configured,
//   so TraceContextPropagation can tell an explicit caller value (or null removal) from its own.
// - StartCommand runs inside each execution mode, right before the CliWrap call. That is the start-time
//   choice: a CommandResult built before a span opens still propagates the span active when it runs.
// - A Pipe composition keeps its stages. CliWrap's pipe operator hides the downstream command inside a
//   pipe target, so StartCommand rebuilds the left fold (stage0 | stage1 | ...) with each stage's
//   environment resolved. That fold is the same shape PipeCore builds.
// - TtyPassthroughAsync and the JSON-RPC child fill ProcessStartInfo themselves and call
//   TraceContextPropagation.VariablesToAdd directly.
#endregion

namespace TimeWarp.Amuru;

public partial class CommandResult
{
  // Null for a single command. For a Pipe composition, the stage commands in pipe order.
  private IReadOnlyList<Command>? PipeStages { get; }

  /// <summary>
  /// The command to start now: <see cref="InternalCommand"/> with the current trace context in the
  /// environment of every stage. Call only after the null-command check.
  /// </summary>
  private Command StartCommand()
  {
    TraceContext? trace = TraceContextPropagation.Capture();
    if (trace is null)
    {
      return InternalCommand!;
    }

    if (PipeStages is null)
    {
      return TraceContextPropagation.Apply(InternalCommand!, trace);
    }

    Command composed = TraceContextPropagation.Apply(PipeStages[0], trace);
    for (int index = 1; index < PipeStages.Count; index++)
    {
      composed |= TraceContextPropagation.Apply(PipeStages[index], trace);
    }

    return composed;
  }

  /// <summary>
  /// Fills the TTY passthrough child's environment: the caller's variables (null removes), then the
  /// current trace context for keys the caller did not set.
  /// </summary>
  private void FillTtyEnvironment(System.Collections.Specialized.StringDictionary environment)
  {
    foreach (KeyValuePair<string, string?> variable in InternalCommand!.EnvironmentVariables)
    {
      if (variable.Value is null)
      {
        environment.Remove(variable.Key);
      }
      else
      {
        environment[variable.Key] = variable.Value;
      }
    }

    TraceContext? trace = TraceContextPropagation.Capture();
    foreach (KeyValuePair<string, string> traceVariable in
      TraceContextPropagation.VariablesToAdd(InternalCommand.EnvironmentVariables, trace))
    {
      environment[traceVariable.Key] = traceVariable.Value;
    }
  }

  /// <summary>
  /// The stages of this command when it becomes the upstream side of a new Pipe stage.
  /// </summary>
  private IReadOnlyList<Command> StagesForPipe(Command next) =>
    PipeStages is null ? [InternalCommand!, next] : [.. PipeStages, next];
}
