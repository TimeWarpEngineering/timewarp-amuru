namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Reference command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet reference' command.
  /// </summary>
  /// <returns>A DotNetReferenceBuilder for configuring the dotnet reference command</returns>
  public static DotNetReferenceBuilder Reference()
  {
    return new DotNetReferenceBuilder();
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet reference' command with a specific project.
  /// </summary>
  /// <param name="project">The project file to operate on</param>
  /// <returns>A DotNetReferenceBuilder for configuring the dotnet reference command</returns>
  public static DotNetReferenceBuilder Reference(string project)
  {
    return new DotNetReferenceBuilder().WithProject(project);
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet reference' commands.
/// </summary>
public class DotNetReferenceBuilder
{
  private string? Project;
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the project file to operate on.
  /// </summary>
  /// <param name="project">The project file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetReferenceBuilder WithProject(string project)
  {
    Project = project;
    return this;
  }

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetReferenceBuilder WithWorkingDirectory(string directory)
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
  public DotNetReferenceBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet reference add' command.
  /// </summary>
  /// <param name="projectPath">The project path to add as a reference</param>
  /// <returns>A DotNetReferenceAddBuilder for configuring the add command</returns>
  public DotNetReferenceAddBuilder Add(string projectPath)
  {
    return new DotNetReferenceAddBuilder(projectPath, Project, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet reference add' command with multiple projects.
  /// </summary>
  /// <param name="projectPaths">The project paths to add as references</param>
  /// <returns>A DotNetReferenceAddBuilder for configuring the add command</returns>
  public DotNetReferenceAddBuilder Add(params string[] projectPaths)
  {
    return new DotNetReferenceAddBuilder(projectPaths, Project, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet reference list' command.
  /// </summary>
  /// <returns>A DotNetReferenceListBuilder for configuring the list command</returns>
  public DotNetReferenceListBuilder List()
  {
    return new DotNetReferenceListBuilder(Project, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet reference remove' command.
  /// </summary>
  /// <param name="projectPath">The project path to remove as a reference</param>
  /// <returns>A DotNetReferenceRemoveBuilder for configuring the remove command</returns>
  public DotNetReferenceRemoveBuilder Remove(string projectPath)
  {
    return new DotNetReferenceRemoveBuilder(projectPath, Project, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet reference remove' command with multiple projects.
  /// </summary>
  /// <param name="projectPaths">The project paths to remove as references</param>
  /// <returns>A DotNetReferenceRemoveBuilder for configuring the remove command</returns>
  public DotNetReferenceRemoveBuilder Remove(params string[] projectPaths)
  {
    return new DotNetReferenceRemoveBuilder(projectPaths, Project, Options);
  }
}

/// <summary>
/// Fluent builder for 'dotnet reference add' commands.
/// </summary>
public class DotNetReferenceAddBuilder
{
  private readonly string[] ProjectPaths;
  private readonly string? Project;
  private readonly CommandOptions Options;

  public DotNetReferenceAddBuilder(string projectPath, string? project, CommandOptions options)
  {
    ProjectPaths = [projectPath ?? throw new ArgumentNullException(nameof(projectPath))];
    Project = project;
    Options = options;
  }

  public DotNetReferenceAddBuilder(string[] projectPaths, string? project, CommandOptions options)
  {
    ProjectPaths = projectPaths ?? throw new ArgumentNullException(nameof(projectPaths));
    Project = project;
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "reference" };

    // Add project if specified
    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
      arguments.Add(Project);
    }

    arguments.Add("add");
    arguments.AddRange(ProjectPaths);

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
  }

  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken);
  }

  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken);
  }
  
  public async Task<ExecutionResult> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken);
  }
  
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken);
  }
}

/// <summary>
/// Fluent builder for 'dotnet reference list' commands.
/// </summary>
public class DotNetReferenceListBuilder
{
  private readonly string? Project;
  private readonly CommandOptions Options;

  public DotNetReferenceListBuilder(string? project, CommandOptions options)
  {
    Project = project;
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "reference" };

    // Add project if specified
    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
      arguments.Add(Project);
    }

    arguments.Add("list");

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
  }

  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken);
  }

  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken);
  }
  
  public async Task<ExecutionResult> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken);
  }
  
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken);
  }
}

/// <summary>
/// Fluent builder for 'dotnet reference remove' commands.
/// </summary>
public class DotNetReferenceRemoveBuilder
{
  private readonly string[] ProjectPaths;
  private readonly string? Project;
  private readonly CommandOptions Options;

  public DotNetReferenceRemoveBuilder(string projectPath, string? project, CommandOptions options)
  {
    ProjectPaths = [projectPath ?? throw new ArgumentNullException(nameof(projectPath))];
    Project = project;
    Options = options;
  }

  public DotNetReferenceRemoveBuilder(string[] projectPaths, string? project, CommandOptions options)
  {
    ProjectPaths = projectPaths ?? throw new ArgumentNullException(nameof(projectPaths));
    Project = project;
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "reference" };

    // Add project if specified
    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
      arguments.Add(Project);
    }

    arguments.Add("remove");
    arguments.AddRange(ProjectPaths);

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
  }

  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken);
  }

  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken);
  }
  
  public async Task<ExecutionResult> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken);
  }
  
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken);
  }
}