namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for gwq (Git worktree manager) integration.
/// </summary>
public static class Gwq
{
  /// <summary>
  /// Creates a fluent builder for the 'gwq' command.
  /// </summary>
  /// <returns>A GwqBuilder for configuring the gwq command</returns>
  public static GwqBuilder Builder()
  {
    return new GwqBuilder();
  }
}

/// <summary>
/// Fluent builder for configuring 'gwq' commands.
/// </summary>
public partial class GwqBuilder
{
  private CommandOptions Options = new();
  private List<string> Arguments = new();
  private string? SubCommand;
  private string? Target;
  private List<string> SubCommandArguments = new();
  private List<string> ExecCommand = new();

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public GwqBuilder WithWorkingDirectory(string directory)
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
  public GwqBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  // Build methods
  public CommandResult Build()
  {
    List<string> arguments = new() { "gwq" };
    
    // Add subcommand
    if (!string.IsNullOrEmpty(SubCommand))
    {
      arguments.Add(SubCommand);
    }
    
    // Add subcommand arguments (flags before positional args)
    arguments.AddRange(SubCommandArguments);
    
    // Add target if specified
    if (!string.IsNullOrEmpty(Target))
    {
      arguments.Add(Target);
    }
    
    // Add exec command if specified
    if (ExecCommand.Count > 0 && SubCommand == "exec")
    {
      arguments.Add("--");
      arguments.AddRange(ExecCommand);
    }
    
    // Add global arguments
    arguments.AddRange(Arguments);

    return CommandExtensions.Run("gwq", arguments.Skip(1).ToArray(), Options);
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