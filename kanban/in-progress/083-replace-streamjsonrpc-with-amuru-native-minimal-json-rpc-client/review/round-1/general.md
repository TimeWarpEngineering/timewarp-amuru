# Round 1 — general
**Date:** 2026-11-08
**Scope reviewed:** branch vs master: `source/timewarp-amuru/json-rpc/*`, `core/shell-builder.cs`, `PublicAPI.Unshipped.txt`, `tests/.../json-rpc/*`, `samples/json-rpc/aot-json-rpc-example.cs`, docs (how-to, design, reference)

## Summary

The change adds a working request/response JSON-RPC client over a child process on the .NET 11 pipe and handle APIs. It sits on the task 139 machine without rewriting it. M9 is resolved with a per-entry completion and a `ConcurrentDictionary`. Correlation, error mapping, timeout, cancellation, stderr separation, and exit-status capture all behave as described. I re-ran the client tests (14/14) and the machine tests (31/31 and 12/12), and all passed. There are two problems. First, the reflection serialization path silences the AOT and trim analyzers with no annotation flowing to callers. Second, the checklist item "validate compatibility with real MCP servers" is checked, but the only test target is a Python mock. The rest are robustness and coverage improvements.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-amuru/json-rpc/json-rpc-client.cs:304
- Description: `SerializeParams` and `DeserializeResult` carry `[UnconditionalSuppressMessage]` for IL2026 (RequiresUnreferencedCode) and IL3050 (RequiresDynamicCode). They then call the reflection overloads `JsonSerializer.Serialize<T>(value, options)` and `JsonSerializer.Deserialize<T>(json, options)`. The justification says the reflection path runs only when no context was supplied. That describes a runtime branch. It does not make the code trim- or AOT-safe. A NativeAOT consumer that calls the public `WithJsonSerializerOptions` gets no build warning, and the call fails at runtime. The suppression exists only to make the trim and AOT analyzers pass (`IsAotCompatible` publish clean). The work that analyzer measures was not done.
- Suggestion: Remove both suppressions. In the options branch, resolve metadata through the AOT-safe API: `JsonTypeInfo info = SerializerOptions.GetTypeInfo(typeof(T))`, then `JsonSerializer.Serialize(value, info)` / `JsonSerializer.Deserialize(json, info)`. `JsonSerializerOptions.GetTypeInfo(Type)` has no RUC/RDC annotation in the net11 reference assembly. It works for options that carry a source-generated `TypeInfoResolver`. When reflection is enabled it falls back to the default reflection resolver through the System.Text.Json feature switch. Another option is to put `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]` on `JsonRpcClientBuilder.WithJsonSerializerOptions` so AOT callers are warned. In both cases the suppression must stop hiding a reachable reflection path.
- Status: open

### Issue 2 — Severity: bug
- File: tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-client.send-async.cs:76
- Description: The task checklist item "Add/update MCP sample/tests to validate compatibility with real MCP servers" is checked. The only MCP evidence is `InitializeAndToolsList_Should_MatchTheMcpShapes`, which runs against `json-rpc-fixture.py`. That fixture is a hand-written mock that returns the shapes the test expects. No real MCP server is exercised, so required coverage is missing and the checked box overstates the work. The implementer restatement narrowed this to "an MCP initialize and tools/list exchange works over the same client" (see Learnings in task.md).
- Suggestion: Add a validation against a real MCP stdio server. One way is a file-based server built on the ModelContextProtocol C# SDK, run by the test. Another is a test that skips when `npx` / `@modelcontextprotocol/server-everything` is unavailable. The validation runs `initialize` and `tools/list` (plus `notifications/initialized` if Issue 3 is taken). If the operator decides a mock is enough, mark this wontfix with that decision recorded, and uncheck or reword the checklist item.
- Status: open

