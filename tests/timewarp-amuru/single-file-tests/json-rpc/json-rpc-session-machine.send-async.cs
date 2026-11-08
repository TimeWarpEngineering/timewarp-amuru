#!/usr/bin/env -S dotnet --
#:include json-rpc-session-machine.fake-seam.cs

#region Purpose
// Steps the JSON-RPC session machine through every accepted signal and every refusal.
#endregion

#region Design
// The fake seam stands in for the child process. SendAsync's MachineStep is the assertion.
// Started is not sent by the test. The Starting activity's completion posts it.
// ChildExited is not sent by the host after Dispose. The Stopping activity's completion posts it.
// InternalsVisibleTo on TimeWarp.Amuru names this file's assembly and the multi-file runner.
// #:include pulls in the fake seam, so this file runs alone. The namespace is block-scoped
// because the standalone entry point is a top-level statement.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace JsonRpcSessionMachine_
{

  using System.Text.Json;
  using TimeWarp.Amuru.JsonRpc;
  using TimeWarp.Ramani;

  [TestTag("JsonRpc")]
  public class SendAsync_Given_
  {
    private static readonly TimeSpan Wait = TimeSpan.FromSeconds(5);

    [ModuleInitializer]
    internal static void Register() => RegisterTests<SendAsync_Given_>();

    public static async Task Definition_Should_Build()
    {
      StateMachineDefinition definition = JsonRpcSessionMachine.Definition();

      definition.Name.ShouldBe("json-rpc-session");
      await Task.CompletedTask;
    }

    public static async Task Frame_Should_BeNewlineDelimitedJson()
    {
      string frame = JsonRpcFrames.WriteRequest("9", "initialize", """{"x":1}""");

      frame.ShouldEndWith("\n");
      using var document = JsonDocument.Parse(frame.TrimEnd('\n'));
      document.RootElement.GetProperty("jsonrpc").GetString().ShouldBe("2.0");
      document.RootElement.GetProperty("method").GetString().ShouldBe("initialize");
      document.RootElement.GetProperty("params").GetProperty("x").GetInt32().ShouldBe(1);
      JsonRpcFrames.TryReadId(frame, out string id).ShouldBeTrue();
      id.ShouldBe("9");
      JsonRpcFrames.TryReadId("""{"id":3}""", out string numeric).ShouldBeTrue();
      numeric.ShouldBe("3");
      JsonRpcFrames.TryReadId("{}", out _).ShouldBeFalse();
      await Task.CompletedTask;
    }

    public static async Task Idle_Start_Should_EnterStarting()
    {
      (JsonRpcSessionMachine machine, _, FakeJsonRpcProcessSeam seam) = Create(holdStart: true);

      MachineStep step = await machine.SendAsync(new Start());

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      step.Configuration.ActiveStates.ShouldBe(["Starting"]);
      machine.State.ShouldBe(JsonRpcSessionState.Starting);
      seam.StartCount.ShouldBe(0);
    }

    public static async Task Idle_IllegalSignals_Should_BeRefused()
    {
      (JsonRpcSessionMachine machine, _, _) = Create();

      await AssertRefusedAsync(machine, JsonRpcSessionState.Idle, new Started(), "the child has not started");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Idle, new Send("1", "m", null), "the child is not ready");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Idle, new Response("1", "{}"), "the child is not ready");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Idle, new RequestTimeout("1"), "the child is not ready");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Idle, new ChildExited(), "the child is not running");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Idle, new ReadFault(), "the child is not running");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Idle, new Dispose(), "the child is not running");
    }

    public static async Task Starting_Completion_Should_EnterReadyWithoutHostStarted()
    {
      (JsonRpcSessionMachine machine, _, FakeJsonRpcProcessSeam seam) = Create();
      StepLog log = new();
      machine.StepObserver = log;

      MachineStep step = await machine.SendAsync(new Start());

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      await UntilAsync(() => machine.State == JsonRpcSessionState.Ready);
      seam.StartCount.ShouldBe(1);
      log.Snapshot().ShouldContain(candidate => candidate.Signal is Started && candidate.Outcome == MachineStepOutcome.Taken);
    }

    public static async Task Starting_Send_Should_RefuseBecauseTheChildIsNotReady()
    {
      (JsonRpcSessionMachine machine, _, _) = await StartingAsync();

      await AssertRefusedAsync(machine, JsonRpcSessionState.Starting, new Send("1", "m", null), "the child is not ready");
    }

    public static async Task Starting_SecondStart_Should_Refuse()
    {
      (JsonRpcSessionMachine machine, _, _) = await StartingAsync();

      await AssertRefusedAsync(machine, JsonRpcSessionState.Starting, new Start(), "the child is starting");
    }

    public static async Task Starting_ChildExited_Should_EnterExited()
    {
      (JsonRpcSessionMachine machine, _, _) = await StartingAsync();

      MachineStep step = await machine.SendAsync(new ChildExited());

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      step.Configuration.ActiveStates.ShouldBe(["Exited"]);
      machine.State.ShouldBe(JsonRpcSessionState.Exited);
    }

    public static async Task Starting_ReadFault_Should_EnterFaulted()
    {
      (JsonRpcSessionMachine machine, _, _) = await StartingAsync();

      MachineStep step = await machine.SendAsync(new ReadFault());

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Faulted);
    }

    public static async Task Starting_OtherSignals_Should_BeRefused()
    {
      (JsonRpcSessionMachine machine, _, _) = await StartingAsync();

      await AssertRefusedAsync(machine, JsonRpcSessionState.Starting, new Response("1", "{}"), "the child is not ready");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Starting, new RequestTimeout("1"), "the child is not ready");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Starting, new Dispose(), "the child is not ready");
    }

    public static async Task Ready_Send_Should_RecordTheIdAndWriteTheFrame()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();

      MachineStep step = await machine.SendAsync(new Send("7", "tools/list", """{"cursor":"0"}"""));

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Ready);
      context.InFlight["7"].Status.ShouldBe(JsonRpcInFlightStatus.Pending);
      await UntilAsync(() => seam.Frames().Length == 1);
      string frame = seam.Frames()[0];
      frame.ShouldEndWith("\n");
      using var document = JsonDocument.Parse(frame.TrimEnd('\n'));
      document.RootElement.GetProperty("id").GetString().ShouldBe("7");
      document.RootElement.GetProperty("method").GetString().ShouldBe("tools/list");
      document.RootElement.GetProperty("params").GetProperty("cursor").GetString().ShouldBe("0");
    }

    public static async Task Ready_OverlappingSends_Should_StayReady()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, _) = await ReadyAsync();

      MachineStep first = await machine.SendAsync(new Send("a", "one", null));
      MachineStep second = await machine.SendAsync(new Send("b", "two", null));

      first.Outcome.ShouldBe(MachineStepOutcome.Taken);
      second.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Ready);
      context.InFlight.Count.ShouldBe(2);
    }

    public static async Task Ready_Response_Should_CompleteTheId()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, _) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));

      MachineStep step = await machine.SendAsync(new Response("7", """{"id":"7","result":1}"""));

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Ready);
      context.InFlight["7"].Status.ShouldBe(JsonRpcInFlightStatus.Completed);
      context.InFlight["7"].Payload.ShouldBe("""{"id":"7","result":1}""");
    }

    public static async Task Ready_UnknownResponse_Should_Refuse()
    {
      (JsonRpcSessionMachine machine, _, _) = await ReadyAsync();

      await AssertRefusedAsync(machine, JsonRpcSessionState.Ready, new Response("missing", "{}"), "unknown request id");
    }

    public static async Task Ready_RequestTimeout_Should_CompleteTheIdAndStayReady()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, _) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));
      await machine.SendAsync(new Send("8", "tools/call", null));

      MachineStep step = await machine.SendAsync(new RequestTimeout("7"));

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Ready);
      context.InFlight["7"].Status.ShouldBe(JsonRpcInFlightStatus.TimedOut);
      context.InFlight["8"].Status.ShouldBe(JsonRpcInFlightStatus.Pending);
      MachineStep again = await machine.SendAsync(new Send("9", "ping", null));
      again.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Ready);
    }

    public static async Task Ready_UnknownRequestTimeout_Should_Refuse()
    {
      (JsonRpcSessionMachine machine, _, _) = await ReadyAsync();

      await AssertRefusedAsync(machine, JsonRpcSessionState.Ready, new RequestTimeout("missing"), "unknown request id");
    }

    public static async Task Ready_ChildExited_Should_FailEveryInFlightId()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, _) = await ReadyAsync();
      await machine.SendAsync(new Send("a", "one", null));
      await machine.SendAsync(new Send("b", "two", null));

      MachineStep step = await machine.SendAsync(new ChildExited());

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      step.Configuration.ActiveStates.ShouldBe(["Exited"]);
      machine.State.ShouldBe(JsonRpcSessionState.Exited);
      context.InFlight["a"].Status.ShouldBe(JsonRpcInFlightStatus.Failed);
      context.InFlight["b"].Status.ShouldBe(JsonRpcInFlightStatus.Failed);
      context.InFlight["a"].Payload.ShouldBe("the child exited");
    }

    public static async Task Ready_Dispose_Should_EnterStoppingAndStopTheSeam()
    {
      (JsonRpcSessionMachine machine, _, FakeJsonRpcProcessSeam seam) = await ReadyAsync();
      seam.HoldStop();

      MachineStep step = await machine.SendAsync(new Dispose());

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Stopping);
      seam.ReleaseStop();
      await UntilAsync(() => seam.StopCount == 1);
      await UntilAsync(() => machine.State == JsonRpcSessionState.Exited);
    }

    public static async Task Ready_ReadFault_Should_EnterFaulted()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, _) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));

      MachineStep step = await machine.SendAsync(new ReadFault());

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Faulted);
      context.InFlight["7"].Status.ShouldBe(JsonRpcInFlightStatus.Failed);
    }

    public static async Task Ready_ReadException_Should_FaultTheSession()
    {
      (JsonRpcSessionMachine machine, _, FakeJsonRpcProcessSeam seam) = await ReadyAsync();

      seam.EnqueueFault(new InvalidOperationException("broken pipe"));

      await UntilAsync(() => machine.State == JsonRpcSessionState.Faulted);
    }

    public static async Task Ready_InboundFrame_Should_CompleteTheId()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));
      await UntilAsync(() => seam.Frames().Length == 1);

      seam.Enqueue("""{"jsonrpc":"2.0","id":"7","result":true}""");

      await UntilAsync(() => context.InFlight["7"].Status == JsonRpcInFlightStatus.Completed);
      machine.State.ShouldBe(JsonRpcSessionState.Ready);
      context.InFlight["7"].Payload.ShouldBe("""{"jsonrpc":"2.0","id":"7","result":true}""");
    }

    public static async Task Ready_EndOfStream_Should_FailInFlightIds()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();
      await machine.SendAsync(new Send("a", "one", null));
      await machine.SendAsync(new Send("b", "two", null));
      await UntilAsync(() => seam.Frames().Length == 2);

      seam.EnqueueEof();

      await UntilAsync(() => machine.State == JsonRpcSessionState.Exited);
      context.InFlight["a"].Status.ShouldBe(JsonRpcInFlightStatus.Failed);
      context.InFlight["b"].Status.ShouldBe(JsonRpcInFlightStatus.Failed);
    }

    public static async Task Ready_Start_Should_RefuseBecauseAlreadyRunning()
    {
      (JsonRpcSessionMachine machine, _, _) = await ReadyAsync();

      await AssertRefusedAsync(machine, JsonRpcSessionState.Ready, new Start(), "already running");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Ready, new Started(), "already running");
    }

    public static async Task Stopping_ChildExitedFromReadyRequest_Should_FailTheId()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, _) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));
      MachineStep dispose = await machine.SendAsync(new Dispose());
      dispose.Outcome.ShouldBe(MachineStepOutcome.Taken);

      MachineStep step = await machine.SendAsync(new ChildExited());

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Exited);
      context.InFlight["7"].Status.ShouldBe(JsonRpcInFlightStatus.Failed);
    }

    public static async Task Stopping_ReadFault_Should_EnterFaulted()
    {
      (JsonRpcSessionMachine machine, _, _) = await StoppingAsync();

      MachineStep step = await machine.SendAsync(new ReadFault());

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Faulted);
    }

    public static async Task Stopping_Send_Should_Refuse()
    {
      (JsonRpcSessionMachine machine, _, _) = await StoppingAsync();

      await AssertRefusedAsync(machine, JsonRpcSessionState.Stopping, new Send("1", "m", null), "the child is stopping");
    }

    public static async Task Stopping_OtherSignals_Should_BeRefused()
    {
      (JsonRpcSessionMachine machine, _, _) = await StoppingAsync();

      await AssertRefusedAsync(machine, JsonRpcSessionState.Stopping, new Start(), "the child is stopping");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Stopping, new Started(), "the child is stopping");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Stopping, new Response("1", "{}"), "the child is stopping");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Stopping, new RequestTimeout("1"), "the child is stopping");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Stopping, new Dispose(), "the child is stopping");
    }

    public static async Task Exited_SendAndStart_Should_BeRefused()
    {
      (JsonRpcSessionMachine machine, _, _) = await ExitedAsync();

      await AssertRefusedAsync(machine, JsonRpcSessionState.Exited, new Send("1", "m", null), "the child has exited");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Exited, new Start(), "the child has exited");
    }

    public static async Task Exited_OtherSignals_Should_BeRefused()
    {
      (JsonRpcSessionMachine machine, _, _) = await ExitedAsync();

      await AssertRefusedAsync(machine, JsonRpcSessionState.Exited, new Started(), "the child has exited");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Exited, new Response("1", "{}"), "the child has exited");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Exited, new RequestTimeout("1"), "the child has exited");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Exited, new ChildExited(), "the child has exited");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Exited, new ReadFault(), "the child has exited");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Exited, new Dispose(), "the child has exited");
    }

    public static async Task Faulted_SendAndStart_Should_BeRefused()
    {
      (JsonRpcSessionMachine machine, _, _) = await FaultedAsync();

      await AssertRefusedAsync(machine, JsonRpcSessionState.Faulted, new Send("1", "m", null), "the session is faulted");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Faulted, new Start(), "the session is faulted");
    }

    public static async Task Faulted_OtherSignals_Should_BeRefused()
    {
      (JsonRpcSessionMachine machine, _, _) = await FaultedAsync();

      await AssertRefusedAsync(machine, JsonRpcSessionState.Faulted, new Started(), "the session is faulted");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Faulted, new Response("1", "{}"), "the session is faulted");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Faulted, new RequestTimeout("1"), "the session is faulted");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Faulted, new ChildExited(), "the session is faulted");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Faulted, new ReadFault(), "the session is faulted");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Faulted, new Dispose(), "the session is faulted");
    }

    private static (JsonRpcSessionMachine Machine, JsonRpcSessionContext Context, FakeJsonRpcProcessSeam Seam) Create(bool holdStart = false)
    {
      FakeJsonRpcProcessSeam seam = new() { HoldStart = holdStart };
      JsonRpcSessionMachine machine = new();
      JsonRpcSessionContext context = new(seam, machine);
      machine.Start(context);
      return (machine, context, seam);
    }

    private static async Task<(JsonRpcSessionMachine Machine, JsonRpcSessionContext Context, FakeJsonRpcProcessSeam Seam)> StartingAsync()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = Create(holdStart: true);
      MachineStep step = await machine.SendAsync(new Start());
      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Starting);
      return (machine, context, seam);
    }

    private static async Task<(JsonRpcSessionMachine Machine, JsonRpcSessionContext Context, FakeJsonRpcProcessSeam Seam)> ReadyAsync()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = Create();
      MachineStep step = await machine.SendAsync(new Start());
      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      await UntilAsync(() => machine.State == JsonRpcSessionState.Ready);
      return (machine, context, seam);
    }

    private static async Task<(JsonRpcSessionMachine Machine, JsonRpcSessionContext Context, FakeJsonRpcProcessSeam Seam)> StoppingAsync()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();
      seam.HoldStop();
      MachineStep step = await machine.SendAsync(new Dispose());
      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Stopping);
      return (machine, context, seam);
    }

    private static async Task<(JsonRpcSessionMachine Machine, JsonRpcSessionContext Context, FakeJsonRpcProcessSeam Seam)> ExitedAsync()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();
      MachineStep step = await machine.SendAsync(new ChildExited());
      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      return (machine, context, seam);
    }

    private static async Task<(JsonRpcSessionMachine Machine, JsonRpcSessionContext Context, FakeJsonRpcProcessSeam Seam)> FaultedAsync()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();
      MachineStep step = await machine.SendAsync(new ReadFault());
      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      return (machine, context, seam);
    }

    private static async Task AssertRefusedAsync
    (
      JsonRpcSessionMachine machine,
      JsonRpcSessionState state,
      ISignal signal,
      string reason
    )
    {
      MachineStep step = await machine.SendAsync(signal);
      step.Outcome.ShouldBe(MachineStepOutcome.Refused);
      step.Reason.ShouldBe(reason);
      machine.State.ShouldBe(state);
    }

    private static async Task UntilAsync(Func<bool> condition)
    {
      DateTime deadline = DateTime.UtcNow + Wait;
      while (!condition())
      {
        if (DateTime.UtcNow > deadline)
        {
          throw new TimeoutException("the session did not reach the expected state");
        }

        await Task.Delay(5);
      }
    }

    private sealed class StepLog : IMachineStepObserver
    {
      private readonly List<MachineStep> Steps = [];

      public void OnStep(MachineStep step)
      {
        lock (Steps)
        {
          Steps.Add(step);
        }
      }

      public MachineStep[] Snapshot()
      {
        lock (Steps)
        {
          return [.. Steps];
        }
      }
    }
  }
}
