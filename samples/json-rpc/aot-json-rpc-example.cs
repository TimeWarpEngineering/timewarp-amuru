#!/usr/bin/env -S dotnet --
#:project $(SourceDirectory)timewarp-amuru/timewarp-amuru.csproj
#:property LangVersion=preview
#:property EnablePreviewFeatures=true
#:property IsAotCompatible=true

// JSON-RPC 2.0 over a child process, with a source-generated serializer.
// Pass "run" to start the Python fixture. A build or NativeAOT publish does not start a process.

using System.Globalization;
using System.Text.Json.Serialization;
using TimeWarp.Amuru;
using TimeWarp.Amuru.Samples.JsonRpc;
using TimeWarp.Terminal;

string? scriptDirectory = AppContext.EntryPointFileDirectoryPath();
string fixture = scriptDirectory is null
  ? "json-rpc-fixture.py"
  : Path.GetFullPath(Path.Combine(
      scriptDirectory,
      "..",
      "..",
      "tests",
      "timewarp-amuru",
      "single-file-tests",
      "json-rpc",
      "json-rpc-fixture.py"
    ));
if (args.Length > 1)
{
  fixture = args[1];
}

JsonRpcClientBuilder builder = Shell.Builder("python3")
  .WithArguments(fixture)
  .WithTimeout(TimeSpan.FromSeconds(5))
  .AsJsonRpcClient()
  .WithJsonSerializerContext(FixtureJsonContext.Default);

if (args.Length > 0 && args[0] == "run")
{
  if (!File.Exists(fixture))
  {
    await TimeWarpTerminal.Default.WriteLineAsync("fixture not found: " + fixture);
    return 1;
  }

  await using JsonRpcClient client = await builder.StartAsync();
  AddResult? added = await client.SendAsync<AddParams, AddResult>("add", new AddParams(2, 3));
  await TimeWarpTerminal.Default.WriteLineAsync((added?.Sum ?? 0).ToString(CultureInfo.InvariantCulture));
  return 0;
}

await TimeWarpTerminal.Default.WriteLineAsync("JSON-RPC client builder is ready.");
await TimeWarpTerminal.Default.WriteLineAsync(builder.GetType().Name);
return 0;

namespace TimeWarp.Amuru.Samples.JsonRpc
{
  /// <summary>Parameters for the fixture add method.</summary>
  internal sealed record AddParams(int Left, int Right);

  /// <summary>Result of the fixture add method.</summary>
  internal sealed record AddResult(int Sum);

  /// <summary>Source-generated serializer for the sample types.</summary>
  [JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
  )]
  [JsonSerializable(typeof(AddParams))]
  [JsonSerializable(typeof(AddResult))]
  internal sealed partial class FixtureJsonContext : JsonSerializerContext
  {
  }
}
