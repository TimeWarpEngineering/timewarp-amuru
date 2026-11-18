#region Purpose
// Publish-profile, source, property, and validation methods for the dotnet publish builder.
#endregion

#region Design
// Lives beside dot-net.publish.cs as a partial of DotNetPublishBuilder because that file is at its line limit (TWA0038).
#endregion

namespace TimeWarp.Amuru;

public partial class DotNetPublishBuilder
{
  /// <summary>
  /// Publishes the .NET runtime with the application (self-contained deployment).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithSelfContained()
  {
    SelfContained = true;
    NoSelfContained = false;
    return this;
  }

  /// <summary>
  /// Publishes the application as a framework-dependent application.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithNoSelfContained()
  {
    NoSelfContained = true;
    SelfContained = false;
    return this;
  }

  /// <summary>
  /// Forces all dependencies to be resolved even if the last restore was successful.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithForce()
  {
    Force = true;
    return this;
  }

  /// <summary>
  /// Allows the command to pause for user input or action.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithInteractive()
  {
    Interactive = true;
    return this;
  }

  /// <summary>
  /// Compiles application assemblies as ReadyToRun (R2R) format.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithReadyToRun()
  {
    PublishReadyToRun = true;
    return this;
  }

  /// <summary>
  /// Produces a single-file application.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithSingleFile()
  {
    PublishSingleFile = true;
    return this;
  }

  /// <summary>
  /// Trims unused code from the application.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithTrimmed()
  {
    PublishTrimmed = true;
    return this;
  }

  /// <summary>
  /// Adds a NuGet package source to use during the restore operation.
  /// </summary>
  /// <param name="source">The URI of the NuGet package source</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithSource(string source)
  {
    Sources.Add(source);
    return this;
  }

  /// <summary>
  /// Adds multiple NuGet package sources to use during the restore operation.
  /// </summary>
  /// <param name="sources">The URIs of the NuGet package sources</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithSources(params string[] sources)
  {
    Sources.AddRange(sources);
    return this;
  }

  /// <summary>
  /// Sets an MSBuild property.
  /// </summary>
  /// <param name="name">The property name</param>
  /// <param name="value">The property value</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithProperty(string name, string value)
  {
    Properties[name] = value;
    return this;
  }

  /// <summary>
  /// Sets multiple MSBuild properties.
  /// </summary>
  /// <param name="properties">Dictionary of property names and values</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithProperties(Dictionary<string, string> properties)
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
  public DotNetPublishBuilder WithWorkingDirectory(string directory)
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
  public DotNetPublishBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPublishBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }
}
