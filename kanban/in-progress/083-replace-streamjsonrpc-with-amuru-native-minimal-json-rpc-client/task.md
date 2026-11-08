# Add an Amuru-native minimal JSON-RPC client over child-process stdio

## Goal (implementer restatement)

Ship a small JSON-RPC 2.0 client that Amuru owns. A caller starts an arbitrary child with `ShellBuilder.AsJsonRpcClient()`, sends a request on the child's stdin, and reads one correlated response from stdout. The client does not generate proxies, dispatch server methods, or speak events.

The child runs on the .NET 11 process APIs: Amuru creates the stdio pipes, redirects them with handle properties, and asks the OS to kill the child if the parent dies. Stderr is read and can be observed. It is never mixed into the JSON-RPC stream. Request bytes and response bytes use `System.Text.Json`. A source-generated `JsonSerializerContext` is the NativeAOT path. `JsonSerializerOptions` remains available for callers who are not publishing NativeAOT. There is no formatter interface.

The session still uses the task 139 machine and its fake seam. This work adds the real process seam and the public client on top. A caller waiting for a result waits on a completion that belongs to that one in-flight entry. The in-flight map is a concurrent dictionary so taking that completion is safe while the pump accepts or releases another id.

Acceptance: the package still has no StreamJsonRpc and no Newtonsoft.Json dependency; request, response, error, timeout, and cancellation work against a child; an MCP initialize and tools/list exchange works over the same client; the public surface, XML docs, and an AOT sample are updated; existing machine tests still pass.

## Description

**Reframed 2026-10-07.** Task 084 already removed `StreamJsonRpc` (and its transitive `Newtonsoft.Json`) from Amuru; no JSON-RPC code ships today (only a commented-out `AsJsonRpcClient` stub in `core/shell-builder.cs`). This is therefore an **additive feature**, not a replacement: a small Amuru-native JSON-RPC client that talks to an arbitrary child process over stdin/stdout, written to this repository's conventions and AOT-clean.

**Depends on 119 (.NET 11 upgrade).** .NET 11 (GA 2026-11-10) adds the process primitives this feature wants, so build it once on net11 rather than on net10 and rework it a month later: handle-based stdio redirection on `ProcessStartInfo` (`StandardInputHandle`/`StandardOutputHandle`/`StandardErrorHandle`) with `SafeFileHandle.CreateAnonymousPipe` so Amuru owns the pipe ends; `KillOnParentExit` so an orphaned server dies with the host; `ProcessExitStatus` / `WaitForExitStatusAsync` to tell a crash from a signal; `SafeProcessHandle.WaitForExitOrKillOnCancellationAsync` for shutdown. Windows redirected pipes use overlapped I/O in 11, so async reads are real. Stdio stays the documented transport for JSON-RPC to a child (StreamJsonRpc docs; no .NET guidance recommends another). PTY is not needed here.

**Transport decision:** build on raw `Process` / the net11 handle APIs behind an Amuru-owned abstraction, **not** CliWrap. CliWrap exposes no raw bidirectional streams and is a poor fit for a long-lived protocol channel. Keep stderr separate from the JSON-RPC stream (observe/log it; never merge). Reuse `CommandOptions.Timeout` semantics (task 044) for per-request timeouts where sensible.

Original intent (still valid): avoid any `Newtonsoft.Json` path so NativeAOT consumers stay clean; MCP servers are one consumer but the client stays general JSON-RPC.

The new implementation should support Amuru's intended scope: JSON-RPC request/response communication with arbitrary child processes over stdin/stdout. MCP is one example consumer, but the implementation should remain general JSON-RPC rather than MCP-specific.

## Checklist

