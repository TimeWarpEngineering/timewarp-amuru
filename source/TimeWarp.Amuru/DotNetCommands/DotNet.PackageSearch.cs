namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Package search command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet package search' command.
  /// </summary>
  /// <param name="searchTerm">The search term to filter package names, descriptions, and tags</param>
  /// <returns>A DotNetPackageSearchBuilder for configuring the dotnet package search command</returns>
  public static DotNetPackageSearchBuilder PackageSearch(string searchTerm)
  {
    return new DotNetPackageSearchBuilder(searchTerm);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet package search' command without a search term.
  /// </summary>
  /// <returns>A DotNetPackageSearchBuilder for configuring the dotnet package search command</returns>
  public static DotNetPackageSearchBuilder PackageSearch()
  {
    return new DotNetPackageSearchBuilder();
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet package search' commands.
/// </summary>
public class DotNetPackageSearchBuilder : ICommandBuilder<DotNetPackageSearchBuilder>
{
  private readonly string? SearchTerm;
  private List<string> Sources = new();
  private int? Take;
  private int? Skip;
  private bool ExactMatch;
  private bool Interactive;
  private bool Prerelease;
  private string? ConfigFile;
  private string? Format;
  private string? Verbosity;
  private CommandOptions Options = new();

  /// <summary>
  /// Initializes a new instance of the DotNetPackageSearchBuilder class.
  /// </summary>
  /// <param name="searchTerm">The search term (optional)</param>
  public DotNetPackageSearchBuilder(string? searchTerm = null)
  {
    SearchTerm = searchTerm;
  }

  /// <summary>
  /// Adds a package source to search. You can pass multiple sources to search multiple package sources.
  /// </summary>
  /// <param name="source">The package source URL</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackageSearchBuilder WithSource(string source)
  {
    Sources.Add(source);
    return this;
  }

  /// <summary>
  /// Adds multiple package sources to search.
  /// </summary>
  /// <param name="sources">The package source URLs</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackageSearchBuilder WithSources(params string[] sources)
  {
    Sources.AddRange(sources);
    return this;
  }

  /// <summary>
  /// Specifies the number of results to return.
  /// </summary>
  /// <param name="take">Number of results to return (default 20)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackageSearchBuilder WithTake(int take)
  {
    Take = take;
    return this;
  }

  /// <summary>
  /// Specifies the number of results to skip, to allow pagination.
  /// </summary>
  /// <param name="skip">Number of results to skip (default 0)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackageSearchBuilder WithSkip(int skip)
  {
    Skip = skip;
    return this;
  }

  /// <summary>
  /// Requires that the search term exactly match the name of the package.
  /// Causes Take and Skip options to be ignored.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackageSearchBuilder WithExactMatch()
  {
    ExactMatch = true;
    return this;
  }

  /// <summary>
  /// Allows the command to stop and wait for user input or action (for example to complete authentication).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackageSearchBuilder WithInteractive()
  {
    Interactive = true;
    return this;
  }

  /// <summary>
  /// Includes prerelease packages in the search results.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackageSearchBuilder WithPrerelease()
  {
    Prerelease = true;
    return this;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackageSearchBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  /// <summary>
  /// Formats the output accordingly.
  /// </summary>
  /// <param name="format">Either "table" or "json" (default is "table")</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackageSearchBuilder WithFormat(string format)
  {
    Format = format;
    return this;
  }

  /// <summary>
  /// Sets the verbosity level of the command.
  /// </summary>
  /// <param name="verbosity">The verbosity level (normal, minimal, detailed)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackageSearchBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackageSearchBuilder WithWorkingDirectory(string directory)
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
  public DotNetPackageSearchBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetPackageSearchBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Builds the command arguments and executes the dotnet package search command.
  /// </summary>
  /// <returns>A CommandResult for further processing</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "package", "search" };

    // Add search term if specified
    if (!string.IsNullOrWhiteSpace(SearchTerm))
    {
      arguments.Add(SearchTerm);
    }

    // Add sources
    foreach (string source in Sources)
    {
      arguments.Add("--source");
      arguments.Add(source);
    }

    // Add take if specified
    if (Take.HasValue)
    {
      arguments.Add("--take");
      arguments.Add(Take.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    // Add skip if specified
    if (Skip.HasValue)
    {
      arguments.Add("--skip");
      arguments.Add(Skip.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    // Add config file if specified
    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
    }

    // Add format if specified
    if (!string.IsNullOrWhiteSpace(Format))
    {
      arguments.Add("--format");
      arguments.Add(Format);
    }

    // Add verbosity if specified
    if (!string.IsNullOrWhiteSpace(Verbosity))
    {
      arguments.Add("--verbosity");
      arguments.Add(Verbosity);
    }

    // Add boolean flags
    if (ExactMatch)
    {
      arguments.Add("--exact-match");
    }

    if (Interactive)
    {
      arguments.Add("--interactive");
    }

    if (Prerelease)
    {
      arguments.Add("--prerelease");
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

  public async Task<ExecutionResult> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken);
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