namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - New command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet new' command to create a new project or item.
  /// </summary>
  /// <param name="templateName">The short name of the template to create (e.g., "console", "web", "classlib")</param>
  /// <returns>A DotNetNewBuilder for configuring the dotnet new command</returns>
  public static DotNetNewBuilder New(string templateName)
  {
    return new DotNetNewBuilder(templateName);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet new' command without specifying a template.
  /// Use this with subcommands like List(), Search(), etc.
  /// </summary>
  /// <returns>A DotNetNewBuilder for configuring the dotnet new command</returns>
  public static DotNetNewBuilder New()
  {
    return new DotNetNewBuilder();
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet new' commands.
/// </summary>
public class DotNetNewBuilder : ICommandBuilder<DotNetNewBuilder>
{
  private readonly string? TemplateName;
  private string? Output;
  private string? Name;
  private string? Project;
  private string? Verbosity;
  private bool DryRun;
  private bool Force;
  private bool NoUpdateCheck;
  private bool Diagnostics;
  private List<string> TemplateArgs = new();
  private CommandOptions Options = new();

  /// <summary>
  /// Initializes a new instance of the DotNetNewBuilder class.
  /// </summary>
  /// <param name="templateName">The template name (optional)</param>
  public DotNetNewBuilder(string? templateName = null)
  {
    TemplateName = templateName;
  }

  /// <summary>
  /// Specifies the location to place the generated output.
  /// </summary>
  /// <param name="output">The output directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNewBuilder WithOutput(string output)
  {
    Output = output;
    return this;
  }

  /// <summary>
  /// Specifies the name for the output being created.
  /// </summary>
  /// <param name="name">The name for the output</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNewBuilder WithName(string name)
  {
    Name = name;
    return this;
  }

  /// <summary>
  /// Specifies the project that should be used for context evaluation.
  /// </summary>
  /// <param name="project">The project file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNewBuilder WithProject(string project)
  {
    Project = project;
    return this;
  }

  /// <summary>
  /// Sets the verbosity level of the command.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNewBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Displays a summary of what would happen without actually creating the template.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNewBuilder WithDryRun()
  {
    DryRun = true;
    return this;
  }

  /// <summary>
  /// Forces content to be generated even if it would change existing files.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNewBuilder WithForce()
  {
    Force = true;
    return this;
  }

  /// <summary>
  /// Disables checking for template package updates when instantiating a template.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNewBuilder WithNoUpdateCheck()
  {
    NoUpdateCheck = true;
    return this;
  }

  /// <summary>
  /// Enables diagnostic output.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNewBuilder WithDiagnostics()
  {
    Diagnostics = true;
    return this;
  }

  /// <summary>
  /// Adds a template-specific argument.
  /// </summary>
  /// <param name="templateArg">The template-specific argument</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNewBuilder WithTemplateArg(string templateArg)
  {
    TemplateArgs.Add(templateArg);
    return this;
  }

  /// <summary>
  /// Adds multiple template-specific arguments.
  /// </summary>
  /// <param name="templateArgs">The template-specific arguments</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNewBuilder WithTemplateArgs(params string[] templateArgs)
  {
    TemplateArgs.AddRange(templateArgs);
    return this;
  }

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNewBuilder WithWorkingDirectory(string directory)
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
  public DotNetNewBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNewBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet new list' command.
  /// </summary>
  /// <param name="templateName">Optional template name to filter by</param>
  /// <returns>A DotNetNewListBuilder for configuring the list command</returns>
  public DotNetNewListBuilder List(string? templateName = null)
  {
    return new DotNetNewListBuilder(templateName, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet new search' command.
  /// </summary>
  /// <param name="templateName">The template name to search for</param>
  /// <returns>A DotNetNewSearchBuilder for configuring the search command</returns>
  public DotNetNewSearchBuilder Search(string templateName)
  {
    return new DotNetNewSearchBuilder(templateName, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet new install' command.
  /// </summary>
  /// <param name="packageName">The template package to install</param>
  /// <returns>A DotNetNewInstallBuilder for configuring the install command</returns>
  public DotNetNewInstallBuilder Install(string packageName)
  {
    return new DotNetNewInstallBuilder(packageName, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet new uninstall' command.
  /// </summary>
  /// <param name="packageName">The template package to uninstall</param>
  /// <returns>A DotNetNewUninstallBuilder for configuring the uninstall command</returns>
  public DotNetNewUninstallBuilder Uninstall(string packageName)
  {
    return new DotNetNewUninstallBuilder(packageName, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet new update' command.
  /// </summary>
  /// <returns>A DotNetNewUpdateBuilder for configuring the update command</returns>
  public DotNetNewUpdateBuilder Update()
  {
    return new DotNetNewUpdateBuilder(Options);
  }

  /// <summary>
  /// Builds the command arguments and executes the dotnet new command.
  /// </summary>
  /// <returns>A CommandResult for further processing</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "new" };

    // Add template name if specified
    if (!string.IsNullOrWhiteSpace(TemplateName))
    {
      arguments.Add(TemplateName);
    }

    // Add template args
    arguments.AddRange(TemplateArgs);

    // Add options
    if (!string.IsNullOrWhiteSpace(Output))
    {
      arguments.Add("--output");
      arguments.Add(Output);
    }

    if (!string.IsNullOrWhiteSpace(Name))
    {
      arguments.Add("--name");
      arguments.Add(Name);
    }

    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
      arguments.Add(Project);
    }

    if (!string.IsNullOrWhiteSpace(Verbosity))
    {
      arguments.Add("--verbosity");
      arguments.Add(Verbosity);
    }

    // Add boolean flags
    if (DryRun)
    {
      arguments.Add("--dry-run");
    }

    if (Force)
    {
      arguments.Add("--force");
    }

    if (NoUpdateCheck)
    {
      arguments.Add("--no-update-check");
    }

    if (Diagnostics)
    {
      arguments.Add("--diagnostics");
    }

    return CommandExtensions.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Executes the command and streams output to the console in real-time.
  /// This is the default behavior matching shell execution.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The exit code of the command</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken);
  }

  /// <summary>
  /// Executes the command silently and captures all output.
  /// No output is written to the console.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>CommandOutput with stdout, stderr, combined output and exit code</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken);
  }
  
  /// <summary>
  /// Passes the command through to the terminal with full interactive control.
  /// This allows commands like vim, fzf, or REPLs to work with user input and terminal UI.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings will be empty since output goes to console)</returns>
  public async Task<ExecutionResult> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken);
  }
  
  /// <summary>
  /// Executes an interactive selection command and returns the selected value.
  /// The UI is rendered to the console (via stderr) while stdout is captured and returned.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The selected value from the interactive command</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken);
  }
}

/// <summary>
/// Fluent builder for 'dotnet new list' commands.
/// </summary>
public class DotNetNewListBuilder
{
  private readonly string? TemplateName;
  private readonly CommandOptions Options;

  public DotNetNewListBuilder(string? templateName, CommandOptions options)
  {
    TemplateName = templateName;
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "new", "list" };

    if (!string.IsNullOrWhiteSpace(TemplateName))
    {
      arguments.Add(TemplateName);
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
/// Fluent builder for 'dotnet new search' commands.
/// </summary>
public class DotNetNewSearchBuilder
{
  private readonly string TemplateName;
  private readonly CommandOptions Options;

  public DotNetNewSearchBuilder(string templateName, CommandOptions options)
  {
    TemplateName = templateName ?? throw new ArgumentNullException(nameof(templateName));
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "new", "search", TemplateName };
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
/// Fluent builder for 'dotnet new install' commands.
/// </summary>
public class DotNetNewInstallBuilder
{
  private readonly string PackageName;
  private readonly CommandOptions Options;

  public DotNetNewInstallBuilder(string packageName, CommandOptions options)
  {
    PackageName = packageName ?? throw new ArgumentNullException(nameof(packageName));
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "new", "install", PackageName };
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
/// Fluent builder for 'dotnet new uninstall' commands.
/// </summary>
public class DotNetNewUninstallBuilder
{
  private readonly string PackageName;
  private readonly CommandOptions Options;

  public DotNetNewUninstallBuilder(string packageName, CommandOptions options)
  {
    PackageName = packageName ?? throw new ArgumentNullException(nameof(packageName));
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "new", "uninstall", PackageName };
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
/// Fluent builder for 'dotnet new update' commands.
/// </summary>
public class DotNetNewUpdateBuilder
{
  private readonly CommandOptions Options;

  public DotNetNewUpdateBuilder(CommandOptions options)
  {
    Options = options;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "new", "update" };
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