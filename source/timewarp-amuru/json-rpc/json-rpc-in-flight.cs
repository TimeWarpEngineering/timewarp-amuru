#region Purpose
// One JSON-RPC request waiting on the session, or the way that wait ended.
#endregion

#region Design
// The map lives on the context. Effects on the pump are its only writers, including Release.
// Status is published after the payload so a reader that observes Completed or Failed can read Payload.
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
  private int StatusValue;

  public JsonRpcInFlight(string id)
  {
    ArgumentException.ThrowIfNullOrEmpty(id);
    Id = id;
  }

  public string Id { get; }

  public string? Payload { get; private set; }

  public JsonRpcInFlightStatus Status => (JsonRpcInFlightStatus)Volatile.Read(ref StatusValue);

  public void Complete(string payload)
  {
    Payload = payload;
    Volatile.Write(ref StatusValue, (int)JsonRpcInFlightStatus.Completed);
  }

  public void TimeOut()
  {
    Volatile.Write(ref StatusValue, (int)JsonRpcInFlightStatus.TimedOut);
  }

  public void Fail(string reason)
  {
    Payload = reason;
    Volatile.Write(ref StatusValue, (int)JsonRpcInFlightStatus.Failed);
  }
}
