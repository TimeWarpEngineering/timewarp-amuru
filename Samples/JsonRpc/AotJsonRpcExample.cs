#!/usr/bin/dotnet run
#:project ../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:package StreamJsonRpc

using TimeWarp.Amuru;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StreamJsonRpc;

// Example: Using JSON-RPC with AOT compilation
// TimeWarp.Amuru requires a formatter for AOT compatibility

Console.WriteLine("JSON-RPC AOT Example");
Console.WriteLine("====================\n");

// Step 1: Create JsonSerializerOptions for your protocol
// For a real application, use JsonSourceGenerationOptions and JsonSerializable attributes
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
};

// Step 2: Create SystemTextJsonFormatter (AOT-compatible)
// Note: For full AOT, you would use a JsonSerializerContext:
// var formatter = new SystemTextJsonFormatter(MyJsonContext.Default);
#pragma warning disable CA2000 // Formatter is disposed by JsonRpc
#pragma warning disable IL2026, IL3050 // Example shows non-AOT for simplicity
var formatter = new SystemTextJsonFormatter
{
    JsonSerializerOptions = jsonOptions
};
#pragma warning restore IL2026, IL3050
#pragma warning restore CA2000

// Step 3: Create the JSON-RPC client with required formatter
await using IJsonRpcClient client = await Shell.Builder("your-jsonrpc-server")
    .AsJsonRpcClient()
    .WithFormatter(formatter) // REQUIRED for AOT
    .WithTimeout(TimeSpan.FromSeconds(30))
    .StartAsync();

Console.WriteLine("✓ JSON-RPC client created with AOT-compatible formatter");

// Step 4: Use the client
// var response = await client.SendRequestAsync<MyResponseType>("method", parameters);

/*
For production AOT usage, create a JsonSerializerContext:

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(InitializeRequest))]
[JsonSerializable(typeof(InitializeResponse))]
[JsonSerializable(typeof(Dictionary<string, object>))]
public partial class MyProtocolJsonContext : JsonSerializerContext
{
}

Then use it:
var formatter = new SystemTextJsonFormatter(MyProtocolJsonContext.Default);
*/

Console.WriteLine("\n📝 Key Points:");
Console.WriteLine("- Formatter is REQUIRED (no default for AOT compatibility)");
Console.WriteLine("- Use SystemTextJsonFormatter, not JsonMessageFormatter");
Console.WriteLine("- For full AOT, use JsonSerializerContext with source generation");
Console.WriteLine("- Newtonsoft.Json is not supported (not AOT-compatible)");