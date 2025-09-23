namespace TimeWarp.Amuru.JsonRpc;

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
  }

  /// <inheritdoc />
  public Task<TResponse?> SendRequestAsync<TResponse>
  (
    string method,
    object? parameters = null,
    CancellationToken cancellationToken = default
  )
  {
    _ = processTask; // Will check if process is still running
    _ = inputStream; // Will read responses from here
    _ = outputStream; // Will write requests to here
    _ = timeout; // Will use for request timeout
    throw new NotImplementedException("JSON-RPC request sending not yet implemented");
  }

  /// <inheritdoc />
  public async ValueTask DisposeAsync()
  {
    // Clean up streams
    inputStream?.Dispose();
    outputStream?.Dispose();

    // If we have a process, dispose it
    if (processTask != null)
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