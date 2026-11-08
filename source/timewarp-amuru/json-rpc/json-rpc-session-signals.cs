#region Purpose
// Signals the JSON-RPC child session machine accepts or refuses.
#endregion

#region Design
// Declared with .Signals so exhaustiveness includes a signal no state happens to mention.
// None of these records carries a completion source. The caller reads MachineStep from SendAsync.
// Started is posted by the Starting activity's completion. The host does not send it.
#endregion

namespace TimeWarp.Amuru.JsonRpc;

using TimeWarp.Ramani;

internal sealed record Start : ISignal;

internal sealed record Started : ISignal;

internal sealed record Send(string Id, string Method, string? ParamsJson) : ISignal;

internal sealed record Response(string Id, string Frame) : ISignal;

internal sealed record RequestTimeout(string Id) : ISignal;

internal sealed record ChildExited : ISignal;

internal sealed record ReadFault : ISignal;

internal sealed record Dispose : ISignal;
