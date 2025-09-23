namespace TimeWarp.Amuru;

/// <summary>
/// Implementation of JSON-RPC client for communicating with interactive processes.
/// </summary>
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class JsonRpcClient : IJsonRpcClient
#pragma warning restore CA1812
{
  private readonly CommandTask<CliWrap.CommandResult>? processTask;
  private readonly Stream? inputStream;
  private readonly Stream? outputStream;
  private readonly JsonRpc? jsonRpc;
  private readonly TimeSpan timeout;

  /// <summary>
  /// Initializes a new instance of the JsonRpcClient class.
  /// </summary>
  public JsonRpcClient()
  {
    timeout = TimeSpan.FromSeconds(30);
  }

  /// <summary>
  /// Initializes a new instance of the JsonRpcClient class with a running process.
  /// </summary>
  public JsonRpcClient
  (
    CommandTask<CliWrap.CommandResult> processTask,
    Stream inputStream,
    Stream outputStream,
    TimeSpan timeout
  )
  {
    this.processTask = processTask;
    this.inputStream = inputStream;
    this.outputStream = outputStream;
    this.timeout = timeout;

    // Create and attach JsonRpc to the streams
    // Attach already starts listening, so we don't need to call StartListening()
    jsonRpc = JsonRpc.Attach(outputStream, inputStream);
  }

  /// <inheritdoc />
  public async Task<TResponse?> SendRequestAsync<TResponse>
  (
    string method,
    object? parameters = null,
    CancellationToken cancellationToken = default
  )
  {
    if (jsonRpc is null)
    {
      throw new InvalidOperationException("JSON-RPC client not initialized");
    }

    // Use StreamJsonRpc to send the request
    // It handles request ID generation, correlation, and response parsing
    TResponse? result = await jsonRpc.InvokeAsync<TResponse>(method, parameters, cancellationToken);

    return result;
  }

  /// <inheritdoc />
  public async ValueTask DisposeAsync()
  {
    // Dispose JsonRpc first (before streams)
    jsonRpc?.Dispose();

    // Clean up streams
    inputStream?.Dispose();
    outputStream?.Dispose();

    // If we have a process, dispose it
    if (processTask is not null)
    {
      try
      {
        // Give the process a chance to exit gracefully
        await processTask.Task.ConfigureAwait(false);
      }
      catch
      {
        // Process may have already exited or errored
      }
      finally
      {
        // Dispose the CommandTask
        processTask.Dispose();
      }
    }
  }
}