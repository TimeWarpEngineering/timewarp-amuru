#region Purpose
// TODO: Add purpose description
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for 'dotnet tool install' commands.
/// </summary>
public class DotNetToolInstallBuilder
{
  private readonly string PackageId;
  private CommandOptions Options;
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

  /// <summary>
  /// Creates a builder for the <c>dotnet tool install</c> command using the given tool package id.
  /// </summary>
  /// <param name="packageId">Package id of the .NET tool to install.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetToolInstallBuilder(string packageId, CommandOptions options)
  {
    PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolInstallBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolInstallBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
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

  /// <summary>
  /// Assembles the arguments for <c>dotnet tool install</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
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

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet tool install</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet tool install</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet tool install</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet tool install</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet tool install</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}
