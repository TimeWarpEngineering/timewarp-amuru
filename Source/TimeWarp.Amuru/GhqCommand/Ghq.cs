namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for ghq (GitHub repository manager) integration.
/// </summary>
public static class Ghq
{
  /// <summary>
  /// Creates a fluent builder for the 'ghq' command.
  /// </summary>
  /// <returns>A GhqBuilder for configuring the ghq command</returns>
  public static GhqBuilder Builder()
  {
    return new GhqBuilder();
  }
}

/// <summary>
/// Fluent builder for configuring 'ghq' commands.
/// </summary>
public partial class GhqBuilder
{
  private CommandOptions Options = new();
  private readonly List<string> Arguments = [];
  private string? SubCommand;
  private string? Repository;
  private readonly List<string> SubCommandArguments = [];

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhqBuilder WithWorkingDirectory(string directory)
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
  public GhqBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  // Build methods
  public CommandResult Build()
  {
    List<string> arguments = new() { "ghq" };
    
    // Add subcommand
    if (!string.IsNullOrEmpty(SubCommand))
    {
      arguments.Add(SubCommand);
    }
    
    // Add subcommand arguments
    arguments.AddRange(SubCommandArguments);
    
    // Add repository if specified
    if (!string.IsNullOrEmpty(Repository))
    {
      arguments.Add(Repository);
    }
    
    // Add global arguments
    arguments.AddRange(Arguments);

    return CommandExtensions.Run("ghq", arguments.Skip(1).ToArray(), Options);
  }

  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken);
  }

  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken);
  }

  public async Task<CommandOutput> RunAndCaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAndCaptureAsync(cancellationToken);
  }

  public async Task<ExecutionResult> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken);
  }

  public async Task<ExecutionResult> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken);
  }

  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken);
  }
}