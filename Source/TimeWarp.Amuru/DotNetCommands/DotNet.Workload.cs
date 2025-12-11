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
  private readonly CommandOptions Options;

  public DotNetWorkloadInfoBuilder(CommandOptions options)
  {
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "--info" };
    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
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
/// Fluent builder for 'dotnet workload --version' commands.
/// </summary>
public class DotNetWorkloadVersionBuilder
{
  private readonly CommandOptions Options;

  public DotNetWorkloadVersionBuilder(CommandOptions options)
  {
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "--version" };
    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
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
/// Fluent builder for 'dotnet workload install' commands.
/// </summary>
public class DotNetWorkloadInstallBuilder
{
  private readonly string[] WorkloadIds;
  private readonly CommandOptions Options;
  private string? ConfigFile;
  private bool IncludePreview;
  private bool SkipManifestUpdate;
  private List<string> Sources = new();
  private string? Version;

  public DotNetWorkloadInstallBuilder(string[] workloadIds, CommandOptions options)
  {
    WorkloadIds = workloadIds ?? throw new ArgumentNullException(nameof(workloadIds));
    Options = options;
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

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
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
/// Fluent builder for 'dotnet workload list' commands.
/// </summary>
public class DotNetWorkloadListBuilder
{
  private readonly CommandOptions Options;
  private string? Verbosity;

  public DotNetWorkloadListBuilder(CommandOptions options)
  {
    Options = options;
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

  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "list" };

    if (!string.IsNullOrWhiteSpace(Verbosity))
    {
      arguments.Add("--verbosity");
      arguments.Add(Verbosity);
    }

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
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
/// Fluent builder for 'dotnet workload search' commands.
/// </summary>
public class DotNetWorkloadSearchBuilder
{
  private readonly string? SearchString;
  private readonly CommandOptions Options;
  private string? Verbosity;

  public DotNetWorkloadSearchBuilder(string? searchString, CommandOptions options)
  {
    SearchString = searchString;
    Options = options;
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

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
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
/// Fluent builder for 'dotnet workload uninstall' commands.
/// </summary>
public class DotNetWorkloadUninstallBuilder
{
  private readonly string[] WorkloadIds;
  private readonly CommandOptions Options;

  public DotNetWorkloadUninstallBuilder(string[] workloadIds, CommandOptions options)
  {
    WorkloadIds = workloadIds ?? throw new ArgumentNullException(nameof(workloadIds));
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "uninstall" };
    arguments.AddRange(WorkloadIds);

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
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
/// Fluent builder for 'dotnet workload update' commands.
/// </summary>
public class DotNetWorkloadUpdateBuilder
{
  private readonly CommandOptions Options;
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

  public DotNetWorkloadUpdateBuilder(CommandOptions options)
  {
    Options = options;
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

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
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
/// Fluent builder for 'dotnet workload repair' commands.
/// </summary>
public class DotNetWorkloadRepairBuilder
{
  private readonly CommandOptions Options;
  private string? ConfigFile;
  private bool DisableParallel;
  private bool IgnoreFailedSources;
  private bool Interactive;
  private bool NoCache;
  private List<string> Sources = new();
  private string? TempDir;
  private string? Verbosity;

  public DotNetWorkloadRepairBuilder(CommandOptions options)
  {
    Options = options;
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

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
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
/// Fluent builder for 'dotnet workload clean' commands.
/// </summary>
public class DotNetWorkloadCleanBuilder
{
  private readonly CommandOptions Options;
  private bool All;

  public DotNetWorkloadCleanBuilder(CommandOptions options)
  {
    Options = options;
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

  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "clean" };

    if (All)
    {
      arguments.Add("--all");
    }

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
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
/// Fluent builder for 'dotnet workload restore' commands.
/// </summary>
public class DotNetWorkloadRestoreBuilder
{
  private readonly string? ProjectOrSolution;
  private readonly CommandOptions Options;
  private string? ConfigFile;
  private bool DisableParallel;
  private bool IncludePreview;
  private bool Interactive;
  private bool NoCache;
  private List<string> Sources = new();
  private string? TempDir;
  private string? Verbosity;
  private string? Version;

  public DotNetWorkloadRestoreBuilder(string? projectOrSolution, CommandOptions options)
  {
    ProjectOrSolution = projectOrSolution;
    Options = options;
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

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
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
/// Fluent builder for 'dotnet workload config' commands.
/// </summary>
public class DotNetWorkloadConfigBuilder
{
  private readonly CommandOptions Options;
  private string? UpdateMode;

  public DotNetWorkloadConfigBuilder(CommandOptions options)
  {
    Options = options;
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

  public CommandResult Build()
  {
    List<string> arguments = new() { "workload", "config" };

    if (!string.IsNullOrWhiteSpace(UpdateMode))
    {
      arguments.Add("--update-mode");
      arguments.Add(UpdateMode);
    }

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
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