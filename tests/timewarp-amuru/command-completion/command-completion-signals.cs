#region Purpose
// Signals the command-completion test oracle accepts or refuses.
#endregion

#region Design
// Declared with .Signals so exhaustiveness includes a signal a state only refuses.
// None of these records carries a completion source. The caller reads MachineStep from SendAsync.
// ChildExited carries the child's exit code. The other three signals carry no payload.
#endregion

namespace TimeWarp.Amuru.Testing.CommandCompletion;

using TimeWarp.Ramani;

internal sealed record ChildExited(int ExitCode) : ISignal;

internal sealed record TimeoutElapsed : ISignal;

internal sealed record GraceElapsed : ISignal;

internal sealed record CallerCanceled : ISignal;
