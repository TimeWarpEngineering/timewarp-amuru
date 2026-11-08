#region Purpose
// Test oracle for the phases of one command run.
#endregion

#region Design
// The chain is one expression with a constant name, which is what the Ramani generator reads.
// .Exhaustive() and .Signals close every (state, signal) pair. Refusals are per state.
// A machine-wide RefuseOthers would make that proof vacuous.
// Running's do-activity only exists so SendAsync returns the step. It waits on the host token.
// The entry of Running records the TTY start checks and refuses before mock-match.
// An exit code is copied in the effect. The entry of Exited records it with TimedOut false.
// The entry of TimedOut records exit 124, then throws only for strict validation of a non-TTY run.
// The entry of Canceled throws and does not write LastOutput.
// GraceElapsed during Interrupting goes to Canceled when the caller token has already fired.
// Exited, TimedOut, and Canceled stay non-final and refuse every signal with a reason.
// A final state cannot refuse. SM010 is suppressed on this file for that reason.
#endregion

namespace TimeWarp.Amuru.Testing.CommandCompletion;

using TimeWarp.Ramani;

internal sealed partial class CommandCompletionMachine
{
  public static StateMachineDefinition Definition() =>
    Machine.Named<CommandCompletionState, CommandCompletionContext>("command-completion")
      .Exhaustive()
      .Signals<ChildExited, TimeoutElapsed, GraceElapsed, CallerCanceled>()
      .Initial(CommandCompletionState.Running)
      .State(CommandCompletionState.Running)
        .Entry(static context => context.BeginRun())
        .Do(static (_, token) => Task.Delay(Timeout.Infinite, token))
        .On<ChildExited>().GoTo(CommandCompletionState.Exited).Effect(static (context, signal) => context.NoteExit(signal)).Done()
        .On<TimeoutElapsed>().GoTo(CommandCompletionState.Interrupting).Done()
        .On<CallerCanceled>().GoTo(CommandCompletionState.Canceled).Done()
        .Refuse<GraceElapsed>("the grace period has not started")
        .Done()
      .State(CommandCompletionState.Interrupting)
        .On<ChildExited>().GoTo(CommandCompletionState.Exited).Effect(static (context, signal) => context.NoteExit(signal)).Done()
        .On<GraceElapsed>().When(static (context, _) => context.CallerTokenFired).GoTo(CommandCompletionState.Canceled).Done()
        .On<GraceElapsed>().Else().GoTo(CommandCompletionState.TimedOut).Done()
        .On<CallerCanceled>().GoTo(CommandCompletionState.Canceled).Done()
        .Refuse<TimeoutElapsed>("the run is already interrupting")
        .Done()
      .State(CommandCompletionState.Exited)
        .Entry(static context => context.RememberExit())
        .Refuse<ChildExited>("the run has exited")
        .Refuse<TimeoutElapsed>("the run has exited")
        .Refuse<GraceElapsed>("the run has exited")
        .Refuse<CallerCanceled>("the run has exited")
        .Done()
      .State(CommandCompletionState.TimedOut)
        .Entry(static context => context.RememberTimeout())
        .Refuse<ChildExited>("the run has timed out")
        .Refuse<TimeoutElapsed>("the run has timed out")
        .Refuse<GraceElapsed>("the run has timed out")
        .Refuse<CallerCanceled>("the run has timed out")
        .Done()
      .State(CommandCompletionState.Canceled)
        .Entry(static _ => throw new OperationCanceledException())
        .Refuse<ChildExited>("the run was canceled")
        .Refuse<TimeoutElapsed>("the run was canceled")
        .Refuse<GraceElapsed>("the run was canceled")
        .Refuse<CallerCanceled>("the run was canceled")
        .Done()
      .Build();
}
