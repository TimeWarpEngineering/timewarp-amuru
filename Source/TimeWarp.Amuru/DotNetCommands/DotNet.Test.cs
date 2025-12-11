namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Test command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet test' command.
  /// </summary>
  /// <returns>A DotNetTestBuilder for configuring the dotnet test command</returns>
  public static DotNetTestBuilder Test()
  {
    return new DotNetTestBuilder();
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet test' commands.
/// </summary>
public class DotNetTestBuilder : ICommandBuilder<DotNetTestBuilder>
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
  private string? Filter;
  private string? TestAdapterPath;
  private string? ResultsDirectory;
  private string? SettingsFile;
  private bool NoRestore;
  private bool NoBuild;
  private bool NoLogo;
  private bool Blame;
  private bool Collect;
  private List<string> Loggers = new();
  private Dictionary<string, string> Properties = new();
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the project file to test. If not specified, searches the current directory for one.
  /// </summary>
  /// <param name="project">Path to the project file (.csproj, .fsproj, .vbproj) or directory containing one</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithProject(string project)
  {
    Project = project;
    return this;
  }

  /// <summary>
  /// Specifies the configuration to test (Debug or Release).
  /// </summary>
  /// <param name="configuration">The configuration name (e.g., "Debug", "Release")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithConfiguration(string configuration)
  {
    Configuration = configuration;
    return this;
  }

  /// <summary>
  /// Specifies the target framework to test for.
  /// </summary>
  /// <param name="framework">The target framework moniker (e.g., "net8.0", "net10.0")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithFramework(string framework)
  {
    Framework = framework;
    return this;
  }

  /// <summary>
  /// Specifies the target runtime to test for.
  /// </summary>
  /// <param name="runtime">The runtime identifier (e.g., "win-x64", "linux-x64")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithRuntime(string runtime)
  {
    Runtime = runtime;
    return this;
  }

  /// <summary>
  /// Specifies the target architecture.
  /// </summary>
  /// <param name="architecture">The target architecture (e.g., "x64", "arm64")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithArchitecture(string architecture)
  {
    Architecture = architecture;
    return this;
  }

  /// <summary>
  /// Specifies the target operating system.
  /// </summary>
  /// <param name="operatingSystem">The target OS (e.g., "win", "linux", "osx")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithOperatingSystem(string operatingSystem)
  {
    OperatingSystem = operatingSystem;
    return this;
  }

  /// <summary>
  /// Specifies the output directory for built artifacts.
  /// </summary>
  /// <param name="outputPath">The output directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithOutput(string outputPath)
  {
    OutputPath = outputPath;
    return this;
  }

  /// <summary>
  /// Sets the verbosity level of the command.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, detailed, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Sets the terminal logger for test output.
  /// </summary>
  /// <param name="mode">The terminal logger mode ("auto", "on", "off")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithTerminalLogger(string mode)
  {
    TerminalLogger = mode;
    return this;
  }

  /// <summary>
  /// Filters tests based on the given expression.
  /// </summary>
  /// <param name="filter">The filter expression (e.g., "Category=Unit", "TestCategory=Integration")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithFilter(string filter)
  {
    Filter = filter;
    return this;
  }

  /// <summary>
  /// Specifies the path to the test adapter.
  /// </summary>
  /// <param name="path">The path to the test adapter</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithTestAdapterPath(string path)
  {
    TestAdapterPath = path;
    return this;
  }

  /// <summary>
  /// Specifies the directory where test results are stored.
  /// </summary>
  /// <param name="directory">The results directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithResultsDirectory(string directory)
  {
    ResultsDirectory = directory;
    return this;
  }

  /// <summary>
  /// Specifies the settings file to use for running tests.
  /// </summary>
  /// <param name="settingsFile">The path to the settings file</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithSettings(string settingsFile)
  {
    SettingsFile = settingsFile;
    return this;
  }

  /// <summary>
  /// Adds a logger for test results.
  /// </summary>
  /// <param name="logger">The logger specification (e.g., "console", "trx", "html")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithLogger(string logger)
  {
    Loggers.Add(logger);
    return this;
  }

  /// <summary>
  /// Skips the implicit restore during the test command.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithNoRestore()
  {
    NoRestore = true;
    return this;
  }

  /// <summary>
  /// Skips the implicit build during the test command.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithNoBuild()
  {
    NoBuild = true;
    return this;
  }

  /// <summary>
  /// Doesn't display the startup banner or the copyright message.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithNoLogo()
  {
    NoLogo = true;
    return this;
  }

  /// <summary>
  /// Runs the tests in blame mode, which is helpful for diagnosing test host crashes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithBlame()
  {
    Blame = true;
    return this;
  }

  /// <summary>
  /// Enables code coverage data collection.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithCollect()
  {
    Collect = true;
    return this;
  }

  /// <summary>
  /// Sets an MSBuild property.
  /// </summary>
  /// <param name="name">The property name</param>
  /// <param name="value">The property value</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithProperty(string name, string value)
  {
    Properties[name] = value;
    return this;
  }

  /// <summary>
  /// Sets multiple MSBuild properties.
  /// </summary>
  /// <param name="properties">Dictionary of property names and values</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithProperties(Dictionary<string, string> properties)
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
  public DotNetTestBuilder WithWorkingDirectory(string directory)
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
  public DotNetTestBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Builds the command arguments and executes the dotnet test command.
  /// </summary>
  /// <returns>A CommandResult for further processing</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "test" };

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

    // Add filter if specified
    if (!string.IsNullOrWhiteSpace(Filter))
    {
      arguments.Add("--filter");
      arguments.Add(Filter);
    }

    // Add test adapter path if specified
    if (!string.IsNullOrWhiteSpace(TestAdapterPath))
    {
      arguments.Add("--test-adapter-path");
      arguments.Add(TestAdapterPath);
    }

    // Add results directory if specified
    if (!string.IsNullOrWhiteSpace(ResultsDirectory))
    {
      arguments.Add("--results-directory");
      arguments.Add(ResultsDirectory);
    }

    // Add settings file if specified
    if (!string.IsNullOrWhiteSpace(SettingsFile))
    {
      arguments.Add("--settings");
      arguments.Add(SettingsFile);
    }

    // Add loggers
    foreach (string logger in Loggers)
    {
      arguments.Add("--logger");
      arguments.Add(logger);
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

    if (NoLogo)
    {
      arguments.Add("--nologo");
    }

    if (Blame)
    {
      arguments.Add("--blame");
    }

    if (Collect)
    {
      arguments.Add("--collect");
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
  /// Executes the command with true TTY passthrough for TUI applications.
  /// Unlike PassthroughAsync which pipes Console streams, this method
  /// allows the child process to inherit the terminal's TTY characteristics.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings will be empty since output is inherited)</returns>
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