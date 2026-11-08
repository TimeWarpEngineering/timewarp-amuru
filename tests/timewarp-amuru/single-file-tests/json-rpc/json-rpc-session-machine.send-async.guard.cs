#!/usr/bin/env -S dotnet --
#:include json-rpc-session-machine.fake-seam.cs

#region Purpose
// Proves the review fixes for duplicate ids, inbound frames, stop completion, and map retention.
#endregion

#region Design
// This file runs alone. #:include brings in the fake seam, and the namespace is block-scoped
// because the standalone entry point is a top-level statement.
// The aggregate runner compiles it with JARIBU_MULTI, which drops that entry point.
// A finished payload stays until the host sends Release. The next Send does not drop it.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace JsonRpcSessionMachine_
{
  using TimeWarp.Amuru.JsonRpc;
  using TimeWarp.Ramani;

  [TestTag("JsonRpc")]
  public class SendAsync_Given_Guard_
  {
    private static readonly TimeSpan Wait = TimeSpan.FromSeconds(5);

    [ModuleInitializer]
    internal static void Register() => RegisterTests<SendAsync_Given_Guard_>();

    public static async Task Ready_Notify_Should_WriteAFrameWithoutAnId()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();

      MachineStep step = await machine.SendAsync(new Notify("notifications/initialized", null));

      step.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Ready);
      context.InFlight.Count.ShouldBe(0);
      await UntilAsync(() => seam.Frames().Length == 1);
      string frame = seam.Frames()[0];
      frame.ShouldEndWith("\n");
      frame.ShouldContain("\"method\":\"notifications/initialized\"");
      frame.ShouldNotContain("\"id\"");
    }

    public static async Task Notify_Should_BeRefusedWhenTheChildIsNotReadyOrHasExited()
    {
      (JsonRpcSessionMachine idle, _, _) = Create();
      await AssertRefusedAsync(
        idle,
        JsonRpcSessionState.Idle,
        new Notify("notifications/initialized", null),
        "the child is not ready"
      );

      (JsonRpcSessionMachine exited, _, _) = await ReadyAsync();
      MachineStep childExited = await exited.SendAsync(new ChildExited());
      childExited.Outcome.ShouldBe(MachineStepOutcome.Taken);
      await AssertRefusedAsync(
        exited,
        JsonRpcSessionState.Exited,
        new Notify("notifications/initialized", null),
        "the child has exited"
      );
    }

    public static async Task InboundFrame_Should_ClassifyResponseNotificationAndRequest()
    {
      JsonRpcInbound response = JsonRpcFrames.ReadInbound("""{"jsonrpc":"2.0","id":"7","result":true}""");
      response.Kind.ShouldBe(JsonRpcInboundKind.Response);
      response.Id.ShouldBe("7");
      JsonRpcFrames.ReadInbound("""{"jsonrpc":"2.0","id":3,"error":{"code":1}}""").Kind
        .ShouldBe(JsonRpcInboundKind.Response);
      JsonRpcFrames.ReadInbound("""{"jsonrpc":"2.0","method":"notifications/message"}""").Kind
        .ShouldBe(JsonRpcInboundKind.Ignored);
      JsonRpcFrames.ReadInbound("""{"jsonrpc":"2.0","id":null,"error":{"code":-32700,"message":"Parse error"}}""").Kind
        .ShouldBe(JsonRpcInboundKind.Ignored);
      JsonRpcFrames.ReadInbound("""{"jsonrpc":"2.0","id":"7","method":"sampling/createMessage"}""").Kind
        .ShouldBe(JsonRpcInboundKind.Ignored);
      JsonRpcFrames.ReadInbound("{}").Kind.ShouldBe(JsonRpcInboundKind.Malformed);
      JsonRpcFrames.ReadInbound("not-json").Kind.ShouldBe(JsonRpcInboundKind.Malformed);
      await Task.CompletedTask;
    }

    public static async Task DuplicatePendingId_Should_Refuse()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));
      await UntilAsync(() => seam.Frames().Length == 1);

      await AssertRefusedAsync(machine, JsonRpcSessionState.Ready, new Send("7", "tools/list", null), "duplicate request id");
      await AssertRefusedAsync(machine, JsonRpcSessionState.Ready, new Release("7"), "the request is not finished");

      context.InFlight.Count.ShouldBe(1);
      context.InFlight["7"].Status.ShouldBe(JsonRpcInFlightStatus.Pending);
      seam.Frames().Length.ShouldBe(1);
    }

    public static async Task FinishedId_Should_RefuseUntilReleased()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, _) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));
      MachineStep response = await machine.SendAsync(new Response("7", """{"id":"7","result":1}"""));
      response.Outcome.ShouldBe(MachineStepOutcome.Taken);
      context.InFlight["7"].Status.ShouldBe(JsonRpcInFlightStatus.Completed);

      await AssertRefusedAsync(machine, JsonRpcSessionState.Ready, new Send("7", "again", null), "duplicate request id");
      MachineStep released = await machine.SendAsync(new Release("7"));
      released.Outcome.ShouldBe(MachineStepOutcome.Taken);
      context.InFlight.ContainsKey("7").ShouldBeFalse();

      MachineStep again = await machine.SendAsync(new Send("7", "again", null));
      again.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Ready);
      context.InFlight["7"].Status.ShouldBe(JsonRpcInFlightStatus.Pending);
    }

    public static async Task NextSend_Should_KeepAnUnreadResult()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, _) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));
      await machine.SendAsync(new Send("8", "tools/call", null));
      string payload = """{"id":"7","result":1}""";
      await machine.SendAsync(new Response("7", payload));

      MachineStep next = await machine.SendAsync(new Send("9", "ping", null));

      next.Outcome.ShouldBe(MachineStepOutcome.Taken);
      context.InFlight["7"].Status.ShouldBe(JsonRpcInFlightStatus.Completed);
      context.InFlight["7"].Payload.ShouldBe(payload);
      context.InFlight["8"].Status.ShouldBe(JsonRpcInFlightStatus.Pending);
      context.InFlight["9"].Status.ShouldBe(JsonRpcInFlightStatus.Pending);
      context.InFlight.Count.ShouldBe(3);

      MachineStep released = await machine.SendAsync(new Release("7"));
      released.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Ready);
      context.InFlight.ContainsKey("7").ShouldBeFalse();
      context.InFlight.Count.ShouldBe(2);
    }

    public static async Task Stopping_Release_Should_DropAFinishedId()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));
      await machine.SendAsync(new Response("7", """{"id":"7","result":1}"""));
      seam.HoldStop();
      MachineStep dispose = await machine.SendAsync(new Dispose());
      dispose.Outcome.ShouldBe(MachineStepOutcome.Taken);

      MachineStep released = await machine.SendAsync(new Release("7"));

      released.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Stopping);
      context.InFlight.ContainsKey("7").ShouldBeFalse();
    }

    public static async Task Faulted_Release_Should_DropTheFailedId()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));
      seam.EnqueueFault(new InvalidOperationException("broken pipe"));
      await UntilAsync(() => machine.State == JsonRpcSessionState.Faulted);
      context.InFlight["7"].Status.ShouldBe(JsonRpcInFlightStatus.Failed);
      context.InFlight["7"].Payload.ShouldBe("broken pipe");

      MachineStep released = await machine.SendAsync(new Release("7"));

      released.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Faulted);
      context.InFlight.ContainsKey("7").ShouldBeFalse();
    }

    public static async Task Exited_Release_Should_DropTheFailedId()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, _) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));
      MachineStep exited = await machine.SendAsync(new ChildExited());
      exited.Outcome.ShouldBe(MachineStepOutcome.Taken);
      context.InFlight["7"].Status.ShouldBe(JsonRpcInFlightStatus.Failed);
      context.InFlight["7"].Payload.ShouldBe("the child exited");

      MachineStep released = await machine.SendAsync(new Release("7"));

      released.Outcome.ShouldBe(MachineStepOutcome.Taken);
      machine.State.ShouldBe(JsonRpcSessionState.Exited);
      context.InFlight.ContainsKey("7").ShouldBeFalse();
    }

    public static async Task Notification_Should_LeaveThePendingId()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));
      await UntilAsync(() => seam.Frames().Length == 1);
      string result = """{"jsonrpc":"2.0","id":"7","result":true}""";

      seam.Enqueue("""{"jsonrpc":"2.0","method":"notifications/message","params":{"x":1}}""");
      seam.Enqueue(result);

      await UntilAsync(() => context.InFlight["7"].Status == JsonRpcInFlightStatus.Completed);
      machine.State.ShouldBe(JsonRpcSessionState.Ready);
      context.InFlight["7"].Payload.ShouldBe(result);
    }

    public static async Task NullIdError_Should_LeaveThePendingId()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));
      await UntilAsync(() => seam.Frames().Length == 1);
      string result = """{"jsonrpc":"2.0","id":"7","result":true}""";

      seam.Enqueue("""{"jsonrpc":"2.0","id":null,"error":{"code":-32700,"message":"Parse error"}}""");
      seam.Enqueue(result);

      await UntilAsync(() => context.InFlight["7"].Status == JsonRpcInFlightStatus.Completed);
      machine.State.ShouldBe(JsonRpcSessionState.Ready);
      context.InFlight["7"].Payload.ShouldBe(result);
    }

    public static async Task ServerRequest_Should_NotCompleteTheClientId()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));
      await UntilAsync(() => seam.Frames().Length == 1);
      string result = """{"jsonrpc":"2.0","id":"7","result":true}""";

      seam.Enqueue("""{"jsonrpc":"2.0","id":"7","method":"sampling/createMessage"}""");
      seam.Enqueue(result);

      await UntilAsync(() => context.InFlight["7"].Status == JsonRpcInFlightStatus.Completed);
      machine.State.ShouldBe(JsonRpcSessionState.Ready);
      context.InFlight["7"].Payload.ShouldBe(result);
    }

    public static async Task MalformedFrame_Should_FaultTheSession()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));
      await UntilAsync(() => seam.Frames().Length == 1);

      seam.Enqueue("not-json");

      await UntilAsync(() => machine.State == JsonRpcSessionState.Faulted);
      context.InFlight["7"].Status.ShouldBe(JsonRpcInFlightStatus.Failed);
      context.InFlight["7"].Payload.ShouldBe("a JSON-RPC frame is not a response");
    }

    public static async Task StoppingCompletion_Should_FailThePendingId()
    {
      (JsonRpcSessionMachine machine, JsonRpcSessionContext context, FakeJsonRpcProcessSeam seam) = await ReadyAsync();
      await machine.SendAsync(new Send("7", "tools/list", null));
      StepLog log = new();
      machine.StepObserver = log;

      MachineStep dispose = await machine.SendAsync(new Dispose());

      dispose.Outcome.ShouldBe(MachineStepOutcome.Taken);
      await UntilAsync(() => machine.State == JsonRpcSessionState.Exited);
      seam.StopCount.ShouldBe(1);
      context.InFlight["7"].Status.ShouldBe(JsonRpcInFlightStatus.Failed);
      context.InFlight["7"].Payload.ShouldBe("the child exited");
      log.Snapshot().ShouldContain(candidate => candidate.Signal is ChildExited && candidate.Outcome == MachineStepOutcome.Taken);
    }

    private static (JsonRpcSessionMachine Machine, JsonRpcSessionContext Context, FakeJsonRpcProcessSeam Seam) Create()
    {
      FakeJsonRpcProcessSeam seam = new();
      JsonRpcSessionMachine machine = new();
      JsonRpcSessionContext context = new(seam, machine);
      machine.Start(context);
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
