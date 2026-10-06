#region Purpose
// Fluent builder for dotnet watch.
#endregion

#region Design
// Include, exclude, and property flags are omitted. dotnet watch does not define them and ignores those arguments.
// Run, test, and build builders forward validation to the parent. Build reads options after that update.
// The target framework method is WithFramework, matching the other dotnet builders.
#endregion

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
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
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
  /// Specifies the target framework to build for.
  /// </summary>
  /// <param name="framework">The target framework moniker</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuilder WithFramework(string framework)
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

  /// <summary>
  /// Creates a builder for the <c>dotnet watch run</c> command using the given parent watch builder.
  /// </summary>
  /// <param name="watchBuilder">Watch builder whose project and watch options this run command inherits.</param>
  public DotNetWatchRunBuilder(DotNetWatchBuilder watchBuilder)
  {
    WatchBuilder = watchBuilder;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchRunBuilder WithNoValidation()
  {
    WatchBuilder.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchRunBuilder WithZeroExitCodeValidation()
  {
    WatchBuilder.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet watch run</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = WatchBuilder.BuildBaseArguments();
    arguments.Add("run");
    arguments.AddRange(WatchBuilder.GetAdditionalArguments());

    return Shell.Run("dotnet", arguments.ToArray(), WatchBuilder.GetOptions());
  }

  /// <summary>
  /// Runs <c>dotnet watch run</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet watch run</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet watch run</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet watch run</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet watch run</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet watch test' commands.
/// </summary>
public class DotNetWatchTestBuilder
{
  private readonly DotNetWatchBuilder WatchBuilder;

  /// <summary>
  /// Creates a builder for the <c>dotnet watch test</c> command using the given parent watch builder.
  /// </summary>
  /// <param name="watchBuilder">Watch builder whose project and watch options this test command inherits.</param>
  public DotNetWatchTestBuilder(DotNetWatchBuilder watchBuilder)
  {
    WatchBuilder = watchBuilder;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchTestBuilder WithNoValidation()
  {
    WatchBuilder.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchTestBuilder WithZeroExitCodeValidation()
  {
    WatchBuilder.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet watch test</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = WatchBuilder.BuildBaseArguments();
    arguments.Add("test");
    arguments.AddRange(WatchBuilder.GetAdditionalArguments());

    return Shell.Run("dotnet", arguments.ToArray(), WatchBuilder.GetOptions());
  }

  /// <summary>
  /// Runs <c>dotnet watch test</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet watch test</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet watch test</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet watch test</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet watch test</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet watch build' commands.
/// </summary>
public class DotNetWatchBuildBuilder
{
  private readonly DotNetWatchBuilder WatchBuilder;

  /// <summary>
  /// Creates a builder for the <c>dotnet watch build</c> command using the given parent watch builder.
  /// </summary>
  /// <param name="watchBuilder">Watch builder whose project and watch options this build command inherits.</param>
  public DotNetWatchBuildBuilder(DotNetWatchBuilder watchBuilder)
  {
    WatchBuilder = watchBuilder;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuildBuilder WithNoValidation()
  {
    WatchBuilder.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWatchBuildBuilder WithZeroExitCodeValidation()
  {
    WatchBuilder.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet watch build</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = WatchBuilder.BuildBaseArguments();
    arguments.Add("build");
    arguments.AddRange(WatchBuilder.GetAdditionalArguments());

    return Shell.Run("dotnet", arguments.ToArray(), WatchBuilder.GetOptions());
  }

  /// <summary>
  /// Runs <c>dotnet watch build</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet watch build</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet watch build</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet watch build</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet watch build</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}
