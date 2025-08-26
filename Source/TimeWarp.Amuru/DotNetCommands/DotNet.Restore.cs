namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Restore command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet restore' command.
  /// </summary>
  /// <returns>A DotNetRestoreBuilder for configuring the dotnet restore command</returns>
  public static DotNetRestoreBuilder Restore()
  {
    return new DotNetRestoreBuilder();
  }
  
  /// <summary>
  /// Creates a fluent builder for the 'dotnet restore' command with a specific project.
  /// </summary>
  /// <param name="project">The project file path</param>
  /// <returns>A DotNetRestoreBuilder for configuring the dotnet restore command</returns>
  public static DotNetRestoreBuilder Restore(string project)
  {
    return new DotNetRestoreBuilder().WithProject(project);
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet restore' commands.
/// </summary>
public class DotNetRestoreBuilder : ICommandBuilder<DotNetRestoreBuilder>
{
  private string? Project;
  private string? Runtime;
  private string? Verbosity;
  private string? PackagesDirectory;
  private string? LockFilePath;
  private string? TerminalLogger;
  private bool NoCache;
  private bool NoDependencies;
  private bool Interactive;
  private bool LockedMode;
  private bool Force;
  private List<string> Sources = new();
  private Dictionary<string, string> Properties = new();
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the project file to restore. If not specified, searches the current directory for one.
  /// </summary>
  /// <param name="project">Path to the project file (.csproj, .fsproj, .vbproj) or directory containing one</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithProject(string project)
  {
    Project = project;
    return this;
  }

  /// <summary>
  /// Specifies the target runtime to restore for.
  /// </summary>
  /// <param name="runtime">The runtime identifier (e.g., "win-x64", "linux-x64")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithRuntime(string runtime)
  {
    Runtime = runtime;
    return this;
  }

  /// <summary>
  /// Sets the verbosity level of the command.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, detailed, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Specifies the directory for restored packages.
  /// </summary>
  /// <param name="directory">The packages directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithPackagesDirectory(string directory)
  {
    PackagesDirectory = directory;
    return this;
  }

  /// <summary>
  /// Specifies the output location where project lock file is written.
  /// </summary>
  /// <param name="lockFilePath">The lock file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithLockFilePath(string lockFilePath)
  {
    LockFilePath = lockFilePath;
    return this;
  }

  /// <summary>
  /// Sets the terminal logger for restore output.
  /// </summary>
  /// <param name="mode">The terminal logger mode ("auto", "on", "off")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithTerminalLogger(string mode)
  {
    TerminalLogger = mode;
    return this;
  }

  /// <summary>
  /// Specifies to not cache HTTP requests.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithNoCache()
  {
    NoCache = true;
    return this;
  }

  /// <summary>
  /// When restoring a project with project-to-project (P2P) references, restores the root project and not the references.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithNoDependencies()
  {
    NoDependencies = true;
    return this;
  }

  /// <summary>
  /// Allows the command to pause for user input or action (e.g., to complete authentication).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithInteractive()
  {
    Interactive = true;
    return this;
  }

  /// <summary>
  /// Don't allow updating project lock file.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithLockedMode()
  {
    LockedMode = true;
    return this;
  }

  /// <summary>
  /// Forces all dependencies to be resolved even if the last restore was successful.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithForce()
  {
    Force = true;
    return this;
  }

  /// <summary>
  /// Adds a NuGet package source to use during the restore operation.
  /// </summary>
  /// <param name="source">The URI of the NuGet package source</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithSource(string source)
  {
    Sources.Add(source);
    return this;
  }

  /// <summary>
  /// Adds multiple NuGet package sources to use during the restore operation.
  /// </summary>
  /// <param name="sources">The URIs of the NuGet package sources</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithSources(params string[] sources)
  {
    Sources.AddRange(sources);
    return this;
  }

  /// <summary>
  /// Sets an MSBuild property.
  /// </summary>
  /// <param name="name">The property name</param>
  /// <param name="value">The property value</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithProperty(string name, string value)
  {
    Properties[name] = value;
    return this;
  }

  /// <summary>
  /// Sets multiple MSBuild properties.
  /// </summary>
  /// <param name="properties">Dictionary of property names and values</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithProperties(Dictionary<string, string> properties)
  {
    foreach (KeyValuePair<string, string> kvp in properties)
    {
      Properties[kvp.Key] = kvp.Value;
    }
    
    return this;
  }

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithWorkingDirectory(string directory)
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
  public DotNetRestoreBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRestoreBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Builds the command arguments and executes the dotnet restore command.
  /// </summary>
  /// <returns>A CommandResult for further processing</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "restore" };

    // Add project if specified
    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add(Project);
    }

    // Add runtime if specified
    if (!string.IsNullOrWhiteSpace(Runtime))
    {
      arguments.Add("--runtime");
      arguments.Add(Runtime);
    }

    // Add verbosity if specified
    if (!string.IsNullOrWhiteSpace(Verbosity))
    {
      arguments.Add("--verbosity");
      arguments.Add(Verbosity);
    }

    // Add packages directory if specified
    if (!string.IsNullOrWhiteSpace(PackagesDirectory))
    {
      arguments.Add("--packages");
      arguments.Add(PackagesDirectory);
    }

    // Add lock file path if specified
    if (!string.IsNullOrWhiteSpace(LockFilePath))
    {
      arguments.Add("--lock-file-path");
      arguments.Add(LockFilePath);
    }

    // Add terminal logger if specified
    if (!string.IsNullOrWhiteSpace(TerminalLogger))
    {
      arguments.Add("--tl");
      arguments.Add(TerminalLogger);
    }

    // Add sources
    foreach (string source in Sources)
    {
      arguments.Add("--source");
      arguments.Add(source);
    }

    // Add boolean flags
    if (NoCache)
    {
      arguments.Add("--no-cache");
    }

    if (NoDependencies)
    {
      arguments.Add("--no-dependencies");
    }

    if (Interactive)
    {
      arguments.Add("--interactive");
    }

    if (LockedMode)
    {
      arguments.Add("--locked-mode");
    }

    if (Force)
    {
      arguments.Add("--force");
    }

    // Add MSBuild properties
    foreach (KeyValuePair<string, string> property in Properties)
    {
      arguments.Add($"--property:{property.Key}={property.Value}");
    }

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
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