#region Purpose
// Internal Ramani machine for one JSON-RPC child session.
#endregion

#region Design
// The chain is one expression with a constant name, which is what the Ramani generator reads.
// .Exhaustive() and .Signals close every (state, signal) pair. Refusals are per state.
// A machine-wide RefuseOthers would make that proof vacuous.
// Ready keeps Send, Notify, Response, RequestTimeout, and Release internal so the do-activity keeps running.
// Notify writes one outbound frame and does not touch the in-flight map.
// Send is guarded on "id not in the map". A duplicate, including a finished id, is Refuse.
// Release is guarded on a finished id. It removes that id and stays in the state. Accept does not.
// Release is a second Signals call and Notify is a third. One call holds eight type arguments.
// An unknown id is a guard plus Refuse. A guard alone would leave the pair undecided.
// Stopping's activity completion posts ChildExited, the same way Starting posts Started.
// OnFault on Ready, Starting, and Stopping maps a thrown activity to Faulted.
// ReadFault remains a signal the fake can inject. Both routes end in Faulted.
// A request timeout completes one id and stays in Ready.
#endregion

namespace TimeWarp.Amuru.JsonRpc;

using TimeWarp.Ramani;

internal sealed partial class JsonRpcSessionMachine
{
  public static StateMachineDefinition Definition() =>
    Machine.Named<JsonRpcSessionState, JsonRpcSessionContext>("json-rpc-session")
      .Exhaustive()
      .Signals<Start, Started, Send, Response, RequestTimeout, ChildExited, ReadFault, Dispose>()
      .Signals<Release>()
      .Signals<Notify>()
      .Initial(JsonRpcSessionState.Idle)
      .State(JsonRpcSessionState.Idle)
        .On<Start>().GoTo(JsonRpcSessionState.Starting).Done()
        .Refuse<Started>("the child has not started")
        .Refuse<Send>("the child is not ready")
        .Refuse<Notify>("the child is not ready")
        .Refuse<Response>("the child is not ready")
        .Refuse<RequestTimeout>("the child is not ready")
        .Refuse<Release>("the child is not running")
        .Refuse<ChildExited>("the child is not running")
        .Refuse<ReadFault>("the child is not running")
        .Refuse<Dispose>("the child is not running")
        .Done()
      .State(JsonRpcSessionState.Starting)
        .Do<bool>(JsonRpcSessionActivities.StartChildAsync, static (context, _) => context.Machine.Send(new Started()))
        .On<Started>().GoTo(JsonRpcSessionState.Ready).Effect(static (context, _) => context.MarkReady()).Done()
        .On<ChildExited>().GoTo(JsonRpcSessionState.Exited).Effect(static (context, _) => context.FailPending("the child exited")).Done()
        .On<ReadFault>().GoTo(JsonRpcSessionState.Faulted).Effect(static (context, _) => context.FailPending("read failed")).Done()
        .OnFault().GoTo(JsonRpcSessionState.Faulted).Effect(static (context, fault) => context.FailPending(fault.Exception.Message)).Done()
        .Refuse<Start>("the child is starting")
        .Refuse<Send>("the child is not ready")
        .Refuse<Notify>("the child is not ready")
        .Refuse<Response>("the child is not ready")
        .Refuse<RequestTimeout>("the child is not ready")
        .Refuse<Release>("the child is not ready")
        .Refuse<Dispose>("the child is not ready")
        .Done()
      .State(JsonRpcSessionState.Ready)
        .Do(JsonRpcSessionActivities.RunReadyAsync)
        .On<Send>().When(static (context, signal) => !context.Contains(signal.Id)).Internal().Effect(static (context, signal) => context.Accept(signal)).Done()
        .Refuse<Send>("duplicate request id")
        .On<Notify>().Internal().Effect(static (context, signal) => context.WriteNotification(signal)).Done()
        .On<Response>().When(static (context, signal) => context.ContainsPending(signal.Id)).Internal().Effect(static (context, signal) => context.Complete(signal)).Done()
        .Refuse<Response>("unknown request id")
        .On<RequestTimeout>().When(static (context, signal) => context.ContainsPending(signal.Id)).Internal().Effect(static (context, signal) => context.TimeOut(signal)).Done()
        .Refuse<RequestTimeout>("unknown request id")
        .On<Release>().When(static (context, signal) => context.ContainsFinished(signal.Id)).Internal().Effect(static (context, signal) => context.Release(signal)).Done()
        .Refuse<Release>("the request is not finished")
        .On<ChildExited>().GoTo(JsonRpcSessionState.Exited).Effect(static (context, _) => context.FailPending("the child exited")).Done()
        .On<ReadFault>().GoTo(JsonRpcSessionState.Faulted).Effect(static (context, _) => context.FailPending("read failed")).Done()
        .On<Dispose>().GoTo(JsonRpcSessionState.Stopping).Done()
        .OnFault().GoTo(JsonRpcSessionState.Faulted).Effect(static (context, fault) => context.FailPending(fault.Exception.Message)).Done()
        .Refuse<Start>("already running")
        .Refuse<Started>("already running")
        .Done()
      .State(JsonRpcSessionState.Stopping)
        .Do<bool>(JsonRpcSessionActivities.StopChildAsync, static (context, _) => context.Machine.Send(new ChildExited()))
        .On<ChildExited>().GoTo(JsonRpcSessionState.Exited).Effect(static (context, _) => context.FailPending("the child exited")).Done()
        .On<ReadFault>().GoTo(JsonRpcSessionState.Faulted).Effect(static (context, _) => context.FailPending("read failed")).Done()
        .OnFault().GoTo(JsonRpcSessionState.Faulted).Effect(static (context, fault) => context.FailPending(fault.Exception.Message)).Done()
        .On<Release>().When(static (context, signal) => context.ContainsFinished(signal.Id)).Internal().Effect(static (context, signal) => context.Release(signal)).Done()
        .Refuse<Release>("the request is not finished")
        .Refuse<Start>("the child is stopping")
        .Refuse<Started>("the child is stopping")
        .Refuse<Send>("the child is stopping")
        .Refuse<Notify>("the child is stopping")
        .Refuse<Response>("the child is stopping")
        .Refuse<RequestTimeout>("the child is stopping")
        .Refuse<Dispose>("the child is stopping")
        .Done()
      .State(JsonRpcSessionState.Exited)
        .On<Release>().When(static (context, signal) => context.ContainsFinished(signal.Id)).Internal().Effect(static (context, signal) => context.Release(signal)).Done()
        .Refuse<Release>("the request is not finished")
        .Refuse<Start>("the child has exited")
        .Refuse<Started>("the child has exited")
        .Refuse<Send>("the child has exited")
        .Refuse<Notify>("the child has exited")
        .Refuse<Response>("the child has exited")
        .Refuse<RequestTimeout>("the child has exited")
        .Refuse<ChildExited>("the child has exited")
        .Refuse<ReadFault>("the child has exited")
        .Refuse<Dispose>("the child has exited")
        .Done()
      .State(JsonRpcSessionState.Faulted)
        .On<Release>().When(static (context, signal) => context.ContainsFinished(signal.Id)).Internal().Effect(static (context, signal) => context.Release(signal)).Done()
        .Refuse<Release>("the request is not finished")
        .Refuse<Start>("the session is faulted")
        .Refuse<Started>("the session is faulted")
        .Refuse<Send>("the session is faulted")
        .Refuse<Notify>("the session is faulted")
        .Refuse<Response>("the session is faulted")
        .Refuse<RequestTimeout>("the session is faulted")
        .Refuse<ChildExited>("the session is faulted")
        .Refuse<ReadFault>("the session is faulted")
        .Refuse<Dispose>("the session is faulted")
        .Done()
      .Build();
}
