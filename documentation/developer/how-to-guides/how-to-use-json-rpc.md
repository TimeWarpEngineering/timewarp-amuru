# How to call a JSON-RPC child

## Overview

`Shell.Builder(...).AsJsonRpcClient()` starts a child process and speaks JSON-RPC 2.0 on its standard input and standard output. One request gets one correlated response. Standard error stays off that stream.

## Prerequisites

- .NET 11 SDK
- TimeWarp.Amuru package
- A child that reads one JSON object per line and writes one JSON object per line

## Basic usage

Pass a source-generated `JsonSerializerContext`. That path works when the host disables reflection serialization, including NativeAOT and the file-based programs in this repository.

```csharp
#!/usr/bin/dotnet --
#:package TimeWarp.Amuru

using System.Text.Json.Serialization;
using TimeWarp.Amuru;

await using JsonRpcClient client = await Shell.Builder("python3")
  .WithArguments("server.py")
  .WithTimeout(TimeSpan.FromSeconds(5))
  .AsJsonRpcClient()
  .WithJsonSerializerContext(ProtocolJsonContext.Default)
  .StartAsync();

AddResult? added = await client.SendAsync<AddParams, AddResult>("add", new AddParams(2, 3));
ToolsListResult? tools = await client.SendAsync<ToolsListResult>("tools/list");

sealed record AddParams(int Left, int Right);
sealed record AddResult(int Sum);
sealed record ToolsListResult(ToolInfo[] Tools);
sealed record ToolInfo(string Name);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(AddParams))]
[JsonSerializable(typeof(AddResult))]
[JsonSerializable(typeof(ToolsListResult))]
[JsonSerializable(typeof(ToolInfo))]
partial class ProtocolJsonContext : JsonSerializerContext
{
}
```

`WithTimeout` limits each request. Dispose stops the child. `WithTimeoutGracePeriod` is the time shutdown waits after the terminate signal before the process is killed. Dispose waits for the session to settle, and that wait is the grace period plus a short margin.

Closing the child's standard output ends the session. The client waits briefly for the process to exit, then kills a child that is still running.

`samples/json-rpc/aot-json-rpc-example.cs` is a file-based program in this shape. Build it, or publish it with `PublishAot=true`. Pass `run` to start the repository fixture. A second argument is the fixture path when the process has no entry-point directory, which is the published binary.

## Reflection serializer

A host can pass `JsonSerializerOptions` instead of a context. The client resolves metadata with `JsonSerializerOptions.GetTypeInfo` and serializes through that `JsonTypeInfo`. When both are set, the context performs serialization. NativeAOT and the file-based programs in this repository disable reflection, so options need a `TypeInfoResolver` (often the same source-generated context). A missing parameter or result type throws `JsonRpcException`.

```csharp
JsonSerializerOptions options = new()
{
  PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

await using JsonRpcClient client = await Shell.Builder("python3")
  .WithArguments("server.py")
  .AsJsonRpcClient()
  .WithJsonSerializerOptions(options)
  .StartAsync();
```

## Errors, timeout, and cancellation

| What happened | What the caller sees |
| --- | --- |
| The response has an error object | `JsonRpcException` with `Error` set to the code, message, and raw data JSON |
| The session refused the request, the serializer has no metadata for the type, or the result could not be read | `JsonRpcException` with `Error` null |
| `WithTimeout` elapsed | `TimeoutException` |
| The caller's `CancellationToken` fired | `OperationCanceledException` |

A notification from the child is ignored. The next response for the outstanding request is still delivered.

## Outbound notifications

`NotifyAsync` writes one frame with no id and does not wait for a response. MCP clients send `notifications/initialized` after `initialize` and before `tools/list`.

```csharp
InitializeResult? initialize = await client.SendAsync<InitializeParams, InitializeResult>(
  "initialize",
  new InitializeParams("2025-06-18", new ClientCapabilities(), new ClientInfo("amuru", "1"))
);
await client.NotifyAsync("notifications/initialized");
ToolsListResult? tools = await client.SendAsync<ToolsListResult>("tools/list");
```

## Standard error

```csharp
await using JsonRpcClient client = await Shell.Builder("python3")
  .WithArguments("server.py")
  .AsJsonRpcClient()
  .WithJsonSerializerContext(ProtocolJsonContext.Default)
  .WithStandardErrorObserver(line => Console.Error.WriteLine(line))
  .StartAsync();
```

The callback runs on the reader's task and must return. An exception from the callback is ignored.

## Limits

The client does not generate a proxy, dispatch methods the child calls, or deliver events. `WithStandardInput` cannot be combined with `AsJsonRpcClient`.
