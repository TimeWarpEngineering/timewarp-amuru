#!/usr/bin/dotnet --
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:package ModelContextProtocol.Core

using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using StreamJsonRpc;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using static System.Console;

WriteLine("Testing MCP Server Integration...");

try
{
  await TestMcpInitialize();
  WriteLine("✅ All MCP tests passed!");
}
catch (Exception ex)
{
  WriteLine($"❌ Test failed: {ex.Message}");
  WriteLine(ex.StackTrace);
  Environment.Exit(1);
}

static async Task TestMcpInitialize()
{
  WriteLine("\n🧪 Testing MCP Initialize...");

  // Configure SystemTextJsonFormatter for AOT
  // MCP SDK already has everything configured in McpJsonUtilities.DefaultOptions
#pragma warning disable CA2000 // Formatter is disposed by JsonRpc
#pragma warning disable IL2026, IL3050 // Suppress AOT warnings for testing
  var formatter = new SystemTextJsonFormatter
  {
    JsonSerializerOptions = McpJsonUtilities.DefaultOptions
  };
#pragma warning restore IL2026, IL3050
#pragma warning restore CA2000

  // Use SystemTextJsonFormatter for AOT support
  await using IJsonRpcClient client = await Shell.Builder("dnx")
      .WithArguments("TimeWarp.Nuru.Mcp@2.1.0-beta.11", "--yes")
      .AsJsonRpcClient()
      .WithTimeout(TimeSpan.FromSeconds(10))
      .WithFormatter(formatter) // Using SystemTextJsonFormatter
      .StartAsync();

  WriteLine("✓ MCP server started");

  // Give the MCP server time to fully start
  await Task.Delay(2000);
  WriteLine("✓ Waited for server startup");

  // Send initialize request - use strongly-typed classes for AOT
  InitializeRequestParams initializeRequestParams = new()
  {
    ProtocolVersion = "2025-06-18",
    ClientInfo = new Implementation
    {
      Name = "test-client",
      Version = "1.0.0"
    }
  };

  // The formatter will handle serialization with the MCP SDK options

  // Get response as dynamic object to work around System.Text.Json limitations
  WriteLine("\nSending initialize request...");

  // // TEMP: Check what type we actually get back
  // object? tempResponse = null;
  // try
  // {
  //   tempResponse = await client.SendRequestAsync<object>("initialize", initializeRequestParams);
  //   Console.WriteLine($"TEMP: Got response type: {tempResponse?.GetType()}");
  //   Console.WriteLine($"TEMP: Response value: {tempResponse}");
  // }
  // catch (Exception ex)
  // {
  //   Console.WriteLine($"TEMP: Request failed: {ex.GetType().Name}: {ex.Message}");
  // }

  // Try receiving as the expected InitializeResult type
  try
  {
    // Use a timeout to ensure we don't hang forever
    using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
    InitializeResult? response = await client.SendRequestAsync<InitializeResult>("initialize", initializeRequestParams, cts.Token);
    WriteLine($"✓ Got InitializeResult response");
    if (response != null)
    {
      WriteLine($"  Protocol version: {response.ProtocolVersion}");
      WriteLine($"  Server: {response.ServerInfo?.Name} v{response.ServerInfo?.Version}");
    }

    return; // Success!

  }
  catch (TimeoutException)
  {
    WriteLine("❌ Request timed out - the server might not be receiving our messages");
  }
  catch (Exception ex)
  {
    WriteLine($"❌ Request failed: {ex.GetType().Name}: {ex.Message}");
    if (ex.InnerException != null)
    {
      WriteLine($"  Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
    }
  }
}

// No longer need our own context - using McpJsonUtilities.DefaultOptions from the SDK
