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
    // Create bidirectional streams for JSON-RPC communication
    // These will be connected to the process's stdin/stdout
    var inputStream = new MemoryStream();
    var outputStream = new MemoryStream();

    // Create the CliWrap command with our configuration
    Command command = Cli.Wrap(executable)
      .WithArguments(arguments)
      .WithWorkingDirectory(options.WorkingDirectory ?? Directory.GetCurrentDirectory())
      .WithValidation(CommandResultValidation.None); // JSON-RPC processes stay alive

    // Configure for bidirectional communication
    command = command
      .WithStandardInputPipe(PipeSource.FromStream(outputStream)) // What we write goes to process stdin
      .WithStandardOutputPipe(PipeTarget.ToStream(inputStream))   // Process stdout comes to us
      .WithStandardErrorPipe(PipeTarget.Null); // Ignore stderr for JSON-RPC

    // Start the process (but don't await it - it runs in background)
    CommandTask<CliWrap.CommandResult> processTask = command.ExecuteAsync(cancellationToken);

    // Create client with the process and streams
    // We'll connect StreamJsonRpc to these streams in the next step
    JsonRpcClient client = new(processTask, inputStream, outputStream, timeout);

    await Task.CompletedTask;
    return client;
  }
}