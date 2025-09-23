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
    // Create the CliWrap command with our configuration
    Command command = Cli.Wrap(executable)
      .WithArguments(arguments)
      .WithWorkingDirectory(options.WorkingDirectory ?? Directory.GetCurrentDirectory())
      .WithValidation(CommandResultValidation.None); // JSON-RPC processes stay alive

    // Configure for bidirectional communication
    // For now, just set up the pipes - we'll connect StreamJsonRpc later
    command = command
      .WithStandardInputPipe(PipeSource.FromStream(Stream.Null)) // Placeholder for now
      .WithStandardOutputPipe(PipeTarget.ToStream(Stream.Null)) // Placeholder for now
      .WithStandardErrorPipe(PipeTarget.Null); // Ignore stderr for JSON-RPC

    // Start the process (but don't await it - it runs in background)
    CommandTask<CliWrap.CommandResult> processTask = command.ExecuteAsync(cancellationToken);

    // For now, just create a simple client
    // We'll connect StreamJsonRpc in the next step
    JsonRpcClient client = new();

    await Task.CompletedTask;
    return client;
  }
}