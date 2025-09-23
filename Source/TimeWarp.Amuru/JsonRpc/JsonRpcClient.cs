namespace TimeWarp.Amuru.JsonRpc;

/// <summary>
/// Implementation of JSON-RPC client for communicating with interactive processes.
/// </summary>
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class JsonRpcClient : IJsonRpcClient
#pragma warning restore CA1812
{
  private readonly CommandResult? commandResult;
  private readonly TimeSpan timeout;

  /// <summary>
  /// Initializes a new instance of the JsonRpcClient class.
  /// </summary>
  public JsonRpcClient()
  {
    timeout = TimeSpan.FromSeconds(30);
  }

  /// <summary>
  /// Initializes a new instance of the JsonRpcClient class with a command.
  /// </summary>
  public JsonRpcClient(CommandResult commandResult, TimeSpan timeout)
  {
    this.commandResult = commandResult;
    this.timeout = timeout;
  }

  /// <inheritdoc />
  public Task<TResponse?> SendRequestAsync<TResponse>
  (
    string method,
    object? parameters = null,
    CancellationToken cancellationToken = default
  )
  {
    _ = commandResult; // Will use to send to process
    _ = timeout; // Will use for request timeout
    throw new NotImplementedException("JSON-RPC request sending not yet implemented");
  }

  /// <inheritdoc />
  public async ValueTask DisposeAsync()
  {
    await Task.CompletedTask;
  }
}