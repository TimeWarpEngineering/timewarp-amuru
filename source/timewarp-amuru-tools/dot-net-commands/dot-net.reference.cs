#region Purpose
// Fluent builders for `dotnet reference add`, `list`, and `remove`.
#endregion

#region Design
// `--project` is emitted before the subcommand. Tokens after `add` or `remove` are the referenced projects,
// so the owning project cannot be appended.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Reference command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet reference' command.
  /// </summary>
  /// <returns>A DotNetReferenceBuilder for configuring the dotnet reference command</returns>
  public static DotNetReferenceBuilder Reference()
  {
    return new DotNetReferenceBuilder();
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet reference' command with a specific project.
  /// </summary>
  /// <param name="project">The project file to operate on</param>
  /// <returns>A DotNetReferenceBuilder for configuring the dotnet reference command</returns>
  public static DotNetReferenceBuilder Reference(string project)
  {
    return new DotNetReferenceBuilder().WithProject(project);
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet reference' commands.
/// </summary>
public class DotNetReferenceBuilder
{
  private string? Project;
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the project file to operate on.
  /// </summary>
  /// <param name="project">The project file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetReferenceBuilder WithProject(string project)
  {
    Project = project;
    return this;
  }

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetReferenceBuilder WithWorkingDirectory(string directory)
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
  public DotNetReferenceBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetReferenceBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetReferenceBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet reference add' command.
  /// </summary>
  /// <param name="projectPath">The project path to add as a reference</param>
  /// <returns>A DotNetReferenceAddBuilder for configuring the add command</returns>
  public DotNetReferenceAddBuilder Add(string projectPath)
  {
    return new DotNetReferenceAddBuilder(projectPath, Project, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet reference add' command with multiple projects.
  /// </summary>
  /// <param name="projectPaths">The project paths to add as references</param>
  /// <returns>A DotNetReferenceAddBuilder for configuring the add command</returns>
  public DotNetReferenceAddBuilder Add(params string[] projectPaths)
  {
    return new DotNetReferenceAddBuilder(projectPaths, Project, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet reference list' command.
  /// </summary>
  /// <returns>A DotNetReferenceListBuilder for configuring the list command</returns>
  public DotNetReferenceListBuilder List()
  {
    return new DotNetReferenceListBuilder(Project, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet reference remove' command.
  /// </summary>
  /// <param name="projectPath">The project path to remove as a reference</param>
  /// <returns>A DotNetReferenceRemoveBuilder for configuring the remove command</returns>
  public DotNetReferenceRemoveBuilder Remove(string projectPath)
  {
    return new DotNetReferenceRemoveBuilder(projectPath, Project, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet reference remove' command with multiple projects.
  /// </summary>
  /// <param name="projectPaths">The project paths to remove as references</param>
  /// <returns>A DotNetReferenceRemoveBuilder for configuring the remove command</returns>
  public DotNetReferenceRemoveBuilder Remove(params string[] projectPaths)
  {
    return new DotNetReferenceRemoveBuilder(projectPaths, Project, Options);
  }
}

/// <summary>
/// Fluent builder for 'dotnet reference add' commands.
/// </summary>
public class DotNetReferenceAddBuilder
{
  private readonly string[] ProjectPaths;
  private readonly string? Project;
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet reference add</c> command using the given project path and target project.
  /// </summary>
  /// <param name="projectPath">Path of the project added as a reference.</param>
  /// <param name="project">Project that receives the reference, or null to use the project in the working directory.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetReferenceAddBuilder(string projectPath, string? project, CommandOptions options)
  {
    ProjectPaths = [projectPath ?? throw new ArgumentNullException(nameof(projectPath))];
    Project = project;
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetReferenceAddBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetReferenceAddBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Creates a builder for the <c>dotnet reference add</c> command using the given project paths and target project.
  /// </summary>
  /// <param name="projectPaths">Paths of the projects added as references.</param>
  /// <param name="project">Project that receives the reference, or null to use the project in the working directory.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetReferenceAddBuilder(string[] projectPaths, string? project, CommandOptions options)
  {
    ProjectPaths = projectPaths ?? throw new ArgumentNullException(nameof(projectPaths));
    Project = project;
    Options = options;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet reference add</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "reference" };

    // Add project if specified
    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
      arguments.Add(Project);
    }

    arguments.Add("add");
    arguments.AddRange(ProjectPaths);

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet reference add</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet reference add</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet reference add</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet reference add</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet reference add</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet reference list' commands.
/// </summary>
public class DotNetReferenceListBuilder
{
  private readonly string? Project;
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet reference list</c> command using the given target project.
  /// </summary>
  /// <param name="project">Project whose references are listed, or null to use the project in the working directory.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetReferenceListBuilder(string? project, CommandOptions options)
  {
    Project = project;
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetReferenceListBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetReferenceListBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet reference list</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "reference" };

    // Add project if specified
    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
      arguments.Add(Project);
    }

    arguments.Add("list");

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet reference list</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet reference list</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet reference list</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet reference list</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet reference list</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet reference remove' commands.
/// </summary>
public class DotNetReferenceRemoveBuilder
{
  private readonly string[] ProjectPaths;
  private readonly string? Project;
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet reference remove</c> command using the given project path and target project.
  /// </summary>
  /// <param name="projectPath">Path of the referenced project to remove.</param>
  /// <param name="project">Project that loses the reference, or null to use the project in the working directory.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetReferenceRemoveBuilder(string projectPath, string? project, CommandOptions options)
  {
    ProjectPaths = [projectPath ?? throw new ArgumentNullException(nameof(projectPath))];
    Project = project;
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetReferenceRemoveBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetReferenceRemoveBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Creates a builder for the <c>dotnet reference remove</c> command using the given project paths and target project.
  /// </summary>
  /// <param name="projectPaths">Paths of the referenced projects to remove.</param>
  /// <param name="project">Project that loses the reference, or null to use the project in the working directory.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetReferenceRemoveBuilder(string[] projectPaths, string? project, CommandOptions options)
  {
    ProjectPaths = projectPaths ?? throw new ArgumentNullException(nameof(projectPaths));
    Project = project;
    Options = options;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet reference remove</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "reference" };

    // Add project if specified
    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
      arguments.Add(Project);
    }

    arguments.Add("remove");
    arguments.AddRange(ProjectPaths);

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet reference remove</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet reference remove</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet reference remove</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet reference remove</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet reference remove</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}
