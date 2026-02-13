#!/usr/bin/dotnet --
#:package ModelContextProtocol.Core

#region Purpose
// Tests for IJsonRpcClient MCP initialize - validates MCP server handshake
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: JsonRpcClient (the JSON-RPC client interface)
// Action: McpInitialize (sending MCP initialize request)
// Tests verify MCP protocol handshake works correctly
#endregion

using System.Text.Json;
using StreamJsonRpc;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace JsonRpcClient_
{
  [TestTag("JsonRpc")]
  public class McpInitialize_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<McpInitialize_Given_>();

    public static async Task ValidServer_Should_ReturnServerInfo()
    {
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
        .WithFormatter(formatter)
        .StartAsync();

      client.ShouldNotBeNull("MCP server should start");

      // Give the MCP server time to fully start
      await Task.Delay(2000);

      // Send initialize request - use strongly-typed classes for AOT
      var initializeRequestParams = new InitializeRequestParams
      {
        ProtocolVersion = "2025-06-18",
        ClientInfo = new Implementation
        {
          Name = "test-client",
          Version = "1.0.0"
        }
      };

      // Use a timeout to ensure we don't hang forever
      using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
      InitializeResult? response = await client.SendRequestAsync<InitializeResult>(
        "initialize",
        initializeRequestParams,
        cts.Token
      );

      response.ShouldNotBeNull("Initialize response should not be null");
      response.ProtocolVersion.ShouldNotBeNullOrEmpty("Protocol version should be returned");
      response.ServerInfo.ShouldNotBeNull("Server info should be returned");
    }
  }
}
