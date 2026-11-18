#region Purpose
// Fluent builder for dotnet format. Task 154.
#endregion

#region Design
// The project or solution path is a positional argument, the same shape as `dotnet format <path>`.
// --severity is one pair of tokens. A later WithSeverity replaces the level.
// Each WithExclude adds its own --exclude <path>, in call order. dotnet format treats the value
// as a relative file, folder, or glob.
// --verify-no-changes is a flag. The execution methods match DotNet.MsBuild.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - format command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet format' command.
  /// </summary>
  /// <returns>A DotNetFormatBuilder for configuring the dotnet format command</returns>
  public static DotNetFormatBuilder Format()
  {
    return new DotNetFormatBuilder();
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet format' command with a project or solution.
  /// </summary>
  /// <param name="projectOrSolution">The project or solution file path</param>
  /// <returns>A DotNetFormatBuilder for configuring the dotnet format command</returns>
  public static DotNetFormatBuilder Format(string projectOrSolution)
  {
    return new DotNetFormatBuilder().WithProject(projectOrSolution);
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet format' commands.
/// </summary>
public class DotNetFormatBuilder : ICommandBuilder<DotNetFormatBuilder>
{
  private readonly List<string> Excludes = [];
  private string? Project;
  private string? Severity;
  private bool VerifyNoChanges;
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the project or solution file to format. If not specified, the command searches the current directory.
  /// </summary>
  /// <param name="projectOrSolution">Path to the project or solution file</param>
  /// <returns>The builder instance for method chaining</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="projectOrSolution"/> is null or whitespace.</exception>
  public DotNetFormatBuilder WithProject(string projectOrSolution)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(projectOrSolution);
    Project = projectOrSolution;
    return this;
  }

  /// <summary>
  /// Sets the minimum diagnostic severity to fix. Emits <c>--severity &lt;level&gt;</c>.
  /// A later call replaces the level. Common levels are info, warn, and error.
  /// </summary>
  /// <param name="severity">The severity level</param>
  /// <returns>The builder instance for method chaining</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="severity"/> is null or whitespace.</exception>
  public DotNetFormatBuilder WithSeverity(string severity)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(severity);
    Severity = severity;
    return this;
  }

  /// <summary>
  /// Excludes a relative file, folder, or glob. Emits <c>--exclude &lt;path&gt;</c>. Call again to exclude more.
  /// </summary>
  /// <param name="path">The relative path or glob to exclude</param>
  /// <returns>The builder instance for method chaining</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null or whitespace.</exception>
  public DotNetFormatBuilder WithExclude(string path)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    Excludes.Add(path);
    return this;
  }

  /// <summary>
  /// Verifies that formatting would change nothing. Emits <c>--verify-no-changes</c>.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetFormatBuilder WithVerifyNoChanges()
  {
    VerifyNoChanges = true;
    return this;
  }

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetFormatBuilder WithWorkingDirectory(string directory)
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
  public DotNetFormatBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command result validation, allowing the command to exit with non-zero codes without throwing exceptions.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetFormatBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetFormatBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet format</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = ["format"];

    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add(Project);
    }

    if (!string.IsNullOrWhiteSpace(Severity))
    {
      arguments.Add("--severity");
      arguments.Add(Severity);
    }

    foreach (string path in Excludes)
    {
      arguments.Add("--exclude");
      arguments.Add(path);
    }

    if (VerifyNoChanges)
    {
      arguments.Add("--verify-no-changes");
    }

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet format</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet format</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet format</c>, streams output to the console, and also captures it.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> RunAndCaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAndCaptureAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet format</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet format</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet format</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}
