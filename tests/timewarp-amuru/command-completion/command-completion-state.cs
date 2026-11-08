#region Purpose
// State ids for the command-completion test oracle.
#endregion

#region Design
// One enum, because Ramani requires every state id to be an enum member.
// Running and Interrupting are the only phases that still accept a signal that changes the phase.
// Exited, TimedOut, and Canceled are terminal: each refuses the whole signal set.
#endregion

namespace TimeWarp.Amuru.Testing.CommandCompletion;

internal enum CommandCompletionState
{
  Running,
  Interrupting,
  Exited,
  TimedOut,
  Canceled,
}
