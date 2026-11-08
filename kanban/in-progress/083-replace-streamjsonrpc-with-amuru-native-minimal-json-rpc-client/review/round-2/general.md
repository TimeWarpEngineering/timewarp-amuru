# Round 2 — general
**Date:** 2026-11-08
**Scope reviewed:** the fix delta since round 1, `a087dba..b47b9bb` (commits 2afada3 and b47b9bb). That covers `source/timewarp-amuru/json-rpc/*`, `PublicAPI.Unshipped.txt`, `Directory.Packages.props`, the two JSON-RPC docs, the client and guard tests, the Python fixture, the new `mcp-stdio-server/` validation server, and the aggregate runner props.

## Summary

The fix pass closes M1 through M8. Both serializer paths now resolve `JsonTypeInfo` and call those overloads. The IL2026 and IL3050 suppressions are gone, and the library builds Release with 0 warnings. `NotifyAsync` is a new machine signal (`Notify`). It is handled in Ready, refused in every other state, and writes a frame with no id. A real ModelContextProtocol 2.4.0 stdio server (the official C# SDK) completes initialize, `notifications/initialized`, and tools/list. Exit waits are asynchronous, `StopAsync` disposes the stderr cancellation source, and dispose waits on a `Settled` completion bounded by grace plus 2 s. Risk is low. Two nits remain, and neither changes behavior.

Re-run in this round: `json-rpc-client.send-async.cs` passed 20 of 20, including the real MCP server, closed stdout (SIGKILL status), serializer options, missing metadata, malformed stdout, and send after dispose. `json-rpc-session-machine.send-async.cs` passed 31 of 31. `json-rpc-session-machine.send-async.guard.cs` passed 14 of 14, including the notify frame and the notify refusals. `dotnet build source/timewarp-amuru/timewarp-amuru.csproj -c Release` reported 0 warnings and 0 errors.

## Prior findings re-verified

- M1 fixed: `json-rpc-client.cs:412` `TypeInfo(Type)` resolves through `SerializerContext.GetTypeInfo` or `SerializerOptions.GetTypeInfo`. No `UnconditionalSuppressMessage` remains under `json-rpc/`. A NativeAOT host that passes options with no resolver now gets an `InvalidOperationException` from System.Text.Json, wrapped as `JsonRpcException`, and the builder XML doc says so. This is the AOT-safe behavior.
- M2 fixed: `mcp-stdio-server/mcp-stdio-server.cs` uses `AddMcpServer().WithStdioServerTransport()`. `RealMcpServer_Should_InitializeAndListTools` builds its apphost and asserts protocol `2025-06-18`, the server name, and the `add` tool. The restatement now says "a real MCP SDK stdio server", which matches the task checklist.
- M3 fixed: `NotifyAsync` (3 overloads, in PublicAPI) sends `Notify`. `JsonRpcSessionContext.WriteNotification` posts to the outbound channel and creates no in-flight entry. The fixture skips frames with no id.
- M4 fixed: `WaitForExitStatusAsync(TimeSpan)` replaces both `TryWaitForExitStatus` calls. The EOF-ends-session policy is in the Design region and both docs.
- M5 fixed: serialize and deserialize wrap `JsonException`, `InvalidOperationException`, and `NotSupportedException` in `JsonRpcException`. The XML docs and the how-to table list the metadata case.
- M6 fixed: the three requested tests exist and pass.
- M7 fixed: `StopAsync` ends with `StandardErrorCancellation.Dispose()`. The `Stopped` interlock means it runs once.
- M8 fixed: dispose awaits `Context.Settled` with a timeout of `Grace + SettleMargin`. Every transition into Exited or Faulted calls `FailPending`, which completes `Settled`. Start waits on `Ready` or `Settled`.

## Issues

### Issue 1 — Severity: nit
- File: source/timewarp-amuru/json-rpc/json-rpc-client.cs:453
- Description: `WaitUntilStatePublishedAsync` loops on `Task.Yield()` with no delay for up to 2 s while the state is Starting or Stopping. `Ready` and `Settled` complete inside the transition effect, which runs before the machine assigns the state, so the start path still polls `machine.State`. In practice the gap is a few pump instructions. In theory it is a hot spin. Start could decide success from the gates alone (`Ready` completed and `Settled` not completed) and not read `machine.State`.
- Suggestion: Decide start success from `context.Ready` and `context.Settled`, or add a short `Task.Delay` to the loop.
- Status: open

### Issue 2 — Severity: nit
- File: tests/timewarp-amuru/single-file-tests/json-rpc/json-rpc-client.send-async.cs:863
- Description: `McpServerPathAsync` caches the server build in a fixed temp directory. It rebuilds only when the `.cs` source is newer than the binary. A change to `Directory.Packages.props` or the server's `Directory.Build.props` reuses a stale build. On Windows the apphost is `mcp-stdio-server.exe`, so `File.Exists` is false and every run rebuilds. Two concurrent runs can also build into the same directory.
- Suggestion: Include the props timestamps in the staleness check and the `.exe` suffix on Windows, or build into a per-run directory.
- Status: open
