# Implement JSON-RPC Support for Interactive Process Communication

**GitHub Issue:** #18
**Type:** Library Feature
**Priority:** High
**Estimated Effort:** Large

## User Story

As a developer using Amuru, I need first-class JSON-RPC support so I can communicate with interactive processes like MCP servers, Language Server Protocol implementations, and custom JSON-RPC services without writing 200+ lines of boilerplate code for stream management, request correlation, and error handling.

## Background

Currently, working with processes that use JSON-RPC for communication requires verbose Process.Start() code with manual stream management, timeout handling, and request/response correlation. This defeats Amuru's core mission of eliminating process execution verbosity.

## Requirements

### Core API
- Fluent builder pattern to convert Shell commands to JSON-RPC clients
- Type-safe request/response handling with generic return types
- Support for both requests (with responses) and notifications (fire-and-forget)
- Event handling for unsolicited server notifications
- Proper lifecycle management with IAsyncDisposable

### Implementation Details
- Automatic request ID generation and correlation
- Support for concurrent requests with proper queuing
- Handle both newline-delimited JSON and Content-Length header formats
- Configurable timeouts per request
- Graceful shutdown with fallback to force kill

### Testing Support
- MockJsonRpcClient for unit testing
- Integration tests with real MCP server

## Acceptance Criteria

- [ ] Can establish JSON-RPC communication with 10-20 lines of code (vs current 200+)
- [ ] Supports MCP server protocol (initialize, tools/list, tools/call)
- [ ] Supports Language Server Protocol communication patterns
- [ ] Handles concurrent requests with proper correlation
- [ ] Provides type-safe responses with JsonRpcResponse<T>
- [ ] Gracefully handles timeouts, malformed JSON, and process crashes
- [ ] Includes comprehensive unit and integration tests
- [ ] Documentation with real-world examples for MCP, LSP, and custom protocols

## Definition of Ready

### Library Feature
- [x] Requirements and acceptance criteria defined clearly
- [x] Impact on existing API surface understood (extends Shell.Builder with AsJsonRpcClient())
- [x] Backward compatibility considered (purely additive, no breaking changes)
- [x] Dependencies identified and available (System.Text.Json, existing Amuru infrastructure)

## Definition of Done

### Library Feature

**Implementation:**
- [ ] *Core functionality implemented (required)
  - [ ] IJsonRpcClient interface
  - [ ] JsonRpcClient implementation
  - [ ] JsonRpcClientBuilder
  - [ ] ShellBuilderJsonRpcExtensions
- [ ] *Public API additions/changes (required if applicable)
  - [ ] AsJsonRpcClient() extension method
  - [ ] JsonRpcResponse and JsonRpcResponse<T> types
  - [ ] JsonRpcError type
- [ ] Configuration options added
  - [ ] Timeout configuration
  - [ ] Encoding configuration
  - [ ] JsonSerializerOptions configuration
- [ ] Error handling implemented
  - [ ] Process crash detection
  - [ ] Timeout handling
  - [ ] Malformed JSON handling
  - [ ] Connection state management
- [ ] *Backward compatibility maintained (required)

**Testing:**
- [ ] *Integration tests added/updated (required)
  - [ ] MCP server communication tests
  - [ ] Concurrent request handling tests
  - [ ] Error scenario tests
- [ ] Edge case scenarios tested
  - [ ] Partial message handling
  - [ ] Large message handling
  - [ ] Process termination during request
- [ ] Performance impact validated
- [ ] Cross-platform compatibility verified

**Documentation:**
- [ ] *API documentation updated (required for public changes)
- [ ] Usage examples provided
  - [ ] MCP server example
  - [ ] LSP example
  - [ ] Custom protocol example
- [ ] Architectural decisions documented

## Implementation Notes

### Proposed API Example
```csharp
var client = await Shell.Builder("dotnet")
    .WithArguments("run", "--project", mcpPath)
    .AsJsonRpcClient()
    .StartAsync();

var initResponse = await client.SendRequestAsync("initialize", new {
    protocolVersion = "1.0.0",
    clientInfo = new { name = "test-client", version = "1.0.0" }
});

var toolsResponse = await client.SendRequestAsync("tools/list");
```

### Key Design Decisions
1. Consider using StreamJsonRpc library vs custom implementation
   - StreamJsonRpc is mature, battle-tested, and handles many edge cases
   - Would reduce implementation complexity significantly
   - Already supports stdio, pipes, and other transports
2. Use System.Text.Json for serialization (or leverage StreamJsonRpc's built-in)
3. Support both typed and dynamic responses
4. Implement as extension to existing Shell.Builder pattern
5. Handle request correlation with thread-safe dictionary (or leverage StreamJsonRpc)
6. Use channels or queues for concurrent request management

### Future Enhancements
- Streaming response support for progressive results
- Batch request support for multiple operations
- Connection pooling for multiple client instances

## References
- [GitHub Issue #18](https://github.com/TimeWarpEngineering/timewarp-amuru/issues/18)
- [JSON-RPC 2.0 Specification](https://www.jsonrpc.org/specification)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Language Server Protocol](https://microsoft.github.io/language-server-protocol/)
- [StreamJsonRpc Library](https://microsoft.github.io/vs-streamjsonrpc/) - Microsoft's mature JSON-RPC implementation that could be leveraged
- [Grok Implementation Discussion](https://grok.com/share/bGVnYWN5LWNvcHk%3D_aefc1cb0-0f87-4fcf-9f03-5e21d9c5230e) - Analysis and implementation approaches