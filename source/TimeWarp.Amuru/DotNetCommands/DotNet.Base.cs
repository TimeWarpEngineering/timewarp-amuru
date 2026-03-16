namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Base command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for base 'dotnet' commands.
  /// </summary>
  /// <returns>A DotNetBuilder for configuring dotnet global options</returns>
  public static DotNetBuilder Builder()
  {
    return new DotNetBuilder();
  }
  
  /// <summary>
  /// Creates a pre-configured builder for 'dotnet --list-sdks' command.
  /// </summary>
  /// <returns>A DotNetBuilder configured with --list-sdks</returns>
  public static DotNetBuilder WithListSdks()
  {
    return new DotNetBuilder().WithArguments("--list-sdks");
  }
  
  /// <summary>
  /// Creates a pre-configured builder for 'dotnet --list-runtimes' command.
  /// </summary>
  /// <returns>A DotNetBuilder configured with --list-runtimes</returns>
  public static DotNetBuilder WithListRuntimes()
  {
    return new DotNetBuilder().WithArguments("--list-runtimes");
  }
  
  /// <summary>
  /// Creates a pre-configured builder for 'dotnet --version' command.
  /// </summary>
  /// <returns>A DotNetBuilder configured with --version</returns>
  public static DotNetBuilder WithVersion()
  {
    return new DotNetBuilder().WithArguments("--version");
  }
  
  /// <summary>
  /// Creates a pre-configured builder for 'dotnet --info' command.
  /// </summary>
  /// <returns>A DotNetBuilder configured with --info</returns>
  public static DotNetBuilder WithInfo()
  {
    return new DotNetBuilder().WithArguments("--info");
  }
}

/// <summary>
/// Fluent builder for configuring base 'dotnet' commands.
/// </summary>
public class DotNetBuilder : ICommandBuilder<DotNetBuilder>
{
  private readonly List<string> Arguments = new();
  private CommandOptions Options = new();

  /// <summary>
  /// Adds arguments to the command.
  /// </summary>
  /// <param name="arguments">Arguments to add to the command</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuilder WithArguments(params string[] arguments)
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
  public DotNetBuilder WithWorkingDirectory(string directory)
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
  public DotNetBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Builds the command and returns a CommandResult.
  /// </summary>
  /// <returns>A CommandResult for further processing</returns>
  public CommandResult Build()
  {
    return CommandExtensions.Run("dotnet", Arguments.ToArray(), Options);
  }

  /// <summary>
  /// Executes the command and streams output to the console in real-time.
  /// This is the default behavior matching shell execution.
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
  /// Executes the command with true TTY passthrough for TUI applications.
  /// Unlike PassthroughAsync which pipes Console streams, this method
  /// allows the child process to inherit the terminal's TTY characteristics.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings will be empty since output is inherited)</returns>
  public async Task<ExecutionResult> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken);
  }
  
  /// <summary>
  /// Executes an interactive selection command and returns the selected value.
  /// The UI is rendered to the console (via stderr) while stdout is captured and returned.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The selected value from the interactive command</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken);
  }
}