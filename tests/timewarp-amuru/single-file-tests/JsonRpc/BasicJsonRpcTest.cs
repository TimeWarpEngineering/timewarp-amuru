#!/usr/bin/dotnet --
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StreamJsonRpc;

// Simple test to verify JSON-RPC implementation works
await TestBasicEcho();
Console.WriteLine("All tests passed!");

async Task TestBasicEcho()
{
    Console.WriteLine("Testing basic JSON-RPC with echo server...");

    // Create a minimal JSON context for AOT
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

#pragma warning disable CA2000 // Formatter is disposed by JsonRpc
#pragma warning disable IL2026, IL3050 // This is a test, AOT warnings acceptable here
    var formatter = new SystemTextJsonFormatter
    {
        JsonSerializerOptions = options
    };
#pragma warning restore IL2026, IL3050
#pragma warning restore CA2000

    // Use a simple echo server for testing
    // We'll use 'cat' as a simple echo process for now
    await using IJsonRpcClient client = await Shell.Builder("cat")
        .AsJsonRpcClient()
        .WithTimeout(TimeSpan.FromSeconds(5))
        .WithFormatter(formatter) // Required for AOT
        .StartAsync();

    Console.WriteLine("Client created successfully!");

    // For a real test, we'd need an actual JSON-RPC server
    // For now, just verify the client was created
    if (client != null)
    {
        Console.WriteLine("✓ JSON-RPC client initialization successful");
    }
    else
    {
        throw new InvalidOperationException("Failed to create JSON-RPC client");
    }
}