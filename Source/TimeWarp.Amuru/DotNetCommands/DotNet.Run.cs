namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Run command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet run' command.
  /// </summary>
  /// <returns>A DotNetRunBuilder for configuring the dotnet run command</returns>
  public static DotNetRunBuilder Run()
  {
    return new DotNetRunBuilder();
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet run' commands.
/// </summary>
public class DotNetRunBuilder : ICommandBuilder<DotNetRunBuilder>
{
  private string? Project;
  private string? Configuration;
  private string? Framework;
  private string? Runtime;
  private string? Architecture;
  private string? OperatingSystem;
  private string? LaunchProfile;
  private bool NoRestore;
  private bool NoBuild;
  private bool NoDependencies;
  private bool NoLaunchProfile;
  private bool Force;
  private bool Interactive;
  private string? Verbosity;
  private string? TerminalLogger;
  private readonly Dictionary<string, string> Properties = [];
  private readonly Dictionary<string, string> EnvironmentVariables = [];
  private string[]? ProgramArguments;
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the project file to run. If not specified, searches the current directory for one.
  /// </summary>
  /// <param name="project">Path to the project file (.csproj, .fsproj, .vbproj) or directory containing one</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithProject(string project)
  {
    Project = project;
    return this;
  }

  /// <summary>
  /// Specifies the configuration to use (Debug or Release).
  /// </summary>
  /// <param name="configuration">The configuration name (e.g., "Debug", "Release")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithConfiguration(string configuration)
  {
    Configuration = configuration;
    return this;
  }

  /// <summary>
  /// Specifies the target framework to run for.
  /// </summary>
  /// <param name="framework">The target framework moniker (e.g., "net8.0", "net10.0")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithFramework(string framework)
  {
    Framework = framework;
    return this;
  }

  /// <summary>
  /// Specifies the target runtime to run for.
  /// </summary>
  /// <param name="runtime">The runtime identifier (e.g., "win-x64", "linux-x64")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithRuntime(string runtime)
  {
    Runtime = runtime;
    return this;
  }

  /// <summary>
  /// Skips the implicit restore during the run command.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithNoRestore()
  {
    NoRestore = true;
    return this;
  }

  /// <summary>
  /// Skips the implicit build during the run command.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithNoBuild()
  {
    NoBuild = true;
    return this;
  }

  /// <summary>
  /// Skips project-to-project references and only restores the root project.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithNoDependencies()
  {
    NoDependencies = true;
    return this;
  }

  /// <summary>
  /// Sets the verbosity level of the command.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, detailed, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Specifies arguments to pass to the program being run.
  /// </summary>
  /// <param name="arguments">Arguments to pass to the program</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithArguments(params string[]? arguments)
  {
    ProgramArguments = arguments;
    return this;
  }

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithWorkingDirectory(string directory)
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
  public DotNetRunBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Specifies the target architecture (shorthand for runtime identifier).
  /// </summary>
  /// <param name="architecture">The target architecture (e.g., "x64", "arm64")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithArchitecture(string architecture)
  {
    Architecture = architecture;
    return this;
  }

  /// <summary>
  /// Specifies the target operating system.
  /// </summary>
  /// <param name="operatingSystem">The target OS (e.g., "win", "linux", "osx")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithOperatingSystem(string operatingSystem)
  {
    OperatingSystem = operatingSystem;
    return this;
  }

  /// <summary>
  /// Specifies the launch profile from launchSettings.json.
  /// </summary>
  /// <param name="profileName">The name of the launch profile to use</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithLaunchProfile(string profileName)
  {
    LaunchProfile = profileName;
    return this;
  }

  /// <summary>
  /// Doesn't use launchSettings.json when running the application.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithNoLaunchProfile()
  {
    NoLaunchProfile = true;
    return this;
  }

  /// <summary>
  /// Forces all dependencies to be resolved even if the last restore was successful.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithForce()
  {
    Force = true;
    return this;
  }

  /// <summary>
  /// Allows the command to pause for user input or action.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithInteractive()
  {
    Interactive = true;
    return this;
  }

  /// <summary>
  /// Sets the terminal logger for build output.
  /// </summary>
  /// <param name="mode">The terminal logger mode ("auto", "on", "off")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithTerminalLogger(string mode)
  {
    TerminalLogger = mode;
    return this;
  }

  /// <summary>
  /// Sets an MSBuild property.
  /// </summary>
  /// <param name="name">The property name</param>
  /// <param name="value">The property value</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithProperty(string name, string value)
  {
    Properties[name] = value;
    return this;
  }

  /// <summary>
  /// Sets multiple MSBuild properties.
  /// </summary>
  /// <param name="properties">Dictionary of property names and values</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithProperties(Dictionary<string, string> properties)
  {
    foreach (KeyValuePair<string, string> kvp in properties)
    {
      Properties[kvp.Key] = kvp.Value;
    }
    
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Sets an environment variable for the running process (uses -e option).
  /// </summary>
  /// <param name="key">The environment variable name</param>
  /// <param name="value">The environment variable value</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithProcessEnvironmentVariable(string key, string value)
  {
    EnvironmentVariables[key] = value;
    return this;
  }

  /// <summary>
  /// Builds the command arguments and executes the dotnet run command.
  /// </summary>
  /// <returns>A CommandResult for further processing</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "run" };

    // Add project if specified
    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
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

    // Add launch profile if specified
    if (!string.IsNullOrWhiteSpace(LaunchProfile))
    {
      arguments.Add("--launch-profile");
      arguments.Add(LaunchProfile);
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

    if (NoBuild)
    {
      arguments.Add("--no-build");
    }

    if (NoDependencies)
    {
      arguments.Add("--no-dependencies");
    }

    if (NoLaunchProfile)
    {
      arguments.Add("--no-launch-profile");
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

    // Add process environment variables
    foreach (KeyValuePair<string, string> envVar in EnvironmentVariables)
    {
      arguments.Add("-e");
      arguments.Add($"{envVar.Key}={envVar.Value}");
    }

    // Add program arguments if specified
    if (ProgramArguments?.Length > 0)
    {
      arguments.Add("--");
      arguments.AddRange(ProgramArguments);
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