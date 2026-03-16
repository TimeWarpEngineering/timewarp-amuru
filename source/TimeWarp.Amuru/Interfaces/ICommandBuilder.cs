namespace TimeWarp.Amuru;

/// <summary>
/// Interface for command builders to ensure consistency across all builder implementations.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type for fluent interface support</typeparam>
public interface ICommandBuilder<TBuilder> where TBuilder : ICommandBuilder<TBuilder>
{
  /// <summary>
  /// Builds the command arguments and returns a CommandResult.
  /// </summary>
  /// <returns>A CommandResult for further processing</returns>
  CommandResult Build();

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  TBuilder WithNoValidation();

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  TBuilder WithWorkingDirectory(string directory);

  /// <summary>
  /// Adds an environment variable for the command execution.
  /// </summary>
  /// <param name="key">The environment variable name</param>
  /// <param name="value">The environment variable value</param>
  /// <returns>The builder instance for method chaining</returns>
  TBuilder WithEnvironmentVariable(string key, string? value);
}