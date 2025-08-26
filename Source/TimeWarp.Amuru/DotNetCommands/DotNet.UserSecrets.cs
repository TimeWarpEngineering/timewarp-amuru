namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - User-secrets command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet user-secrets' command.
  /// </summary>
  /// <returns>A DotNetUserSecretsBuilder for configuring the dotnet user-secrets command</returns>
  public static DotNetUserSecretsBuilder UserSecrets()
  {
    return new DotNetUserSecretsBuilder();
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet user-secrets' commands.
/// </summary>
public class DotNetUserSecretsBuilder
{
  private CommandOptions Options = new();
  private string? Project;
  private string? Id;

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsBuilder WithWorkingDirectory(string directory)
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
  public DotNetUserSecretsBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Specifies the project file to use.
  /// </summary>
  /// <param name="project">The project file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsBuilder WithProject(string project)
  {
    Project = project;
    return this;
  }

  /// <summary>
  /// Specifies the user secrets ID.
  /// </summary>
  /// <param name="id">The user secrets ID</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsBuilder WithId(string id)
  {
    Id = id;
    return this;
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet user-secrets init' command.
  /// </summary>
  /// <returns>A DotNetUserSecretsInitBuilder for configuring the init command</returns>
  public DotNetUserSecretsInitBuilder Init()
  {
    return new DotNetUserSecretsInitBuilder(Project, Id, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet user-secrets set' command.
  /// </summary>
  /// <param name="key">The secret key</param>
  /// <param name="value">The secret value</param>
  /// <returns>A DotNetUserSecretsSetBuilder for configuring the set command</returns>
  public DotNetUserSecretsSetBuilder Set(string key, string value)
  {
    return new DotNetUserSecretsSetBuilder(key, value, Project, Id, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet user-secrets remove' command.
  /// </summary>
  /// <param name="key">The secret key to remove</param>
  /// <returns>A DotNetUserSecretsRemoveBuilder for configuring the remove command</returns>
  public DotNetUserSecretsRemoveBuilder Remove(string key)
  {
    return new DotNetUserSecretsRemoveBuilder(key, Project, Id, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet user-secrets list' command.
  /// </summary>
  /// <returns>A DotNetUserSecretsListBuilder for configuring the list command</returns>
  public DotNetUserSecretsListBuilder List()
  {
    return new DotNetUserSecretsListBuilder(Project, Id, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet user-secrets clear' command.
  /// </summary>
  /// <returns>A DotNetUserSecretsClearBuilder for configuring the clear command</returns>
  public DotNetUserSecretsClearBuilder Clear()
  {
    return new DotNetUserSecretsClearBuilder(Project, Id, Options);
  }
}

/// <summary>
/// Fluent builder for 'dotnet user-secrets init' commands.
/// </summary>
public class DotNetUserSecretsInitBuilder
{
  private readonly string? Project;
  private readonly string? Id;
  private readonly CommandOptions Options;

  public DotNetUserSecretsInitBuilder(string? project, string? id, CommandOptions options)
  {
    Project = project;
    Id = id;
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "user-secrets", "init" };

    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
      arguments.Add(Project);
    }

    if (!string.IsNullOrWhiteSpace(Id))
    {
      arguments.Add("--id");
      arguments.Add(Id);
    }

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
/// Fluent builder for 'dotnet user-secrets set' commands.
/// </summary>
public class DotNetUserSecretsSetBuilder
{
  private readonly string Key;
  private readonly string Value;
  private readonly string? Project;
  private readonly string? Id;
  private readonly CommandOptions Options;

  public DotNetUserSecretsSetBuilder(string key, string value, string? project, string? id, CommandOptions options)
  {
    Key = key ?? throw new ArgumentNullException(nameof(key));
    Value = value ?? throw new ArgumentNullException(nameof(value));
    Project = project;
    Id = id;
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "user-secrets", "set", Key, Value };

    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
      arguments.Add(Project);
    }

    if (!string.IsNullOrWhiteSpace(Id))
    {
      arguments.Add("--id");
      arguments.Add(Id);
    }

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
/// Fluent builder for 'dotnet user-secrets remove' commands.
/// </summary>
public class DotNetUserSecretsRemoveBuilder
{
  private readonly string Key;
  private readonly string? Project;
  private readonly string? Id;
  private readonly CommandOptions Options;

  public DotNetUserSecretsRemoveBuilder(string key, string? project, string? id, CommandOptions options)
  {
    Key = key ?? throw new ArgumentNullException(nameof(key));
    Project = project;
    Id = id;
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "user-secrets", "remove", Key };

    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
      arguments.Add(Project);
    }

    if (!string.IsNullOrWhiteSpace(Id))
    {
      arguments.Add("--id");
      arguments.Add(Id);
    }

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
/// Fluent builder for 'dotnet user-secrets list' commands.
/// </summary>
public class DotNetUserSecretsListBuilder
{
  private readonly string? Project;
  private readonly string? Id;
  private readonly CommandOptions Options;

  public DotNetUserSecretsListBuilder(string? project, string? id, CommandOptions options)
  {
    Project = project;
    Id = id;
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "user-secrets", "list" };

    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
      arguments.Add(Project);
    }

    if (!string.IsNullOrWhiteSpace(Id))
    {
      arguments.Add("--id");
      arguments.Add(Id);
    }

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
/// Fluent builder for 'dotnet user-secrets clear' commands.
/// </summary>
public class DotNetUserSecretsClearBuilder
{
  private readonly string? Project;
  private readonly string? Id;
  private readonly CommandOptions Options;

  public DotNetUserSecretsClearBuilder(string? project, string? id, CommandOptions options)
  {
    Project = project;
    Id = id;
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "user-secrets", "clear" };

    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
      arguments.Add(Project);
    }

    if (!string.IsNullOrWhiteSpace(Id))
    {
      arguments.Add("--id");
      arguments.Add(Id);
    }

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