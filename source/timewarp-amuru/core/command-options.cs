#region Purpose
// Configuration options for shell command execution
// Controls working directory, environment variables, result validation, and per-command timeout
#endregion

#region Design
// - Immutable-style fluent API: With* methods return new instances
// - Properties use init-only or setters for flexible configuration
// - ApplyTo method translates options into CliWrap Command configuration
// - Validation defaults to None: non-zero exit codes are reported via ExitCode/Success, never thrown.
//   ApplyTo always applies the resolved validation so CliWrap's own default (ZeroExitCode) can't leak in.
// - Environment variables are merged with parent process environment
// - With* methods copy EnvironmentVariables, Timeout, and TimeoutGracePeriod rather than aliasing them
// - Timeout is null when unset. There is no process-wide default. TimeoutGracePeriod defaults to 5 seconds
//   and is the wait between the graceful signal and the force kill. CliWrap applies neither; CommandResult does.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Configuration options for command execution, providing control over working directory,
/// environment variables, validation, and per-command timeout.
/// </summary>
public class CommandOptions
{
  /// <summary>
  /// Grace period used when <see cref="WithTimeoutGracePeriod"/> has not been called.
  /// Five seconds, matching a short window for a child to handle the graceful signal before it is killed.
  /// </summary>
  public static readonly TimeSpan DefaultTimeoutGracePeriod = TimeSpan.FromSeconds(5);

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
  /// Gets the maximum time the command may run. Null means no timeout.
  /// The caller's <see cref="System.Threading.CancellationToken"/> still applies and is not replaced by this value.
  /// </summary>
  public TimeSpan? Timeout { get; private set; }

  /// <summary>
  /// Gets how long to wait after the graceful termination signal before the process is killed.
  /// Defaults to <see cref="DefaultTimeoutGracePeriod"/>. Zero kills immediately after the signal is sent.
  /// On Windows the graceful signal is not delivered to non-console children, so this wait ends in a force kill.
  /// </summary>
  public TimeSpan TimeoutGracePeriod { get; private set; } = DefaultTimeoutGracePeriod;

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
    CommandOptions copy = Copy();
    copy.WorkingDirectory = directory;
    return copy;
  }

  /// <summary>
  /// Adds a single environment variable for command execution.
  /// </summary>
  /// <param name="key">The environment variable name</param>
  /// <param name="value">The environment variable value</param>
  /// <returns>A new CommandOptions instance with the environment variable added</returns>
  public CommandOptions WithEnvironmentVariable(string key, string? value)
  {
    Dictionary<string, string?> variables = EnvironmentVariables != null
      ? new Dictionary<string, string?>(EnvironmentVariables)
      : [];
    variables[key] = value;
    return Copy(variables);
  }

  /// <summary>
  /// Sets multiple environment variables for command execution.
  /// </summary>
  /// <param name="variables">Dictionary of environment variables to set</param>
  /// <returns>A new CommandOptions instance with the environment variables set</returns>
  public CommandOptions WithEnvironmentVariables(Dictionary<string, string?> variables)
  {
    return Copy(new Dictionary<string, string?>(variables));
  }

  /// <summary>
  /// Disables command result validation, allowing commands to exit with non-zero codes without throwing exceptions.
  /// This is the default behavior; the method exists to make the intent explicit at call sites.
  /// </summary>
  /// <returns>A new CommandOptions instance with validation disabled</returns>
  public CommandOptions WithNoValidation()
  {
    CommandOptions copy = Copy();
    copy.Validation = CommandResultValidation.None;
    return copy;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>A new CommandOptions instance with zero-exit-code validation enabled</returns>
  public CommandOptions WithZeroExitCodeValidation()
  {
    CommandOptions copy = Copy();
    copy.Validation = CommandResultValidation.ZeroExitCode;
    return copy;
  }

  /// <summary>
  /// Sets the maximum time the command may run.
  /// Null is not representable here; a command with no timeout leaves <see cref="Timeout"/> unset.
  /// </summary>
  /// <param name="timeout">Positive duration. Values above <see cref="int.MaxValue"/> milliseconds are rejected because the timer is millisecond-based.</param>
  /// <returns>A new CommandOptions instance with the timeout set</returns>
  /// <exception cref="ArgumentOutOfRangeException">The timeout is not positive or exceeds <see cref="int.MaxValue"/> milliseconds.</exception>
  public CommandOptions WithTimeout(TimeSpan timeout)
  {
    ThrowIfInvalidDuration(timeout, nameof(timeout), requirePositive: true);
    CommandOptions copy = Copy();
    copy.Timeout = timeout;
    return copy;
  }

  /// <summary>
  /// Sets how long to wait after the graceful termination signal before the process is killed.
  /// </summary>
  /// <param name="gracePeriod">Zero or a positive duration. Zero kills immediately after the signal is sent.</param>
  /// <returns>A new CommandOptions instance with the grace period set</returns>
  /// <exception cref="ArgumentOutOfRangeException">The grace period is negative or exceeds <see cref="int.MaxValue"/> milliseconds.</exception>
  public CommandOptions WithTimeoutGracePeriod(TimeSpan gracePeriod)
  {
    ThrowIfInvalidDuration(gracePeriod, nameof(gracePeriod), requirePositive: false);
    CommandOptions copy = Copy();
    copy.TimeoutGracePeriod = gracePeriod;
    return copy;
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

  private CommandOptions Copy() => Copy(CopyEnvironmentVariables());

  private CommandOptions Copy(Dictionary<string, string?>? environmentVariables)
  {
    return new CommandOptions
    {
      WorkingDirectory = WorkingDirectory,
      EnvironmentVariables = environmentVariables,
      Validation = Validation,
      Timeout = Timeout,
      TimeoutGracePeriod = TimeoutGracePeriod
    };
  }

  private Dictionary<string, string?>? CopyEnvironmentVariables()
  {
    return EnvironmentVariables is null
      ? null
      : new Dictionary<string, string?>(EnvironmentVariables);
  }

  private static void ThrowIfInvalidDuration(TimeSpan duration, string paramName, bool requirePositive)
  {
    bool tooSmall = requirePositive ? duration <= TimeSpan.Zero : duration < TimeSpan.Zero;
    if (tooSmall || duration.TotalMilliseconds > int.MaxValue)
    {
      string message = requirePositive
        ? "Timeout must be positive and no greater than Int32.MaxValue milliseconds."
        : "Timeout grace period must be zero or positive and no greater than Int32.MaxValue milliseconds.";
      throw new ArgumentOutOfRangeException(paramName, duration, message);
    }
  }
}
