#region Purpose
// Context for the JSON-RPC session machine: the process seam, the in-flight map, and the outbound frame channel.
#endregion

#region Design
// Effects on the pump are the only writers of the map and the channel. Release is one of those effects.
// The host sends Release after it has read a finished payload. That effect is the only removal.
// Accept does not remove finished entries. The map is the only copy of a result, so a later Send
// leaves an unread payload in place. A pending id stays until it completes, times out, or fails.
// The Ready do-activity does not write this context. It posts Response or ChildExited; those effects
// complete or fail map entries. The host reads Status and Payload. It does not add or remove keys.
// The machine reference lets a behavior call Send. SendAsync stays with the host.
#endregion

namespace TimeWarp.Amuru.JsonRpc;

using System.Threading.Channels;

internal sealed class JsonRpcSessionContext
{
  private readonly Dictionary<string, JsonRpcInFlight> Requests = [];

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

  public IReadOnlyDictionary<string, JsonRpcInFlight> InFlight => Requests;

  public bool Contains(string id) => Requests.ContainsKey(id);

  public bool ContainsPending(string id) =>
    Requests.TryGetValue(id, out JsonRpcInFlight? entry) && entry.Status == JsonRpcInFlightStatus.Pending;

  public bool ContainsFinished(string id) =>
    Requests.TryGetValue(id, out JsonRpcInFlight? entry) && entry.Status != JsonRpcInFlightStatus.Pending;

  public void Accept(Send send)
  {
    Requests.Add(send.Id, new JsonRpcInFlight(send.Id));
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

    Requests.Remove(release.Id);
  }
}
