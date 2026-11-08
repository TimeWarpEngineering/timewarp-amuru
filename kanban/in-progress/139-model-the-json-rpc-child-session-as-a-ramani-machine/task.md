# Model the JSON-RPC child session as a Ramani machine

## Description

Task 083 adds an Amuru-native JSON-RPC client over a child process. The session is long-lived: start, ready, requests in flight, stop, and the child dying are different phases, and a send is legal in only one of them. Implement that session as a Ramani state machine in `TimeWarp.Amuru`. Task 083 keeps the public entry point, the MCP sample, and the NativeAOT consumer check. This task does not uncomment `ShellBuilder.AsJsonRpcClient()`.

`TimeWarp.Ramani` becomes a package dependency of `TimeWarp.Amuru`. That cost is accepted here because the machine is the session, not a test double. Do not reference Ramani from `TimeWarp.Amuru.Tools`.

## Goal (implementer restatement)

The JSON-RPC child session is a long-lived conversation, and a send is legal only while that child is ready. This task models the session as an internal Ramani machine in `TimeWarp.Amuru`: Idle, Starting, Ready, Stopping, Exited, and Faulted, exhaustive over the eight signals, with a refusal reason on every illegal pair. `TimeWarp.Ramani` 1.0.0-beta.23 is a direct dependency of the core library only. The public surface, including the commented `AsJsonRpcClient()` stub, and `<Version>` `3.0.0-beta.1` stay as they are. Tests drive a narrow process seam and read `MachineStep` from `SendAsync`.

The fix pass closes the open review. A `Send` whose id is already in the map, pending or finished, is `Refused` with `duplicate request id` and does not throw. The Ready read posts `Response` only for a response frame (`result` or `error`, and no `method`). A notification, an error with a null id, and a server request do not fault the session and do not complete a pending client id. When Stopping's stop activity finishes, its completion posts `ChildExited`, the session enters Exited, and pending ids fail. A finished map entry is removed by `Release` and by the next accepted `Send`, so a live session does not retain one payload per request. Each session test file runs by itself: a block-scoped namespace, the fake seam included beside it, and `InternalsVisibleTo` for that assembly. `dev build`, `dev test`, and `ganda repo audit` pass.

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

- [x] `TimeWarp.Ramani` 1.0.0-beta.23 or later pinned; core package only; restore works with nuget.org alone
- [x] Internal exhaustive Ramani machine with explicit `.Signals<...>()` and per-state refusals
- [x] Process seam interface and fake
- [x] Outbound channel drained and stdout read inside the single `Ready` do-activity
- [x] `Starting` do-activity drives `Started`
- [x] `.OnFault()` on `Ready` into `Faulted`
- [x] Tests step every accepted transition and every refusal, through the fake seam, reading `MachineStep` from `SendAsync`
- [x] A request timeout completes one id and stays in `Ready`
- [x] `ChildExited` fails every in-flight id
- [x] Unknown `Response` id refused with a reason
- [x] Public API unchanged, including the commented `AsJsonRpcClient()` stub
- [x] Tests reach the internal machine: decide `InternalsVisibleTo` for the test tree or a test-only seam, and record the choice in Notes
- [x] `./tools/dev-cli/dev.cs build` and `./tools/dev-cli/dev.cs test` pass
- [x] `ganda repo audit` is clean
- [x] Duplicate `Send` id, including a finished id, is `Refused` with `duplicate request id`
- [x] Notifications, null-id errors, and server requests do not fault the session or complete a client id
- [x] Stopping's activity completion posts `ChildExited` and fails pending ids
- [x] Finished in-flight entries leave on `Release` and on the next accepted `Send`
- [x] `json-rpc-session-machine.send-async.cs` and the guard file each run alone

## Session

- Created: 01a1b4c4-827b-7d70-9fd0-358726b63b0b (2026-11-07)
- Spec review: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-07). Blocked on Ramani publishing to nuget.org (Ramani 016) and on `SendAsync` returning the step (Ramani 013). Both shipped in 1.0.0-beta.23; spec revised 2026-11-08.
- Implement: internal machine, seam, and SendAsync tests (2026-11-08). `ganda repo audit --fix` wrote the local gitignored `.vscode/settings.json` so the window-icon advisory passes in this worktree. The test file was split so the fake seam is its own file under the 500-line cap.
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1ba1d-527e-7e32-b55f-ee53bd850cfd, max-turns 500, Done — 2026-11-08T08:23:50Z
- Review oracle round 1: claude-opus-5-5 (Claude Code, headless), effort 3, roster general, 2026-11-08. Found 5 open findings (2 bug, 3 suggestion) and requested a fix pass. See `review/round-1/merged.md`.
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 2d45f039-f119-4ed9-9f1d-86940ac5713f, max-turns 200, artifacts general.md, merged.md — 2026-11-08T08:29:23Z
- Fix pass: closed M1–M5 on this branch (2026-11-08). Duplicate ids are refused, non-response frames are ignored, Stopping posts `ChildExited`, finished map entries are dropped, and both session test files run alone.

## Review

- Round 1: Blocked — open M1, M2, M3, M4, M5
- Fix pass: M1–M5 implemented. Round 2 is the host review.

## Notes

