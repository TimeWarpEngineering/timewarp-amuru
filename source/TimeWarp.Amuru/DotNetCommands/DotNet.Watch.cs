namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Watch command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet watch' command.
  /// </summary>
  /// <returns>A DotNetWatchBuilder for configuring the dotnet watch command</returns>
  public static DotNetWatchBuilder Watch()
  {
    return new DotNetWatchBuilder();
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet watch' commands.
/// </summary>
public class DotNetWatchBuilder
{
  private CommandOptions Options = new();
  private string? Project;
  private bool Quiet;
  private bool Verbose;
  private bool List;
  private bool NoRestore;
  private bool NoLaunchProfile;
  private bool NoHotReload;
  private bool NoBuild;
  private List<string> Include = new();
  private List<string> Exclude = new();
  private List<string> Properties = new();
  private string? TargetFramework;
  private string? Configuration;
  private string? Runtime;
  private string? Verbosity;
  private string? LaunchProfile;
  private List<string> AdditionalArguments = new();

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithWorkingDirectory(string directory)
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
  public DotNetWatchBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Specifies the project file to watch.
  /// </summary>
  /// <param name="project">The project file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithProject(string project)
  {
    Project = project;
    return this;
  }

  /// <summary>
  /// Suppresses informational messages.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithQuiet()
  {
    Quiet = true;
    return this;
  }

  /// <summary>
  /// Shows verbose output.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithVerbose()
  {
    Verbose = true;
    return this;
  }

  /// <summary>
  /// Lists all watched files and exits.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithList()
  {
    List = true;
    return this;
  }

  /// <summary>
  /// Doesn't restore the project before building.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithNoRestore()
  {
    NoRestore = true;
    return this;
  }

  /// <summary>
  /// Doesn't use launch profiles when running the application.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithNoLaunchProfile()
  {
    NoLaunchProfile = true;
    return this;
  }

  /// <summary>
  /// Disables hot reload functionality.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithNoHotReload()
  {
    NoHotReload = true;
    return this;
  }

  /// <summary>
  /// Doesn't build the project before running.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithNoBuild()
  {
    NoBuild = true;
    return this;
  }

  /// <summary>
  /// Adds a file pattern to include in watching.
  /// </summary>
  /// <param name="pattern">The file pattern to include</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithInclude(string pattern)
  {
    Include.Add(pattern);
    return this;
  }

  /// <summary>
  /// Adds a file pattern to exclude from watching.
  /// </summary>
  /// <param name="pattern">The file pattern to exclude</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithExclude(string pattern)
  {
    Exclude.Add(pattern);
    return this;
  }

  /// <summary>
  /// Adds a build property.
  /// </summary>
  /// <param name="property">The build property in key=value format</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithProperty(string property)
  {
    Properties.Add(property);
    return this;
  }

  /// <summary>
  /// Specifies the target framework to build for.
  /// </summary>
  /// <param name="framework">The target framework moniker</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithTargetFramework(string framework)
  {
    TargetFramework = framework;
    return this;
  }

  /// <summary>
  /// Specifies the build configuration.
  /// </summary>
  /// <param name="configuration">The build configuration</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithConfiguration(string configuration)
  {
    Configuration = configuration;
    return this;
  }

  /// <summary>
  /// Specifies the target runtime to build for.
  /// </summary>
  /// <param name="runtime">The target runtime identifier</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithRuntime(string runtime)
  {
    Runtime = runtime;
    return this;
  }

  /// <summary>
  /// Specifies the verbosity level of the output.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, detailed, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Specifies the launch profile to use.
  /// </summary>
  /// <param name="launchProfile">The launch profile name</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithLaunchProfile(string launchProfile)
  {
    LaunchProfile = launchProfile;
    return this;
  }

  /// <summary>
  /// Adds additional arguments to pass to the application.
  /// </summary>
  /// <param name="arguments">The arguments to pass to the application</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithArguments(params string[] arguments)
  {
    AdditionalArguments.AddRange(arguments);
    return this;
  }

  /// <summary>
  /// Adds a single additional argument to pass to the application.
  /// </summary>
  /// <param name="argument">The argument to pass to the application</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithArgument(string argument)
  {
    AdditionalArguments.Add(argument);
    return this;
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet watch run' command.
  /// </summary>
  /// <returns>A DotNetWatchRunBuilder for configuring the watch run command</returns>
  public DotNetWatchRunBuilder Run()
  {
    return new DotNetWatchRunBuilder(this);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet watch test' command.
  /// </summary>
  /// <returns>A DotNetWatchTestBuilder for configuring the watch test command</returns>
  public DotNetWatchTestBuilder Test()
  {
    return new DotNetWatchTestBuilder(this);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet watch build' command.
  /// </summary>
  /// <returns>A DotNetWatchBuildBuilder for configuring the watch build command</returns>
  public DotNetWatchBuildBuilder Build()
  {
    return new DotNetWatchBuildBuilder(this);
  }

  internal List<string> BuildBaseArguments()
  {
    List<string> arguments = new() { "watch" };

    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
      arguments.Add(Project);
    }

    if (Quiet)
    {
      arguments.Add("--quiet");
    }

    if (Verbose)
    {
      arguments.Add("--verbose");
    }

    if (List)
    {
      arguments.Add("--list");
    }

    if (NoRestore)
    {
      arguments.Add("--no-restore");
    }

    if (NoLaunchProfile)
    {
      arguments.Add("--no-launch-profile");
    }

    if (NoHotReload)
    {
      arguments.Add("--no-hot-reload");
    }

    if (NoBuild)
    {
      arguments.Add("--no-build");
    }

    foreach (string pattern in Include)
    {
      arguments.Add("--include");
      arguments.Add(pattern);
    }

    foreach (string pattern in Exclude)
    {
      arguments.Add("--exclude");
      arguments.Add(pattern);
    }

    foreach (string property in Properties)
    {
      arguments.Add("--property");
      arguments.Add(property);
    }

    if (!string.IsNullOrWhiteSpace(TargetFramework))
    {
      arguments.Add("--framework");
      arguments.Add(TargetFramework);
    }

    if (!string.IsNullOrWhiteSpace(Configuration))
    {
      arguments.Add("--configuration");
      arguments.Add(Configuration);
    }

    if (!string.IsNullOrWhiteSpace(Runtime))
    {
      arguments.Add("--runtime");
      arguments.Add(Runtime);
    }

    if (!string.IsNullOrWhiteSpace(Verbosity))
    {
      arguments.Add("--verbosity");
      arguments.Add(Verbosity);
    }

    if (!string.IsNullOrWhiteSpace(LaunchProfile))
    {
      arguments.Add("--launch-profile");
      arguments.Add(LaunchProfile);
    }

    return arguments;
  }

  internal CommandOptions GetOptions() => Options;
  internal List<string> GetAdditionalArguments() => AdditionalArguments;
}

/// <summary>
/// Fluent builder for 'dotnet watch run' commands.
/// </summary>
public class DotNetWatchRunBuilder
{
  private readonly DotNetWatchBuilder WatchBuilder;

  public DotNetWatchRunBuilder(DotNetWatchBuilder watchBuilder)
  {
    WatchBuilder = watchBuilder;
  }

  public CommandResult Build()
  {
    List<string> arguments = WatchBuilder.BuildBaseArguments();
    arguments.Add("run");
    arguments.AddRange(WatchBuilder.GetAdditionalArguments());

    return CommandExtensions.Run("dotnet", arguments.ToArray(), WatchBuilder.GetOptions());
  }

  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken);
  }

  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken);
  }
  
  public async Task<ExecutionResult> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken);
  }

  public async Task<ExecutionResult> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken);
  }
  
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken);
  }
}

