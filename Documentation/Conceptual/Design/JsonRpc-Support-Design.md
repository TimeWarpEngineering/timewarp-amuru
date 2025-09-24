# JSON-RPC Support Design Document

## Executive Summary

This document outlines the design and implementation of JSON-RPC support for TimeWarp.Amuru, enabling seamless communication with interactive processes like MCP servers, Language Server Protocol implementations, and custom JSON-RPC services. The feature extends Amuru's fluent builder pattern to eliminate the 200+ lines of boilerplate typically required for JSON-RPC communication.

## Problem Statement

Currently, developers using Amuru must write verbose Process.Start() code with manual stream management, timeout handling, and request/response correlation when working with JSON-RPC processes. This contradicts Amuru's core mission of eliminating process execution verbosity.

## Design Goals

1. **Simplicity**: Enable JSON-RPC communication with 10-20 lines of code
2. **Type Safety**: Provide strongly-typed request/response handling
3. **Reliability**: Handle edge cases, timeouts, and errors gracefully
4. **Extensibility**: Support various JSON-RPC protocols (MCP, LSP, custom)
5. **Testability**: Provide mock implementations for unit testing
6. **Performance**: Support concurrent requests with proper correlation

## Architecture Overview

### Component Diagram

```
┌─────────────────────────────────────────────────────┐
│                   User Application                   │
└─────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────┐
│              Shell.Builder Extension                 │
│                 AsJsonRpcClient()                    │
└─────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────┐
│                  IJsonRpcClient                      │
│  - SendRequestAsync<T>()                             │
│  - SendNotificationAsync()                           │
│  - DisposeAsync()                                    │
└─────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────┐
│                   JsonRpcClient                      │
│  ┌──────────────────────────────────────────────┐   │
│  │            StreamJsonRpc.JsonRpc              │   │
│  │  - Request/Response Correlation               │   │
│  │  - Message Serialization                      │   │
│  │  - Stream Management                          │   │
│  └──────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────┐
│                  Process (stdio)                     │
│            MCP Server / LSP Server / etc.            │
└─────────────────────────────────────────────────────┘
```

## Core Components

### 1. IJsonRpcClient Interface

```csharp
public interface IJsonRpcClient : IAsyncDisposable
{
    Task<TResponse> SendRequestAsync<TResponse>(
        string method,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task SendNotificationAsync(
        string method,
        object? parameters = null);

    event EventHandler<JsonRpcNotificationEventArgs> NotificationReceived;

    bool IsConnected { get; }
}
```

### 2. JsonRpcClient Implementation

The implementation leverages Microsoft's StreamJsonRpc library for robust JSON-RPC handling:

- **Message Transport**: Uses stdio streams (stdin/stdout) for process communication
- **Serialization**: System.Text.Json with configurable options
- **Request Correlation**: Handled by StreamJsonRpc
- **Lifecycle Management**: IAsyncDisposable with graceful shutdown

### 3. JsonRpcClientBuilder

Provides fluent configuration for JSON-RPC clients:

```csharp
public class JsonRpcClientBuilder
{
    public JsonRpcClientBuilder WithTimeout(TimeSpan timeout);
    public JsonRpcClientBuilder WithJsonOptions(JsonSerializerOptions options);
    public JsonRpcClientBuilder WithTracing(bool enable);
    public Task<IJsonRpcClient> StartAsync();
}
```

### 4. Shell Extension

```csharp
public static class ShellBuilderJsonRpcExtensions
{
    public static JsonRpcClientBuilder AsJsonRpcClient(
        this IShellBuilder builder);
}
```

## Implementation Details

### Message Format Support

1. **Newline-Delimited JSON**: Default for MCP servers
2. **Content-Length Headers**: LSP-style communication
3. **Custom Formats**: Extensible through formatters

### Error Handling Strategy

| Error Type | Handling Approach |
|------------|-------------------|
| Timeout | CancellationTokenSource with configurable duration |
| Malformed JSON | ConnectionLostException with details |
| Process Crash | Dispose client, raise ProcessExited event |
| Network Issues | Retry with exponential backoff (future) |
| Protocol Errors | JsonRpcError with error code and message |

### Concurrent Request Management

- StreamJsonRpc handles request queuing and correlation
- Thread-safe request tracking
- Proper cancellation token propagation
- Deadlock prevention through async/await patterns

### Lifecycle Management

1. **Initialization**:
   - Start process using existing Shell infrastructure
   - Establish stdio streams
   - Initialize StreamJsonRpc.JsonRpc instance
   - Configure message formatter and handlers

2. **Operation**:
   - Send requests with automatic ID generation
   - Correlate responses using StreamJsonRpc
   - Handle notifications through event system
   - Monitor process health

3. **Shutdown**:
   - Send shutdown request (if protocol supports)
   - Graceful timeout period
   - Force kill if necessary
   - Clean up resources

## API Usage Examples

### Basic MCP Server Communication

