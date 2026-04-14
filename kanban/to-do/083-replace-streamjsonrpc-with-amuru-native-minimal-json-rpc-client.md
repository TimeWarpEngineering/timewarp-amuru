# Replace StreamJsonRpc with Amuru-native minimal JSON-RPC client

## Description

Replace the `StreamJsonRpc` dependency in TimeWarp.Amuru with a small Amuru-native JSON-RPC client implementation written fresh to match this repository's conventions and avoid the transitive `Newtonsoft.Json` dependency that poisons NativeAOT consumers.

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
- [ ] Replace `JsonRpcClient` internals to remove `StreamJsonRpc` usage
- [ ] Replace `JsonRpcClientBuilder` formatter dependency with Amuru-native configuration if needed
- [ ] Decide whether `IJsonRpcMessageFormatter` remains part of the public API or is replaced with Amuru abstractions
- [ ] Remove `StreamJsonRpc` package reference from Amuru if no longer needed
- [ ] Verify downstream AOT consumers no longer inherit `Newtonsoft.Json` through Amuru
- [ ] Add/update tests for generic JSON-RPC request/response behavior
- [ ] Add/update MCP sample/tests to validate compatibility with real MCP servers
- [ ] Update documentation and examples to reflect the new JSON-RPC implementation

## Non-Goals

- [ ] Do not reimplement full StreamJsonRpc feature parity
- [ ] Do not add proxy generation, local target dispatch, events, marshalable objects, or multiplexing unless Amuru actually needs them
- [ ] Do not copy source wholesale from `vs-streamjsonrpc`; reimplement a minimal subset inspired by required behavior only

## Session

- Created: ses_27dd18c7effe1K4rnFRhnQezjn (2026-04-14)

## Notes

### Why this task exists

`StreamJsonRpc` still carries a direct `Newtonsoft.Json` package dependency even in current releases and prereleases. This creates NativeAOT/trimming pain for downstream packages that depend on Amuru, even when Amuru itself uses `SystemTextJsonFormatter` or avoids the Newtonsoft formatter path.

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

### Public API question

Today `JsonRpcClientBuilder` accepts a `StreamJsonRpc.IJsonRpcMessageFormatter`. That is a leaky external dependency in the Amuru public API. This task should explicitly decide whether:

1. the formatter concept remains, but becomes an Amuru abstraction; or
2. the builder instead accepts `JsonSerializerOptions`, `JsonSerializerContext`, or another Amuru-native configuration model.

### Success criteria

- Amuru no longer depends on `StreamJsonRpc`
- Amuru no longer pulls `Newtonsoft.Json` into downstream package graphs through JSON-RPC support
- Existing JSON-RPC scenarios continue to work for generic process-based request/response use cases
- MCP sample/test scenarios still work as validation, but do not define the entire feature surface
