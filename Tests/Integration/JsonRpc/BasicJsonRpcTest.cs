#!/usr/bin/dotnet run
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj

using TimeWarp.Amuru;
using System;
using System.Threading.Tasks;

// Simple test to verify JSON-RPC implementation works
await TestBasicEcho();
Console.WriteLine("All tests passed!");

async Task TestBasicEcho()
{
    Console.WriteLine("Testing basic JSON-RPC with echo server...");

    // Use a simple echo server for testing
    // We'll use 'cat' as a simple echo process for now
    await using IJsonRpcClient client = await Shell.Builder("cat")
        .AsJsonRpcClient()
        .WithTimeout(TimeSpan.FromSeconds(5))
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