/// <summary>
/// Fluent builder for 'dotnet watch test' commands.
/// </summary>
public class DotNetWatchTestBuilder
{
  private readonly DotNetWatchBuilder WatchBuilder;

  public DotNetWatchTestBuilder(DotNetWatchBuilder watchBuilder)
  {
    WatchBuilder = watchBuilder;
  }

  public CommandResult Build()
  {
    List<string> arguments = WatchBuilder.BuildBaseArguments();
    arguments.Add("test");
    arguments.AddRange(WatchBuilder.GetAdditionalArguments());

    return CommandExtensions.Run("dotnet", arguments.ToArray(), WatchBuilder.GetOptions());
  }

  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken);
  }

  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken);
  }
  
  public async Task<ExecutionResult> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken);
  }

  public async Task<ExecutionResult> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken);
  }
  
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken);
  }
}

/// <summary>
/// Fluent builder for 'dotnet watch build' commands.
/// </summary>
public class DotNetWatchBuildBuilder
{
  private readonly DotNetWatchBuilder WatchBuilder;

  public DotNetWatchBuildBuilder(DotNetWatchBuilder watchBuilder)
  {
    WatchBuilder = watchBuilder;
  }

  public CommandResult Build()
  {
    List<string> arguments = WatchBuilder.BuildBaseArguments();
    arguments.Add("build");
    arguments.AddRange(WatchBuilder.GetAdditionalArguments());

    return CommandExtensions.Run("dotnet", arguments.ToArray(), WatchBuilder.GetOptions());
  }

  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken);
  }

  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken);
  }
  
  public async Task<ExecutionResult> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken);
  }

  public async Task<ExecutionResult> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken);
  }
  
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken);
  }
}