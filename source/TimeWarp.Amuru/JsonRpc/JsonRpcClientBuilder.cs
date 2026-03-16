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
  private IJsonRpcMessageFormatter? formatter;

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
  /// Sets a custom JSON-RPC message formatter.
  /// </summary>
  public JsonRpcClientBuilder WithFormatter(IJsonRpcMessageFormatter formatter)
  {
    this.formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
    return this;
  }

  /// <summary>
  /// Starts the process and creates the JSON-RPC client.
  /// </summary>
  /// <exception cref="InvalidOperationException">Thrown when formatter is not configured for AOT compatibility.</exception>
  public async Task<IJsonRpcClient> StartAsync(CancellationToken cancellationToken = default)
  {
    if (formatter == null)
    {
      throw new InvalidOperationException(
        "Formatter is required for AOT compatibility. " +
        "Call WithFormatter(new SystemTextJsonFormatter(YourJsonContext.Default)) before StartAsync(). " +
        "Example: .WithFormatter(new SystemTextJsonFormatter(MyJsonContext.Default))");
    }

    // We need to create a duplex stream for bidirectional communication
    // Since CliWrap doesn't directly expose the process streams, we'll use
    // System.IO.Pipelines to create a proper duplex stream
    var inputPipe = new System.IO.Pipelines.Pipe();
    var outputPipe = new System.IO.Pipelines.Pipe();

    // Create the CliWrap command with our configuration
    Command command = Cli.Wrap(executable)
      .WithArguments(arguments)
      .WithWorkingDirectory(options.WorkingDirectory ?? Directory.GetCurrentDirectory())
      .WithValidation(CommandResultValidation.None); // JSON-RPC processes stay alive

    // Create a pipe for stderr to capture any errors
    var errorPipe = new System.IO.Pipelines.Pipe();

    // Configure for bidirectional communication
    // The process reads from inputPipe.Reader, writes to outputPipe.Writer
    command = command
      .WithStandardInputPipe(PipeSource.FromStream(inputPipe.Reader.AsStream()))
      .WithStandardOutputPipe(PipeTarget.ToStream(outputPipe.Writer.AsStream()))
      .WithStandardErrorPipe(PipeTarget.ToStream(errorPipe.Writer.AsStream())); // Capture stderr

    // Start the process (but don't await it - it runs in background)
    CommandTask<CliWrap.CommandResult> processTask = command.ExecuteAsync(cancellationToken);

    // Create client with the process and streams
    // For JSON-RPC: we write to inputPipe.Writer and read from outputPipe.Reader
    Stream readStream = outputPipe.Reader.AsStream();
    Stream writeStream = inputPipe.Writer.AsStream();
    Stream errorStream = errorPipe.Reader.AsStream();

    // Note: JsonRpcClient constructor expects (processTask, inputStream, outputStream, errorStream, formatter, timeout)
    // where inputStream is what we write to and outputStream is what we read from
    JsonRpcClient client = new(processTask, writeStream, readStream, errorStream, formatter, timeout);

    await Task.CompletedTask;
    return client;
  }
}