namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for Fzf (fuzzy finder) integration.
/// </summary>
public static class Fzf
{
  /// <summary>
  /// Creates a fluent builder for the 'fzf' command.
  /// </summary>
  /// <returns>A FzfBuilder for configuring the fzf command</returns>
  public static FzfBuilder Builder()
  {
    return new FzfBuilder();
  }
}

/// <summary>
/// Fluent builder for configuring 'fzf' commands.
/// </summary>
public partial class FzfBuilder
{
  private CommandOptions Options = new();
  private List<string> Arguments = new();
  private List<string> InputItems = new();
  private string? InputCommand;
  private string? InputGlob;
  private bool UseStdin;

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public FzfBuilder WithWorkingDirectory(string directory)
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
  public FzfBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "fzf" };
    arguments.AddRange(Arguments);

    CommandResult command = CommandExtensions.Run("fzf", arguments.Skip(1).ToArray(), Options);

    // Handle input sources
    if (InputItems.Count > 0)
    {
      // Create a pipeline with echo for the input items
      string input = string.Join("\n", InputItems);
      command = CommandExtensions.Run("echo", new[] { input }, Options).Pipe("fzf", arguments.Skip(1).ToArray());
    }
    else if (!string.IsNullOrEmpty(InputGlob))
    {
      // Use find command for file glob
      command = CommandExtensions.Run("find", new[] { ".", "-name", InputGlob }, Options).Pipe("fzf", arguments.Skip(1).ToArray());
    }
    else if (!string.IsNullOrEmpty(InputCommand))
    {
      // Parse and execute the input command
      string[] parts = InputCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length > 0)
      {
        string exe = parts[0];
        string[] args = parts.Skip(1).ToArray();
        command = CommandExtensions.Run(exe, args, Options).Pipe("fzf", arguments.Skip(1).ToArray());
      }
    }
    else if (UseStdin)
    {
      // Use fzf directly with stdin (no pipeline needed)
      command = CommandExtensions.Run("fzf", arguments.Skip(1).ToArray(), Options);
    }

    return command;
  }

  public async Task<string> GetStringAsync(CancellationToken cancellationToken = default)
  {
    return await Build().GetStringAsync(cancellationToken);
  }

  public async Task<string[]> GetLinesAsync(CancellationToken cancellationToken = default)
  {
    return await Build().GetLinesAsync(cancellationToken);
  }

  public async Task<ExecutionResult> ExecuteAsync(CancellationToken cancellationToken = default)
  {
    return await Build().ExecuteAsync(cancellationToken);
  }
  
  /// <summary>
  /// Executes fzf interactively, allowing user to select items with keyboard navigation.
  /// This method connects stdin, stdout, and stderr to the console for full interactive use.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings will be empty since output goes to console)</returns>
  public async Task<ExecutionResult> ExecuteInteractiveAsync(CancellationToken cancellationToken = default)
  {
    return await Build().ExecuteInteractiveAsync(cancellationToken);
  }
  
  /// <summary>
  /// Executes fzf interactively and returns the selected item(s).
  /// The fzf UI is displayed on the console, but the selection is captured and returned.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The selected item(s) as a string, or empty string if cancelled</returns>
  public async Task<string> GetStringInteractiveAsync(CancellationToken cancellationToken = default)
  {
    return await Build().GetStringInteractiveAsync(cancellationToken);
  }
}