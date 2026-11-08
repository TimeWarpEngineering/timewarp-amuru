# Add an Amuru-native minimal JSON-RPC client over child-process stdio

## Goal (implementer restatement)

Ship a small JSON-RPC 2.0 client that Amuru owns. A caller starts an arbitrary child with `ShellBuilder.AsJsonRpcClient()`, sends a request on the child's stdin, and reads one correlated response from stdout. The client does not generate proxies or dispatch methods the child calls back.

The child runs on the .NET 11 process APIs. Amuru creates the stdio pipes, redirects them with handle properties, and asks the OS to kill the child if the parent dies. Stderr is observed and never mixed into the JSON-RPC stream. Bytes use `System.Text.Json`. Metadata comes from `JsonTypeInfo` (a source-generated context, or options whose resolver supplies that metadata). There is no formatter interface and no trim suppression hiding a reflection serializer.

The session still uses the task 139 machine. This pass adds an outbound notification (a frame with no id and no in-flight entry), because MCP requires `notifications/initialized`. Closing stdout ends the session: the client waits asynchronously, then kills a child that is still running. Dispose waits on the session's settled completion, bounded by the shutdown grace. Serializer metadata misses throw `JsonRpcException`. The standard-error cancellation source is disposed when the child stops.

Acceptance: the package still has no StreamJsonRpc and no Newtonsoft.Json dependency; request, response, error, timeout, cancellation, notification, and dispose work against a child; a real MCP SDK stdio server completes initialize, `notifications/initialized`, and tools/list; the public surface, XML docs, and the AOT sample stay accurate; existing machine tests still pass.

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
- Review oracle round 1: general reviewer (claude-opus-5-5), effort 3, turn cap 200; harness session id not exposed. 2 bugs and 6 other findings open, fix pass requested — 2026-11-08
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 6a6489e7-96a3-4850-af4f-4fa5ee83f23c, max-turns 200, artifacts general.md, merged.md — 2026-11-08T11:02:29Z
- Fix pass for review round 1 (M1–M8): implementer-grok, session 01a1bad2-f495-76a3-ba93-d65e90ab9555 — 2026-11-08
- The MCP server shebang was not executable, so the commit hook skipped attestation. The bit is set — 2026-11-08
- Review oracle round 2: one general reviewer (claude-opus-5-5), effort 3, turn cap 200, fix delta `a087dba..b47b9bb`; the harness did not expose a session id. M1–M8 fixed. Nits M9 and M10 wontfix. Disposition accepted-exceptions — 2026-11-08

## Review

- Round 1: findings M1–M8 implemented in this pass. `review/round-1/merged.md` stays the reviewer's ledger until round 2.
- Round 2: Done
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

The public client is `Shell.Builder(...).AsJsonRpcClient()`. It starts one child on anonymous pipes and the .NET 11 handle redirection APIs, then speaks JSON-RPC 2.0 on stdin and stdout. Standard error is drained on its own task and is never merged into that stream. Both serializer paths resolve `JsonTypeInfo` and call those overloads. A source-generated context is the NativeAOT path. `JsonSerializerOptions` is the other path, and a file-based host sets `TypeInfoResolver` because reflection is disabled. There is no formatter interface and no IL2026/IL3050 suppression. `NotifyAsync` writes one frame with no id and no in-flight entry. Closing the child's stdout descriptor ends the session: the read path waits asynchronously, then kills a child that is still running. Dispose waits on the session settled completion, bounded by the shutdown grace plus a short margin. A missing serializer type throws `JsonRpcException`. `StopAsync` disposes the standard-error cancellation source.

The task 139 machine gained a `Notify` signal. Its other signals and the fake seam stay. The in-flight map is still a `ConcurrentDictionary` with one completion per entry.

M9: each in-flight entry has its own completion, and the map is a `ConcurrentDictionary`. The public client waits on that completion and then sends Release. The completion is not placed on the signal.

