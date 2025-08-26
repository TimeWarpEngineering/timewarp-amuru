namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Build command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet build' command.
  /// </summary>
  /// <returns>A DotNetBuildBuilder for configuring the dotnet build command</returns>
  public static DotNetBuildBuilder Build()
  {
    return new DotNetBuildBuilder();
  }
  
  /// <summary>
  /// Creates a fluent builder for the 'dotnet build' command with a specific project.
  /// </summary>
  /// <param name="project">The project file path</param>
  /// <returns>A DotNetBuildBuilder for configuring the dotnet build command</returns>
  public static DotNetBuildBuilder Build(string project)
  {
    return new DotNetBuildBuilder().WithProject(project);
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet build' commands.
/// </summary>
public class DotNetBuildBuilder : ICommandBuilder<DotNetBuildBuilder>
{
  private string? Project;
  private string? Configuration;
  private string? Framework;
  private string? Runtime;
  private string? Architecture;
  private string? OperatingSystem;
  private string? OutputPath;
  private string? Verbosity;
  private string? TerminalLogger;
  private bool NoRestore;
  private bool NoDependencies;
  private bool NoIncremental;
  private bool NoLogo;
  private bool Force;
  private bool Interactive;
  private Dictionary<string, string> Properties = new();
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the project file to build. If not specified, searches the current directory for one.
  /// </summary>
  /// <param name="project">Path to the project file (.csproj, .fsproj, .vbproj) or directory containing one</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithProject(string project)
  {
    Project = project;
    return this;
  }

  /// <summary>
  /// Specifies the configuration to build (Debug or Release).
  /// </summary>
  /// <param name="configuration">The configuration name (e.g., "Debug", "Release")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithConfiguration(string configuration)
  {
    Configuration = configuration;
    return this;
  }

  /// <summary>
  /// Specifies the target framework to build for.
  /// </summary>
  /// <param name="framework">The target framework moniker (e.g., "net8.0", "net10.0")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithFramework(string framework)
  {
    Framework = framework;
    return this;
  }

  /// <summary>
  /// Specifies the target runtime to build for.
  /// </summary>
  /// <param name="runtime">The runtime identifier (e.g., "win-x64", "linux-x64")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithRuntime(string runtime)
  {
    Runtime = runtime;
    return this;
  }

  /// <summary>
  /// Specifies the target architecture.
  /// </summary>
  /// <param name="architecture">The target architecture (e.g., "x64", "arm64")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithArchitecture(string architecture)
  {
    Architecture = architecture;
    return this;
  }

  /// <summary>
  /// Specifies the target operating system.
  /// </summary>
  /// <param name="operatingSystem">The target OS (e.g., "win", "linux", "osx")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithOperatingSystem(string operatingSystem)
  {
    OperatingSystem = operatingSystem;
    return this;
  }

  /// <summary>
  /// Specifies the output directory for built artifacts.
  /// </summary>
  /// <param name="outputPath">The output directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithOutput(string outputPath)
  {
    OutputPath = outputPath;
    return this;
  }

  /// <summary>
  /// Sets the verbosity level of the command.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, detailed, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Sets the terminal logger for build output.
  /// </summary>
  /// <param name="mode">The terminal logger mode ("auto", "on", "off")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithTerminalLogger(string mode)
  {
    TerminalLogger = mode;
    return this;
  }

  /// <summary>
  /// Skips the implicit restore during the build command.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithNoRestore()
  {
    NoRestore = true;
    return this;
  }

  /// <summary>
  /// Skips project-to-project references and only builds the root project.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithNoDependencies()
  {
    NoDependencies = true;
    return this;
  }

  /// <summary>
  /// Disables incremental builds.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithNoIncremental()
  {
    NoIncremental = true;
    return this;
  }

  /// <summary>
  /// Doesn't display the startup banner or the copyright message.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithNoLogo()
  {
    NoLogo = true;
    return this;
  }

  /// <summary>
  /// Forces all dependencies to be resolved even if the last restore was successful.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithForce()
  {
    Force = true;
    return this;
  }

  /// <summary>
  /// Allows the command to pause for user input or action.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithInteractive()
  {
    Interactive = true;
    return this;
  }

  /// <summary>
  /// Sets an MSBuild property.
  /// </summary>
  /// <param name="name">The property name</param>
  /// <param name="value">The property value</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithProperty(string name, string value)
  {
    Properties[name] = value;
    return this;
  }

  /// <summary>
  /// Sets multiple MSBuild properties.
  /// </summary>
  /// <param name="properties">Dictionary of property names and values</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithProperties(Dictionary<string, string> properties)
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
  public DotNetBuildBuilder WithWorkingDirectory(string directory)
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
  public DotNetBuildBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command result validation, allowing the build to exit with non-zero codes without throwing exceptions.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Builds the command arguments and executes the dotnet build command.
  /// </summary>
  /// <returns>A CommandResult for further processing</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "build" };

    // Add project if specified
    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add(Project);
    }

    // Add configuration if specified
    if (!string.IsNullOrWhiteSpace(Configuration))
    {
      arguments.Add("--configuration");
      arguments.Add(Configuration);
    }

    // Add framework if specified
    if (!string.IsNullOrWhiteSpace(Framework))
    {
      arguments.Add("--framework");
      arguments.Add(Framework);
    }

    // Add runtime if specified
    if (!string.IsNullOrWhiteSpace(Runtime))
    {
      arguments.Add("--runtime");
      arguments.Add(Runtime);
    }

    // Add architecture if specified
    if (!string.IsNullOrWhiteSpace(Architecture))
    {
      arguments.Add("--arch");
      arguments.Add(Architecture);
    }

    // Add operating system if specified
    if (!string.IsNullOrWhiteSpace(OperatingSystem))
    {
      arguments.Add("--os");
      arguments.Add(OperatingSystem);
    }

    // Add output path if specified
    if (!string.IsNullOrWhiteSpace(OutputPath))
    {
      arguments.Add("--output");
      arguments.Add(OutputPath);
    }

    // Add verbosity if specified
    if (!string.IsNullOrWhiteSpace(Verbosity))
    {
      arguments.Add("--verbosity");
      arguments.Add(Verbosity);
    }

    // Add terminal logger if specified
    if (!string.IsNullOrWhiteSpace(TerminalLogger))
    {
      arguments.Add("--tl");
      arguments.Add(TerminalLogger);
    }

    // Add boolean flags
    if (NoRestore)
    {
      arguments.Add("--no-restore");
    }

    if (NoDependencies)
    {
      arguments.Add("--no-dependencies");
    }

    if (NoIncremental)
    {
      arguments.Add("--no-incremental");
    }

    if (NoLogo)
    {
      arguments.Add("--nologo");
    }

    if (Force)
    {
      arguments.Add("--force");
    }

    if (Interactive)
    {
      arguments.Add("--interactive");
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