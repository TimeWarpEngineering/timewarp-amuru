#region Purpose
// Fluent builder for dotnet nuget verify, and the DotNet.NuGet().Verify(...) entry point. Task 151.
#endregion

#region Design
// Lives beside dot-net.nu-get.cs as a partial of DotNetNuGetBuilder because that file is at its line limit (TWA0038).
// Each certificate fingerprint is its own --certificate-fingerprint pair, in call order.
// Like every nuget sub-builder it does not tag Activity.Current.
#endregion

namespace TimeWarp.Amuru;

public partial class DotNetNuGetBuilder
{
  /// <summary>
  /// Creates a fluent builder for the 'dotnet nuget verify' command.
  /// </summary>
  /// <param name="packagePath">The path of the package (or packages, with wildcards) to verify</param>
  /// <returns>A DotNetNuGetVerifyBuilder for configuring the verify command</returns>
  public DotNetNuGetVerifyBuilder Verify(string packagePath)
  {
    return new DotNetNuGetVerifyBuilder(packagePath, Options);
  }
}

/// <summary>
/// Fluent builder for 'dotnet nuget verify' commands.
/// </summary>
public class DotNetNuGetVerifyBuilder
{
  private readonly string PackagePath;
  private readonly List<string> CertificateFingerprints = [];
  private CommandOptions Options;
  private bool All;
  private string? Verbosity;
  private string? ConfigFile;

  /// <summary>
  /// Creates a builder for the <c>dotnet nuget verify</c> command using the given package path.
  /// </summary>
  /// <param name="packagePath">Path of the nupkg to verify. Wildcards are passed through to NuGet.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetNuGetVerifyBuilder(string packagePath, CommandOptions options)
  {
    PackagePath = packagePath ?? throw new ArgumentNullException(nameof(packagePath));
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetVerifyBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetVerifyBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Runs every verification. Emits <c>--all</c>.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetVerifyBuilder WithAll()
  {
    All = true;
    return this;
  }

  /// <summary>
  /// Adds a SHA-256 certificate fingerprint the package signature must match. Call again for more fingerprints.
  /// </summary>
  /// <param name="fingerprint">The SHA-256 certificate fingerprint</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetVerifyBuilder WithCertificateFingerprint(string fingerprint)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(fingerprint);
    CertificateFingerprints.Add(fingerprint);
    return this;
  }

  /// <summary>
  /// Sets the verbosity level of the command.
  /// </summary>
  /// <param name="verbosity">The verbosity level (quiet, minimal, normal, detailed, diagnostic)</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetVerifyBuilder WithVerbosity(string verbosity)
  {
    Verbosity = verbosity;
    return this;
  }

  /// <summary>
  /// Specifies the NuGet configuration file to use.
  /// </summary>
  /// <param name="configFile">The NuGet configuration file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetNuGetVerifyBuilder WithConfigFile(string configFile)
  {
    ConfigFile = configFile;
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet nuget verify</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = ["nuget", "verify", PackagePath];

    if (All)
    {
      arguments.Add("--all");
    }

    foreach (string fingerprint in CertificateFingerprints)
    {
      arguments.Add("--certificate-fingerprint");
      arguments.Add(fingerprint);
    }

    if (!string.IsNullOrWhiteSpace(ConfigFile))
    {
      arguments.Add("--configfile");
      arguments.Add(ConfigFile);
    }

    if (!string.IsNullOrWhiteSpace(Verbosity))
    {
      arguments.Add("--verbosity");
      arguments.Add(Verbosity);
    }

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet nuget verify</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet nuget verify</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet nuget verify</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet nuget verify</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet nuget verify</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}
