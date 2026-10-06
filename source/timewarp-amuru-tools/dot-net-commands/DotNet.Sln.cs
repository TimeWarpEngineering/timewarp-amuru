#region Purpose
// TODO: Add purpose description
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Solution command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet sln' command.
  /// </summary>
  /// <returns>A DotNetSlnBuilder for configuring the dotnet sln command</returns>
  public static DotNetSlnBuilder Sln()
  {
    return new DotNetSlnBuilder();
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet sln' command with a specific solution file.
  /// </summary>
  /// <param name="slnFile">The solution file to operate on</param>
  /// <returns>A DotNetSlnBuilder for configuring the dotnet sln command</returns>
  public static DotNetSlnBuilder Sln(string slnFile)
  {
    return new DotNetSlnBuilder().WithSolutionFile(slnFile);
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet sln' commands.
/// </summary>
public class DotNetSlnBuilder
{
  private string? SlnFile;
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the solution file to operate on.
  /// </summary>
  /// <param name="slnFile">The solution file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetSlnBuilder WithSolutionFile(string slnFile)
  {
    SlnFile = slnFile;
    return this;
  }

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetSlnBuilder WithWorkingDirectory(string directory)
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
  public DotNetSlnBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetSlnBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetSlnBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet sln add' command.
  /// </summary>
  /// <param name="projectPath">The project path to add to the solution</param>
  /// <returns>A DotNetSlnAddBuilder for configuring the add command</returns>
  public DotNetSlnAddBuilder Add(string projectPath)
  {
    return new DotNetSlnAddBuilder(projectPath, SlnFile, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet sln add' command with multiple projects.
  /// </summary>
  /// <param name="projectPaths">The project paths to add to the solution</param>
  /// <returns>A DotNetSlnAddBuilder for configuring the add command</returns>
  public DotNetSlnAddBuilder Add(params string[] projectPaths)
  {
    return new DotNetSlnAddBuilder(projectPaths, SlnFile, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet sln list' command.
  /// </summary>
  /// <returns>A DotNetSlnListBuilder for configuring the list command</returns>
  public DotNetSlnListBuilder List()
  {
    return new DotNetSlnListBuilder(SlnFile, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet sln remove' command.
  /// </summary>
  /// <param name="projectPath">The project path to remove from the solution</param>
  /// <returns>A DotNetSlnRemoveBuilder for configuring the remove command</returns>
  public DotNetSlnRemoveBuilder Remove(string projectPath)
  {
    return new DotNetSlnRemoveBuilder(projectPath, SlnFile, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet sln remove' command with multiple projects.
  /// </summary>
  /// <param name="projectPaths">The project paths to remove from the solution</param>
  /// <returns>A DotNetSlnRemoveBuilder for configuring the remove command</returns>
  public DotNetSlnRemoveBuilder Remove(params string[] projectPaths)
  {
    return new DotNetSlnRemoveBuilder(projectPaths, SlnFile, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet sln migrate' command.
  /// </summary>
  /// <returns>A DotNetSlnMigrateBuilder for configuring the migrate command</returns>
  public DotNetSlnMigrateBuilder Migrate()
  {
    return new DotNetSlnMigrateBuilder(SlnFile, Options);
  }
}

/// <summary>
/// Fluent builder for 'dotnet sln add' commands.
/// </summary>
public class DotNetSlnAddBuilder
{
  private readonly string[] ProjectPaths;
  private readonly string? SlnFile;
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet sln add</c> command using the given project path and solution file.
  /// </summary>
  /// <param name="projectPath">Path of the project added to the solution.</param>
  /// <param name="slnFile">Solution file to update, or null to use the solution in the working directory.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetSlnAddBuilder(string projectPath, string? slnFile, CommandOptions options)
  {
    ProjectPaths = [projectPath ?? throw new ArgumentNullException(nameof(projectPath))];
    SlnFile = slnFile;
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetSlnAddBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetSlnAddBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Creates a builder for the <c>dotnet sln add</c> command using the given project paths and solution file.
  /// </summary>
  /// <param name="projectPaths">Paths of the projects added to the solution.</param>
  /// <param name="slnFile">Solution file to update, or null to use the solution in the working directory.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetSlnAddBuilder(string[] projectPaths, string? slnFile, CommandOptions options)
  {
    ProjectPaths = projectPaths ?? throw new ArgumentNullException(nameof(projectPaths));
    SlnFile = slnFile;
    Options = options;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet sln add</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "sln" };

    // Add solution file if specified
    if (!string.IsNullOrWhiteSpace(SlnFile))
    {
      arguments.Add(SlnFile);
    }

    arguments.Add("add");
    arguments.AddRange(ProjectPaths);

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet sln add</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet sln add</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet sln add</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet sln add</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet sln add</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet sln list' commands.
/// </summary>
public class DotNetSlnListBuilder
{
  private readonly string? SlnFile;
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet sln list</c> command using the given solution file.
  /// </summary>
  /// <param name="slnFile">Solution file to list, or null to use the solution in the working directory.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetSlnListBuilder(string? slnFile, CommandOptions options)
  {
    SlnFile = slnFile;
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetSlnListBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetSlnListBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet sln list</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "sln" };

    // Add solution file if specified
    if (!string.IsNullOrWhiteSpace(SlnFile))
    {
      arguments.Add(SlnFile);
    }

    arguments.Add("list");

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet sln list</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet sln list</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet sln list</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet sln list</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet sln list</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet sln remove' commands.
/// </summary>
public class DotNetSlnRemoveBuilder
{
  private readonly string[] ProjectPaths;
  private readonly string? SlnFile;
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet sln remove</c> command using the given project path and solution file.
  /// </summary>
  /// <param name="projectPath">Path of the project removed from the solution.</param>
  /// <param name="slnFile">Solution file to update, or null to use the solution in the working directory.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetSlnRemoveBuilder(string projectPath, string? slnFile, CommandOptions options)
  {
    ProjectPaths = [projectPath ?? throw new ArgumentNullException(nameof(projectPath))];
    SlnFile = slnFile;
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetSlnRemoveBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetSlnRemoveBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Creates a builder for the <c>dotnet sln remove</c> command using the given project paths and solution file.
  /// </summary>
  /// <param name="projectPaths">Paths of the projects removed from the solution.</param>
  /// <param name="slnFile">Solution file to update, or null to use the solution in the working directory.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetSlnRemoveBuilder(string[] projectPaths, string? slnFile, CommandOptions options)
  {
    ProjectPaths = projectPaths ?? throw new ArgumentNullException(nameof(projectPaths));
    SlnFile = slnFile;
    Options = options;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet sln remove</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "sln" };

    // Add solution file if specified
    if (!string.IsNullOrWhiteSpace(SlnFile))
    {
      arguments.Add(SlnFile);
    }

    arguments.Add("remove");
    arguments.AddRange(ProjectPaths);

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet sln remove</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet sln remove</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet sln remove</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet sln remove</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet sln remove</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet sln migrate' commands.
/// </summary>
public class DotNetSlnMigrateBuilder
{
  private readonly string? SlnFile;
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet sln migrate</c> command using the given solution file.
  /// </summary>
  /// <param name="slnFile">Solution file to migrate, or null to use the solution in the working directory.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetSlnMigrateBuilder(string? slnFile, CommandOptions options)
  {
    SlnFile = slnFile;
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetSlnMigrateBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetSlnMigrateBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet sln migrate</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "sln" };

    // Add solution file if specified
    if (!string.IsNullOrWhiteSpace(SlnFile))
    {
      arguments.Add(SlnFile);
    }

    arguments.Add("migrate");

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet sln migrate</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet sln migrate</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet sln migrate</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet sln migrate</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet sln migrate</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}
