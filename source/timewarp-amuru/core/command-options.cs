#region Purpose
// Configuration options for shell command execution
// Controls working directory, environment variables, and result validation behavior
#endregion

#region Design
// - Immutable-style fluent API: With* methods return new instances
// - Properties use init-only or setters for flexible configuration
// - ApplyTo method translates options into CliWrap Command configuration
// - Validation defaults to None: non-zero exit codes are reported via ExitCode/Success, never thrown.
//   ApplyTo always applies the resolved validation so CliWrap's own default (ZeroExitCode) can't leak in.
// - Environment variables are merged with parent process environment
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Configuration options for command execution, providing control over working directory,
/// environment variables, and other process settings.
/// </summary>
public class CommandOptions
{
  /// <summary>
  /// Gets or sets the working directory for the command execution.
  /// If not specified, uses the current working directory.
  /// </summary>
  public string? WorkingDirectory { get; set; }

  /// <summary>
  /// Gets additional environment variables for the command execution.
  /// These are added to the inherited environment variables from the parent process.
  /// </summary>
  public Dictionary<string, string?>? EnvironmentVariables { get; init; }

  /// <summary>
  /// Gets the command result validation behavior.
  /// Defaults to no validation: non-zero exit codes are reported via the result, not thrown.
  /// Use <see cref="WithZeroExitCodeValidation"/> to opt in to throwing on non-zero exit codes.
  /// </summary>
  internal CommandResultValidation? Validation { get; set; }

  /// <summary>
  /// Creates a new instance of CommandOptions with default settings.
  /// </summary>
  public CommandOptions()
  {
    // Default constructor with no configuration
  }

  /// <summary>
  /// Sets the working directory for command execution.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>A new CommandOptions instance with the working directory set</returns>
  public CommandOptions WithWorkingDirectory(string directory)
  {
    return new CommandOptions
    {
      WorkingDirectory = directory,
      EnvironmentVariables = EnvironmentVariables,
      Validation = Validation
    };
  }

  /// <summary>
  /// Adds a single environment variable for command execution.
  /// </summary>
  /// <param name="key">The environment variable name</param>
  /// <param name="value">The environment variable value</param>
  /// <returns>A new CommandOptions instance with the environment variable added</returns>
  public CommandOptions WithEnvironmentVariable(string key, string? value)
  {
    var newOptions = new CommandOptions
    {
      WorkingDirectory = WorkingDirectory,
      EnvironmentVariables = EnvironmentVariables != null
        ? new Dictionary<string, string?>(EnvironmentVariables)
        : [],
      Validation = Validation
    };

    newOptions.EnvironmentVariables[key] = value;
    return newOptions;
  }

  /// <summary>
  /// Sets multiple environment variables for command execution.
  /// </summary>
  /// <param name="variables">Dictionary of environment variables to set</param>
  /// <returns>A new CommandOptions instance with the environment variables set</returns>
  public CommandOptions WithEnvironmentVariables(Dictionary<string, string?> variables)
  {
    return new CommandOptions
    {
      WorkingDirectory = WorkingDirectory,
      EnvironmentVariables = new Dictionary<string, string?>(variables),
      Validation = Validation
    };
  }

  /// <summary>
  /// Disables command result validation, allowing commands to exit with non-zero codes without throwing exceptions.
  /// This is the default behavior; the method exists to make the intent explicit at call sites.
  /// </summary>
  /// <returns>A new CommandOptions instance with validation disabled</returns>
  public CommandOptions WithNoValidation()
  {
    return new CommandOptions
    {
      WorkingDirectory = WorkingDirectory,
      EnvironmentVariables = EnvironmentVariables,
      Validation = CommandResultValidation.None
    };
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>A new CommandOptions instance with zero-exit-code validation enabled</returns>
  public CommandOptions WithZeroExitCodeValidation()
  {
    return new CommandOptions
    {
      WorkingDirectory = WorkingDirectory,
      EnvironmentVariables = EnvironmentVariables,
      Validation = CommandResultValidation.ZeroExitCode
    };
  }

  /// <summary>
  /// Applies the configuration options to a CliWrap Command.
  /// </summary>
  /// <param name="command">The CliWrap Command to configure</param>
  /// <returns>The configured CliWrap Command</returns>
  internal Command ApplyTo(Command command)
  {
    Command configuredCommand = command;

    // Apply working directory if specified
    if (!string.IsNullOrWhiteSpace(WorkingDirectory))
    {
      configuredCommand = configuredCommand.WithWorkingDirectory(WorkingDirectory);
    }

    // Apply environment variables if specified
    if (EnvironmentVariables?.Count > 0)
    {
      configuredCommand = configuredCommand.WithEnvironmentVariables(EnvironmentVariables);
    }

    // Always apply validation so CliWrap's own default (ZeroExitCode) can't leak in;
    // Amuru's contract is None unless the caller opts in to strict validation.
    configuredCommand = configuredCommand.WithValidation(Validation ?? CommandResultValidation.None);

    return configuredCommand;
  }
}