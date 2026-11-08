#region Purpose
// Context for the command-completion test oracle: validation, TTY, and the recorded result.
#endregion

#region Design
// Behaviors are static lambdas, so every fact they read lives here.
// ZeroExitValidation and IsTty select the timeout outcome. A TTY run records exit 124 and does not throw.
// CallerTokenFired is the caller token TimeoutSession reads beside the grace timer.
// GraceElapsed takes Canceled when that flag is set, which is the race the catch order decides.
// Order is the start sequence. A TTY refusal appends its token and stops. mock-match is later in the same list.
// LastOutput is written for an exit and for a timeout, including a timeout that then throws.
// Cancellation throws and leaves LastOutput unset.
#endregion

namespace TimeWarp.Amuru.Testing.CommandCompletion;

internal sealed class CommandCompletionContext
{
  internal const string TtyConfiguredStandardInputMessage =
    "TtyPassthroughAsync requires console stdin; configured standard input cannot be used with a TTY";

  internal const string TtyPipelineMessage =
    "TtyPassthroughAsync cannot run a pipeline; the upstream stage would be dropped";

  internal const string TtyStandardInputCheck = "tty-stdin";

  internal const string TtyPipelineCheck = "tty-pipeline";

  internal const string Refusal = "refusal";

  internal const string MockMatch = "mock-match";

  private readonly List<string> Steps = [];

  public bool ZeroExitValidation { get; init; }

  public bool IsTty { get; init; }

  public string? ConfiguredStandardInput { get; init; }

  public bool IsPipeline { get; init; }

  public bool CallerTokenFired { get; set; }

  public int ExitCode { get; private set; }

  public CommandOutput? LastOutput { get; private set; }

  public IReadOnlyList<string> Order => Steps;

  public void BeginRun()
  {
    Steps.Add(TtyStandardInputCheck);
    if (IsTty && ConfiguredStandardInput is not null)
    {
      Steps.Add(Refusal);
      throw new InvalidOperationException(TtyConfiguredStandardInputMessage);
    }

    Steps.Add(TtyPipelineCheck);
    if (IsTty && IsPipeline)
    {
      Steps.Add(Refusal);
      throw new InvalidOperationException(TtyPipelineMessage);
    }

    Steps.Add(MockMatch);
  }

  public void NoteExit(ChildExited signal)
  {
    ExitCode = signal.ExitCode;
  }

  public void RememberExit()
  {
    LastOutput = new CommandOutput(string.Empty, string.Empty, ExitCode);
  }

  public void RememberTimeout()
  {
    LastOutput = new CommandOutput(string.Empty, string.Empty, CommandResult.TimeoutExitCode)
    {
      TimedOut = true
    };

    if (ZeroExitValidation && !IsTty)
    {
      throw new TimeoutException("Command timed out and was terminated.");
    }
  }
}
