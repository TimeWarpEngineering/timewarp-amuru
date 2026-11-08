#!/usr/bin/env -S dotnet --

#region Purpose
// Steps the command-completion oracle through every accepted signal and every refusal.
#endregion

#region Design
// SendAsync's MachineStep is the assertion. No step observer and no completion source on a signal.
// The host token cancels the Running do-activity when the harness is disposed.
// A thrown entry still leaves the machine in the state it entered, so the result is readable after the exception.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace CommandCompletionMachine_
{
  using TimeWarp.Amuru.Testing.CommandCompletion;
  using TimeWarp.Ramani;

  [TestTag("Core")]
  public class SendAsync_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<SendAsync_Given_>();

    public static async Task Definition_Should_Build()
    {
      StateMachineDefinition definition = CommandCompletionMachine.Definition();

      definition.Name.ShouldBe("command-completion");
      await Task.CompletedTask;
    }

    public static async Task Running_ChildExited_Should_ExitWithThatCode()
    {
      using var run = new Run(new CommandCompletionContext());

      MachineStep step = await run.Machine.SendAsync(new ChildExited(3));

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      run.Machine.State.ShouldBe(CommandCompletionState.Exited);
      run.Context.LastOutput.ShouldNotBeNull();
      run.Context.LastOutput.ExitCode.ShouldBe(3);
      run.Context.LastOutput.TimedOut.ShouldBeFalse();
      run.Context.Order.ShouldBe([
        CommandCompletionContext.TtyStandardInputCheck,
        CommandCompletionContext.TtyPipelineCheck,
        CommandCompletionContext.MockMatch
      ]);
    }

    public static async Task Running_TimeoutElapsed_Should_InterruptWithoutAResult()
    {
      using var run = new Run(new CommandCompletionContext());

      MachineStep step = await run.Machine.SendAsync(new TimeoutElapsed());

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      run.Machine.State.ShouldBe(CommandCompletionState.Interrupting);
      run.Context.LastOutput.ShouldBeNull();
    }

    public static async Task Interrupting_ChildExited_Should_ExitWithTheChildCode()
    {
      using var run = new Run(new CommandCompletionContext());
      await run.Machine.SendAsync(new TimeoutElapsed());

      MachineStep step = await run.Machine.SendAsync(new ChildExited(130));

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      run.Machine.State.ShouldBe(CommandCompletionState.Exited);
      run.Context.LastOutput.ShouldNotBeNull();
      run.Context.LastOutput.ExitCode.ShouldBe(130);
      run.Context.LastOutput.TimedOut.ShouldBeFalse();
    }

    public static async Task Interrupting_GraceElapsed_Should_TimeOutWith124()
    {
      using var run = new Run(new CommandCompletionContext());
      await run.Machine.SendAsync(new TimeoutElapsed());

      MachineStep step = await run.Machine.SendAsync(new GraceElapsed());

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      run.Machine.State.ShouldBe(CommandCompletionState.TimedOut);
      run.Context.LastOutput.ShouldNotBeNull();
      run.Context.LastOutput.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
      run.Context.LastOutput.TimedOut.ShouldBeTrue();
    }

    public static async Task Running_CallerCanceled_Should_CancelWithoutLastOutput()
    {
      using var run = new Run(new CommandCompletionContext());

      await Should.ThrowAsync<OperationCanceledException>(() => run.Machine.SendAsync(new CallerCanceled()));

      run.Machine.State.ShouldBe(CommandCompletionState.Canceled);
      run.Context.LastOutput.ShouldBeNull();
    }

    public static async Task Interrupting_CallerCanceled_Should_CancelWithoutLastOutput()
    {
      using var run = new Run(new CommandCompletionContext());
      await run.Machine.SendAsync(new TimeoutElapsed());

      await Should.ThrowAsync<OperationCanceledException>(() => run.Machine.SendAsync(new CallerCanceled()));

      run.Machine.State.ShouldBe(CommandCompletionState.Canceled);
      run.Context.LastOutput.ShouldBeNull();
    }

    public static async Task GraceElapsed_WhenCallerTokenFired_Should_CancelInsteadOfTimingOut()
    {
      using var run = new Run(new CommandCompletionContext());
      await run.Machine.SendAsync(new TimeoutElapsed());
      run.Context.CallerTokenFired = true;

      await Should.ThrowAsync<OperationCanceledException>(() => run.Machine.SendAsync(new GraceElapsed()));

      run.Machine.State.ShouldBe(CommandCompletionState.Canceled);
      run.Context.LastOutput.ShouldBeNull();
    }

    public static async Task TimedOut_ZeroExitValidation_Should_RecordThenThrow()
    {
      using var run = new Run(new CommandCompletionContext { ZeroExitValidation = true });
      await run.Machine.SendAsync(new TimeoutElapsed());

      TimeoutException exception = await Should.ThrowAsync<TimeoutException>(
        () => run.Machine.SendAsync(new GraceElapsed()));

      exception.Message.ShouldContain("timed out");
      exception.Message.ShouldContain("terminated");
      run.Machine.State.ShouldBe(CommandCompletionState.TimedOut);
      run.Context.LastOutput.ShouldNotBeNull();
      run.Context.LastOutput.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
      run.Context.LastOutput.TimedOut.ShouldBeTrue();
    }

    public static async Task TimedOut_TtyZeroExit_Should_RecordWithoutThrowing()
    {
      using var run = new Run(new CommandCompletionContext
      {
        ZeroExitValidation = true,
        IsTty = true
      });
      await run.Machine.SendAsync(new TimeoutElapsed());

      MachineStep step = await run.Machine.SendAsync(new GraceElapsed());

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      run.Machine.State.ShouldBe(CommandCompletionState.TimedOut);
      run.Context.LastOutput.ShouldNotBeNull();
      run.Context.LastOutput.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
      run.Context.LastOutput.TimedOut.ShouldBeTrue();
    }

    public static async Task TtyConfiguredStandardInput_Should_RefuseBeforeMockMatch()
    {
      var context = new CommandCompletionContext
      {
        IsTty = true,
        ConfiguredStandardInput = "data"
      };

      InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => new Run(context));

      exception.Message.ShouldBe(CommandCompletionContext.TtyConfiguredStandardInputMessage);
      context.Order.ShouldBe([
        CommandCompletionContext.TtyStandardInputCheck,
        CommandCompletionContext.Refusal
      ]);
      context.Order.ShouldNotContain(CommandCompletionContext.MockMatch);
      await Task.CompletedTask;
    }

    public static async Task TtyEmptyStandardInput_Should_RefuseBeforeMockMatch()
    {
      var context = new CommandCompletionContext
      {
        IsTty = true,
        ConfiguredStandardInput = string.Empty
      };

      InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => new Run(context));

      exception.Message.ShouldBe(CommandCompletionContext.TtyConfiguredStandardInputMessage);
      context.Order.ShouldNotContain(CommandCompletionContext.MockMatch);
      await Task.CompletedTask;
    }

    public static async Task TtyPipeline_Should_RefuseBeforeMockMatch()
    {
      var context = new CommandCompletionContext
      {
        IsTty = true,
        IsPipeline = true
      };

      InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => new Run(context));

      exception.Message.ShouldBe(CommandCompletionContext.TtyPipelineMessage);
      context.Order.ShouldBe([
        CommandCompletionContext.TtyStandardInputCheck,
        CommandCompletionContext.TtyPipelineCheck,
        CommandCompletionContext.Refusal
      ]);
      context.Order.ShouldNotContain(CommandCompletionContext.MockMatch);
      await Task.CompletedTask;
    }

    public static async Task TtyPipelineWithStandardInput_Should_RefuseStandardInputFirst()
    {
      var context = new CommandCompletionContext
      {
        IsTty = true,
        IsPipeline = true,
        ConfiguredStandardInput = "data"
      };

      InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => new Run(context));

      exception.Message.ShouldBe(CommandCompletionContext.TtyConfiguredStandardInputMessage);
      context.Order.ShouldBe([
        CommandCompletionContext.TtyStandardInputCheck,
        CommandCompletionContext.Refusal
      ]);
      await Task.CompletedTask;
    }

    public static async Task Running_GraceElapsed_Should_BeRefused()
    {
      using var run = new Run(new CommandCompletionContext());

      await AssertRefusedAsync(
        run.Machine,
        CommandCompletionState.Running,
        new GraceElapsed(),
        "the grace period has not started");
    }

    public static async Task Interrupting_TimeoutElapsed_Should_BeRefused()
    {
      using var run = new Run(new CommandCompletionContext());
      await run.Machine.SendAsync(new TimeoutElapsed());

      await AssertRefusedAsync(
        run.Machine,
        CommandCompletionState.Interrupting,
        new TimeoutElapsed(),
        "the run is already interrupting");
    }

    public static async Task Exited_Signals_Should_BeRefused()
    {
      using var run = new Run(new CommandCompletionContext());
      await run.Machine.SendAsync(new ChildExited(0));

      await AssertTerminalAsync(run.Machine, CommandCompletionState.Exited, "the run has exited");
    }

    public static async Task TimedOut_Signals_Should_BeRefused()
    {
      using var run = new Run(new CommandCompletionContext());
      await run.Machine.SendAsync(new TimeoutElapsed());
      await run.Machine.SendAsync(new GraceElapsed());

      await AssertTerminalAsync(run.Machine, CommandCompletionState.TimedOut, "the run has timed out");
    }

    public static async Task Canceled_Signals_Should_BeRefused()
    {
      using var run = new Run(new CommandCompletionContext());
      await Should.ThrowAsync<OperationCanceledException>(() => run.Machine.SendAsync(new CallerCanceled()));

      await AssertTerminalAsync(run.Machine, CommandCompletionState.Canceled, "the run was canceled");
    }

    private static async Task AssertTerminalAsync(
      CommandCompletionMachine machine,
      CommandCompletionState state,
      string reason)
    {
      await AssertRefusedAsync(machine, state, new ChildExited(9), reason);
      await AssertRefusedAsync(machine, state, new TimeoutElapsed(), reason);
      await AssertRefusedAsync(machine, state, new GraceElapsed(), reason);
      await AssertRefusedAsync(machine, state, new CallerCanceled(), reason);
    }

    private static async Task AssertRefusedAsync(
      CommandCompletionMachine machine,
      CommandCompletionState state,
      ISignal signal,
      string reason)
    {
      MachineStep step = await machine.SendAsync(signal);
      step.Outcome.ShouldBe(MachineStepOutcome.Refused);
      step.Reason.ShouldBe(reason);
      machine.State.ShouldBe(state);
    }

    private sealed class Run : IDisposable
    {
      private readonly CancellationTokenSource Cancellation;

      public Run(CommandCompletionContext context)
      {
        Context = context;
        Cancellation = new CancellationTokenSource();
        Machine = new CommandCompletionMachine();
        Machine.HostCancellation = Cancellation.Token;
        try
        {
          Machine.Start(context);
        }
        catch
        {
          Cancellation.Cancel();
          Cancellation.Dispose();
          throw;
        }
      }

      public CommandCompletionMachine Machine { get; }

      public CommandCompletionContext Context { get; }

      public void Dispose()
      {
        Cancellation.Cancel();
        Cancellation.Dispose();
      }
    }
  }
}
