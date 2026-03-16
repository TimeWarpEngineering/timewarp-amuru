namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Publish command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet publish' command.
  /// </summary>
  /// <returns>A DotNetPublishBuilder for configuring the dotnet publish command</returns>
  public static DotNetPublishBuilder Publish()
  {
    return new DotNetPublishBuilder();
  }
  
  /// <summary>
  /// Creates a fluent builder for the 'dotnet publish' command with a specific project.
  /// </summary>
  /// <param name="project">The project file path</param>
  /// <returns>A DotNetPublishBuilder for configuring the dotnet publish command</returns>
  public static DotNetPublishBuilder Publish(string project)
  {
    return new DotNetPublishBuilder().WithProject(project);
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet publish' commands.
/// </summary>
public class DotNetPublishBuilder : ICommandBuilder<DotNetPublishBuilder>
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
  private string? ManifestFile;
  private bool NoRestore;
  private bool NoBuild;
  private bool NoDependencies;
  private bool NoLogo;
  private bool SelfContained;
  private bool NoSelfContained;
  private bool Force;
  private bool Interactive;
  private bool PublishReadyToRun;
  private bool PublishSingleFile;
  private bool PublishTrimmed;
  private List<string> Sources = new();
  private Dictionary<string, string> Properties = new();
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the project file to publish. If not specified, searches the current directory for one.
  /// </summary>
  /// <param name="project">Path to the project file (.csproj, .fsproj, .vbproj) or directory containing one</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithProject(string project)
  {
    Project = project;
    return this;
  }

  /// <summary>
  /// Specifies the configuration to publish (Debug or Release).
  /// </summary>
  /// <param name="configuration">The configuration name (e.g., "Debug", "Release")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithConfiguration(string configuration)
  {
    Configuration = configuration;
    return this;
  }

  /// <summary>
  /// Specifies the target framework to publish for.
  /// </summary>
  /// <param name="framework">The target framework moniker (e.g., "net8.0", "net10.0")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithFramework(string framework)
  {
    Framework = framework;
    return this;
  }

  /// <summary>
  /// Specifies the target runtime to publish for.
  /// </summary>
  /// <param name="runtime">The runtime identifier (e.g., "win-x64", "linux-x64")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithRuntime(string runtime)
  {
    Runtime = runtime;
    return this;
  }

  /// <summary>
  /// Specifies the target architecture.
  /// </summary>
  /// <param name="architecture">The target architecture (e.g., "x64", "arm64")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithArchitecture(string architecture)
  {
    Architecture = architecture;
    return this;
  }

  /// <summary>
  /// Specifies the target operating system.
  /// </summary>
  /// <param name="operatingSystem">The target OS (e.g., "win", "linux", "osx")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithOperatingSystem(string operatingSystem)
  {
    OperatingSystem = operatingSystem;
    return this;
  }

  /// <summary>
  /// Specifies the output directory for published artifacts.
  /// </summary>
  /// <param name="outputPath">The output directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithOutput(string outputPath)
  {
    OutputPath = outputPath;
    return this;
  }

  /// <summary>
  /// Sets the verbosity level of the command.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, detailed, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Sets the terminal logger for publish output.
  /// </summary>
  /// <param name="mode">The terminal logger mode ("auto", "on", "off")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithTerminalLogger(string mode)
  {
    TerminalLogger = mode;
    return this;
  }

  /// <summary>
  /// Specifies the path to a target manifest file.
  /// </summary>
  /// <param name="manifestFile">The manifest file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithManifest(string manifestFile)
  {
    ManifestFile = manifestFile;
    return this;
  }

  /// <summary>
  /// Skips the implicit restore during the publish command.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithNoRestore()
  {
    NoRestore = true;
    return this;
  }

  /// <summary>
  /// Skips the implicit build during the publish command.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithNoBuild()
  {
    NoBuild = true;
    return this;
  }

  /// <summary>
  /// Skips project-to-project references and only publishes the root project.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithNoDependencies()
  {
    NoDependencies = true;
    return this;
  }

  /// <summary>
  /// Doesn't display the startup banner or the copyright message.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithNoLogo()
  {
    NoLogo = true;
    return this;
  }

  /// <summary>
  /// Publishes the .NET runtime with the application (self-contained deployment).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithSelfContained()
  {
    SelfContained = true;
    NoSelfContained = false;
    return this;
  }

  /// <summary>
  /// Publishes the application as a framework-dependent application.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithNoSelfContained()
  {
    NoSelfContained = true;
    SelfContained = false;
    return this;
  }

  /// <summary>
  /// Forces all dependencies to be resolved even if the last restore was successful.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithForce()
  {
    Force = true;
    return this;
  }

  /// <summary>
  /// Allows the command to pause for user input or action.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithInteractive()
  {
    Interactive = true;
    return this;
  }

  /// <summary>
  /// Compiles application assemblies as ReadyToRun (R2R) format.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithReadyToRun()
  {
    PublishReadyToRun = true;
    return this;
  }

  /// <summary>
  /// Produces a single-file application.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithSingleFile()
  {
    PublishSingleFile = true;
    return this;
  }

  /// <summary>
  /// Trims unused code from the application.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithTrimmed()
  {
    PublishTrimmed = true;
    return this;
  }

  /// <summary>
  /// Adds a NuGet package source to use during the restore operation.
  /// </summary>
  /// <param name="source">The URI of the NuGet package source</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithSource(string source)
  {
    Sources.Add(source);
    return this;
  }

  /// <summary>
  /// Adds multiple NuGet package sources to use during the restore operation.
  /// </summary>
  /// <param name="sources">The URIs of the NuGet package sources</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithSources(params string[] sources)
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
  public DotNetPublishBuilder WithProperty(string name, string value)
  {
    Properties[name] = value;
    return this;
  }

  /// <summary>
  /// Sets multiple MSBuild properties.
  /// </summary>
  /// <param name="properties">Dictionary of property names and values</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithProperties(Dictionary<string, string> properties)
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
  public DotNetPublishBuilder WithWorkingDirectory(string directory)
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
  public DotNetPublishBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Builds the command arguments and executes the dotnet publish command.
  /// </summary>
  /// <returns>A CommandResult for further processing</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "publish" };

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

    // Add manifest file if specified
    if (!string.IsNullOrWhiteSpace(ManifestFile))
    {
      arguments.Add("--manifest");
      arguments.Add(ManifestFile);
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

    if (SelfContained)
    {
      arguments.Add("--self-contained");
    }

    if (NoSelfContained)
    {
      arguments.Add("--no-self-contained");
    }

    if (Force)
    {
      arguments.Add("--force");
    }

    if (Interactive)
    {
      arguments.Add("--interactive");
    }

    // Add MSBuild properties for advanced features
    if (PublishReadyToRun)
    {
      Properties["PublishReadyToRun"] = "true";
    }

    if (PublishSingleFile)
    {
      Properties["PublishSingleFile"] = "true";
    }

    if (PublishTrimmed)
    {
      Properties["PublishTrimmed"] = "true";
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

  public async Task<ExecutionResult> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken);
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