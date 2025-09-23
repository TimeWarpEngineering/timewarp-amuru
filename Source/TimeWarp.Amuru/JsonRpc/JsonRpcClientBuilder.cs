namespace TimeWarp.Amuru;

/// <summary>
/// Builder for configuring JSON-RPC client options.
/// </summary>
public class JsonRpcClientBuilder
{
  private readonly string executable;
  private readonly List<string> arguments;
  private readonly CommandOptions options;
  private TimeSpan timeout = TimeSpan.FromSeconds(30);

  /// <summary>
  /// Initializes a new instance of the JsonRpcClientBuilder class.
  /// </summary>
  internal JsonRpcClientBuilder(string executable, List<string> arguments, CommandOptions options)
  {
    this.executable = executable ?? throw new ArgumentNullException(nameof(executable));
    this.arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    this.options = options ?? throw new ArgumentNullException(nameof(options));
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
    // For now, just create a simple client with the configuration
    // We'll add actual process launching in the next step
    _ = executable; // Will use to launch process
    _ = arguments; // Will use as process arguments
    _ = options; // Will use for working directory, env vars, etc.
    _ = timeout; // Will pass to client
    _ = cancellationToken; // Will use for cancellation

    JsonRpcClient client = new();

    await Task.CompletedTask;
    return client;
  }
}