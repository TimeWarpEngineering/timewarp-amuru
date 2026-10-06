#region Purpose
// Entry point for `dotnet tool` that shares working directory and validation with the subcommand builders.
#endregion

#region Design
// This builder does not assemble arguments. Install, uninstall, update, list, run, search, and restore
// each return a dedicated builder that copies CommandOptions.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Tool command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet tool' command.
  /// </summary>
  /// <returns>A DotNetToolBuilder for configuring the dotnet tool command</returns>
  public static DotNetToolBuilder Tool()
  {
    return new DotNetToolBuilder();
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet tool' commands.
/// </summary>
public class DotNetToolBuilder
{
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolBuilder WithWorkingDirectory(string directory)
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
  public DotNetToolBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet tool install' command.
  /// </summary>
  /// <param name="packageId">The package ID of the tool to install</param>
  /// <returns>A DotNetToolInstallBuilder for configuring the install command</returns>
  public DotNetToolInstallBuilder Install(string packageId)
  {
    return new DotNetToolInstallBuilder(packageId, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet tool uninstall' command.
  /// </summary>
  /// <param name="packageId">The package ID of the tool to uninstall</param>
  /// <returns>A DotNetToolUninstallBuilder for configuring the uninstall command</returns>
  public DotNetToolUninstallBuilder Uninstall(string packageId)
  {
    return new DotNetToolUninstallBuilder(packageId, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet tool update' command.
  /// </summary>
  /// <param name="packageId">The package ID of the tool to update</param>
  /// <returns>A DotNetToolUpdateBuilder for configuring the update command</returns>
  public DotNetToolUpdateBuilder Update(string packageId)
  {
    return new DotNetToolUpdateBuilder(packageId, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet tool list' command.
  /// </summary>
  /// <returns>A DotNetToolListBuilder for configuring the list command</returns>
  public DotNetToolListBuilder List()
  {
    return new DotNetToolListBuilder(Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet tool run' command.
  /// </summary>
  /// <param name="commandName">The name of the local tool command to run</param>
  /// <returns>A DotNetToolRunBuilder for configuring the run command</returns>
  public DotNetToolRunBuilder Run(string commandName)
  {
    return new DotNetToolRunBuilder(commandName, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet tool search' command.
  /// </summary>
  /// <param name="searchTerm">The search term to find tools</param>
  /// <returns>A DotNetToolSearchBuilder for configuring the search command</returns>
  public DotNetToolSearchBuilder Search(string searchTerm)
  {
    return new DotNetToolSearchBuilder(searchTerm, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet tool restore' command.
  /// </summary>
  /// <returns>A DotNetToolRestoreBuilder for configuring the restore command</returns>
  public DotNetToolRestoreBuilder Restore()
  {
    return new DotNetToolRestoreBuilder(Options);
  }
}