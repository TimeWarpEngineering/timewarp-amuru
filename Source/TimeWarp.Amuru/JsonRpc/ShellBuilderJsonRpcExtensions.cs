namespace TimeWarp.Amuru;

/// <summary>
/// Extension methods for adding JSON-RPC support to RunBuilder.
/// </summary>
public static class ShellBuilderJsonRpcExtensions
{
  /// <summary>
  /// Configures the command to be executed as a JSON-RPC client.
  /// </summary>
  /// <param name="builder">The RunBuilder to convert to JSON-RPC client.</param>
  /// <returns>A JsonRpcClientBuilder for further configuration.</returns>
  public static JsonRpcClientBuilder AsJsonRpcClient(this RunBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);
    return new JsonRpcClientBuilder(builder);
  }
}

/// <summary>
/// Builder for configuring JSON-RPC client options.
/// </summary>
public class JsonRpcClientBuilder
{
  private readonly RunBuilder runBuilder;
  private TimeSpan timeout = TimeSpan.FromSeconds(30);

  /// <summary>
  /// Initializes a new instance of the JsonRpcClientBuilder class.
  /// </summary>
  internal JsonRpcClientBuilder(RunBuilder runBuilder)
  {
    this.runBuilder = runBuilder ?? throw new ArgumentNullException(nameof(runBuilder));
  }

  /// <summary>
  /// Sets the timeout for JSON-RPC requests.
  /// </summary>
  public JsonRpcClientBuilder WithTimeout(TimeSpan timeout)
  {
    this.timeout = timeout;
    return this;
  }

  /// <summary>
  /// Starts the process and creates the JSON-RPC client.
  /// </summary>
  public async Task<IJsonRpcClient> StartAsync(CancellationToken cancellationToken = default)
  {
    // Build the CliWrap command from our RunBuilder
    CommandResult commandResult = runBuilder.Build();
    _ = cancellationToken; // Will use for cancellation

    // Create the client with the command and timeout
    JsonRpcClient client = new(commandResult, timeout);

    await Task.CompletedTask;
    return client;
  }
}