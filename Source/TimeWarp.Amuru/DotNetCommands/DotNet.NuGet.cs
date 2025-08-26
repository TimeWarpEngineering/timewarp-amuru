namespace TimeWarp.Amuru;

/// <summary>
/// Represents the available NuGet cache types for the 'dotnet nuget locals' command.
/// </summary>
public enum NuGetCacheType
{
  /// <summary>
  /// All cache locations.
  /// </summary>
  All,
  
  /// <summary>
  /// HTTP request cache.
  /// </summary>
  HttpCache,
  
  /// <summary>
  /// Global packages folder.
  /// </summary>
  GlobalPackages,
  
  /// <summary>
  /// Temporary cache files.
  /// </summary>
  Temp
}

/// <summary>
/// Fluent API for .NET CLI commands - NuGet command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet nuget' command.
  /// </summary>
  /// <returns>A DotNetNuGetBuilder for configuring the dotnet nuget command</returns>
  public static DotNetNuGetBuilder NuGet()
  {
    return new DotNetNuGetBuilder();
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet nuget' commands.
/// </summary>
public class DotNetNuGetBuilder
{
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetBuilder WithWorkingDirectory(string directory)
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
  public DotNetNuGetBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet nuget push' command.
  /// </summary>
  /// <param name="packagePath">The path to the package to push</param>
  /// <returns>A DotNetNuGetPushBuilder for configuring the push command</returns>
  public DotNetNuGetPushBuilder Push(string packagePath)
  {
    return new DotNetNuGetPushBuilder(packagePath, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet nuget delete' command.
  /// </summary>
  /// <param name="packageName">The name of the package to delete</param>
  /// <param name="version">The version of the package to delete</param>
  /// <returns>A DotNetNuGetDeleteBuilder for configuring the delete command</returns>
  public DotNetNuGetDeleteBuilder Delete(string packageName, string version)
  {
    return new DotNetNuGetDeleteBuilder(packageName, version, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet nuget list source' command.
  /// </summary>
  /// <returns>A DotNetNuGetListSourceBuilder for configuring the list source command</returns>
  public DotNetNuGetListSourceBuilder ListSources()
  {
    return new DotNetNuGetListSourceBuilder(Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet nuget add source' command.
  /// </summary>
  /// <param name="source">The package source URL or path</param>
  /// <returns>A DotNetNuGetAddSourceBuilder for configuring the add source command</returns>
  public DotNetNuGetAddSourceBuilder AddSource(string source)
  {
    return new DotNetNuGetAddSourceBuilder(source, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet nuget remove source' command.
  /// </summary>
  /// <param name="name">The name of the source to remove</param>
  /// <returns>A DotNetNuGetRemoveSourceBuilder for configuring the remove source command</returns>
  public DotNetNuGetRemoveSourceBuilder RemoveSource(string name)
  {
    return new DotNetNuGetRemoveSourceBuilder(name, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet nuget update source' command.
  /// </summary>
  /// <param name="name">The name of the source to update</param>
  /// <returns>A DotNetNuGetUpdateSourceBuilder for configuring the update source command</returns>
  public DotNetNuGetUpdateSourceBuilder UpdateSource(string name)
  {
    return new DotNetNuGetUpdateSourceBuilder(name, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet nuget enable source' command.
  /// </summary>
  /// <param name="name">The name of the source to enable</param>
  /// <returns>A DotNetNuGetEnableSourceBuilder for configuring the enable source command</returns>
  public DotNetNuGetEnableSourceBuilder EnableSource(string name)
  {
    return new DotNetNuGetEnableSourceBuilder(name, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet nuget disable source' command.
  /// </summary>
  /// <param name="name">The name of the source to disable</param>
  /// <returns>A DotNetNuGetDisableSourceBuilder for configuring the disable source command</returns>
  public DotNetNuGetDisableSourceBuilder DisableSource(string name)
  {
    return new DotNetNuGetDisableSourceBuilder(name, Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet nuget locals' command.
  /// </summary>
  /// <returns>A DotNetNuGetLocalsBuilder for configuring the locals command</returns>
  public DotNetNuGetLocalsBuilder Locals()
  {
    return new DotNetNuGetLocalsBuilder(Options);
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet nuget why' command.
  /// </summary>
  /// <param name="packageName">The name of the package to analyze</param>
  /// <returns>A DotNetNuGetWhyBuilder for configuring the why command</returns>
  public DotNetNuGetWhyBuilder Why(string packageName)
  {
    return new DotNetNuGetWhyBuilder(packageName, Options);
  }
}

/// <summary>
/// Fluent builder for 'dotnet nuget push' commands.
/// </summary>
public class DotNetNuGetPushBuilder
{
  private readonly string PackagePath;
  private readonly CommandOptions Options;
  private string? Source;
  private string? SymbolSource;
  private int? Timeout;
  private string? ApiKey;
  private string? SymbolApiKey;
  private bool DisableBuffering;
  private bool NoSymbols;
  private bool NoServiceEndpoint;
  private bool Interactive;
  private bool SkipDuplicate;
  private string? ConfigFile;

  public DotNetNuGetPushBuilder(string packagePath, CommandOptions options)
  {
    PackagePath = packagePath ?? throw new ArgumentNullException(nameof(packagePath));
    Options = options;
  }

  /// <summary>
  /// Specifies the package source to use.
  /// </summary>
  /// <param name="source">Package source (URL, UNC/folder path or package source name)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetPushBuilder WithSource(string source)
  {
    Source = source;
    return this;
  }

  /// <summary>
  /// Specifies the symbol server URL to use.
  /// </summary>
  /// <param name="symbolSource">Symbol server URL</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetPushBuilder WithSymbolSource(string symbolSource)
  {
    SymbolSource = symbolSource;
    return this;
  }

  /// <summary>
  /// Specifies the timeout for pushing to a server in seconds.
  /// </summary>
  /// <param name="timeout">Timeout in seconds (default is 300)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetPushBuilder WithTimeout(int timeout)
  {
    Timeout = timeout;
    return this;
  }

  /// <summary>
  /// Specifies the API key for the server.
  /// </summary>
  /// <param name="apiKey">The API key</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetPushBuilder WithApiKey(string apiKey)
  {
    ApiKey = apiKey;
    return this;
  }

  /// <summary>
  /// Specifies the API key for the symbol server.
  /// </summary>
  /// <param name="symbolApiKey">The symbol API key</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetPushBuilder WithSymbolApiKey(string symbolApiKey)
  {
    SymbolApiKey = symbolApiKey;
    return this;
  }

  /// <summary>
  /// Disables buffering when pushing to an HTTP(S) server to decrease memory usage.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetPushBuilder WithDisableBuffering()
  {
    DisableBuffering = true;
    return this;
  }

  /// <summary>
  /// If a symbols package exists, it will not be pushed to a symbols server.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetPushBuilder WithNoSymbols()
  {
    NoSymbols = true;
    return this;
  }

  /// <summary>
  /// Does not append "api/v2/package" to the source URL.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetPushBuilder WithNoServiceEndpoint()
  {
    NoServiceEndpoint = true;
    return this;
  }

  /// <summary>
  /// Allows the command to block and require manual action for operations like authentication.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetPushBuilder WithInteractive()
  {
    Interactive = true;
    return this;
  }

  /// <summary>
  /// If a package and version already exists, skip it and continue with the next package in the push.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetPushBuilder WithSkipDuplicate()
  {
    SkipDuplicate = true;
    return this;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetPushBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "nuget", "push", PackagePath };

    if (!string.IsNullOrWhiteSpace(Source))
    {
      arguments.Add("--source");
      arguments.Add(Source);
    }

    if (!string.IsNullOrWhiteSpace(SymbolSource))
    {
      arguments.Add("--symbol-source");
      arguments.Add(SymbolSource);
    }

    if (Timeout.HasValue)
    {
      arguments.Add("--timeout");
      arguments.Add(Timeout.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    if (!string.IsNullOrWhiteSpace(ApiKey))
    {
      arguments.Add("--api-key");
      arguments.Add(ApiKey);
    }

    if (!string.IsNullOrWhiteSpace(SymbolApiKey))
    {
      arguments.Add("--symbol-api-key");
      arguments.Add(SymbolApiKey);
    }

    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
    }

    if (DisableBuffering)
    {
      arguments.Add("--disable-buffering");
    }

    if (NoSymbols)
    {
      arguments.Add("--no-symbols");
    }

    if (NoServiceEndpoint)
    {
      arguments.Add("--no-service-endpoint");
    }

    if (Interactive)
    {
      arguments.Add("--interactive");
    }

    if (SkipDuplicate)
    {
      arguments.Add("--skip-duplicate");
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
/// Fluent builder for 'dotnet nuget delete' commands.
/// </summary>
public class DotNetNuGetDeleteBuilder
{
  private readonly string PackageName;
  private readonly string Version;
  private readonly CommandOptions Options;
  private string? Source;
  private string? ApiKey;
  private bool Interactive;
  private string? ConfigFile;

  public DotNetNuGetDeleteBuilder(string packageName, string version, CommandOptions options)
  {
    PackageName = packageName ?? throw new ArgumentNullException(nameof(packageName));
    Version = version ?? throw new ArgumentNullException(nameof(version));
    Options = options;
  }

  /// <summary>
  /// Specifies the package source to use.
  /// </summary>
  /// <param name="source">Package source (URL, UNC/folder path or package source name)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetDeleteBuilder WithSource(string source)
  {
    Source = source;
    return this;
  }

  /// <summary>
  /// Specifies the API key for the server.
  /// </summary>
  /// <param name="apiKey">The API key</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetDeleteBuilder WithApiKey(string apiKey)
  {
    ApiKey = apiKey;
    return this;
  }

  /// <summary>
  /// Allows the command to block and require manual action for operations like authentication.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetDeleteBuilder WithInteractive()
  {
    Interactive = true;
    return this;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetDeleteBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "nuget", "delete", PackageName, Version };

    if (!string.IsNullOrWhiteSpace(Source))
    {
      arguments.Add("--source");
      arguments.Add(Source);
    }

    if (!string.IsNullOrWhiteSpace(ApiKey))
    {
      arguments.Add("--api-key");
      arguments.Add(ApiKey);
    }

    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
    }

    if (Interactive)
    {
      arguments.Add("--interactive");
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
/// Fluent builder for 'dotnet nuget list source' commands.
/// </summary>
public class DotNetNuGetListSourceBuilder
{
  private readonly CommandOptions Options;
  private string? Format;
  private string? ConfigFile;

  public DotNetNuGetListSourceBuilder(CommandOptions options)
  {
    Options = options;
  }

  /// <summary>
  /// Specifies the format of the list command output.
  /// </summary>
  /// <param name="format">The format: "Detailed" (default) or "Short"</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetListSourceBuilder WithFormat(string format)
  {
    Format = format;
    return this;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetListSourceBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "nuget", "list", "source" };

    if (!string.IsNullOrWhiteSpace(Format))
    {
      arguments.Add("--format");
      arguments.Add(Format);
    }

    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
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
/// Fluent builder for 'dotnet nuget add source' commands.
/// </summary>
public class DotNetNuGetAddSourceBuilder
{
  private readonly string Source;
  private readonly CommandOptions Options;
  private string? Name;
  private string? Username;
  private string? Password;
  private string? ConfigFile;

  public DotNetNuGetAddSourceBuilder(string source, CommandOptions options)
  {
    Source = source ?? throw new ArgumentNullException(nameof(source));
    Options = options;
  }

  /// <summary>
  /// Specifies the name of the source.
  /// </summary>
  /// <param name="name">The name of the source</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetAddSourceBuilder WithName(string name)
  {
    Name = name;
    return this;
  }

  /// <summary>
  /// Specifies the username for the source.
  /// </summary>
  /// <param name="username">The username</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetAddSourceBuilder WithUsername(string username)
  {
    Username = username;
    return this;
  }

  /// <summary>
  /// Specifies the password for the source.
  /// </summary>
  /// <param name="password">The password</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetAddSourceBuilder WithPassword(string password)
  {
    Password = password;
    return this;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetAddSourceBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "nuget", "add", "source", Source };

    if (!string.IsNullOrWhiteSpace(Name))
    {
      arguments.Add("--name");
      arguments.Add(Name);
    }

    if (!string.IsNullOrWhiteSpace(Username))
    {
      arguments.Add("--username");
      arguments.Add(Username);
    }

    if (!string.IsNullOrWhiteSpace(Password))
    {
      arguments.Add("--password");
      arguments.Add(Password);
    }

    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
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
/// Fluent builder for 'dotnet nuget remove source' commands.
/// </summary>
public class DotNetNuGetRemoveSourceBuilder
{
  private readonly string Name;
  private readonly CommandOptions Options;
  private string? ConfigFile;

  public DotNetNuGetRemoveSourceBuilder(string name, CommandOptions options)
  {
    Name = name ?? throw new ArgumentNullException(nameof(name));
    Options = options;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetRemoveSourceBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "nuget", "remove", "source", Name };

    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
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
/// Fluent builder for 'dotnet nuget update source' commands.
/// </summary>
public class DotNetNuGetUpdateSourceBuilder
{
  private readonly string Name;
  private readonly CommandOptions Options;
  private string? Source;
  private string? Username;
  private string? Password;
  private string? ConfigFile;

  public DotNetNuGetUpdateSourceBuilder(string name, CommandOptions options)
  {
    Name = name ?? throw new ArgumentNullException(nameof(name));
    Options = options;
  }

  /// <summary>
  /// Specifies the new source URL.
  /// </summary>
  /// <param name="source">The new source URL</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetUpdateSourceBuilder WithSource(string source)
  {
    Source = source;
    return this;
  }

  /// <summary>
  /// Specifies the username for the source.
  /// </summary>
  /// <param name="username">The username</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetUpdateSourceBuilder WithUsername(string username)
  {
    Username = username;
    return this;
  }

  /// <summary>
  /// Specifies the password for the source.
  /// </summary>
  /// <param name="password">The password</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetUpdateSourceBuilder WithPassword(string password)
  {
    Password = password;
    return this;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetUpdateSourceBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "nuget", "update", "source", Name };

    if (!string.IsNullOrWhiteSpace(Source))
    {
      arguments.Add("--source");
      arguments.Add(Source);
    }

    if (!string.IsNullOrWhiteSpace(Username))
    {
      arguments.Add("--username");
      arguments.Add(Username);
    }

    if (!string.IsNullOrWhiteSpace(Password))
    {
      arguments.Add("--password");
      arguments.Add(Password);
    }

    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
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
/// Fluent builder for 'dotnet nuget enable source' commands.
/// </summary>
public class DotNetNuGetEnableSourceBuilder
{
  private readonly string Name;
  private readonly CommandOptions Options;
  private string? ConfigFile;

  public DotNetNuGetEnableSourceBuilder(string name, CommandOptions options)
  {
    Name = name ?? throw new ArgumentNullException(nameof(name));
    Options = options;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetEnableSourceBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "nuget", "enable", "source", Name };

    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
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
/// Fluent builder for 'dotnet nuget disable source' commands.
/// </summary>
public class DotNetNuGetDisableSourceBuilder
{
  private readonly string Name;
  private readonly CommandOptions Options;
  private string? ConfigFile;

  public DotNetNuGetDisableSourceBuilder(string name, CommandOptions options)
  {
    Name = name ?? throw new ArgumentNullException(nameof(name));
    Options = options;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetDisableSourceBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "nuget", "disable", "source", Name };

    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
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
/// Fluent builder for 'dotnet nuget locals' commands.
/// </summary>
public class DotNetNuGetLocalsBuilder
{
  private readonly CommandOptions Options;
  private NuGetCacheType? ClearCacheType;
  private NuGetCacheType? ListCacheType;

  public DotNetNuGetLocalsBuilder(CommandOptions options)
  {
    Options = options;
  }

  /// <summary>
  /// Clears the specified local cache.
  /// </summary>
  /// <param name="cacheType">The cache type to clear</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetLocalsBuilder Clear(NuGetCacheType cacheType)
  {
    ClearCacheType = cacheType;
    return this;
  }

  /// <summary>
  /// Lists the specified local cache.
  /// </summary>
  /// <param name="cacheType">The cache type to list</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetLocalsBuilder List(NuGetCacheType cacheType)
  {
    ListCacheType = cacheType;
    return this;
  }

  private static string GetCacheTypeString(NuGetCacheType cacheType)
  {
    return cacheType switch
    {
      NuGetCacheType.All => "all",
      NuGetCacheType.HttpCache => "http-cache",
      NuGetCacheType.GlobalPackages => "global-packages",
      NuGetCacheType.Temp => "temp",
      _ => throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null)
    };
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "nuget", "locals" };

    if (ClearCacheType.HasValue)
    {
      arguments.Add(GetCacheTypeString(ClearCacheType.Value));
      arguments.Add("--clear");
    }
    else if (ListCacheType.HasValue)
    {
      arguments.Add(GetCacheTypeString(ListCacheType.Value));
      arguments.Add("--list");
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
/// Fluent builder for 'dotnet nuget why' commands.
/// </summary>
public class DotNetNuGetWhyBuilder
{
  private readonly string PackageName;
  private readonly CommandOptions Options;
  private string? Project;
  private string? Framework;

  public DotNetNuGetWhyBuilder(string packageName, CommandOptions options)
  {
    PackageName = packageName ?? throw new ArgumentNullException(nameof(packageName));
    Options = options;
  }

  /// <summary>
  /// Specifies the project file to analyze.
  /// </summary>
  /// <param name="project">The project file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetWhyBuilder WithProject(string project)
  {
    Project = project;
    return this;
  }

  /// <summary>
  /// Specifies the target framework to analyze.
  /// </summary>
  /// <param name="framework">The target framework moniker</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetWhyBuilder WithFramework(string framework)
  {
    Framework = framework;
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "nuget", "why" };

    if (!string.IsNullOrWhiteSpace(Project))
    {
      arguments.Add("--project");
      arguments.Add(Project);
    }

    if (!string.IsNullOrWhiteSpace(Framework))
    {
      arguments.Add("--framework");
      arguments.Add(Framework);
    }

    arguments.Add(PackageName);

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