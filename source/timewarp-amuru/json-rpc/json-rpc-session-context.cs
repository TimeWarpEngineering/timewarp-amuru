#region Purpose
// Context for the JSON-RPC session machine: the process seam, the in-flight map, and the outbound frame channel.
#endregion

#region Design
// Effects on the pump are the only writers of the map and the channel. Release is one of those effects.
// The map is a ConcurrentDictionary so a host thread can take an entry while another effect
// accepts or releases a different id. The host waits on JsonRpcInFlight.Completion.
// It does not add or remove keys. The host sends Release after it has read a finished payload.
// Accept does not remove finished entries. The map is the only copy of a result, so a later Send
// leaves an unread payload in place. A pending id stays until it completes, times out, or fails.
// The Ready do-activity does not write this context. It posts Response or ChildExited; those effects
// complete or fail map entries.
// The machine reference lets a behavior call Send. SendAsync stays with the host.
// FaultReason keeps the first failure text, including a start failure that has no in-flight id.
// Ready completes when the Started effect runs. Settled completes when a transition fails pending
// work, which is every path into Exited or Faulted. The host awaits those tasks instead of polling.
#endregion

namespace TimeWarp.Amuru.JsonRpc;

using System.Threading.Channels;

internal sealed class JsonRpcSessionContext
{
  private readonly ConcurrentDictionary<string, JsonRpcInFlight> Requests = new();
  private readonly TaskCompletionSource ReadyGate =
    new(TaskCreationOptions.RunContinuationsAsynchronously);
  private readonly TaskCompletionSource SettledGate =
    new(TaskCreationOptions.RunContinuationsAsynchronously);

  public JsonRpcSessionContext(IJsonRpcProcessSeam jsonRpcProcessSeam, JsonRpcSessionMachine machine)
  {
    ArgumentNullException.ThrowIfNull(jsonRpcProcessSeam);
    ArgumentNullException.ThrowIfNull(machine);
    JsonRpcProcessSeam = jsonRpcProcessSeam;
    Machine = machine;
  }

  public IJsonRpcProcessSeam JsonRpcProcessSeam { get; }

  public JsonRpcSessionMachine Machine { get; }

  public Channel<string> Outbound { get; } =
    Channel.CreateUnbounded<string>
    (
      new UnboundedChannelOptions
      {
        SingleReader = true,
        SingleWriter = true
      }
    );

  public string? FaultReason { get; private set; }

  public Task Ready => ReadyGate.Task;

  public Task Settled => SettledGate.Task;

  public IReadOnlyDictionary<string, JsonRpcInFlight> InFlight => Requests;

  public bool Contains(string id) => Requests.ContainsKey(id);

  public bool ContainsPending(string id) =>
    Requests.TryGetValue(id, out JsonRpcInFlight? entry) && entry.Status == JsonRpcInFlightStatus.Pending;

  public bool ContainsFinished(string id) =>
    Requests.TryGetValue(id, out JsonRpcInFlight? entry) && entry.Status != JsonRpcInFlightStatus.Pending;

  public void Accept(Send send)
  {
    JsonRpcInFlight entry = new(send.Id);
    if (!Requests.TryAdd(send.Id, entry))
    {
      throw new InvalidOperationException("duplicate request id");
    }

    string frame = JsonRpcFrames.WriteRequest(send.Id, send.Method, send.ParamsJson);
    if (!Outbound.Writer.TryWrite(frame))
    {
      throw new InvalidOperationException("the outbound frame channel is closed");
    }
  }

  public void Complete(Response response)
  {
    Requests[response.Id].Complete(response.Frame);
  }

  public void TimeOut(RequestTimeout timeout)
  {
    Requests[timeout.Id].TimeOut();
  }

  public void FailPending(string reason)
  {
    FaultReason ??= reason;
    SettledGate.TrySetResult();
    foreach (JsonRpcInFlight entry in Requests.Values)
    {
      if (entry.Status == JsonRpcInFlightStatus.Pending)
      {
        entry.Fail(reason);
      }
    }
  }

  public void Release(Release release)
  {
    if (!ContainsFinished(release.Id))
    {
      return;
    }

    Requests.TryRemove(release.Id, out _);
  }

  public void MarkReady() => ReadyGate.TrySetResult();

  public void WriteNotification(Notify notify)
  {
    ArgumentException.ThrowIfNullOrEmpty(notify.Method);
    string frame = JsonRpcFrames.WriteNotification(notify.Method, notify.ParamsJson);
    if (!Outbound.Writer.TryWrite(frame))
    {
      throw new InvalidOperationException("the outbound frame channel is closed");
    }
  }
}
