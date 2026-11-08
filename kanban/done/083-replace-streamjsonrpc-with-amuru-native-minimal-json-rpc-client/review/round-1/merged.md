# Round 1 — merged findings
**Date:** 2026-11-08
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 2 | 0 | 0 |
| suggestion | 4 | 0 | 0 |
| nit | 2 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: source/timewarp-amuru/json-rpc/json-rpc-client.cs:304
- Description: The `[UnconditionalSuppressMessage]` for IL2026 and IL3050 on `SerializeParams` / `DeserializeResult` hides a reflection serialization path that a NativeAOT consumer can reach through the public `WithJsonSerializerOptions`. That consumer gets no build warning and fails at runtime. The suppression makes the trim and AOT analyzers pass without doing the work they measure.
- Suggestion: Resolve through `SerializerOptions.GetTypeInfo(typeof(T))` and call the `JsonTypeInfo` overloads, then drop the suppressions. Or annotate `WithJsonSerializerOptions` with `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`.
- Source: general
- Disposition notes:

### M2 — Severity: bug — Status: open
- File: tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-client.send-async.cs:76
- Description: The checked item "validate compatibility with real MCP servers" is backed only by a Python mock (`json-rpc-fixture.py`). No real MCP server is exercised.
- Suggestion: Add a real MCP stdio server validation (a C# SDK file-based server, or a test that skips when `npx` is absent) that runs initialize and tools/list. Otherwise mark wontfix with the operator's decision and reword the checklist item.
- Source: general
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: source/timewarp-amuru/json-rpc/json-rpc-client.cs:104
- Description: There is no outbound notification API. The MCP lifecycle requires `notifications/initialized` after `initialize`.
- Suggestion: Add `NotifyAsync`, which writes a frame with no id and creates no in-flight entry. Use it in the MCP how-to and test, or document the limitation.
- Source: general
- Disposition notes:

### M4 — Severity: suggestion — Status: open
- File: source/timewarp-amuru/json-rpc/json-rpc-child-process.cs:217
- Description: Synchronous `TryWaitForExitStatus` calls (500 ms and 2 s) block a thread inside async read and stop paths. The kill after stdout EOF is undocumented.
- Suggestion: Use `WaitForExitStatusAsync` with a timeout, and document the EOF-ends-session policy.
- Source: general
- Disposition notes:

### M5 — Severity: suggestion — Status: open
- File: source/timewarp-amuru/json-rpc/json-rpc-client.cs:288
- Description: A context missing `TParams` or `TResult` throws `InvalidOperationException` / `NotSupportedException`, not the documented `JsonRpcException`.
- Suggestion: Wrap those in `JsonRpcException`, or document the exception.
- Source: general
- Disposition notes:

### M6 — Severity: suggestion — Status: open
- File: tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-client.send-async.cs:1
- Description: There is no test for `WithJsonSerializerOptions`, for a malformed stdout line faulting the session, or for `SendAsync` after dispose.
- Suggestion: Add those three tests.
- Source: general
- Disposition notes:

### M7 — Severity: nit — Status: open
- File: source/timewarp-amuru/json-rpc/json-rpc-child-process.cs:33
- Description: `StandardErrorCancellation` is never disposed, although the CA1001 justification claims StopAsync releases everything.
- Suggestion: Dispose it at the end of `StopAsync`.
- Source: general
- Disposition notes:

### M8 — Severity: nit — Status: open
- File: source/timewarp-amuru/json-rpc/json-rpc-client.cs:357
- Description: `WaitUntilAsync` polls every 10 ms. In `DisposeAsync` it has no bound and no token.
- Suggestion: Await a completion or state notification, or bound the wait.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- One reviewer, so nothing was collapsed. M3 is related to M2: a real-server test is where a missing initialized notification would show up. They stay separate because M2 is coverage and M3 is API surface.