### Issue 3 — Severity: suggestion
- File: source/timewarp-amuru/json-rpc/json-rpc-client.cs:104
- Description: The client cannot send a client-to-server notification (a frame with no id). The MCP lifecycle requires the client to send `notifications/initialized` after the `initialize` response. A spec-strict MCP server can refuse later requests without it. The docs and the test present `initialize` followed by `tools/list` as the MCP exchange. The task's success criteria name MCP as the validating consumer. This is outbound only, so it is not the "events" non-goal, which is about inbound dispatch.
- Suggestion: Add `NotifyAsync<TParams>(string method, TParams? parameters, CancellationToken)`. It writes a frame through the outbound channel and creates no in-flight entry (this needs a machine signal or a channel write path that the 139 machine accepts). Use it in the MCP how-to and test. If this is deliberately left out, say in the how-to that MCP servers which enforce the initialized notification are not supported yet.
- Status: open

### Issue 4 — Severity: suggestion
- File: source/timewarp-amuru/json-rpc/json-rpc-child-process.cs:217
- Description: `FinishAfterStdoutClosedAsync` runs inside `ReadFrameAsync` on the Ready do-activity. It calls the synchronous `Child.TryWaitForExitStatus(500 ms)`, then `RememberExitStatus` (line 278), which calls the synchronous `TryWaitForExitStatus(2 s)`. `StopAsync` also calls `RememberExitStatus`. These block a thread-pool thread for up to 2.5 s inside async methods. A child that closes stdout but keeps running is also force-killed after 500 ms, and that policy is not documented.
- Suggestion: Use `WaitForExitStatusAsync` with a timeout or a linked token in place of the synchronous waits. Document, in the Design region and the how-to, that closing stdout ends the session and kills the child.
- Status: open

### Issue 5 — Severity: suggestion
- File: source/timewarp-amuru/json-rpc/json-rpc-client.cs:288
- Description: `ReadCompleted` wraps only `JsonException`. Two common misconfigurations escape the documented contract, which promises `JsonRpcException` "the result could not be read": a `JsonSerializerContext` that lacks `TResult`, and a `TParams` missing from the context in `SerializeParams`. Both throw `InvalidOperationException` / `NotSupportedException` from System.Text.Json.
- Suggestion: Also map `InvalidOperationException` and `NotSupportedException` from serialization to `JsonRpcException` with the inner exception attached. Or document `InvalidOperationException` for a missing context type in the XML docs of `SendAsync`.
- Status: open

### Issue 6 — Severity: suggestion
- File: tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-client.send-async.cs:1
- Description: There are coverage gaps for public or documented behavior. No test uses `WithJsonSerializerOptions` (the reflection path, or options with a source-generated `TypeInfoResolver`). No test covers a non-JSON line on stdout, which faults the session and fails pending requests. No test calls `SendAsync` after `DisposeAsync` (`ObjectDisposedException`).
- Suggestion: Add a test for each. For the options path, use options with `TypeInfoResolver = FixtureJsonContext.Default`, because file-based hosts disable reflection.
- Status: open

### Issue 7 — Severity: nit
- File: source/timewarp-amuru/json-rpc/json-rpc-child-process.cs:33
- Description: Nothing disposes `StandardErrorCancellation`. The `CA1001` suppression on the class says "StopAsync releases the process, pipes, and readers", but the CTS is not released there.
- Suggestion: Dispose `StandardErrorCancellation` at the end of `StopAsync`, after the stderr task has been awaited. Then the CA1001 justification is fully true.
- Status: open

### Issue 8 — Severity: nit
- File: source/timewarp-amuru/json-rpc/json-rpc-client.cs:357
- Description: `WaitUntilAsync` polls `Machine.State` every 10 ms. In `DisposeAsync` (line 153) it has no upper bound and no token. If the machine ever stalls outside Exited or Faulted, dispose hangs forever.
- Suggestion: Await a state-change notification if Ramani exposes one, or a completion set by the Exited/Faulted entry. Otherwise put a bound on the wait, for example grace plus a margin.
- Status: open
