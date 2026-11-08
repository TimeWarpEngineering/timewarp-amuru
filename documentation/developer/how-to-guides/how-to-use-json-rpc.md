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

`WithTimeout` limits each request. Dispose stops the child. `WithTimeoutGracePeriod` is the time shutdown waits after the terminate signal before the process is killed.

`samples/json-rpc/aot-json-rpc-example.cs` is a file-based program in this shape. Build it, or publish it with `PublishAot=true`. Pass `run` to start the repository fixture. A second argument is the fixture path when the process has no entry-point directory, which is the published binary.

## Reflection serializer

A host that still allows reflection serialization can pass `JsonSerializerOptions` instead of a context. When both are set, the context performs serialization. A host that has disabled reflection serialization throws `InvalidOperationException` from `System.Text.Json` if the options path is used without a `TypeInfoResolver`.

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
| The session refused the request, or the result could not be read | `JsonRpcException` with `Error` null |
| `WithTimeout` elapsed | `TimeoutException` |
| The caller's `CancellationToken` fired | `OperationCanceledException` |

A notification from the child is ignored. The next response for the outstanding request is still delivered.

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
