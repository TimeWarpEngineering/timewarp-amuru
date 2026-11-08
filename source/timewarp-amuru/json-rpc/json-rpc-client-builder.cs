#region Purpose
// Configures serializers and standard-error observation, then starts a JsonRpcClient.
#endregion

#region Design
// The builder accepts JsonSerializerOptions or a source-generated JsonSerializerContext.
// When both are set, the context performs serialization. There is no formatter interface.
// CommandOptions.Timeout is the per-request limit. TimeoutGracePeriod is the child's shutdown grace.
// Configured standard input is rejected by ShellBuilder.AsJsonRpcClient before this builder exists.
#endregion

namespace TimeWarp.Amuru;

using TimeWarp.Amuru.JsonRpc;

/// <summary>
/// Configures and starts a <see cref="JsonRpcClient"/> for one child process.
/// </summary>
public sealed class JsonRpcClientBuilder
{
  private readonly string Executable;
  private readonly string[] Arguments;
  private readonly CommandOptions Options;
  private JsonSerializerOptions? SerializerOptions;
  private JsonSerializerContext? SerializerContext;
  private Action<string>? StandardErrorObserver;
  private int Started;

  internal JsonRpcClientBuilder(string executable, string[] arguments, CommandOptions options)
  {
    ArgumentException.ThrowIfNullOrEmpty(executable);
    ArgumentNullException.ThrowIfNull(arguments);
    ArgumentNullException.ThrowIfNull(options);
    Executable = executable;
    Arguments = arguments;
    Options = options;
  }

  /// <summary>
  /// Uses reflection serialization with these options.
  /// NativeAOT callers use <see cref="WithJsonSerializerContext"/> instead.
  /// </summary>
  /// <param name="options">The serializer options</param>
  /// <returns>This builder</returns>
  public JsonRpcClientBuilder WithJsonSerializerOptions(JsonSerializerOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);
    SerializerOptions = options;
    return this;
  }

  /// <summary>
  /// Uses a source-generated serializer context. This is the NativeAOT path.
  /// </summary>
  /// <param name="context">The source-generated context</param>
  /// <returns>This builder</returns>
  public JsonRpcClientBuilder WithJsonSerializerContext(JsonSerializerContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    SerializerContext = context;
    return this;
  }

  /// <summary>
  /// Receives each standard-error line from the child.
  /// The JSON-RPC stream is standard output. Standard error is never merged into it.
  /// The callback runs on the reader's task and must return. An exception from the callback
  /// is ignored so a full standard-error pipe cannot stall the child.
  /// </summary>
  /// <param name="observer">The line callback</param>
  /// <returns>This builder</returns>
  public JsonRpcClientBuilder WithStandardErrorObserver(Action<string> observer)
  {
    ArgumentNullException.ThrowIfNull(observer);
    StandardErrorObserver = observer;
    return this;
  }

  /// <summary>
  /// Starts the child and returns a client in the ready state.
  /// </summary>
  /// <param name="cancellationToken">Cancels the wait for the child to become ready</param>
  /// <returns>The started client</returns>
  /// <exception cref="InvalidOperationException">The builder was already started, or neither a serializer context nor serializer options was set.</exception>
  /// <exception cref="JsonRpcException">The child failed to become ready.</exception>
  public async Task<JsonRpcClient> StartAsync(CancellationToken cancellationToken = default)
  {
    if (Interlocked.Exchange(ref Started, 1) != 0)
    {
      throw new InvalidOperationException("the JSON-RPC client is already started");
    }

    if (SerializerContext is null && SerializerOptions is null)
    {
      throw new InvalidOperationException(
        "Call WithJsonSerializerContext or WithJsonSerializerOptions before StartAsync."
      );
    }

    Dictionary<string, string?>? environment = Options.EnvironmentVariables is null
      ? null
      : new Dictionary<string, string?>(Options.EnvironmentVariables);
    JsonRpcChildProcess process = new
    (
      Executable,
      Arguments,
      Options.WorkingDirectory,
      environment,
      Options.TimeoutGracePeriod,
      StandardErrorObserver
    );
    return await JsonRpcClient.StartAsync
    (
      process,
      Options.Timeout,
      SerializerOptions,
      SerializerContext,
      cancellationToken
    ).ConfigureAwait(false);
  }
}
