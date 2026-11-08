# Model the JSON-RPC child session as a Ramani machine

## Description

Task 083 adds an Amuru-native JSON-RPC client over a child process. The session is long-lived: start, ready, requests in flight, stop, and the child dying are different phases, and a send is legal in only one of them. Implement that session as a Ramani state machine in `TimeWarp.Amuru`. Task 083 keeps the public entry point, the MCP sample, and the NativeAOT consumer check. This task does not uncomment `ShellBuilder.AsJsonRpcClient()`.

`TimeWarp.Ramani` becomes a package dependency of `TimeWarp.Amuru`. That cost is accepted here because the machine is the session, not a test double. Do not reference Ramani from `TimeWarp.Amuru.Tools`.

## Requirements

### Dependency

- Pin `TimeWarp.Ramani` at `1.0.0-beta.23` or later in `Directory.Packages.props`. That is the first version on nuget.org and the first with `SendAsync` returning `Task<MachineStep>` (Ramani task 013). Amuru's `nuget.config` stays nuget.org only.
- Ramani's only dependency is `TimeWarp.Builder`, which is public. No GitHub feed and no private package enters the Amuru graph.
- Ramani is `IsAotCompatible` and its generator ships in the same package. One package reference is enough.

### Shape

- The machine is internal. Public API files stay unchanged. `AsJsonRpcClient()` stays commented.
- The machine lives in an `internal sealed partial class`. The whole `Machine.Named(...)...Build()` chain is one expression with a constant name, as the Ramani generator requires.
- States are `Idle`, `Starting`, `Ready`, `Stopping`, `Exited`, and `Faulted`. State ids are one enum.
- Signals are `Start`, `Started`, `Send`, `Response`, `RequestTimeout`, `ChildExited`, `ReadFault`, and `Dispose`. Declare them with `.Signals<...>()` so `.Exhaustive()` checks every one. A signal no state mentions is otherwise ignored silently.
- `.Exhaustive()` is on. Each state decides every signal with a transition, a `.Refuse<T>(reason)`, or a state-level `.RefuseOthers`. Do not use a machine-wide `RefuseOthers`; it makes the proof vacuous (SM029).
- The context holds the process seam, the in-flight request map, and an outbound frame channel. A request is a map entry, not a state. Concurrent sends stay in `Ready`.
- Behaviors are copied into generated static methods. Lambdas capture nothing and reach the seam and the map only through the context.

### Process seam

- The seam is a narrow internal interface: start, write one frame, read one frame, and stop. Tests use a fake. This task does not implement net11 anonymous pipes, `KillOnParentExit`, or `ProcessExitStatus`. Task 083 builds that seam.
- Frames are newline-delimited JSON via `System.Text.Json`. No `Newtonsoft.Json` and no StreamJsonRpc.

### Async rule

- Ramani entry, exit and effect behaviors are synchronous by design. Only a do-activity is async. Writing a frame is therefore not an awaited effect. `Send`'s effect adds the id to the map and enqueues the frame on the context channel. The `Ready` do-activity drains that channel to the seam and reads stdout, both inside the one activity, because a state owns at most one `.Do`.
- The do-activity must not write the context. The read loop sends `Response` back into the machine, and the `Response` effect completes the map entry. The same holds for `ChildExited` and `ReadFault` from the loop.
- `Starting` owns an async do-activity that starts the seam. Its completion drives `Started`. The host does not send `Started`.

### Transitions

- `Idle` accepts `Start` and enters `Starting`. It refuses `Send` and `Response`.
- `Starting` accepts `Started` into `Ready`, `ChildExited` into `Exited`, and `ReadFault` into `Faulted`. It refuses `Send` with the reason `the child is not ready`. It refuses a second `Start`.
- `Ready` accepts `Send`, `Response`, and `RequestTimeout` as internal transitions and stays in `Ready`, so the do-activity keeps running. `Send` adds one in-flight id. `Response` and `RequestTimeout` complete that id. An unknown id is a guard on the map plus `.Else()` or `.Refuse`, never a guard alone; a guarded row with no fallback leaves the pair undecided and the signal ignored.
- `Ready` accepts `ChildExited` into `Exited` and fails every in-flight id. It accepts `Dispose` into `Stopping`. It refuses `Start` with the reason `already running`.
- `Stopping` accepts `ChildExited` into `Exited` and `ReadFault` into `Faulted`. It refuses `Send`.
- `Exited` and `Faulted` refuse `Send` and `Start`.
- A read failure in the `Ready` do-activity is handled by `.OnFault().GoTo(Faulted)`. Without an `OnFault` row a thrown activity stops the pump for good. The `ReadFault` signal stays for the fake seam to inject a fault from outside, and both routes end in `Faulted`.
- A request timeout does not fault the session.

### Caller-visible result

- The client calls `SendAsync` and reads the returned `MachineStep`. `Outcome == Refused` with `Reason` is the refusal the caller sees. No completion source rides on the signal.
- `Send` from inside a behavior is queued and runs after the current step. Only the host calls `SendAsync`.

### Version

- Leave `<Version>` at `3.0.0-beta.1`. Do not cut a release. Task 083 ships the public client and the release note for this dependency.

## Checklist

- [ ] `TimeWarp.Ramani` 1.0.0-beta.23 or later pinned; core package only; restore works with nuget.org alone
- [ ] Internal exhaustive Ramani machine with explicit `.Signals<...>()` and per-state refusals
- [ ] Process seam interface and fake
- [ ] Outbound channel drained and stdout read inside the single `Ready` do-activity
- [ ] `Starting` do-activity drives `Started`
- [ ] `.OnFault()` on `Ready` into `Faulted`
- [ ] Tests step every accepted transition and every refusal, through the fake seam, reading `MachineStep` from `SendAsync`
- [ ] A request timeout completes one id and stays in `Ready`
- [ ] `ChildExited` fails every in-flight id
- [ ] Unknown `Response` id refused with a reason
- [ ] Public API unchanged, including the commented `AsJsonRpcClient()` stub
- [ ] Tests reach the internal machine: decide `InternalsVisibleTo` for the test tree or a test-only seam, and record the choice in Notes
- [ ] `./tools/dev-cli/dev.cs build` and `./tools/dev-cli/dev.cs test` pass
- [ ] `ganda repo audit` is clean

## Session

- Created: 01a1b4c4-827b-7d70-9fd0-358726b63b0b (2026-11-07)
- Spec review: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-07). Blocked on Ramani publishing to nuget.org (Ramani 016) and on `SendAsync` returning the step (Ramani 013). Both shipped in 1.0.0-beta.23; spec revised 2026-11-08.

## Notes

Filed from the Ramani review on 2026-11-07. Task 083 remains the public client: `ShellBuilder.AsJsonRpcClient()` (or the equivalent entry point), `JsonSerializerOptions` / `JsonSerializerContext`, stderr observation, PublicAPI, XML docs, the NativeAOT sample, the MCP sample, and the docs. After this task merges, 083 implements the real process seam behind the interface this machine calls and deletes nothing this task proved.

Do not model one state per request. Two overlapping sends would then be illegal.

Review findings folded in on 2026-11-08: Ramani effects are synchronous (channel plus do-activity for writes), one do-activity per state, the activity never writes the context, `OnFault` maps read exceptions to `Faulted`, `.Signals<...>()` closes the exhaustiveness gap, unknown ids need `.Else()`, and refusals reach the caller through the step `SendAsync` returns. The repo has no `InternalsVisibleTo` today; the checklist item leaves that decision to the implementer.