`ganda repo audit` is clean. Central package versions now pin `ModelContextProtocol` 2.4.0 and `Microsoft.Extensions.Hosting` 10.0.1 for the validation server only. The aggregate runner excludes that server file so its top-level statements do not collide with the runner. The child-process file was over the 500 line cap, so pipe ownership moved to `json-rpc-pipe-ends.cs`. `ganda repo audit --fix` wrote the local VS Code window title into gitignored `.vscode/settings.json`.

### How to validate

Smoke: `dotnet run tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-client.send-async.cs`

Expect: 20 passed. `add` of 2 and 3 returns sum 5 through `WithJsonSerializerOptions` when the options use camelCase and `TypeInfoResolver` is the source-generated context. `close-stdout` records a SIGKILL `ProcessExitStatus` on non-Windows. The ModelContextProtocol 2.4.0 server completes initialize at protocol `2025-06-18`, `notifications/initialized`, and tools/list containing `add`. A short `WithTimeout` around `sleep` throws `TimeoutException`, and dispose records a `ProcessExitStatus`.

The session machine file reports 31 passed. The guard file reports 14 passed, including a notification frame with no id. `./bin/dev test` reported 806 passed, 1 skipped, and 0 failed.

`./bin/dev verify-samples` compiled 3 samples this pass. The earlier implement pass published `samples/json-rpc/aot-json-rpc-example.cs` with `PublishAot=true`. That publish directory and the packed `TimeWarp.Amuru` nuspec contained no Newtonsoft.Json and no StreamJsonRpc, and running that binary with `run` and the fixture path printed `5`. This pass did not publish again.

### Review

**Disposition: accepted-exceptions.** 2 rounds. The final open count is 0.

Each round had one general reviewer at effort 3. Round 1 used the by-diff budget for 2343 lines. Round 2 used the budget for 832 lines and reviewed the fix delta `a087dba..b47b9bb`.

Final counts: bug 2 fixed. Suggestion 4 fixed. Nit 2 fixed and 2 wontfix.

Round 2 re-verified M1 through M8 as fixed. It re-ran the client file (20 passed, including the real MCP server), the machine file (31 passed), and the guard file (14 passed), and a Release build of the library (0 warnings). Round 2 raised two new nits, both marked wontfix by the review oracle:
- M9: start uses a `Task.Yield` loop, bounded at 2 s, to wait for the state to be published.
- M10: the test-only MCP server build cache misses props changes and the Windows `.exe` name.

The rationale is in `review/disposition.md`.

Round 1 had re-run the client tests (14 passed) and the machine tests (31 and 12 passed).

- M1: both serializer paths use `GetTypeInfo` and the `JsonTypeInfo` overloads. The suppression is gone.
- M2: `mcp-stdio-server` is a file-based ModelContextProtocol 2.4.0 stdio server. The client test builds its apphost and runs initialize, `notifications/initialized`, and tools/list.
- M3: `NotifyAsync` writes a frame with no id and creates no in-flight entry.
- M4: stdout end-of-file waits with `WaitForExitStatusAsync`, then kills a child that is still running. The Python fixture closes the descriptor. Closing the Python file object alone leaves it open.
- M5: serialize and deserialize failures are `JsonRpcException`.
- M6: tests cover serializer options, malformed stdout, and `SendAsync` after dispose.
- M7: `StopAsync` disposes the standard-error cancellation source.
- M8: dispose waits on the settled completion, bounded by grace plus margin.

Paths:
- `review/review-framework.md`
- `review/round-1/general.md` and `review/round-1/merged.md` (frozen)
- `review/round-2/general.md` and `review/round-2/merged.md` (the final ledger)
- `review/disposition.md`

## Learnings

- Misread (review round 1): the task checklist says "validate compatibility with real MCP servers". The implementer restatement's acceptance became "an MCP initialize and tools/list exchange works over the same client", and that was met with a hand-written Python mock. The restatement dropped "real", and the checklist box was then checked. See review M2.
- `JsonSerializerOptions.GetTypeInfo` does not reuse a source-generated context's naming policy. Options that set `TypeInfoResolver` to that context still need `PropertyNamingPolicy` when the wire format is camelCase. CPython's stdout wrapper is created with `closefd` false, so `sys.stdout.close()` does not end the pipe.
