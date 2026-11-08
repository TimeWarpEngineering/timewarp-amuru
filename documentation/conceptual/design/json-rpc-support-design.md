# JSON-RPC support

TimeWarp.Amuru speaks JSON-RPC 2.0 with a child process. Requests go to the child's standard input. Responses come back on standard output, one JSON object per line. The client correlates each response to the request that produced it.

The supported surface is request, response, and an outbound notification. Amuru does not generate proxies, dispatch server methods, raise inbound notification events, or multiplex streams. MCP initialize, `notifications/initialized`, and tools/list are one consumer of this surface. They do not define it.

## Entry point

`ShellBuilder.AsJsonRpcClient()` returns a `JsonRpcClientBuilder` for that command. The builder starts one child and returns a `JsonRpcClient`. Configured standard input is refused, because the client owns that stream.

```csharp
await using JsonRpcClient client = await Shell.Builder("python3")
  .WithArguments("server.py")
  .WithTimeout(TimeSpan.FromSeconds(5))
  .AsJsonRpcClient()
  .WithJsonSerializerContext(ProtocolJsonContext.Default)
  .StartAsync();

AddResult? added = await client.SendAsync<AddParams, AddResult>("add", new AddParams(2, 3));
```

`WithTimeout` is the per-request limit. It is the same `CommandOptions.Timeout` used by command execution. It does not limit the life of the child. `WithTimeoutGracePeriod` is how long shutdown waits after the terminate signal before the process is killed.

## Serializer

The builder accepts a source-generated `JsonSerializerContext` or a `JsonSerializerOptions`. When both are set, the context performs serialization. There is no formatter interface. Both paths resolve `JsonTypeInfo` and call the `JsonTypeInfo` overloads. There is no trim suppression on that path.

NativeAOT callers pass a `JsonSerializerContext`, or options whose `TypeInfoResolver` is one. Options with no resolver need reflection serialization, which NativeAOT and the file-based programs in this repository disable. `StartAsync` throws `InvalidOperationException` when neither was set. A parameter or result type missing from the metadata throws `JsonRpcException`.

## Messages

A request is a JSON-RPC 2.0 object with a string id, a method, and an optional params member. A null parameter value omits params. Ids are decimal strings allocated by the client. `NotifyAsync` writes the same object without an id and does not track an in-flight entry.

A response is either a result or an error object. `SendAsync` deserializes the result to `TResult`. An error object becomes `JsonRpcError` (code, message, and the raw JSON of data) and `SendAsync` throws `JsonRpcException`. A session refusal and a frame that cannot be read also throw `JsonRpcException`, with `Error` left null.

A notification, a server request, and an error with a null id are ignored. They are not dispatched.

## Process

The client creates three anonymous pipes and passes the child ends through `ProcessStartInfo` handle properties. The parent keeps the other ends and reads them asynchronously. CliWrap is not on this path. Standard error is drained on its own task. `WithStandardErrorObserver` receives each line. An exception from that callback is ignored so a full standard-error pipe cannot stall the child. Standard error is never merged into the JSON-RPC stream.

Where the operating system supports it, the child is marked to die when the parent dies. `DisposeAsync` closes standard input, sends SIGTERM on Unix, and then waits. The grace period or a cancelled dispose kills the child. The wait for the session to settle is that grace plus a short margin. Closing standard output ends the session: the client waits asynchronously for the child to exit, then kills a child that is still running. `JsonRpcClient.ExitStatus` is a `ProcessExitStatus` once that status is known. `Signal` is set when a signal terminated the child. `Canceled` is set when shutdown killed the child because the grace period elapsed or the caller cancelled.

## Session

An internal Ramani machine tracks the child: Idle, Starting, Ready, Stopping, Exited, and Faulted. The public client sends the machine's signals and waits on a completion that belongs to that one in-flight request. The completion is not placed on the signal. The in-flight map is a concurrent dictionary so the client can take one entry's completion while the pump accepts or releases another id. Pump effects are the only writers of an entry. After the client reads the payload it sends Release, and the effect removes the entry.

A request timeout and caller cancellation both complete that entry. A response that arrives in the same moment is the result the caller receives. Otherwise caller cancellation throws `OperationCanceledException` and the timeout throws `TimeoutException`.

## Package graph

The implementation uses `System.Text.Json` only. The package does not reference StreamJsonRpc or Newtonsoft.Json.
