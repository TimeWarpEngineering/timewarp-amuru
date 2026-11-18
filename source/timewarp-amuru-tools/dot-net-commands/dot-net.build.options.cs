#region Purpose
// Working-directory, environment, validation, and timeout methods for the dotnet build builder.
#endregion

#region Design
// Lives beside dot-net.build.cs as a partial of DotNetBuildBuilder because that file is at its line limit (TWA0038).
#endregion

namespace TimeWarp.Amuru;

public partial class DotNetBuildBuilder
{
  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithWorkingDirectory(string directory)
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
  public DotNetBuildBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command result validation, allowing the build to exit with non-zero codes without throwing exceptions.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Sets the maximum time this build may run. The caller's cancellation token still applies.
  /// </summary>
  /// <param name="timeout">Positive duration</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithTimeout(TimeSpan timeout)
  {
    Options = Options.WithTimeout(timeout);
    return this;
  }

  /// <summary>
  /// Sets how long to wait after the graceful termination signal before the build process is killed.
  /// A child that exits during a positive wait keeps its own exit code. On Unix an unhandled SIGINT is exit 130.
  /// Zero ends the run as exit <see cref="CommandResult.TimeoutExitCode"/>.
  /// </summary>
  /// <param name="gracePeriod">Zero or a positive duration</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetBuildBuilder WithTimeoutGracePeriod(TimeSpan gracePeriod)
  {
    Options = Options.WithTimeoutGracePeriod(gracePeriod);
    return this;
  }
}