- [x] Define the supported JSON-RPC surface Amuru will own (request/response only, no proxy generation, no server dispatch)
- [x] Design Amuru-native JSON-RPC message models for requests, responses, and errors
- [x] Implement newline-delimited JSON-RPC transport over process stdin/stdout
- [x] Implement request id generation and in-flight request correlation
- [x] Implement typed request serialization using `System.Text.Json`
- [x] Implement typed response and error deserialization using `System.Text.Json`
- [x] Implement timeout and cancellation handling for requests
- [x] Implement stderr observation/logging behavior for child processes
- [x] Real `IJsonRpcProcessSeam` over a child process; the 139 fake seam and tests stay
- [x] Thread-safe result read path chosen and recorded (139 handoff M9)
- [x] Process layer on net11 primitives (anonymous pipes + handle redirection, `KillOnParentExit`, `ProcessExitStatus`), wrapped in an Amuru abstraction; no CliWrap in the channel
- [x] Builder accepts `JsonSerializerOptions` / `JsonSerializerContext` (source-generated, AOT-safe); no formatter interface in the public API
- [x] Restore `ShellBuilder.AsJsonRpcClient()` (or equivalent entry point) and remove the commented stub
- [x] PublicAPI Unshipped updated; XML docs; `IsAotCompatible` publish clean
- [x] Verify a NativeAOT sample consumer builds with no `Newtonsoft.Json` in its graph
- [x] Add/update tests for generic JSON-RPC request/response behavior
- [x] Add/update MCP sample/tests to validate compatibility with real MCP servers
- [x] Update documentation and examples to reflect the new JSON-RPC implementation

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
- Implemented the public client, the real process seam, tests, and the AOT sample: implementer oracle (2026-11-08)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1baa5-b54c-7622-a513-e3fc5692ee03, max-turns 500, Done — 2026-11-08T10:57:57Z

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

### M9 result path

Chosen: an awaitable completion on each in-flight entry, and a `ConcurrentDictionary` for the map.

`JsonRpcInFlight.Completion` is a `TaskCompletionSource`. Pump effects are still the only writers. An effect stores the payload, publishes `Status`, then completes the task. `SendAsync` on the public client waits on that task from the caller thread. It does not poll the map. After it reads the payload it sends `Release`, and the effect removes the entry. The dictionary is concurrent so taking one entry's task is safe while another effect accepts or releases a different id. The completion source is not added to the signal. `Machine.SendAsync` returns `MachineStep` and takes only the signal. A `Refused` step becomes `JsonRpcException`.

A response that arrives in the same moment as a timeout or caller cancellation is the result the caller receives. Otherwise caller cancellation throws `OperationCanceledException` and the request timeout throws `TimeoutException`.

### Success criteria

- Amuru no longer depends on `StreamJsonRpc`
- Amuru no longer pulls `Newtonsoft.Json` into downstream package graphs through JSON-RPC support
- Existing JSON-RPC scenarios continue to work for generic process-based request/response use cases
- MCP sample/test scenarios still work as validation, but do not define the entire feature surface

## Results

The public client is `Shell.Builder(...).AsJsonRpcClient()`. It starts one child on anonymous pipes and the .NET 11 handle redirection APIs, then speaks JSON-RPC 2.0 on stdin and stdout. Standard error is drained on its own task and is never merged into that stream. A source-generated `JsonSerializerContext` serializes requests and results. `JsonSerializerOptions` remains for a host that allows reflection. There is no formatter interface. The task 139 machine, its signals, and its tests are unchanged aside from the in-flight map.

M9: each in-flight entry has its own completion, and the map is a `ConcurrentDictionary`. The public client waits on that completion and then sends Release. The completion is not placed on the signal.

`ganda repo audit` is clean. The child-process file was over the 500 line cap, so pipe ownership moved to `json-rpc-pipe-ends.cs`. `ganda repo audit --fix` wrote the local VS Code window title into gitignored `.vscode/settings.json`.

### How to validate

Smoke: `dotnet run tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-client.send-async.cs`

Expect: 14 passed. `add` of 2 and 3 returns sum 5, `initialize` reports protocolVersion `2024-11-05`, a short `WithTimeout` around `sleep` throws `TimeoutException`, and dispose records a `ProcessExitStatus`.

`./bin/dev test` reported 798 passed, 1 skipped, and 0 failed. `./bin/dev verify-samples` compiled 3 samples. `dotnet publish samples/json-rpc/aot-json-rpc-example.cs -c Release -p:PublishAot=true` produced a native binary whose publish directory and the packed `TimeWarp.Amuru` nuspec contain no Newtonsoft.Json and no StreamJsonRpc. Running that binary with `run` and the fixture path prints `5`.
