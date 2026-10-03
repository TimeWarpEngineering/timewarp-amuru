# 042 Expose Comprehensive StreamJsonRpc Features and Process Management

## Problem We're Solving

Users need to debug JSON-RPC connection issues in production, handle server-initiated requests, and manage process lifecycles - but our current API only exposes `SendRequestAsync`. They're forced to either hack around limitations or use StreamJsonRpc directly.

## Target API (What Success Looks Like)

```csharp
// Basic usage stays simple
var client = await Shell.Builder("mcp-server")
    .AsJsonRpcClient()
    .StartAsync();

// Configure diagnostics at build time
var debugClient = await Shell.Builder("mcp-server")
    .AsJsonRpcClient()
    .WithTracing(true)
    .WithTraceListener(new ConsoleTraceListener())
    .WithTraceLevel(SourceLevels.Verbose)
    .StartAsync();

// Power users get full access:
// 1. Debug connection issues
client.TraceSource.Listeners.Add(new TextWriterTraceListener("jsonrpc.log"));
client.Disconnected += (s, e) => Logger.Error($"Lost connection: {e.Reason}", e.Exception);

// 2. Handle server-initiated requests (bidirectional)
client.AddLocalRpcMethod("server/ping", () => "pong");

// 3. Send notifications (fire and forget)
await client.NotifyAsync("textDocument/didOpen", new { uri = "file.txt" });

// 4. Manage the process
await client.Process.RestartAsync();
var exitCode = await client.Process.ExitCode;

// 5. Direct access for edge cases
client.UnderlyingJsonRpc.AllowModificationWhileListening = true;
client.UnderlyingJsonRpc.AddLocalRpcTarget(myComplexHandler);
```

## Design First

Before implementation, update `Documentation/Conceptual/Design/JsonRpc-Support-Design.md` with:
- [ ] Target API examples showing real use cases
- [ ] Architecture decisions (why expose X but not Y)
- [ ] Migration path for users currently working around limitations

## Implementation Phases

### Phase 1: Diagnostics & Connection State

**Goal**: Users can debug connection issues in production

- [ ] Expose `TraceSource`, `Disconnected` event, `IsConnected`, `Completion`, `DisconnectedToken`
- [ ] Add builder methods: `WithTracing()`, `WithTraceListener()`, `WithTraceLevel()`
- [ ] Test disconnection scenarios (process crash, timeout, malformed JSON)
- [ ] Integration test with filesystem MCP server (from https://github.com/modelcontextprotocol/servers)
- [ ] Create example showing how to diagnose "MCP server not responding" issue

### Phase 2: Bidirectional Communication

**Goal**: Support LSP and MCP servers that send requests to clients

- [ ] Add `NotifyAsync()` for fire-and-forget messages
- [ ] Add `AddLocalRpcMethod()` for handling server requests
- [ ] Integration test with OmniSharp or C# LSP (roslyn-based) for bidirectional communication
- [ ] Test handling `window/showMessage` and `window/logMessage` from C# language server
- [ ] Create example of MCP server calling client-side tool

### Phase 3: Process Lifecycle Management

**Goal**: Manage process restarts, monitor health, capture stderr

- [ ] Add `IProcessInfo` with restart, stderr access, exit code tracking
- [ ] Support auto-restart on crash with configurable retry
- [ ] Test process crash recovery scenarios
- [ ] Example: Restarting crashed MCP server automatically

- Named arguments support
- Progress reporting
- Typed proxy generation (`client.Attach<IMyService>()`)
- Direct `UnderlyingJsonRpc` access for edge cases

## Success Metrics

- Users can diagnose "why isn't my MCP server responding" without contacting support
- Bidirectional protocols (LSP, DAP) work without workarounds
- Process crashes are recoverable without restarting the entire application
- Design document reflects actual implementation

## Key Decision: What NOT to Expose

- Synchronous methods (keep async-only)
- Complex formatters (use defaults that work)
- Low-level transport details
- Features that would break if StreamJsonRpc changes internals

## References

- [GitHub Issue #18](https://github.com/TimeWarpEngineering/timewarp-amuru/issues/18)
- [StreamJsonRpc TraceSource](https://microsoft.github.io/vs-streamjsonrpc/docs/troubleshooting.html)
- [StreamJsonRpc Disconnection](https://microsoft.github.io/vs-streamjsonrpc/docs/disconnecting.html)
- Related task: 029_Implement-JSON-RPC-Support.md