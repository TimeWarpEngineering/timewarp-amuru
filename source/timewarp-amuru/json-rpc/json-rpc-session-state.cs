#region Purpose
// State ids for the JSON-RPC child session machine.
#endregion

#region Design
// One enum, because Ramani requires every state id to be an enum member.
// A request is a map entry on the context, not a state: two overlapping sends stay in Ready.
#endregion

namespace TimeWarp.Amuru.JsonRpc;

internal enum JsonRpcSessionState
{
  Idle,
  Starting,
  Ready,
  Stopping,
  Exited,
  Faulted,
}
