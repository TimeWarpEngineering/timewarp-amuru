#!/usr/bin/env -S dotnet --
#:property GeneratePackageOnBuild=false
#:package Microsoft.Extensions.Hosting
#:package ModelContextProtocol

#region Purpose
// Stdio MCP server built on the official C# SDK, used to validate JsonRpcClient.
#endregion

#region Design
// Logs go to standard error. Standard output is the MCP stream and stays free of host text.
// This directory has its own Directory.Build.props so the SDK package stays out of Amuru.
// The package versions are central so the repository audit can see them.
#endregion

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole(consoleLogOptions =>
{
  consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});
builder.Services
  .AddMcpServer()
  .WithStdioServerTransport()
  .WithToolsFromAssembly();

await builder.Build().RunAsync();

/// <summary>
/// One tool the JSON-RPC client test lists.
/// </summary>
[McpServerToolType]
public static class AmuruMcpProbe
{
  /// <summary>
  /// Adds two integers.
  /// </summary>
  /// <param name="left">The first integer</param>
  /// <param name="right">The second integer</param>
  /// <returns>The sum</returns>
  [McpServerTool(Name = "add")]
  [Description("Adds two integers.")]
  public static int Add(int left, int right) => left + right;
}
