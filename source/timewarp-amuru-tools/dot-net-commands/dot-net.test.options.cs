#region Purpose
// Blame, property, validation, extra-argument, and timeout methods for the dotnet test builder.
#endregion

#region Design
// Lives beside dot-net.test.cs as a partial of DotNetTestBuilder because that file is at its line limit (TWA0038).
#endregion

namespace TimeWarp.Amuru;

public partial class DotNetTestBuilder
{
  /// <summary>
  /// Runs the tests in blame mode, which is helpful for diagnosing test host crashes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithBlame()
  {
    Blame = true;
    return this;
  }

  /// <summary>
  /// Collects data with the named data collector.
  /// </summary>
  /// <param name="dataCollector">The data collector friendly name, for example "XPlat Code Coverage".</param>
  /// <returns>The builder instance for method chaining</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="dataCollector"/> is null or whitespace.</exception>
  public DotNetTestBuilder WithCollect(string dataCollector)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(dataCollector);
    Collect = dataCollector;
    return this;
  }

  /// <summary>
  /// Sets an MSBuild property.
  /// </summary>
  /// <param name="name">The property name</param>
  /// <param name="value">The property value</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithProperty(string name, string value)
  {
    Properties[name] = value;
    return this;
  }

  /// <summary>
  /// Sets multiple MSBuild properties.
  /// </summary>
  /// <param name="properties">Dictionary of property names and values</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithProperties(Dictionary<string, string> properties)
  {
    ArgumentNullException.ThrowIfNull(properties);
    foreach (KeyValuePair<string, string> kvp in properties)
    {
      Properties[kvp.Key] = kvp.Value;
    }
    
    return this;
  }

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithWorkingDirectory(string directory)
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
  public DotNetTestBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Appends arguments that have no typed option, such as Microsoft.Testing.Platform extension options.
  /// Tokens are added in call order after the typed options. This method does not insert <c>--</c>.
  /// </summary>
  /// <param name="arguments">Arguments to append. A null array adds nothing.</param>
  /// <returns>The builder instance for method chaining</returns>
  /// <exception cref="ArgumentException">Thrown when an argument is null or whitespace.</exception>
  public DotNetTestBuilder WithArguments(params string[]? arguments)
  {
    if (arguments is null)
    {
      return this;
    }

    foreach (string argument in arguments)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(argument);
      ExtraArguments.Add(argument);
    }

    return this;
  }

  /// <summary>
  /// Sets the maximum time this test run may take. The caller's cancellation token still applies.
  /// </summary>
  /// <param name="timeout">Positive duration</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithTimeout(TimeSpan timeout)
  {
    Options = Options.WithTimeout(timeout);
    return this;
  }

  /// <summary>
  /// Sets how long to wait after the graceful termination signal before the test process is killed.
  /// A child that exits during a positive wait keeps its own exit code. On Unix an unhandled SIGINT is exit 130.
  /// Zero ends the run as exit <see cref="CommandResult.TimeoutExitCode"/>.
  /// </summary>
  /// <param name="gracePeriod">Zero or a positive duration</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetTestBuilder WithTimeoutGracePeriod(TimeSpan gracePeriod)
  {
    Options = Options.WithTimeoutGracePeriod(gracePeriod);
    return this;
  }
}
