#region Purpose
// TODO: Add purpose description
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for 'dotnet tool list' commands.
/// </summary>
public class DotNetToolListBuilder
{
  private CommandOptions Options;
  private bool IsGlobal;
  private bool IsLocal;
  private string? ToolPath;

  public DotNetToolListBuilder(CommandOptions options)
  {
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolListBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolListBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Lists globally installed tools.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolListBuilder Global()
  {
    IsGlobal = true;
    IsLocal = false;
    return this;
  }

  /// <summary>
  /// Lists locally installed tools.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolListBuilder Local()
  {
    IsLocal = true;
    IsGlobal = false;
    return this;
  }

  /// <summary>
  /// Specifies the path where tools are installed.
  /// </summary>
  /// <param name="toolPath">The installation path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolListBuilder WithToolPath(string toolPath)
  {
    ToolPath = toolPath;
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "tool", "list" };

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

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}