#region Purpose
// Fluent builders for `dotnet user-secrets` init, set, remove, list, and clear.
#endregion

#region Design
// The parent builder holds the optional project, the optional secrets id, and CommandOptions.
// Each subcommand builder receives those values and emits `--project` or `--id` only when set.
#endregion

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
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
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
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet user-secrets init</c> command using the given target project and user-secrets id.
  /// </summary>
  /// <param name="project">Project that receives a user-secrets id, or null to use the project in the working directory.</param>
  /// <param name="id">User-secrets id to assign, or null so the CLI generates one.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetUserSecretsInitBuilder(string? project, string? id, CommandOptions options)
  {
    Project = project;
    Id = id;
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsInitBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsInitBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet user-secrets init</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
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

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets init</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets init</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet user-secrets init</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets init</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet user-secrets init</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
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
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet user-secrets set</c> command using the given secret name, secret value, target project, and user-secrets id.
  /// </summary>
  /// <param name="key">Secret name to store.</param>
  /// <param name="value">Secret value stored under the key.</param>
  /// <param name="project">Project whose secret store is updated, or null to use the project in the working directory.</param>
  /// <param name="id">User-secrets id to update instead of the id stored in the project, or null to use the project id.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetUserSecretsSetBuilder(string key, string value, string? project, string? id, CommandOptions options)
  {
    Key = key ?? throw new ArgumentNullException(nameof(key));
    Value = value ?? throw new ArgumentNullException(nameof(value));
    Project = project;
    Id = id;
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsSetBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsSetBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet user-secrets set</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
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

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets set</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets set</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet user-secrets set</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets set</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet user-secrets set</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
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
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet user-secrets remove</c> command using the given secret name, target project, and user-secrets id.
  /// </summary>
  /// <param name="key">Secret name to delete.</param>
  /// <param name="project">Project whose secret store is updated, or null to use the project in the working directory.</param>
  /// <param name="id">User-secrets id to update instead of the id stored in the project, or null to use the project id.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetUserSecretsRemoveBuilder(string key, string? project, string? id, CommandOptions options)
  {
    Key = key ?? throw new ArgumentNullException(nameof(key));
    Project = project;
    Id = id;
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsRemoveBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsRemoveBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet user-secrets remove</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
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

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets remove</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets remove</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet user-secrets remove</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets remove</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet user-secrets remove</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet user-secrets list' commands.
/// </summary>
public class DotNetUserSecretsListBuilder
{
  private readonly string? Project;
  private readonly string? Id;
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet user-secrets list</c> command using the given target project and user-secrets id.
  /// </summary>
  /// <param name="project">Project whose secrets are listed, or null to use the project in the working directory.</param>
  /// <param name="id">User-secrets id to list instead of the id stored in the project, or null to use the project id.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetUserSecretsListBuilder(string? project, string? id, CommandOptions options)
  {
    Project = project;
    Id = id;
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsListBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsListBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet user-secrets list</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
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

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets list</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets list</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet user-secrets list</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets list</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet user-secrets list</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}

/// <summary>
/// Fluent builder for 'dotnet user-secrets clear' commands.
/// </summary>
public class DotNetUserSecretsClearBuilder
{
  private readonly string? Project;
  private readonly string? Id;
  private CommandOptions Options;

  /// <summary>
  /// Creates a builder for the <c>dotnet user-secrets clear</c> command using the given target project and user-secrets id.
  /// </summary>
  /// <param name="project">Project whose secrets are cleared, or null to use the project in the working directory.</param>
  /// <param name="id">User-secrets id to clear instead of the id stored in the project, or null to use the project id.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetUserSecretsClearBuilder(string? project, string? id, CommandOptions options)
  {
    Project = project;
    Id = id;
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsClearBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetUserSecretsClearBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet user-secrets clear</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
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

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets clear</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets clear</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet user-secrets clear</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet user-secrets clear</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet user-secrets clear</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}
