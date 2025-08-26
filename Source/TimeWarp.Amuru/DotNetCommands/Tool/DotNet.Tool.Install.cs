namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for 'dotnet tool install' commands.
/// </summary>
public class DotNetToolInstallBuilder
{
  private readonly string PackageId;
  private readonly CommandOptions Options;
  private bool IsGlobal;
  private bool IsLocal;
  private string? ToolPath;
  private string? Version;
  private string? ConfigFile;
  private string? ToolManifest;
  private bool IncludePrerelease;
  private bool IgnoreFailedSources;
  private bool IsInteractive;
  private List<string> PackageSources = new();
  private string? Framework;
  private string? Arch;

  public DotNetToolInstallBuilder(string packageId, CommandOptions options)
  {
    PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
    Options = options;
  }

  /// <summary>
  /// Installs the tool globally.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolInstallBuilder Global()
  {
    IsGlobal = true;
    IsLocal = false;
    return this;
  }

  /// <summary>
  /// Installs the tool locally (in the current directory).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolInstallBuilder Local()
  {
    IsLocal = true;
    IsGlobal = false;
    return this;
  }

  /// <summary>
  /// Specifies the path where the tool should be installed.
  /// </summary>
  /// <param name="toolPath">The installation path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolInstallBuilder WithToolPath(string toolPath)
  {
    ToolPath = toolPath;
    return this;
  }

  /// <summary>
  /// Specifies the version of the tool to install.
  /// </summary>
  /// <param name="version">The version of the tool</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolInstallBuilder WithVersion(string version)
  {
    Version = version;
    return this;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolInstallBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  /// <summary>
  /// Specifies the path to the tool manifest file.
  /// </summary>
  /// <param name="toolManifest">The tool manifest file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolInstallBuilder WithToolManifest(string toolManifest)
  {
    ToolManifest = toolManifest;
    return this;
  }

  /// <summary>
  /// Allows prerelease packages to be installed.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolInstallBuilder WithPrerelease()
  {
    IncludePrerelease = true;
    return this;
  }

  /// <summary>
  /// Treats package source failures as warnings.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolInstallBuilder WithIgnoreFailedSources()
  {
    IgnoreFailedSources = true;
    return this;
  }

  /// <summary>
  /// Allows the command to pause for user input or action.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolInstallBuilder WithInteractive()
  {
    IsInteractive = true;
    return this;
  }

  /// <summary>
  /// Adds a NuGet package source to use during installation.
  /// </summary>
  /// <param name="source">The URI of the NuGet package source</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolInstallBuilder WithSource(string source)
  {
    PackageSources.Add(source);
    return this;
  }

  /// <summary>
  /// Specifies the target framework for the tool.
  /// </summary>
  /// <param name="framework">The target framework moniker</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolInstallBuilder WithFramework(string framework)
  {
    Framework = framework;
    return this;
  }

  /// <summary>
  /// Specifies the target architecture for the tool.
  /// </summary>
  /// <param name="arch">The target architecture</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolInstallBuilder WithArchitecture(string arch)
  {
    Arch = arch;
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "tool", "install", PackageId };

    if (IsGlobal)
    {
      arguments.Add("--global");
    }

    if (IsLocal)
    {
      arguments.Add("--local");
    }

    if (!string.IsNullOrWhiteSpace(ToolPath))
    {
      arguments.Add("--tool-path");
      arguments.Add(ToolPath);
    }

    if (!string.IsNullOrWhiteSpace(Version))
    {
      arguments.Add("--version");
      arguments.Add(Version);
    }

    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
    }

    if (!string.IsNullOrWhiteSpace(ToolManifest))
    {
      arguments.Add("--tool-manifest");
      arguments.Add(ToolManifest);
    }

    if (!string.IsNullOrWhiteSpace(Framework))
    {
      arguments.Add("--framework");
      arguments.Add(Framework);
    }

    if (!string.IsNullOrWhiteSpace(Arch))
    {
      arguments.Add("--arch");
      arguments.Add(Arch);
    }

    foreach (string source in PackageSources)
    {
      arguments.Add("--add-source");
      arguments.Add(source);
    }

    if (IncludePrerelease)
    {
      arguments.Add("--prerelease");
    }

    if (IgnoreFailedSources)
    {
      arguments.Add("--ignore-failed-sources");
    }

    if (IsInteractive)
    {
      arguments.Add("--interactive");
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
  
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken);
  }
}