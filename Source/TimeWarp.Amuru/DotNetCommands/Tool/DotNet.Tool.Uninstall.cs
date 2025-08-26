namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for 'dotnet tool uninstall' commands.
/// </summary>
public class DotNetToolUninstallBuilder
{
  private readonly string PackageId;
  private readonly CommandOptions Options;
  private bool IsGlobal;
  private bool IsLocal;
  private string? ToolPath;
  private string? ToolManifest;

  public DotNetToolUninstallBuilder(string packageId, CommandOptions options)
  {
    PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
    Options = options;
  }

  /// <summary>
  /// Uninstalls the tool globally.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolUninstallBuilder Global()
  {
    IsGlobal = true;
    IsLocal = false;
    return this;
  }

  /// <summary>
  /// Uninstalls the tool locally.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolUninstallBuilder Local()
  {
    IsLocal = true;
    IsGlobal = false;
    return this;
  }

  /// <summary>
  /// Specifies the path where the tool is installed.
  /// </summary>
  /// <param name="toolPath">The installation path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolUninstallBuilder WithToolPath(string toolPath)
  {
    ToolPath = toolPath;
    return this;
  }

  /// <summary>
  /// Specifies the path to the tool manifest file.
  /// </summary>
  /// <param name="toolManifest">The tool manifest file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolUninstallBuilder WithToolManifest(string toolManifest)
  {
    ToolManifest = toolManifest;
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "tool", "uninstall", PackageId };

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

    if (!string.IsNullOrWhiteSpace(ToolManifest))
    {
      arguments.Add("--tool-manifest");
      arguments.Add(ToolManifest);
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