```csharp
var client = await Shell.Builder("dotnet")
    .WithArguments("run", "--project", mcpServerPath)
    .AsJsonRpcClient()
    .WithTimeout(TimeSpan.FromSeconds(30))
    .StartAsync();

// Initialize connection
var initResponse = await client.SendRequestAsync<InitializeResponse>(
    "initialize",
    new {
        protocolVersion = "1.0.0",
        clientInfo = new { name = "amuru-client", version = "1.0.0" }
    });

// List available tools
var tools = await client.SendRequestAsync<ToolsListResponse>("tools/list");

// Call a tool
var result = await client.SendRequestAsync<dynamic>(
    "tools/call",
    new { toolName = "weather", arguments = new { location = "Seattle" } });

await client.DisposeAsync();
```

### Language Server Protocol

```csharp
var lsp = await Shell.Builder("typescript-language-server")
    .WithArguments("--stdio")
    .AsJsonRpcClient()
    .StartAsync();

// Handle server notifications
lsp.NotificationReceived += (sender, args) =>
{
    if (args.Method == "textDocument/publishDiagnostics")
    {
        var diagnostics = args.Parameters?.ToObject<PublishDiagnosticsParams>();
        // Handle diagnostics
    }
};

// Initialize LSP
await lsp.SendRequestAsync<InitializeResult>(
    "initialize",
    new InitializeParams
    {
        ProcessId = Process.GetCurrentProcess().Id,
        RootUri = "file:///workspace",
        Capabilities = new ClientCapabilities()
    });

// Open a document
await lsp.SendNotificationAsync(
    "textDocument/didOpen",
    new DidOpenTextDocumentParams
    {
        TextDocument = new TextDocumentItem
        {
            Uri = "file:///workspace/test.ts",
            LanguageId = "typescript",
            Version = 1,
            Text = fileContent
        }
    });
```

## Testing Strategy

### Unit Tests

- Mock JsonRpcClient for isolated testing
- Test request/response correlation
- Verify timeout behavior
- Test error handling paths

### Integration Tests

```csharp
[Fact]
public async Task Should_Communicate_With_Real_McpServer()
{
    // Arrange
    var client = await CreateMcpClient();

    // Act
    var response = await client.SendRequestAsync<InitializeResponse>(
        "initialize",
        new { protocolVersion = "1.0.0" });

    // Assert
    response.ProtocolVersion.Should().Be("1.0.0");
    response.ServerInfo.Should().NotBeNull();
}
```

### Performance Tests

- Concurrent request handling (100+ simultaneous requests)
- Large message handling (>1MB payloads)
- Long-running connection stability
- Memory leak detection

## Security Considerations

1. **Input Validation**: Validate all incoming JSON against expected schemas
2. **Process Isolation**: Run JSON-RPC processes with minimal privileges
3. **Timeout Protection**: Prevent resource exhaustion through configurable timeouts
4. **Secret Management**: Never log sensitive data from requests/responses
5. **Injection Prevention**: Sanitize all user inputs before passing to processes

## Migration Path

For existing users with manual JSON-RPC implementations:

1. **Phase 1**: Introduce new API alongside existing code
2. **Phase 2**: Provide migration guide with examples
3. **Phase 3**: Deprecate manual approach in favor of new API

## Future Enhancements

### Short Term (v1.1)
- Batch request support for improved performance
- WebSocket transport for remote servers
- Built-in retry logic with exponential backoff

### Medium Term (v2.0)
- Connection pooling for multiple clients
- Streaming response support (Server-Sent Events)
- Protocol-specific client implementations (McpClient, LspClient)

### Long Term
- gRPC support alongside JSON-RPC
- GraphQL subscription support
- Custom protocol plugins

## Dependencies

### Required
- **StreamJsonRpc** (6.x): Core JSON-RPC implementation
- **System.Text.Json** (10.x): JSON serialization
- **Microsoft.Extensions.Logging**: Diagnostic logging

### Optional
- **Polly**: Resilience and retry policies
- **System.Threading.Channels**: Advanced queue management

## Performance Targets

| Metric | Target | Current |
|--------|--------|---------|
| Request Latency | < 10ms | ~5ms |
| Concurrent Requests | 100+ | Untested |
| Memory per Client | < 10MB | ~8MB |
| Startup Time | < 100ms | ~50ms |

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| StreamJsonRpc bugs | High | Stay on stable versions, monitor issues |
| Protocol incompatibilities | Medium | Extensive integration testing |
| Performance degradation | Medium | Continuous benchmarking |
| Breaking API changes | Low | Semantic versioning, deprecation notices |

## Decision Log

| Date | Decision | Rationale |
|------|----------|-----------|
| 2025-01-24 | Use StreamJsonRpc | Mature, battle-tested, reduces complexity |
| 2025-01-24 | System.Text.Json over Newtonsoft | Better performance, native to .NET |
| 2025-01-24 | Extension method pattern | Consistent with Amuru's API design |
| 2025-01-24 | IAsyncDisposable | Proper async resource cleanup |

## References

- [JSON-RPC 2.0 Specification](https://www.jsonrpc.org/specification)
- [StreamJsonRpc Documentation](https://microsoft.github.io/vs-streamjsonrpc/)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Language Server Protocol](https://microsoft.github.io/language-server-protocol/)
- [GitHub Issue #18](https://github.com/TimeWarpEngineering/timewarp-amuru/issues/18)