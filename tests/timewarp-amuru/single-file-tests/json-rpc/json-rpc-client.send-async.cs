#!/usr/bin/env -S dotnet --

#region Purpose
// Drives JsonRpcClient against a Python child that speaks JSON-RPC on stdin and stdout.
#endregion

#region Design
// The fixture is a file in this folder. Git.FindRoot locates it. Tests do not copy it.
// File-based test hosts disable reflection serialization, so the tests pass a source-generated context.
// CommandOptions.Timeout is the per-request limit. TimeoutGracePeriod is only process shutdown.
// ExitStatus can land after SendAsync returns, because the read loop observes stdout EOF
// on its own turn. Tests poll for it.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace JsonRpcClient_
{
  using System.Collections.Concurrent;
  using System.Runtime.InteropServices;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  [TestTag("JsonRpc")]
  public class SendAsync_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<SendAsync_Given_>();

    public static async Task Add_Should_ReturnTheSum()
    {
      await using JsonRpcClient client = await StartAsync();

      AddResult? result = await client.SendAsync<AddParams, AddResult>("add", new AddParams(2, 3));

      result.ShouldNotBeNull();
      result.Sum.ShouldBe(5);
    }

    public static async Task NoParameters_Should_ListTools()
    {
      await using JsonRpcClient client = await StartAsync();

      ToolsListResult? result = await client.SendAsync<ToolsListResult>("tools/list");

      result.ShouldNotBeNull();
      result.Tools.Length.ShouldBe(1);
      result.Tools[0].Name.ShouldBe("add");
    }

    public static async Task ErrorResponse_Should_ExposeTheJsonRpcError()
    {
      await using JsonRpcClient client = await StartAsync();

      JsonRpcException exception = await Should.ThrowAsync<JsonRpcException>(
        async () => await client.SendAsync<OkResult>("fail")
      );

      exception.Error.ShouldNotBeNull();
      exception.Error.Code.ShouldBe(-32000);
      exception.Error.Message.ShouldBe("nope");
      exception.Error.DataJson.ShouldNotBeNull();
      exception.Error.DataJson.ShouldContain("false");
    }

    public static async Task StandardError_Should_StayOffTheResult()
    {
      ConcurrentBag<string> errors = new();
      await using JsonRpcClient client = await StartAsync(standardError: errors.Add);

      AddResult? result = await client.SendAsync<AddParams, AddResult>("add", new AddParams(1, 1));

      result.ShouldNotBeNull();
      result.Sum.ShouldBe(2);
      await WaitUntilAsync(() => errors.Contains("stderr:add"));
      errors.ShouldContain("stderr:add");
      errors.ShouldNotContain(line => line.Contains("sum", StringComparison.Ordinal));
    }

    public static async Task InitializeAndToolsList_Should_MatchTheMcpShapes()
    {
      await using JsonRpcClient client = await StartAsync();

      InitializeResult? initialize = await client.SendAsync<InitializeResult>("initialize");
      await client.NotifyAsync("notifications/initialized");
      ToolsListResult? tools = await client.SendAsync<ToolsListResult>("tools/list");

      initialize.ShouldNotBeNull();
      initialize.ProtocolVersion.ShouldBe("2024-11-05");
      initialize.ServerInfo.Name.ShouldBe("amuru-json-rpc-fixture");
      tools.ShouldNotBeNull();
      tools.Tools[0].Name.ShouldBe("add");
    }

    public static async Task ConcurrentAdds_Should_CorrelateById()
    {
      await using JsonRpcClient client = await StartAsync();

      Task<AddResult?> left = client.SendAsync<AddParams, AddResult>("add", new AddParams(1, 2));
      Task<AddResult?> right = client.SendAsync<AddParams, AddResult>("add", new AddParams(10, 5));
      AddResult?[] results = await Task.WhenAll(left, right);

      AddResult leftResult = results[0].ShouldNotBeNull();
      AddResult rightResult = results[1].ShouldNotBeNull();
      leftResult.Sum.ShouldBe(3);
      rightResult.Sum.ShouldBe(15);
    }

    public static async Task RequestTimeout_Should_ThrowTimeoutException()
    {
      await using JsonRpcClient client = await StartAsync(
        timeout: TimeSpan.FromMilliseconds(300),
        grace: TimeSpan.FromSeconds(2)
      );

      TimeoutException exception = await Should.ThrowAsync<TimeoutException>(
        async () => await client.SendAsync<OkResult>("sleep")
      );

      exception.Message.ShouldContain("sleep");
    }

    public static async Task CallerCancellation_Should_ThrowOperationCanceledException()
    {
      await using JsonRpcClient client = await StartAsync(
        timeout: TimeSpan.FromSeconds(10),
        grace: TimeSpan.FromSeconds(2)
      );
      using CancellationTokenSource source = new(TimeSpan.FromMilliseconds(200));

      await Should.ThrowAsync<OperationCanceledException>(
        async () => await client.SendAsync<OkResult>("sleep", source.Token)
      );
    }

    public static async Task Dispose_Should_RecordExitStatus()
    {
      JsonRpcClient client = await StartAsync(grace: TimeSpan.FromSeconds(2));

      await client.DisposeAsync();

      ProcessExitStatus? status = await WaitForStatusAsync(client);
      status.ShouldNotBeNull();
    }

    public static async Task ChildExit_Should_RecordTheExitCode()
    {
      await using JsonRpcClient client = await StartAsync(grace: TimeSpan.FromSeconds(2));

      OkResult? result = await client.SendAsync<OkResult>("exit");

      result.ShouldNotBeNull();
      result.Ok.ShouldBeTrue();
      ProcessExitStatus? status = await WaitForStatusAsync(client);
      status.ShouldNotBeNull();
      status.ExitCode.ShouldBe(3);
      status.Signal.ShouldBeNull();
    }

    public static async Task ChildSignal_Should_FailTheRequest()
    {
      await using JsonRpcClient client = await StartAsync(grace: TimeSpan.FromSeconds(2));

      JsonRpcException exception = await Should.ThrowAsync<JsonRpcException>(
        async () => await client.SendAsync<OkResult>("crash")
      );

      exception.Message.ShouldContain("exited");
      ProcessExitStatus status = (await WaitForStatusAsync(client)).ShouldNotBeNull();
      if (!OperatingSystem.IsWindows())
      {
        status.Signal.ShouldBe(PosixSignal.SIGKILL);
      }
    }

    public static async Task MissingExecutable_Should_ThrowJsonRpcException()
    {
      JsonRpcClientBuilder builder = Shell.Builder("amuru-json-rpc-missing-083")
        .AsJsonRpcClient()
        .WithJsonSerializerContext(FixtureJsonContext.Default);

      await Should.ThrowAsync<JsonRpcException>(async () => await builder.StartAsync());
    }

    public static async Task StartWithoutSerializer_Should_ThrowInvalidOperationException()
    {
      JsonRpcClientBuilder builder = Shell.Builder("python3").WithArguments(FixturePath()).AsJsonRpcClient();

      await Should.ThrowAsync<InvalidOperationException>(async () => await builder.StartAsync());
    }

    public static async Task SerializerOptions_Should_ReturnTheSum()
    {
      JsonSerializerOptions options = new()
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = FixtureJsonContext.Default
      };
      await using JsonRpcClient client = await Shell.Builder("python3")
        .WithArguments(FixturePath())
        .AsJsonRpcClient()
        .WithJsonSerializerOptions(options)
        .StartAsync();

      AddResult? result = await client.SendAsync<AddParams, AddResult>("add", new AddParams(2, 3));

      result.ShouldNotBeNull();
      result.Sum.ShouldBe(5);
    }

    public static async Task MissingSerializerMetadata_Should_ThrowJsonRpcException()
    {
      await using JsonRpcClient client = await StartAsync();

      JsonRpcException exception = await Should.ThrowAsync<JsonRpcException>(
        async () => await client.SendAsync<UnknownParams, AddResult>("add", new UnknownParams("x"))
      );

      exception.InnerException.ShouldBeOfType<NotSupportedException>();
    }

    public static async Task MalformedStdout_Should_FaultTheSession()
    {
      await using JsonRpcClient client = await StartAsync();

      JsonRpcException exception = await Should.ThrowAsync<JsonRpcException>(
        async () => await client.SendAsync<OkResult>("garbage")
      );

      exception.Message.ShouldContain("not a response");
    }

    public static async Task SendAfterDispose_Should_ThrowObjectDisposedException()
    {
      JsonRpcClient client = await StartAsync();
      await client.DisposeAsync();

      await Should.ThrowAsync<ObjectDisposedException>(
        async () => await client.SendAsync<OkResult>("add")
      );
    }

    public static async Task ClosedStdout_Should_EndTheSession()
    {
      await using JsonRpcClient client = await StartAsync(grace: TimeSpan.FromSeconds(2));

      OkResult? result = await client.SendAsync<OkResult>("close-stdout");

      result.ShouldNotBeNull();
      result.Ok.ShouldBeTrue();
      ProcessExitStatus status = (await WaitForStatusAsync(client)).ShouldNotBeNull();
      if (!OperatingSystem.IsWindows())
      {
        status.Signal.ShouldBe(PosixSignal.SIGKILL);
      }
    }

    public static async Task RealMcpServer_Should_InitializeAndListTools()
    {
      string server = await McpServerPathAsync();
      await using JsonRpcClient client = await Shell.Builder(server)
        .WithTimeout(TimeSpan.FromSeconds(20))
        .AsJsonRpcClient()
        .WithJsonSerializerContext(FixtureJsonContext.Default)
        .StartAsync();

      InitializeResult? initialize = await client.SendAsync<McpInitializeParams, InitializeResult>(
        "initialize",
        new McpInitializeParams(
          "2025-06-18",
          new McpClientCapabilities(),
          new McpClientInfo("amuru-json-rpc-test", "1")
        )
      );
      await client.NotifyAsync("notifications/initialized");
      ToolsListResult? tools = await client.SendAsync<ToolsListResult>("tools/list");

      initialize.ShouldNotBeNull();
      initialize.ProtocolVersion.ShouldBe("2025-06-18");
      initialize.ServerInfo.Name.ShouldBe("mcp-stdio-server");
      tools.ShouldNotBeNull();
      tools.Tools.ShouldContain(tool => tool.Name == "add");
    }

    public static Task ConfiguredStandardInput_Should_RefuseAsJsonRpcClient()
    {
      Should.Throw<InvalidOperationException>(
        () => Shell.Builder("python3").WithStandardInput("").AsJsonRpcClient()
      );
      return Task.CompletedTask;
    }

    private static async Task<JsonRpcClient> StartAsync
    (
      TimeSpan? timeout = null,
      TimeSpan? grace = null,
      Action<string>? standardError = null
    )
    {
      ShellBuilder builder = Shell.Builder("python3").WithArguments(FixturePath());
      if (timeout is TimeSpan limit)
      {
        builder.WithTimeout(limit);
      }

      if (grace is TimeSpan period)
      {
        builder.WithTimeoutGracePeriod(period);
      }

      JsonRpcClientBuilder client = builder.AsJsonRpcClient().WithJsonSerializerContext(FixtureJsonContext.Default);
      if (standardError is not null)
      {
        client.WithStandardErrorObserver(standardError);
      }

      return await client.StartAsync();
    }

    private static string FixturePath()
    {
      return Path.Combine(JsonRpcDirectory(), "json-rpc-fixture.py");
    }

    private static async Task<ProcessExitStatus?> WaitForStatusAsync(JsonRpcClient client)
    {
      for (int attempt = 0; attempt < 50; attempt++)
      {
        if (client.ExitStatus is ProcessExitStatus status)
        {
          return status;
        }

        await Task.Delay(100);
      }

      return client.ExitStatus;
    }

    private static async Task<string> McpServerPathAsync()
    {
      string source = Path.Combine(JsonRpcDirectory(), "mcp-stdio-server", "mcp-stdio-server.cs");
      string outDir = Path.Combine(Path.GetTempPath(), "timewarp-amuru-083-mcp-server");
      string built = Path.Combine(outDir, "mcp-stdio-server");
      if (File.Exists(built) && File.GetLastWriteTimeUtc(built) >= File.GetLastWriteTimeUtc(source))
      {
        return built;
      }

      Directory.CreateDirectory(outDir);
      CommandOutput output = await Shell.Builder("dotnet")
        .WithArguments("build", source, "-o", outDir, "-c", "Release", "--nologo", "-v", "q")
        .WithNoValidation()
        .CaptureAsync();
      if (output.ExitCode != 0)
      {
        throw new InvalidOperationException(output.Combined);
      }

      return built;
    }

    private static string JsonRpcDirectory()
    {
      string? root = Git.FindRoot();
      root.ShouldNotBeNull();
      return Path.Combine(root, "tests", "timewarp-amuru", "single-file-tests", "json-rpc");
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
      for (int attempt = 0; attempt < 50 && !condition(); attempt++)
      {
        await Task.Delay(20);
      }
    }

    internal sealed record AddParams(int Left, int Right);

    internal sealed record AddResult(int Sum);

    internal sealed record OkResult(bool Ok);

    internal sealed record InitializeResult(string ProtocolVersion, ServerInfo ServerInfo);

    internal sealed record ServerInfo(string Name, string Version);

    internal sealed record ToolsListResult(ToolInfo[] Tools);

    internal sealed record ToolInfo(string Name);

    internal sealed record UnknownParams(string Value);

    internal sealed record McpInitializeParams
    (
      string ProtocolVersion,
      McpClientCapabilities Capabilities,
      McpClientInfo ClientInfo
    );

    internal sealed record McpClientCapabilities();

    internal sealed record McpClientInfo(string Name, string Version);
  }

  [JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
  )]
  [JsonSerializable(typeof(SendAsync_Given_.AddParams))]
  [JsonSerializable(typeof(SendAsync_Given_.AddResult))]
  [JsonSerializable(typeof(SendAsync_Given_.OkResult))]
  [JsonSerializable(typeof(SendAsync_Given_.InitializeResult))]
  [JsonSerializable(typeof(SendAsync_Given_.ServerInfo))]
  [JsonSerializable(typeof(SendAsync_Given_.ToolsListResult))]
  [JsonSerializable(typeof(SendAsync_Given_.ToolInfo))]
  [JsonSerializable(typeof(SendAsync_Given_.McpInitializeParams))]
  [JsonSerializable(typeof(SendAsync_Given_.McpClientCapabilities))]
  [JsonSerializable(typeof(SendAsync_Given_.McpClientInfo))]
  internal sealed partial class FixtureJsonContext : JsonSerializerContext
  {
  }
}
