# Add an Amuru-native minimal JSON-RPC client over child-process stdio

## Description

**Reframed 2026-10-07.** Task 084 already removed `StreamJsonRpc` (and its transitive `Newtonsoft.Json`) from Amuru; no JSON-RPC code ships today (only a commented-out `AsJsonRpcClient` stub in `core/shell-builder.cs`). This is therefore an **additive feature**, not a replacement: a small Amuru-native JSON-RPC client that talks to an arbitrary child process over stdin/stdout, written to this repository's conventions and AOT-clean.

**Depends on 119 (.NET 11 upgrade).** .NET 11 (GA 2026-11-10) adds the process primitives this feature wants, so build it once on net11 rather than on net10 and rework it a month later: handle-based stdio redirection on `ProcessStartInfo` (`StandardInputHandle`/`StandardOutputHandle`/`StandardErrorHandle`) with `SafeFileHandle.CreateAnonymousPipe` so Amuru owns the pipe ends; `KillOnParentExit` so an orphaned server dies with the host; `ProcessExitStatus` / `WaitForExitStatusAsync` to tell a crash from a signal; `SafeProcessHandle.WaitForExitOrKillOnCancellationAsync` for shutdown. Windows redirected pipes use overlapped I/O in 11, so async reads are real. Stdio stays the documented transport for JSON-RPC to a child (StreamJsonRpc docs; no .NET guidance recommends another). PTY is not needed here.

**Transport decision:** build on raw `Process` / the net11 handle APIs behind an Amuru-owned abstraction, **not** CliWrap. CliWrap exposes no raw bidirectional streams and is a poor fit for a long-lived protocol channel. Keep stderr separate from the JSON-RPC stream (observe/log it; never merge). Reuse `CommandOptions.Timeout` semantics (task 044) for per-request timeouts where sensible.

Original intent (still valid): avoid any `Newtonsoft.Json` path so NativeAOT consumers stay clean; MCP servers are one consumer but the client stays general JSON-RPC.

The new implementation should support Amuru's intended scope: JSON-RPC request/response communication with arbitrary child processes over stdin/stdout. MCP is one example consumer, but the implementation should remain general JSON-RPC rather than MCP-specific.

## Checklist

- [ ] Define the supported JSON-RPC surface Amuru will own (request/response only, no proxy generation, no server dispatch)
- [ ] Design Amuru-native JSON-RPC message models for requests, responses, and errors
- [ ] Implement newline-delimited JSON-RPC transport over process stdin/stdout
- [ ] Implement request id generation and in-flight request correlation
- [ ] Implement typed request serialization using `System.Text.Json`
- [ ] Implement typed response and error deserialization using `System.Text.Json`
- [ ] Implement timeout and cancellation handling for requests
- [ ] Implement stderr observation/logging behavior for child processes
- [ ] Real `IJsonRpcProcessSeam` over a child process; the 139 fake seam and tests stay
- [ ] Thread-safe result read path chosen and recorded (139 handoff M9)
- [ ] Process layer on net11 primitives (anonymous pipes + handle redirection, `KillOnParentExit`, `ProcessExitStatus`), wrapped in an Amuru abstraction; no CliWrap in the channel
- [ ] Builder accepts `JsonSerializerOptions` / `JsonSerializerContext` (source-generated, AOT-safe); no formatter interface in the public API
- [ ] Restore `ShellBuilder.AsJsonRpcClient()` (or equivalent entry point) and remove the commented stub
- [ ] PublicAPI Unshipped updated; XML docs; `IsAotCompatible` publish clean
- [ ] Verify a NativeAOT sample consumer builds with no `Newtonsoft.Json` in its graph
- [ ] Add/update tests for generic JSON-RPC request/response behavior
- [ ] Add/update MCP sample/tests to validate compatibility with real MCP servers
- [ ] Update documentation and examples to reflect the new JSON-RPC implementation

## Non-Goals

