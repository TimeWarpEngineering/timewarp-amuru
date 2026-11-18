#region Purpose
// Target, launch, property, validation, artifacts-path, and timeout methods for the dotnet run builder.
#endregion

#region Design
// Lives beside dot-net.run.cs as a partial of DotNetRunBuilder because that file is at its line limit (TWA0038).
// WithStandardInput writes the StandardInput field declared on the other partial.
#endregion

namespace TimeWarp.Amuru;

public partial class DotNetRunBuilder
{
  /// <summary>
  /// Specifies the target architecture (shorthand for runtime identifier).
  /// </summary>
  /// <param name="architecture">The target architecture (e.g., "x64", "arm64")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithArchitecture(string architecture)
  {
    Architecture = architecture;
    return this;
  }

  /// <summary>
  /// Specifies the target operating system.
  /// </summary>
  /// <param name="operatingSystem">The target OS (e.g., "win", "linux", "osx")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithOperatingSystem(string operatingSystem)
  {
    OperatingSystem = operatingSystem;
    return this;
  }

  /// <summary>
  /// Specifies the launch profile from launchSettings.json.
  /// </summary>
  /// <param name="profileName">The name of the launch profile to use</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithLaunchProfile(string profileName)
  {
    LaunchProfile = profileName;
    return this;
  }

  /// <summary>
  /// Doesn't use launchSettings.json when running the application.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithNoLaunchProfile()
  {
    NoLaunchProfile = true;
    return this;
  }

  /// <summary>
  /// Forces all dependencies to be resolved even if the last restore was successful.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithForce()
  {
    Force = true;
    return this;
  }

  /// <summary>
  /// Allows the command to pause for user input or action.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithInteractive()
  {
    Interactive = true;
    return this;
  }

  /// <summary>
  /// Sets the terminal logger for build output.
  /// </summary>
  /// <param name="mode">The terminal logger mode ("auto", "on", "off")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithTerminalLogger(string mode)
  {
    TerminalLogger = mode;
    return this;
  }

  /// <summary>
  /// Sets an MSBuild property.
  /// </summary>
  /// <param name="name">The property name</param>
  /// <param name="value">The property value</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithProperty(string name, string value)
  {
    Properties[name] = value;
    return this;
  }

  /// <summary>
  /// Sets multiple MSBuild properties.
  /// </summary>
  /// <param name="properties">Dictionary of property names and values</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithProperties(Dictionary<string, string> properties)
  {
    ArgumentNullException.ThrowIfNull(properties);
    foreach (KeyValuePair<string, string> kvp in properties)
    {
      Properties[kvp.Key] = kvp.Value;
    }
    
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Places run output under the artifacts directory, in a subfolder per project.
  /// </summary>
  /// <param name="artifactsPath">The artifacts directory</param>
  /// <returns>The builder instance for method chaining</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="artifactsPath"/> is null or whitespace.</exception>
  public DotNetRunBuilder WithArtifactsPath(string artifactsPath)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(artifactsPath);
    ArtifactsPath = artifactsPath;
    return this;
  }

  /// <summary>
  /// Sets the maximum time this run may take. The caller's cancellation token still applies.
  /// </summary>
  /// <param name="timeout">Positive duration</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithTimeout(TimeSpan timeout)
  {
    Options = Options.WithTimeout(timeout);
    return this;
  }

  /// <summary>
  /// Sets how long to wait after the graceful termination signal before the run process is killed.
  /// A child that exits during a positive wait keeps its own exit code. On Unix an unhandled SIGINT is exit 130.
  /// Zero ends the run as exit <see cref="CommandResult.TimeoutExitCode"/>.
  /// </summary>
  /// <param name="gracePeriod">Zero or a positive duration</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetRunBuilder WithTimeoutGracePeriod(TimeSpan gracePeriod)
  {
    Options = Options.WithTimeoutGracePeriod(gracePeriod);
    return this;
  }

  /// <summary>
  /// Pipes text to the child process. An empty string closes standard input immediately.
  /// The text is not a command-line argument.
  /// </summary>
  /// <param name="input">The text to write. Empty is immediate EOF.</param>
  /// <returns>The builder instance for method chaining</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
  public DotNetRunBuilder WithStandardInput(string input)
  {
    ArgumentNullException.ThrowIfNull(input);
    StandardInput = input;
    return this;
  }
}
