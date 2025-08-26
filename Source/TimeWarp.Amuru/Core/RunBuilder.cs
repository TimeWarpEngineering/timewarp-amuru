namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for configuring generic command execution.
/// </summary>
public class RunBuilder : ICommandBuilder<RunBuilder>
{
  private readonly string Executable;
  private readonly List<string> Arguments = new();
  private CommandOptions Options = new();
  private string? StandardInput;

  /// <summary>
  /// Initializes a new instance of the RunBuilder class.
  /// </summary>
  /// <param name="executable">The executable or command to run</param>
  public RunBuilder(string executable)
  {
    Executable = executable ?? throw new ArgumentNullException(nameof(executable));
  }

  /// <summary>
  /// Adds arguments to the command.
  /// </summary>
  /// <param name="arguments">Arguments to add to the command</param>
  /// <returns>The builder instance for method chaining</returns>
  public RunBuilder WithArguments(params string[] arguments)
  {
    if (arguments != null)
    {
      Arguments.AddRange(arguments);
    }
    
    return this;
  }

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public RunBuilder WithWorkingDirectory(string directory)
  {
    Options = Options.WithWorkingDirectory(directory);
    return this;
  }

  /// <summary>
  /// Adds an environment variable for the command execution.
  /// </summary>
  /// <param name="key">The environment variable name</param>
  /// <param name="value">The environment variable value</param>
  /// <returns>The builder instance for method chaining</returns>
  public RunBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public RunBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Provides standard input to the command.
  /// </summary>
  /// <param name="input">The text to provide as standard input</param>
  /// <returns>The builder instance for method chaining</returns>
  public RunBuilder WithStandardInput(string input)
  {
    StandardInput = input ?? throw new ArgumentNullException(nameof(input));
    return this;
  }

  /// <summary>
  /// Builds the command and returns a CommandResult.
  /// </summary>
  /// <returns>A CommandResult for further processing</returns>
  public CommandResult Build()
  {
    return CommandExtensions.Run(Executable, Arguments.ToArray(), Options, StandardInput);
  }

  /// <summary>
  /// Passes the command through to the terminal with full interactive control.
  /// This allows commands like vim, fzf, or REPLs to work with user input and terminal UI.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings will be empty since output goes to console)</returns>
  public async Task<ExecutionResult> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken);
  }
  
  /// <summary>
  /// Executes an interactive selection command and returns the selected value.
  /// The UI is rendered to the console (via stderr) while stdout is captured and returned.
  /// This is ideal for interactive selection tools like fzf.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The selected value from the interactive command</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken);
  }

  /// <summary>
  /// Creates a pipeline by chaining this command with another command.
  /// </summary>
  /// <param name="executable">The next command in the pipeline</param>
  /// <param name="arguments">Arguments for the next command</param>
  /// <returns>A CommandResult representing the pipeline</returns>
  public CommandResult Pipe(string executable, params string[] arguments)
  {
    return Build().Pipe(executable, arguments);
  }

  /// <summary>
  /// Executes the command and streams output to the console in real-time.
  /// This is the default behavior matching shell execution (80% use case).
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The exit code of the command</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken);
  }

  /// <summary>
  /// Executes the command silently and captures all output.
  /// No output is written to the console.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>CommandOutput with stdout, stderr, combined output and exit code</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken);
  }

  /// <summary>
  /// Executes the command, streams output to console AND captures it.
  /// Useful for debugging/logging scenarios where you want to see output and save it.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>CommandOutput with stdout, stderr, combined output and exit code</returns>
  public async Task<CommandOutput> RunAndCaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAndCaptureAsync(cancellationToken);
  }

  /// <summary>
  /// Executes the command and streams stdout lines without buffering.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>An async enumerable of stdout lines</returns>
  public async IAsyncEnumerable<string> StreamStdoutAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    await foreach (string line in Build().StreamStdoutAsync(cancellationToken))
    {
      yield return line;
    }
  }

  /// <summary>
  /// Executes the command and streams stderr lines without buffering.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>An async enumerable of stderr lines</returns>
  public async IAsyncEnumerable<string> StreamStderrAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    await foreach (string line in Build().StreamStderrAsync(cancellationToken))
    {
      yield return line;
    }
  }

  /// <summary>
  /// Executes the command and streams combined output with source information.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>An async enumerable of OutputLine objects</returns>
  public async IAsyncEnumerable<OutputLine> StreamCombinedAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    await foreach (OutputLine line in Build().StreamCombinedAsync(cancellationToken))
    {
      yield return line;
    }
  }

  /// <summary>
  /// Executes the command and streams output directly to a file without buffering.
  /// </summary>
  /// <param name="filePath">Path to the output file</param>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>A task that completes when the command finishes</returns>
  public async Task StreamToFileAsync(string filePath, CancellationToken cancellationToken = default)
  {
    await Build().StreamToFileAsync(filePath, cancellationToken);
  }
}