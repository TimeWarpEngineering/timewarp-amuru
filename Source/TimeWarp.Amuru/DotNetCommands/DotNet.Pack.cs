namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Pack command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet pack' command.
  /// </summary>
  /// <returns>A DotNetPackBuilder for configuring the dotnet pack command</returns>
  public static DotNetPackBuilder Pack()
  {
    return new DotNetPackBuilder();
  }
  
  /// <summary>
  /// Creates a fluent builder for the 'dotnet pack' command with a specific project.
  /// </summary>
  /// <param name="project">The project file path</param>
  /// <returns>A DotNetPackBuilder for configuring the dotnet pack command</returns>
  public static DotNetPackBuilder Pack(string project)
  {
    return new DotNetPackBuilder().WithProject(project);
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet pack' commands.
/// </summary>
public class DotNetPackBuilder : ICommandBuilder<DotNetPackBuilder>
{
  private string? Project;
  private string? Configuration;
  private string? Framework;
  private string? Runtime;
  private string? OutputPath;
  private string? Verbosity;
  private string? TerminalLogger;
  private string? VersionSuffix;
  private bool NoRestore;
  private bool NoBuild;
  private bool NoDependencies;
  private bool NoLogo;
  private bool IncludesSymbols;
  private bool IncludesSource;
  private bool Force;
  private bool Interactive;
  private bool Serviceable;
  private readonly List<string> Sources = [];
  private readonly Dictionary<string, string> Properties = [];
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the project file to pack. If not specified, searches the current directory for one.
  /// </summary>
  /// <param name="project">Path to the project file (.csproj, .fsproj, .vbproj) or directory containing one</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithProject(string project)
  {
    Project = project;
    return this;
  }

  /// <summary>
  /// Specifies the configuration to pack (Debug or Release).
  /// </summary>
  /// <param name="configuration">The configuration name (e.g., "Debug", "Release")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithConfiguration(string configuration)
  {
    Configuration = configuration;
    return this;
  }

  /// <summary>
  /// Specifies the target framework to pack for.
  /// </summary>
  /// <param name="framework">The target framework moniker (e.g., "net8.0", "net10.0")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithFramework(string framework)
  {
    Framework = framework;
    return this;
  }

  /// <summary>
  /// Specifies the target runtime to pack for.
  /// </summary>
  /// <param name="runtime">The runtime identifier (e.g., "win-x64", "linux-x64")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithRuntime(string runtime)
  {
    Runtime = runtime;
    return this;
  }

  /// <summary>
  /// Specifies the output directory for packed artifacts.
  /// </summary>
  /// <param name="outputPath">The output directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithOutput(string outputPath)
  {
    OutputPath = outputPath;
    return this;
  }

  /// <summary>
  /// Sets the verbosity level of the command.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, detailed, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Sets the terminal logger for pack output.
  /// </summary>
  /// <param name="mode">The terminal logger mode ("auto", "on", "off")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithTerminalLogger(string mode)
  {
    TerminalLogger = mode;
    return this;
  }

  /// <summary>
  /// Specifies the version suffix for the package.
  /// </summary>
  /// <param name="suffix">The version suffix (e.g., "beta", "rc1")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithVersionSuffix(string suffix)
  {
    VersionSuffix = suffix;
    return this;
  }

  /// <summary>
  /// Skips the implicit restore during the pack command.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithNoRestore()
  {
    NoRestore = true;
    return this;
  }

  /// <summary>
  /// Skips the implicit build during the pack command.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithNoBuild()
  {
    NoBuild = true;
    return this;
  }

  /// <summary>
  /// Skips project-to-project references and only packs the root project.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithNoDependencies()
  {
    NoDependencies = true;
    return this;
  }

  /// <summary>
  /// Doesn't display the startup banner or the copyright message.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithNoLogo()
  {
    NoLogo = true;
    return this;
  }

  /// <summary>
  /// Creates the symbols package (.snupkg).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder IncludeSymbols()
  {
    IncludesSymbols = true;
    return this;
  }

  /// <summary>
  /// Creates the symbols package with a src folder inside containing the source files.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder IncludeSource()
  {
    IncludesSource = true;
    return this;
  }

  /// <summary>
  /// Forces all dependencies to be resolved even if the last restore was successful.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithForce()
  {
    Force = true;
    return this;
  }

  /// <summary>
  /// Allows the command to pause for user input or action.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithInteractive()
  {
    Interactive = true;
    return this;
  }

  /// <summary>
  /// Sets the serviceable flag in the package manifest.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithServiceable()
  {
    Serviceable = true;
    return this;
  }

  /// <summary>
  /// Adds a NuGet package source to use during the restore operation.
  /// </summary>
  /// <param name="source">The URI of the NuGet package source</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithSource(string source)
  {
    Sources.Add(source);
    return this;
  }

  /// <summary>
  /// Sets an MSBuild property.
  /// </summary>
  /// <param name="name">The property name</param>
  /// <param name="value">The property value</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithProperty(string name, string value)
  {
    Properties[name] = value;
    return this;
  }

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithWorkingDirectory(string directory)
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
  public DotNetPackBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Builds the command arguments and executes the dotnet pack command.
  /// </summary>
  /// <returns>A CommandResult for further processing</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "pack" };

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

    // Add version suffix if specified
    if (!string.IsNullOrWhiteSpace(VersionSuffix))
    {
      arguments.Add("--version-suffix");
      arguments.Add(VersionSuffix);
    }

    // Add sources
    foreach (string source in Sources)
    {
      arguments.Add("--source");
      arguments.Add(source);
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

    if (NoLogo)
    {
      arguments.Add("--nologo");
    }

    if (IncludesSymbols)
    {
      arguments.Add("--include-symbols");
    }

    if (IncludesSource)
    {
      arguments.Add("--include-source");
    }

    if (Force)
    {
      arguments.Add("--force");
    }

    if (Interactive)
    {
      arguments.Add("--interactive");
    }

    if (Serviceable)
    {
      arguments.Add("--serviceable");
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
  /// Executes the command, streams output to console AND captures it.
  /// Useful for debugging/logging scenarios where you want to see output and save it.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>CommandOutput with stdout, stderr, combined output and exit code</returns>
  public async Task<CommandOutput> RunAndCaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAndCaptureAsync(cancellationToken);
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