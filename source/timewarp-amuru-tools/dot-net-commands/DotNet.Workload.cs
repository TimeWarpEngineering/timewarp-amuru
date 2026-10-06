#region Purpose
// TODO: Add purpose description
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Workload command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet workload' command.
  /// </summary>
  /// <returns>A DotNetWorkloadBuilder for configuring the dotnet workload command</returns>
  public static DotNetWorkloadBuilder Workload()
  {
    return new DotNetWorkloadBuilder();
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet workload' commands.
/// </summary>
public class DotNetWorkloadBuilder
{
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadBuilder WithWorkingDirectory(string directory)
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
  public DotNetWorkloadBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Shows detailed information about installed workloads.
  /// </summary>
  /// <returns>A DotNetWorkloadInfoBuilder for configuring the info command</returns>
  public DotNetWorkloadInfoBuilder Info()
  {
    return new DotNetWorkloadInfoBuilder(Options);
  }

  /// <summary>
  /// Shows the current workload set version.
  /// </summary>
  /// <returns>A DotNetWorkloadVersionBuilder for configuring the version command</returns>
  public DotNetWorkloadVersionBuilder Version()
  {
    return new DotNetWorkloadVersionBuilder(Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet workload install' command.
  /// </summary>
  /// <param name="workloadIds">The workload IDs to install</param>
  /// <returns>A DotNetWorkloadInstallBuilder for configuring the install command</returns>
  public DotNetWorkloadInstallBuilder Install(params string[] workloadIds)
  {
    return new DotNetWorkloadInstallBuilder(workloadIds, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet workload list' command.
  /// </summary>
  /// <returns>A DotNetWorkloadListBuilder for configuring the list command</returns>
  public DotNetWorkloadListBuilder List()
  {
    return new DotNetWorkloadListBuilder(Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet workload search' command.
  /// </summary>
  /// <returns>A DotNetWorkloadSearchBuilder for configuring the search command</returns>
  public DotNetWorkloadSearchBuilder Search()
  {
    return new DotNetWorkloadSearchBuilder(null, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet workload search' command with a search term.
  /// </summary>
  /// <param name="searchString">The search term to filter workloads</param>
  /// <returns>A DotNetWorkloadSearchBuilder for configuring the search command</returns>
  public DotNetWorkloadSearchBuilder Search(string searchString)
  {
    return new DotNetWorkloadSearchBuilder(searchString, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet workload uninstall' command.
  /// </summary>
  /// <param name="workloadIds">The workload IDs to uninstall</param>
  /// <returns>A DotNetWorkloadUninstallBuilder for configuring the uninstall command</returns>
  public DotNetWorkloadUninstallBuilder Uninstall(params string[] workloadIds)
  {
    return new DotNetWorkloadUninstallBuilder(workloadIds, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet workload update' command.
  /// </summary>
  /// <returns>A DotNetWorkloadUpdateBuilder for configuring the update command</returns>
  public DotNetWorkloadUpdateBuilder Update()
  {
    return new DotNetWorkloadUpdateBuilder(Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet workload repair' command.
  /// </summary>
  /// <returns>A DotNetWorkloadRepairBuilder for configuring the repair command</returns>
  public DotNetWorkloadRepairBuilder Repair()
  {
    return new DotNetWorkloadRepairBuilder(Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet workload clean' command.
  /// </summary>
  /// <returns>A DotNetWorkloadCleanBuilder for configuring the clean command</returns>
  public DotNetWorkloadCleanBuilder Clean()
  {
    return new DotNetWorkloadCleanBuilder(Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet workload restore' command.
  /// </summary>
  /// <returns>A DotNetWorkloadRestoreBuilder for configuring the restore command</returns>
  public DotNetWorkloadRestoreBuilder Restore()
  {
    return new DotNetWorkloadRestoreBuilder(null, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet workload restore' command with a project or solution.
  /// </summary>
  /// <param name="projectOrSolution">The project or solution file path</param>
  /// <returns>A DotNetWorkloadRestoreBuilder for configuring the restore command</returns>
  public DotNetWorkloadRestoreBuilder Restore(string projectOrSolution)
  {
    return new DotNetWorkloadRestoreBuilder(projectOrSolution, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet workload config' command.
  /// </summary>
  /// <returns>A DotNetWorkloadConfigBuilder for configuring the config command</returns>
  public DotNetWorkloadConfigBuilder Config()
  {
    return new DotNetWorkloadConfigBuilder(Options);
  }
}

/// <summary>
/// Fluent builder for 'dotnet workload --info' commands.
/// </summary>
public class DotNetWorkloadInfoBuilder
{
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet workload --info</c> command.
  /// </summary>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetWorkloadInfoBuilder(CommandOptions options)
  {
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadInfoBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadInfoBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet workload --info</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "--info" };
    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet workload --info</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload --info</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload --info</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload --info</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload --info</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet workload --version' commands.
/// </summary>
public class DotNetWorkloadVersionBuilder
{
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet workload --version</c> command.
  /// </summary>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetWorkloadVersionBuilder(CommandOptions options)
  {
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadVersionBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadVersionBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet workload --version</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "--version" };
    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet workload --version</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload --version</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload --version</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload --version</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload --version</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet workload install' commands.
/// </summary>
public class DotNetWorkloadInstallBuilder
{
  private readonly string[] WorkloadIds;
  private CommandOptions Options;
  private string? ConfigFile;
  private bool IncludePreview;
  private bool SkipManifestUpdate;
  private List<string> Sources = new();
  private string? Version;

  /// <summary>
  /// Creates a builder for the <c>dotnet workload install</c> command using the given workload ids.
  /// </summary>
  /// <param name="workloadIds">Workload ids passed to <c>dotnet workload install</c>.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetWorkloadInstallBuilder(string[] workloadIds, CommandOptions options)
  {
    WorkloadIds = workloadIds ?? throw new ArgumentNullException(nameof(workloadIds));
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadInstallBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadInstallBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadInstallBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  /// <summary>
  /// Allows prerelease workload manifests.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadInstallBuilder WithIncludePreview()
  {
    IncludePreview = true;
    return this;
  }

  /// <summary>
  /// Skips updating workload manifests.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadInstallBuilder WithSkipManifestUpdate()
  {
    SkipManifestUpdate = true;
    return this;
  }

  /// <summary>
  /// Adds a NuGet package source to use during installation.
  /// </summary>
  /// <param name="source">The URI of the NuGet package source</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadInstallBuilder WithSource(string source)
  {
    Sources.Add(source);
    return this;
  }

  /// <summary>
  /// Specifies the workload set version to install.
  /// </summary>
  /// <param name="version">The workload set version</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadInstallBuilder WithVersion(string version)
  {
    Version = version;
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet workload install</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "install" };
    arguments.AddRange(WorkloadIds);

    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
    }

    if (IncludePreview)
    {
      arguments.Add("--include-previews");
    }

    if (SkipManifestUpdate)
    {
      arguments.Add("--skip-manifest-update");
    }

    foreach (string source in Sources)
    {
      arguments.Add("--source");
      arguments.Add(source);
    }

    if (!string.IsNullOrWhiteSpace(Version))
    {
      arguments.Add("--version");
      arguments.Add(Version);
    }

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet workload install</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload install</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload install</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload install</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload install</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet workload list' commands.
/// </summary>
public class DotNetWorkloadListBuilder
{
  private CommandOptions Options;
  private string? Verbosity;

  /// <summary>
  /// Creates a builder for the <c>dotnet workload list</c> command.
  /// </summary>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetWorkloadListBuilder(CommandOptions options)
  {
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadListBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadListBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Specifies the verbosity level of the output.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, detailed, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadListBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet workload list</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "list" };

    if (!string.IsNullOrWhiteSpace(Verbosity))
    {
      arguments.Add("--verbosity");
      arguments.Add(Verbosity);
    }

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet workload list</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload list</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload list</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload list</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload list</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet workload search' commands.
/// </summary>
public class DotNetWorkloadSearchBuilder
{
  private readonly string? SearchString;
  private CommandOptions Options;
  private string? Verbosity;

  /// <summary>
  /// Creates a builder for the <c>dotnet workload search</c> command using the given search text.
  /// </summary>
  /// <param name="searchString">Text matched against workload ids, or null to search with no filter.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetWorkloadSearchBuilder(string? searchString, CommandOptions options)
  {
    SearchString = searchString;
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadSearchBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadSearchBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Specifies the verbosity level of the output.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, detailed, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadSearchBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet workload search</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "search" };

    if (!string.IsNullOrWhiteSpace(SearchString))
    {
      arguments.Add(SearchString);
    }

    if (!string.IsNullOrWhiteSpace(Verbosity))
    {
      arguments.Add("--verbosity");
      arguments.Add(Verbosity);
    }

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet workload search</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload search</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload search</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload search</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload search</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet workload uninstall' commands.
/// </summary>
public class DotNetWorkloadUninstallBuilder
{
  private readonly string[] WorkloadIds;
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet workload uninstall</c> command using the given workload ids.
  /// </summary>
  /// <param name="workloadIds">Workload ids passed to <c>dotnet workload uninstall</c>.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetWorkloadUninstallBuilder(string[] workloadIds, CommandOptions options)
  {
    WorkloadIds = workloadIds ?? throw new ArgumentNullException(nameof(workloadIds));
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadUninstallBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadUninstallBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet workload uninstall</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "uninstall" };
    arguments.AddRange(WorkloadIds);

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet workload uninstall</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload uninstall</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload uninstall</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload uninstall</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload uninstall</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet workload update' commands.
/// </summary>
public class DotNetWorkloadUpdateBuilder
{
  private CommandOptions Options;
  private bool AdvertisingManifestsOnly;
  private string? ConfigFile;
  private bool DisableParallel;
  private bool FromPreviousSdk;
  private bool IncludePreview;
  private bool Interactive;
  private bool NoCache;
  private List<string> Sources = new();
  private string? TempDir;
  private string? Verbosity;

  /// <summary>
  /// Creates a builder for the <c>dotnet workload update</c> command.
  /// </summary>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetWorkloadUpdateBuilder(CommandOptions options)
  {
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadUpdateBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadUpdateBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Downloads advertising manifests without updating workloads.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadUpdateBuilder WithAdvertisingManifestsOnly()
  {
    AdvertisingManifestsOnly = true;
    return this;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadUpdateBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  /// <summary>
  /// Prevents parallel workload restoration.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadUpdateBuilder WithDisableParallel()
  {
    DisableParallel = true;
    return this;
  }

  /// <summary>
  /// Includes workloads from previous SDK versions.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadUpdateBuilder WithFromPreviousSdk()
  {
    FromPreviousSdk = true;
    return this;
  }

  /// <summary>
  /// Allows prerelease workload manifests.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadUpdateBuilder WithIncludePreview()
  {
    IncludePreview = true;
    return this;
  }

  /// <summary>
  /// Allows the command to pause for user input or action.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadUpdateBuilder WithInteractive()
  {
    Interactive = true;
    return this;
  }

  /// <summary>
  /// Prevents package and HTTP request caching.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadUpdateBuilder WithNoCache()
  {
    NoCache = true;
    return this;
  }

  /// <summary>
  /// Adds a NuGet package source to use during update.
  /// </summary>
  /// <param name="source">The URI of the NuGet package source</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadUpdateBuilder WithSource(string source)
  {
    Sources.Add(source);
    return this;
  }

  /// <summary>
  /// Specifies the temporary directory for package downloads.
  /// </summary>
  /// <param name="tempDir">The temporary directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadUpdateBuilder WithTempDir(string tempDir)
  {
    TempDir = tempDir;
    return this;
  }

  /// <summary>
  /// Specifies the verbosity level of the output.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, detailed, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadUpdateBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet workload update</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "update" };

    if (AdvertisingManifestsOnly)
    {
      arguments.Add("--advertising-manifests-only");
    }

    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
    }

    if (DisableParallel)
    {
      arguments.Add("--disable-parallel");
    }

    if (FromPreviousSdk)
    {
      arguments.Add("--from-previous-sdk");
    }

    if (IncludePreview)
    {
      arguments.Add("--include-previews");
    }

    if (Interactive)
    {
      arguments.Add("--interactive");
    }

    if (NoCache)
    {
      arguments.Add("--no-cache");
    }

    foreach (string source in Sources)
    {
      arguments.Add("--source");
      arguments.Add(source);
    }

    if (!string.IsNullOrWhiteSpace(TempDir))
    {
      arguments.Add("--temp-dir");
      arguments.Add(TempDir);
    }

    if (!string.IsNullOrWhiteSpace(Verbosity))
    {
      arguments.Add("--verbosity");
      arguments.Add(Verbosity);
    }

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet workload update</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload update</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload update</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload update</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload update</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet workload repair' commands.
/// </summary>
public class DotNetWorkloadRepairBuilder
{
  private CommandOptions Options;
  private string? ConfigFile;
  private bool DisableParallel;
  private bool IgnoreFailedSources;
  private bool Interactive;
  private bool NoCache;
  private List<string> Sources = new();
  private string? TempDir;
  private string? Verbosity;

  /// <summary>
  /// Creates a builder for the <c>dotnet workload repair</c> command.
  /// </summary>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetWorkloadRepairBuilder(CommandOptions options)
  {
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRepairBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRepairBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRepairBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  /// <summary>
  /// Prevents parallel project restoration.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRepairBuilder WithDisableParallel()
  {
    DisableParallel = true;
    return this;
  }

  /// <summary>
  /// Treats package source failures as warnings.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRepairBuilder WithIgnoreFailedSources()
  {
    IgnoreFailedSources = true;
    return this;
  }

  /// <summary>
  /// Allows the command to pause for user input or action.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRepairBuilder WithInteractive()
  {
    Interactive = true;
    return this;
  }

  /// <summary>
  /// Prevents package and HTTP request caching.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRepairBuilder WithNoCache()
  {
    NoCache = true;
    return this;
  }

  /// <summary>
  /// Adds a NuGet package source to use during repair.
  /// </summary>
  /// <param name="source">The URI of the NuGet package source</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRepairBuilder WithSource(string source)
  {
    Sources.Add(source);
    return this;
  }

  /// <summary>
  /// Specifies the temporary directory for package downloads.
  /// </summary>
  /// <param name="tempDir">The temporary directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRepairBuilder WithTempDir(string tempDir)
  {
    TempDir = tempDir;
    return this;
  }

  /// <summary>
  /// Specifies the verbosity level of the output.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, detailed, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRepairBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet workload repair</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "repair" };

    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
    }

    if (DisableParallel)
    {
      arguments.Add("--disable-parallel");
    }

    if (IgnoreFailedSources)
    {
      arguments.Add("--ignore-failed-sources");
    }

    if (Interactive)
    {
      arguments.Add("--interactive");
    }

    if (NoCache)
    {
      arguments.Add("--no-cache");
    }

    foreach (string source in Sources)
    {
      arguments.Add("--source");
      arguments.Add(source);
    }

    if (!string.IsNullOrWhiteSpace(TempDir))
    {
      arguments.Add("--temp-dir");
      arguments.Add(TempDir);
    }

    if (!string.IsNullOrWhiteSpace(Verbosity))
    {
      arguments.Add("--verbosity");
      arguments.Add(Verbosity);
    }

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet workload repair</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload repair</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload repair</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload repair</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload repair</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet workload clean' commands.
/// </summary>
public class DotNetWorkloadCleanBuilder
{
  private CommandOptions Options;
  private bool All;

  /// <summary>
  /// Creates a builder for the <c>dotnet workload clean</c> command.
  /// </summary>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetWorkloadCleanBuilder(CommandOptions options)
  {
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadCleanBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadCleanBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Cleans all workload packs except those installed by Visual Studio.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadCleanBuilder WithAll()
  {
    All = true;
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet workload clean</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "clean" };

    if (All)
    {
      arguments.Add("--all");
    }

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet workload clean</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload clean</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload clean</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload clean</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload clean</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet workload restore' commands.
/// </summary>
public class DotNetWorkloadRestoreBuilder
{
  private readonly string? ProjectOrSolution;
  private CommandOptions Options;
  private string? ConfigFile;
  private bool DisableParallel;
  private bool IncludePreview;
  private bool Interactive;
  private bool NoCache;
  private List<string> Sources = new();
  private string? TempDir;
  private string? Verbosity;
  private string? Version;

  /// <summary>
  /// Creates a builder for the <c>dotnet workload restore</c> command using the given project or solution.
  /// </summary>
  /// <param name="projectOrSolution">Project or solution whose workloads are restored, or null to search the working directory.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetWorkloadRestoreBuilder(string? projectOrSolution, CommandOptions options)
  {
    ProjectOrSolution = projectOrSolution;
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRestoreBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRestoreBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRestoreBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  /// <summary>
  /// Prevents parallel project restoration.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRestoreBuilder WithDisableParallel()
  {
    DisableParallel = true;
    return this;
  }

  /// <summary>
  /// Allows prerelease workload manifests.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRestoreBuilder WithIncludePreview()
  {
    IncludePreview = true;
    return this;
  }

  /// <summary>
  /// Allows the command to pause for user input or action.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRestoreBuilder WithInteractive()
  {
    Interactive = true;
    return this;
  }

  /// <summary>
  /// Prevents package and HTTP request caching.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRestoreBuilder WithNoCache()
  {
    NoCache = true;
    return this;
  }

  /// <summary>
  /// Adds a NuGet package source to use during restore.
  /// </summary>
  /// <param name="source">The URI of the NuGet package source</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRestoreBuilder WithSource(string source)
  {
    Sources.Add(source);
    return this;
  }

  /// <summary>
  /// Specifies the temporary directory for package downloads.
  /// </summary>
  /// <param name="tempDir">The temporary directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRestoreBuilder WithTempDir(string tempDir)
  {
    TempDir = tempDir;
    return this;
  }

  /// <summary>
  /// Specifies the verbosity level of the output.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, detailed, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRestoreBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Specifies the workload set version to restore.
  /// </summary>
  /// <param name="version">The workload set version</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadRestoreBuilder WithVersion(string version)
  {
    Version = version;
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet workload restore</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "restore" };

    if (!string.IsNullOrWhiteSpace(ProjectOrSolution))
    {
      arguments.Add(ProjectOrSolution);
    }

    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
    }

    if (DisableParallel)
    {
      arguments.Add("--disable-parallel");
    }

    if (IncludePreview)
    {
      arguments.Add("--include-previews");
    }

    if (Interactive)
    {
      arguments.Add("--interactive");
    }

    if (NoCache)
    {
      arguments.Add("--no-cache");
    }

    foreach (string source in Sources)
    {
      arguments.Add("--source");
      arguments.Add(source);
    }

    if (!string.IsNullOrWhiteSpace(TempDir))
    {
      arguments.Add("--temp-dir");
      arguments.Add(TempDir);
    }

    if (!string.IsNullOrWhiteSpace(Verbosity))
    {
      arguments.Add("--verbosity");
      arguments.Add(Verbosity);
    }

    if (!string.IsNullOrWhiteSpace(Version))
    {
      arguments.Add("--version");
      arguments.Add(Version);
    }

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet workload restore</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload restore</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload restore</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload restore</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload restore</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet workload config' commands.
/// </summary>
public class DotNetWorkloadConfigBuilder
{
  private CommandOptions Options;
  private string? UpdateMode;

  /// <summary>
  /// Creates a builder for the <c>dotnet workload config</c> command.
  /// </summary>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetWorkloadConfigBuilder(CommandOptions options)
  {
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadConfigBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadConfigBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Sets the update mode to workload-set.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadConfigBuilder WithUpdateModeWorkloadSet()
  {
    UpdateMode = "workload-set";
    return this;
  }

  /// <summary>
  /// Sets the update mode to manifests.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadConfigBuilder WithUpdateModeManifests()
  {
    UpdateMode = "manifests";
    return this;
  }

  /// <summary>
  /// Sets the update mode to a custom value.
  /// </summary>
  /// <param name="updateMode">The update mode (workload-set or manifests)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetWorkloadConfigBuilder WithUpdateMode(string updateMode)
  {
    UpdateMode = updateMode;
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet workload config</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "config" };

    if (!string.IsNullOrWhiteSpace(UpdateMode))
    {
      arguments.Add("--update-mode");
      arguments.Add(UpdateMode);
    }

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet workload config</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload config</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload config</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet workload config</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet workload config</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}
