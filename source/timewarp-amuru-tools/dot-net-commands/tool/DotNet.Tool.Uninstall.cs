#region Purpose
// TODO: Add purpose description
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for 'dotnet tool uninstall' commands.
/// </summary>
public class DotNetToolUninstallBuilder
{
  private readonly string PackageId;
  private CommandOptions Options;
  private bool IsGlobal;
  private bool IsLocal;
  private string? ToolPath;
  private string? ToolManifest;

  /// <summary>
  /// Creates a builder for the <c>dotnet tool uninstall</c> command using the given tool package id.
  /// </summary>
  /// <param name="packageId">Package id of the .NET tool to uninstall.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetToolUninstallBuilder(string packageId, CommandOptions options)
  {
    PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolUninstallBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolUninstallBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
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

  /// <summary>
  /// Assembles the arguments for <c>dotnet tool uninstall</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
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

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet tool uninstall</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet tool uninstall</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet tool uninstall</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet tool uninstall</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet tool uninstall</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}
