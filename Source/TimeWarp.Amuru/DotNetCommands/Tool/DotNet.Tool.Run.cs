namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for 'dotnet tool run' commands.
/// </summary>
public class DotNetToolRunBuilder
{
  private readonly string CommandName;
  private readonly CommandOptions Options;
  private List<string> ToolArguments = new();

  public DotNetToolRunBuilder(string commandName, CommandOptions options)
  {
    CommandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
    Options = options;
  }

  /// <summary>
  /// Adds arguments to pass to the tool.
  /// </summary>
  /// <param name="arguments">The arguments to pass to the tool</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolRunBuilder WithArguments(params string[] arguments)
  {
    ToolArguments.AddRange(arguments);
    return this;
  }

  /// <summary>
  /// Adds a single argument to pass to the tool.
  /// </summary>
  /// <param name="argument">The argument to pass to the tool</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolRunBuilder WithArgument(string argument)
  {
    ToolArguments.Add(argument);
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "tool", "run", CommandName };
    arguments.AddRange(ToolArguments);

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
  }

  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken);
  }

  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken);
  }
  
  public async Task<ExecutionResult> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken);
  }
  
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken);
  }
}