Filed from the Ramani review on 2026-11-07. Task 083 remains the public client: `ShellBuilder.AsJsonRpcClient()` (or the equivalent entry point), `JsonSerializerOptions` / `JsonSerializerContext`, stderr observation, PublicAPI, XML docs, the NativeAOT sample, the MCP sample, and the docs. After this task merges, 083 implements the real process seam behind the interface this machine calls and deletes nothing this task proved.

Do not model one state per request. Two overlapping sends would then be illegal.

Review findings folded in on 2026-11-08: Ramani effects are synchronous (channel plus do-activity for writes), one do-activity per state, the activity never writes the context, `OnFault` maps read exceptions to `Faulted`, `.Signals<...>()` closes the exhaustiveness gap, unknown ids need `.Else()`, and refusals reach the caller through the step `SendAsync` returns. The repo has no `InternalsVisibleTo` today; the checklist item leaves that decision to the implementer.

InternalsVisibleTo choice: grant `run-tests`, `json-rpc-session-machine.send-async`, and `json-rpc-session-machine.send-async.guard` on `TimeWarp.Amuru`. `dev test` compiles `tests/timewarp-amuru/multi-file-runners/run-tests.cs`, whose assembly name is `run-tests`, and that compilation includes the session tests plus `json-rpc-session-machine.fake-seam.cs`. The other friends are the assembly names `dotnet run` gives those two test files alone. Each file includes the fake with `#:include` and uses a block-scoped namespace so the standalone entry point compiles. `TimeWarp.Amuru.Tools` does not get InternalsVisibleTo and does not reference Ramani. Exited and Faulted stay non-final so a refused `SendAsync` still returns a reason; SM010 is suppressed on the machine file for that reason.

## Results

### Fix pass (round 1 findings)

M1: Ready `Send` is guarded on "id not in the map" and otherwise `.Refuse<Send>("duplicate request id")`. A second send of a pending id, and a send of an id that has completed and has not been released, return `Refused` and do not throw. M2: the Ready read posts `Response` only for a frame with `result` or `error`, no `method`, and a string or number id. A notification, an `"id": null` error, and a server request stay in Ready and leave the pending client id pending. A malformed frame still takes `OnFault`. M3: Stopping's do-activity returns `bool`, and that completion posts `ChildExited`, so pending ids fail without the host sending the signal. M4: `json-rpc-session-machine.send-async.cs` and `json-rpc-session-machine.send-async.guard.cs` use block-scoped namespaces and `#:include` the fake seam. Both `dotnet run` alone. `InternalsVisibleTo` names `run-tests` and those two assembly names. The aggregate omits the fake from the compile glob and suppresses `CS2002`, because both files include that one seam. M5: `Release` drops one finished entry, and the next accepted `Send` drops every finished entry. A pending id stays.

Round 1 artifacts stay at `review/round-1/merged.md`. This pass does not dispose them. Round 2 is the host review.

The session is an internal exhaustive Ramani machine, `JsonRpcSessionMachine`, in `TimeWarp.Amuru.JsonRpc`. `TimeWarp.Ramani` 1.0.0-beta.23 is pinned in `Directory.Packages.props` and referenced only by the core project. `<Version>` is still `3.0.0-beta.1`. `AsJsonRpcClient()` stays commented. PublicAPI.Unshipped.txt is unchanged.

| Gate | Result |
|------|--------|
| `./tools/dev-cli/dev.cs build` | Build succeeded, 0 warnings, 0 errors. Packages: `TimeWarp.Amuru.3.0.0-beta.1.nupkg`, `TimeWarp.Amuru.Tools.3.0.0-beta.1.nupkg` |
| `dotnet run` `json-rpc-session-machine.send-async.cs` | Total 31, Passed 31 |
| `dotnet run` `json-rpc-session-machine.send-async.guard.cs` | Total 9, Passed 9 |
| `./tools/dev-cli/dev.cs test` | Total 782, Passed 781, Failed 0, Skipped 1. `SendAsync_Given_` 31 passed. `SendAsync_Given_Guard_` 9 passed. `All tests passed!` |
| `ganda repo audit` | Passed 38, Failed 0, Skipped 2 |

### How to validate

**Smoke**

```bash
./tools/dev-cli/dev.cs build
dotnet run tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-session-machine.send-async.cs
dotnet run tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-session-machine.send-async.guard.cs
./tools/dev-cli/dev.cs test
ganda repo audit
```

**Expect**

- Build prints `Build succeeded.` with `0 Warning(s)` and `0 Error(s)`, and writes `TimeWarp.Amuru.3.0.0-beta.1.nupkg` and `TimeWarp.Amuru.Tools.3.0.0-beta.1.nupkg`. `source/Directory.Build.props` `<Version>` is still `3.0.0-beta.1`.
- Each standalone file prints `Passed` equal to `Total` (`31` and `9`) and does not fail to compile.
- The suite prints `Failed: 0` with `Total` greater than 0 (this run: Total 782, Passed 781, Skipped 1) and `All tests passed!`. `SendAsync_Given_` is 31 passed and `SendAsync_Given_Guard_` is 9 passed.
- Audit prints `Failed: 0` and `Repository passes all audit checks.` A fresh clone without `.vscode/settings.json` still reports the window-icon advisory; that file is gitignored and `ganda repo audit --fix` creates it locally.
