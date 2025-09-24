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
  private readonly Stream? errorStream;
  private readonly JsonRpc? jsonRpc;
  private readonly TimeSpan timeout;
  private readonly Task? errorReaderTask;
  private readonly CancellationTokenSource? errorReaderCts;

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
    Stream errorStream,
    IJsonRpcMessageFormatter? customFormatter,
    TimeSpan timeout
  )
  {
    this.processTask = processTask;
    this.inputStream = inputStream;
    this.outputStream = outputStream;
    this.errorStream = errorStream;
    this.timeout = timeout;

    // Start reading stderr in the background to log any errors
    errorReaderCts = new CancellationTokenSource();
    errorReaderTask = Task.Run(async () => await ReadErrorStreamAsync(errorStream, errorReaderCts.Token));

    // Create the message handler with newline-delimited format (what MCP expects)
    // Note: NewLineDelimitedMessageHandler expects (writer, reader) parameters
    // We write to inputStream and read from outputStream
#pragma warning disable CA2000 // Dispose objects before losing scope - JsonRpc disposes these
    // Use custom formatter if provided, otherwise default to JsonMessageFormatter
    // TODO: For AOT scenarios, users need to provide a properly configured SystemTextJsonFormatter
    IJsonRpcMessageFormatter formatter = customFormatter ?? new JsonMessageFormatter();
    IJsonRpcMessageHandler handler;
    if (formatter is IJsonRpcMessageTextFormatter textFormatter)
    {
      handler = new NewLineDelimitedMessageHandler(inputStream, outputStream, textFormatter);
    }
    else
    {
      // Fallback for non-text formatters - shouldn't happen with our defaults
      throw new ArgumentException("Formatter must implement IJsonRpcMessageTextFormatter for newline-delimited messages");
    }
#pragma warning restore CA2000 // Dispose objects before losing scope

    // Create JsonRpc with the handler and start listening
    jsonRpc = new JsonRpc(handler);
    jsonRpc.StartListening();
  }

  private static async Task ReadErrorStreamAsync(Stream errorStream, CancellationToken cancellationToken)
  {
    using var reader = new StreamReader(errorStream);
    string? line;
    while (!cancellationToken.IsCancellationRequested && (line = await reader.ReadLineAsync(cancellationToken)) != null)
    {
      await Console.Error.WriteLineAsync($"[MCP stderr]: {line}");
    }
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
    // Use InvokeWithParameterObjectAsync to ensure parameters are sent as an object, not an array
    TResponse? result;
    if (parameters != null)
    {
      result = await jsonRpc.InvokeWithParameterObjectAsync<TResponse>(method, parameters, cancellationToken);
    }
    else
    {
      result = await jsonRpc.InvokeAsync<TResponse>(method, cancellationToken);
    }

    return result;
  }

  /// <inheritdoc />
  public async ValueTask DisposeAsync()
  {
    // Dispose JsonRpc first (it will dispose the handler and formatter)
    // This should close the connection and cause the server to exit
    jsonRpc?.Dispose();

    // Cancel the error reader task
    if (errorReaderCts is not null)
    {
      await errorReaderCts.CancelAsync().ConfigureAwait(false);
    }

    // Clean up streams immediately to signal EOF to the process
    inputStream?.Dispose();
    outputStream?.Dispose();
    errorStream?.Dispose();

    // Wait for error reader to complete (with timeout)
    if (errorReaderTask != null)
    {
      try
      {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
        await errorReaderTask.WaitAsync(cts.Token).ConfigureAwait(false);
      }
      catch
      {
        // Ignore errors from error reader or timeout
      }
    }

    // Dispose the cancellation token source
    errorReaderCts?.Dispose();

    // If we have a process, dispose it
    if (processTask is not null)
    {
      try
      {
        // Give the process a short chance to exit gracefully
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
        await processTask.Task.WaitAsync(cts.Token).ConfigureAwait(false);
      }
      catch (OperationCanceledException)
      {
        // Process didn't exit gracefully in time
        // The Dispose() will handle terminating the process
      }
      catch
      {
        // Process may have already exited or errored
      }
      finally
      {
        // Dispose the CommandTask - this will kill the process if still running
        processTask.Dispose();
      }
    }
  }
}