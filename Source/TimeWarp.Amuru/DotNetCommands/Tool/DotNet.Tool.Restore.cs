namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for 'dotnet tool restore' commands.
/// </summary>
public class DotNetToolRestoreBuilder
{
  private readonly CommandOptions Options;
  private string? ConfigFile;
  private string? ToolManifest;
  private bool IgnoreFailedSources;
  private bool Interactive;
  private List<string> Sources = new();

  public DotNetToolRestoreBuilder(CommandOptions options)
  {
    Options = options;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolRestoreBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  /// <summary>
  /// Specifies the path to the tool manifest file.
  /// </summary>
  /// <param name="toolManifest">The tool manifest file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolRestoreBuilder WithToolManifest(string toolManifest)
  {
    ToolManifest = toolManifest;
    return this;
  }

  /// <summary>
  /// Treats package source failures as warnings.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolRestoreBuilder WithIgnoreFailedSources()
  {
    IgnoreFailedSources = true;
    return this;
  }

  /// <summary>
  /// Allows the command to pause for user input or action.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolRestoreBuilder WithInteractive()
  {
    Interactive = true;
    return this;
  }

  /// <summary>
  /// Adds a NuGet package source to use during restore.
  /// </summary>
  /// <param name="source">The URI of the NuGet package source</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolRestoreBuilder WithSource(string source)
  {
    Sources.Add(source);
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "tool", "restore" };

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

    foreach (string source in Sources)
    {
      arguments.Add("--add-source");
      arguments.Add(source);
    }

    if (IgnoreFailedSources)
    {
      arguments.Add("--ignore-failed-sources");
    }

    if (Interactive)
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