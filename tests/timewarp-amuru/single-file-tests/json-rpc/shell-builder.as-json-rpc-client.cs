#!/usr/bin/dotnet --

#region Purpose
// Tests for ShellBuilder.AsJsonRpcClient() - validates JSON-RPC client creation from shell builder
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: ShellBuilder (the builder class)
// Action: AsJsonRpcClient (the method to create JSON-RPC client)
// Tests verify JSON-RPC client can be created and started
#endregion

using System.Text.Json;
using StreamJsonRpc;

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("JsonRpc")]
  public class AsJsonRpcClient_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<AsJsonRpcClient_Given_>();

    public static async Task Default_Should_CreateClientSuccessfully()
    {
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

      // Use 'cat' as a simple echo process for testing
      await using IJsonRpcClient client = await Shell.Builder("cat")
        .AsJsonRpcClient()
        .WithTimeout(TimeSpan.FromSeconds(5))
        .WithFormatter(formatter)
        .StartAsync();

      client.ShouldNotBeNull("JSON-RPC client should be created successfully");
    }
  }
}
