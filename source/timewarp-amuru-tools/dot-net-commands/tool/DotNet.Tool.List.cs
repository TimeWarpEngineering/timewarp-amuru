#region Purpose
// TODO: Add purpose description
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for 'dotnet tool list' commands.
/// </summary>
public class DotNetToolListBuilder
{
  private CommandOptions Options;
  private bool IsGlobal;
  private bool IsLocal;
  private string? ToolPath;

  /// <summary>
  /// Creates a builder for the <c>dotnet tool list</c> command.
  /// </summary>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetToolListBuilder(CommandOptions options)
  {
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolListBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolListBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Lists globally installed tools.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolListBuilder Global()
  {
    IsGlobal = true;
    IsLocal = false;
    return this;
  }

  /// <summary>
  /// Lists locally installed tools.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolListBuilder Local()
  {
    IsLocal = true;
    IsGlobal = false;
    return this;
  }

  /// <summary>
  /// Specifies the path where tools are installed.
  /// </summary>
  /// <param name="toolPath">The installation path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolListBuilder WithToolPath(string toolPath)
  {
    ToolPath = toolPath;
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet tool list</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "tool", "list" };

    if (IsGlobal)
    {
      arguments.Add("--global");
    }

    if (IsLocal)
    {
      arguments.Add("--local");
    }

    if (!string.IsNullOrWhiteSpace(ToolPath))
    {
      arguments.Add("--tool-path");
      arguments.Add(ToolPath);
    }

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet tool list</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet tool list</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet tool list</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet tool list</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet tool list</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}