- [ ] Do not reimplement full StreamJsonRpc feature parity
- [ ] Do not add proxy generation, local target dispatch, events, marshalable objects, or multiplexing unless Amuru actually needs them
- [ ] Do not copy source wholesale from `vs-streamjsonrpc`; reimplement a minimal subset inspired by required behavior only

## Depends on

- 119
- 139

## Session

- Created: ses_27dd18c7effe1K4rnFRhnQezjn (2026-04-14)
- Reframed as additive, depends on 119: 522eb63d (2026-10-07)
- Spec updated for the 139 machine and the API decision: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-08)

## Notes

### Why this task exists

`StreamJsonRpc` carries a direct `Newtonsoft.Json` dependency, which poisons NativeAOT/trimming for downstream packages. Task 084 removed it; this task adds the capability back without it. Research trail for the net11 primitives: dotnet blog "Process API Improvements in .NET 11" (2026-05-13), .NET 11 RC1 release notes, dotnet/runtime #123380; PTY (#128565) is .NET 12 research and not relevant here.

### What Amuru actually uses today

Current Amuru usage appears limited to a small client-side subset:
- start a child process
- send JSON-RPC requests over stdin/stdout
- receive typed responses
- handle request/response correlation
- dispose cleanly

Amuru does **not** currently appear to need the broader StreamJsonRpc feature set such as:
- proxy generation
- local server target dispatch
- multiplexing
- progress/event support
- MessagePack
- marshalable objects

### Design constraints

- Write the replacement in Amuru style and follow repository C# conventions
- Prefer simple explicit models over reflection-heavy abstractions
- Keep the implementation general JSON-RPC for arbitrary processes, not MCP-specific
- Use `System.Text.Json` and source-generation/AOT-friendly patterns where appropriate
- Favor a minimal, understandable implementation over feature completeness

### Public API decision

Resolved 2026-11-08: the builder accepts `JsonSerializerOptions` and a `JsonSerializerContext` (source-generated, AOT-safe). No formatter abstraction in the public API. `StreamJsonRpc.IJsonRpcMessageFormatter` is gone with task 084 and does not come back under another name.

### Built on task 139

Task 139 (merged 2026-11-08, PR #133) shipped the internal session machine `JsonRpcSessionMachine` in `TimeWarp.Amuru.JsonRpc`, an exhaustive Ramani machine with states Idle, Starting, Ready, Stopping, Exited, Faulted and the internal seam:

```csharp
internal interface IJsonRpcProcessSeam
{
  Task StartAsync(CancellationToken cancellationToken);
  Task WriteFrameAsync(string frame, CancellationToken cancellationToken);
  Task<string?> ReadFrameAsync(CancellationToken cancellationToken);
  Task StopAsync(CancellationToken cancellationToken);
}
```

This task implements that seam over a real child process on the net11 primitives, and builds the public client on top of the machine. Do not rewrite the machine, its signals, or its tests. Add to them. `TimeWarp.Ramani` 1.0.0-beta.23 is already pinned and referenced by the core package only; keep it out of `TimeWarp.Amuru.Tools`.

Handoff from the 139 review (finding M9, deferred here): `JsonRpcSessionContext.Requests` is a plain `Dictionary`. Pump effects are its only writers (`Accept` adds, `Release` removes). A host that reads an entry from another thread while an effect runs is unsafe. Before any concurrent caller can reach the map, choose one: a `ConcurrentDictionary`, reads performed on the pump, or an awaitable completion per entry. The public `SendAsync` result path is where this lands. Record the choice in Notes.

The machine's `SendAsync` returns `MachineStep`. A `Refused` outcome with `Reason` (for example `the child is not ready`, `already running`, `duplicate request id`) is what the public client turns into its error. Do not add a completion source on the signal.

### Success criteria

- Amuru no longer depends on `StreamJsonRpc`
- Amuru no longer pulls `Newtonsoft.Json` into downstream package graphs through JSON-RPC support
- Existing JSON-RPC scenarios continue to work for generic process-based request/response use cases
- MCP sample/test scenarios still work as validation, but do not define the entire feature surface
