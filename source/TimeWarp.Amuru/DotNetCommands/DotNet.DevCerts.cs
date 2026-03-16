namespace TimeWarp.Amuru;

/// <summary>
/// Fluent API for .NET CLI commands - Dev-certs command implementation.
/// </summary>
public static partial class DotNet
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet dev-certs' command.
  /// </summary>
  /// <returns>A DotNetDevCertsBuilder for configuring the dotnet dev-certs command</returns>
  public static DotNetDevCertsBuilder DevCerts()
  {
    return new DotNetDevCertsBuilder();
  }
}

/// <summary>
/// Fluent builder for configuring 'dotnet dev-certs' commands.
/// </summary>
public class DotNetDevCertsBuilder
{
  private CommandOptions Options = new();

  /// <summary>
  /// Specifies the working directory for the command.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsBuilder WithWorkingDirectory(string directory)
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
  public DotNetDevCertsBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return this;
  }

  /// <summary>
  /// Creates a fluent builder for the 'dotnet dev-certs https' command.
  /// </summary>
  /// <returns>A DotNetDevCertsHttpsBuilder for configuring the https command</returns>
  public DotNetDevCertsHttpsBuilder Https()
  {
    return new DotNetDevCertsHttpsBuilder(Options);
  }
}

/// <summary>
/// Fluent builder for 'dotnet dev-certs https' commands.
/// </summary>
public class DotNetDevCertsHttpsBuilder
{
  private readonly CommandOptions Options;
  private bool Check;
  private bool Clean;
  private bool Export;
  private bool Trust;
  private string? ExportPath;
  private string? Password;
  private string? Format;
  private bool NoPassword;
  private bool Verbose;
  private bool Quiet;

  public DotNetDevCertsHttpsBuilder(CommandOptions options)
  {
    Options = options;
  }

  /// <summary>
  /// Checks if a valid certificate exists.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsHttpsBuilder WithCheck()
  {
    Check = true;
    return this;
  }

  /// <summary>
  /// Cleans up existing certificates.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsHttpsBuilder WithClean()
  {
    Clean = true;
    return this;
  }

  /// <summary>
  /// Exports the certificate to a file.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsHttpsBuilder WithExport()
  {
    Export = true;
    return this;
  }

  /// <summary>
  /// Trusts the certificate on the local machine.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsHttpsBuilder WithTrust()
  {
    Trust = true;
    return this;
  }

  /// <summary>
  /// Specifies the path to export the certificate to.
  /// </summary>
  /// <param name="exportPath">The path to export the certificate to</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsHttpsBuilder WithExportPath(string exportPath)
  {
    ExportPath = exportPath;
    return this;
  }

  /// <summary>
  /// Specifies the password for the certificate.
  /// </summary>
  /// <param name="password">The password for the certificate</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsHttpsBuilder WithPassword(string password)
  {
    Password = password;
    return this;
  }

  /// <summary>
  /// Specifies the format of the certificate (Pfx, Pem).
  /// </summary>
  /// <param name="format">The certificate format</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsHttpsBuilder WithFormat(string format)
  {
    Format = format;
    return this;
  }

  /// <summary>
  /// Exports the certificate without a password.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsHttpsBuilder WithNoPassword()
  {
    NoPassword = true;
    return this;
  }

  /// <summary>
  /// Enables verbose logging.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsHttpsBuilder WithVerbose()
  {
    Verbose = true;
    return this;
  }

  /// <summary>
  /// Enables quiet mode (suppresses output).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsHttpsBuilder WithQuiet()
  {
    Quiet = true;
    return this;
  }

  public CommandResult Build()
  {
    List<string> arguments = new() { "dev-certs", "https" };

    if (Check)
    {
      arguments.Add("--check");
    }

    if (Clean)
    {
      arguments.Add("--clean");
    }

    if (Export)
    {
      arguments.Add("--export");
    }

    if (Trust)
    {
      arguments.Add("--trust");
    }

    if (!string.IsNullOrWhiteSpace(ExportPath))
    {
      arguments.Add("--export-path");
      arguments.Add(ExportPath);
    }

    if (!string.IsNullOrWhiteSpace(Password))
    {
      arguments.Add("--password");
      arguments.Add(Password);
    }

    if (!string.IsNullOrWhiteSpace(Format))
    {
      arguments.Add("--format");
      arguments.Add(Format);
    }

    if (NoPassword)
    {
      arguments.Add("--no-password");
    }

    if (Verbose)
    {
      arguments.Add("--verbose");
    }

    if (Quiet)
    {
      arguments.Add("--quiet");
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

  public async Task<ExecutionResult> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken);
  }
  
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken);
  }
}