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
  /// Executes the command and returns the output as a string.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The command output as a string</returns>
  public async Task<string> GetStringAsync(CancellationToken cancellationToken = default)
  {
    return await Build().GetStringAsync(cancellationToken);
  }

  /// <summary>
  /// Executes the command and returns the output as an array of lines.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The command output as an array of lines</returns>
  public async Task<string[]> GetLinesAsync(CancellationToken cancellationToken = default)
  {
    return await Build().GetLinesAsync(cancellationToken);
  }

  /// <summary>
  /// Executes the command and returns the execution result.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>ExecutionResult containing command output and execution details</returns>
  public async Task<ExecutionResult> ExecuteAsync(CancellationToken cancellationToken = default)
  {
    return await Build().ExecuteAsync(cancellationToken);
  }
  
  /// <summary>
  /// Executes the command interactively with stdin, stdout, and stderr connected to the console.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings will be empty since output goes to console)</returns>
  public async Task<ExecutionResult> ExecuteInteractiveAsync(CancellationToken cancellationToken = default)
  {
    return await Build().ExecuteInteractiveAsync(cancellationToken);
  }
  
  /// <summary>
  /// Executes the command interactively and captures the output.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The captured output string from the interactive command</returns>
  public async Task<string> GetStringInteractiveAsync(CancellationToken cancellationToken = default)
  {
    return await Build().GetStringInteractiveAsync(cancellationToken);
  }
}