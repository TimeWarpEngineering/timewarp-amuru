#region Purpose
// One JSON-RPC request waiting on the session, or the way that wait ended.
#endregion

#region Design
// The map lives on the context. Effects on the pump are its only writers, including Release.
// Payload is stored, then Status is published, then Completion is set. A waiter that observes
// the task can read Payload. The task is the public client's result path.
// TimedOut completes the id and is not a session fault. The entry stays until the host sends Release.
#endregion

namespace TimeWarp.Amuru.JsonRpc;

internal enum JsonRpcInFlightStatus
{
  Pending,
  Completed,
  TimedOut,
  Failed,
}

internal sealed class JsonRpcInFlight
{
  private readonly TaskCompletionSource<JsonRpcInFlightStatus> Gate =
    new(TaskCreationOptions.RunContinuationsAsynchronously);

  private int StatusValue;

  public JsonRpcInFlight(string id)
  {
    ArgumentException.ThrowIfNullOrEmpty(id);
    Id = id;
  }

  public string Id { get; }

  public string? Payload { get; private set; }

  public Task<JsonRpcInFlightStatus> Completion => Gate.Task;

  public JsonRpcInFlightStatus Status => (JsonRpcInFlightStatus)Volatile.Read(ref StatusValue);

  public void Complete(string payload)
  {
    Payload = payload;
    Publish(JsonRpcInFlightStatus.Completed);
  }

  public void TimeOut()
  {
    Publish(JsonRpcInFlightStatus.TimedOut);
  }

  public void Fail(string reason)
  {
    Payload = reason;
    Publish(JsonRpcInFlightStatus.Failed);
  }

  private void Publish(JsonRpcInFlightStatus status)
  {
    Volatile.Write(ref StatusValue, (int)status);
    Gate.TrySetResult(status);
  }
}
