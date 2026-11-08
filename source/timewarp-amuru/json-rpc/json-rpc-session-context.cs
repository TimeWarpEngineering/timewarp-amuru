#region Purpose
// Context for the JSON-RPC session machine: the process seam, the in-flight map, and the outbound frame channel.
#endregion

#region Design
// Effects on the pump are the only writers of the map and the channel, except Release.
// Release drops one finished entry after the host has read it. Accept drops every finished entry
// before adding the new id, so a session that keeps sending does not retain one payload per request.
// A pending id stays until it completes, times out, or fails. The Ready do-activity does not write this context.
// It posts Response or ChildExited; those effects complete or fail map entries.
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

  public bool Release(string id)
  {
    if (!Requests.TryGetValue(id, out JsonRpcInFlight? entry)
      || entry.Status == JsonRpcInFlightStatus.Pending)
    {
      return false;
    }

    return Requests.Remove(id);
  }

  public void Accept(Send send)
  {
    RemoveFinished();
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

  private void RemoveFinished()
  {
    List<string>? finished = null;
    foreach (KeyValuePair<string, JsonRpcInFlight> pair in Requests)
    {
      if (pair.Value.Status == JsonRpcInFlightStatus.Pending)
      {
        continue;
      }

      finished ??= [];
      finished.Add(pair.Key);
    }

    if (finished is null)
    {
      return;
    }

    foreach (string id in finished)
    {
      Requests.Remove(id);
    }
  }
}
