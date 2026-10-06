#region Purpose
// Fluent builder for dotnet dev-certs.
#endregion

#region Design
// HTTPS export is --export-path. There is no --export switch. WithExport requires WithExportPath.
// Validation methods replace the options instance. Child builders keep a writable copy so a later call still applies.
#endregion

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
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
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
  private CommandOptions Options;
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

  /// <summary>
  /// Creates a builder for the <c>dotnet dev-certs https</c> command.
  /// </summary>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetDevCertsHttpsBuilder(CommandOptions options)
  {
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsHttpsBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetDevCertsHttpsBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
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
  /// Marks the command as a certificate export. Pair with WithExportPath.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  /// <exception cref="InvalidOperationException">Thrown by Build when an export path is missing.</exception>
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
  /// <exception cref="ArgumentException">Thrown when <paramref name="exportPath"/> is null or whitespace.</exception>
  public DotNetDevCertsHttpsBuilder WithExportPath(string exportPath)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(exportPath);
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

  /// <summary>
  /// Assembles the arguments for <c>dotnet dev-certs https</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
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

    if (Export && string.IsNullOrWhiteSpace(ExportPath))
    {
      throw new InvalidOperationException(
        "WithExport requires WithExportPath. dotnet dev-certs https exports with --export-path.");
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

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet dev-certs https</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet dev-certs https</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet dev-certs https</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet dev-certs https</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet dev-certs https</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}
