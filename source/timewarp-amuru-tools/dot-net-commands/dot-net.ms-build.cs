#region Purpose
// Fluent builder for dotnet msbuild. Task 151.
#endregion

#region Design
// MSBuild switches use the -name:value form. Each target, property, and -get* query is its own
// argument, emitted in the order the caller added it, so -t:Clean -t:Build runs Clean first.
// Setting a property name twice replaces the value and keeps its first position (MSBuild's last
// -p: would win anyway; one entry keeps the argv short).
// Telemetry is the same call dotnet build makes, with command word "msbuild": the configuration
// comes from a Configuration property, and build.publish_aot is false.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - MSBuild command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet msbuild' command.
  /// </summary>
  /// <returns>A DotNetMsBuildBuilder for configuring the dotnet msbuild command</returns>
  public static DotNetMsBuildBuilder MsBuild()
  {
    return new DotNetMsBuildBuilder();
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet msbuild' command with a specific project or solution.
  /// </summary>
  /// <param name="project">The project or solution file path</param>
  /// <returns>A DotNetMsBuildBuilder for configuring the dotnet msbuild command</returns>
  public static DotNetMsBuildBuilder MsBuild(string project)
  {
    return new DotNetMsBuildBuilder().WithProject(project);
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet msbuild' commands.
/// </summary>
public class DotNetMsBuildBuilder : ICommandBuilder<DotNetMsBuildBuilder>
{
  private readonly List<string> Targets = [];
  private readonly List<KeyValuePair<string, string>> Properties = [];
  private readonly List<string> GetProperties = [];
  private readonly List<string> GetItems = [];
  private readonly List<string> GetTargetResults = [];
  private string? Project;
  private string? Verbosity;
  private string? BinaryLogPath;
  private bool BinaryLog;
  private bool Restore;
  private bool NoLogo;
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the project or solution file to build. If not specified, MSBuild searches the current directory.
  /// </summary>
  /// <param name="project">Path to the project or solution file</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetMsBuildBuilder WithProject(string project)
  {
    Project = project;
    return this;
  }

  /// <summary>
  /// Adds a target to run. Emits <c>-t:&lt;target&gt;</c>; call again for more targets, which run in call order.
  /// </summary>
  /// <param name="target">The target name</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetMsBuildBuilder WithTarget(string target)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(target);
    Targets.Add(target);
    return this;
  }

  /// <summary>
  /// Sets an MSBuild property. Emits <c>-p:&lt;name&gt;=&lt;value&gt;</c> in the order properties were first added.
  /// </summary>
  /// <param name="name">The property name</param>
  /// <param name="value">The property value</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetMsBuildBuilder WithProperty(string name, string value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentNullException.ThrowIfNull(value);
    int index = Properties.FindIndex(property => string.Equals(property.Key, name, StringComparison.Ordinal));
    KeyValuePair<string, string> entry = new(name, value);
    if (index >= 0)
    {
      Properties[index] = entry;
    }
    else
    {
      Properties.Add(entry);
    }

    return this;
  }

  /// <summary>
  /// Asks MSBuild to print the evaluated value of a property. Emits <c>-getProperty:&lt;name&gt;</c>.
  /// </summary>
  /// <param name="name">The property name</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetMsBuildBuilder WithGetProperty(string name)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    GetProperties.Add(name);
    return this;
  }

  /// <summary>
  /// Asks MSBuild to print the items of an item type. Emits <c>-getItem:&lt;itemType&gt;</c>.
  /// </summary>
  /// <param name="itemType">The item type</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetMsBuildBuilder WithGetItem(string itemType)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(itemType);
    GetItems.Add(itemType);
    return this;
  }

  /// <summary>
  /// Asks MSBuild to print the result of a target. Emits <c>-getTargetResult:&lt;target&gt;</c>.
  /// </summary>
  /// <param name="target">The target name</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetMsBuildBuilder WithGetTargetResult(string target)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(target);
    GetTargetResults.Add(target);
    return this;
  }

  /// <summary>
  /// Sets the MSBuild verbosity. Emits <c>-verbosity:&lt;level&gt;</c>.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, detailed, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetMsBuildBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Runs the Restore target before the requested targets. Emits <c>-restore</c>.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetMsBuildBuilder WithRestore()
  {
    Restore = true;
    return this;
  }

  /// <summary>
  /// Doesn't display the startup banner or the copyright message. Emits <c>-nologo</c>.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetMsBuildBuilder WithNoLogo()
  {
    NoLogo = true;
    return this;
  }

  /// <summary>
  /// Writes a binary log to msbuild.binlog in the working directory. Emits <c>-bl</c>.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetMsBuildBuilder WithBinaryLog()
  {
    BinaryLog = true;
    BinaryLogPath = null;
    return this;
  }

  /// <summary>
  /// Writes a binary log to the given path. Emits <c>-bl:&lt;path&gt;</c>.
  /// </summary>
  /// <param name="path">The binary log file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetMsBuildBuilder WithBinaryLog(string path)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    BinaryLog = true;
    BinaryLogPath = path;
    return this;
  }

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetMsBuildBuilder WithWorkingDirectory(string directory)
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
  public DotNetMsBuildBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command result validation, allowing the command to exit with non-zero codes without throwing exceptions.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetMsBuildBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetMsBuildBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet msbuild</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = ["msbuild"];

    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add(Project);
    }

    foreach (string target in Targets)
    {
      arguments.Add($"-t:{target}");
    }

    foreach (KeyValuePair<string, string> property in Properties)
    {
      arguments.Add($"-p:{property.Key}={property.Value}");
    }

    foreach (string name in GetProperties)
    {
      arguments.Add($"-getProperty:{name}");
    }

    foreach (string itemType in GetItems)
    {
      arguments.Add($"-getItem:{itemType}");
    }

    foreach (string target in GetTargetResults)
    {
      arguments.Add($"-getTargetResult:{target}");
    }

    if (!string.IsNullOrWhiteSpace(Verbosity))
    {
      arguments.Add($"-verbosity:{Verbosity}");
    }

    if (Restore)
    {
      arguments.Add("-restore");
    }

    if (NoLogo)
    {
      arguments.Add("-nologo");
    }

    if (BinaryLog)
    {
      arguments.Add(BinaryLogPath is null ? "-bl" : $"-bl:{BinaryLogPath}");
    }

    Dictionary<string, string> telemetryProperties = new(StringComparer.Ordinal);
    foreach (KeyValuePair<string, string> property in Properties)
    {
      telemetryProperties[property.Key] = property.Value;
    }

    DotNetBuildTelemetry.Tag("msbuild", null, telemetryProperties, Project, Options.WorkingDirectory);
    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet msbuild</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet msbuild</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet msbuild</c>, streams output to the console, and also captures it.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> RunAndCaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAndCaptureAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet msbuild</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet msbuild</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet msbuild</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}
