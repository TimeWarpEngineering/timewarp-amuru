#region Purpose
// TODO: Add purpose description
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for 'dotnet tool search' commands.
/// </summary>
public class DotNetToolSearchBuilder
{
  private readonly string SearchTerm;
  private CommandOptions Options;
  private bool Detail;
  private int? Skip;
  private int? Take;
  private bool Prerelease;

  /// <summary>
  /// Creates a builder for the <c>dotnet tool search</c> command using the given search text.
  /// </summary>
  /// <param name="searchTerm">Text matched against tool package ids.</param>
  /// <param name="options">Working directory, environment, and exit-code validation carried into the command.</param>
  public DotNetToolSearchBuilder(string searchTerm, CommandOptions options)
  {
    SearchTerm = searchTerm ?? throw new ArgumentNullException(nameof(searchTerm));
    Options = options;
  }

  /// <summary>
  /// Disables command validation, allowing the command to complete without throwing exceptions on non-zero exit codes.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolSearchBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return this;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolSearchBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return this;
  }

  /// <summary>
  /// Shows detailed information about the tools.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolSearchBuilder WithDetail()
  {
    Detail = true;
    return this;
  }

  /// <summary>
  /// Specifies the number of tools to skip.
  /// </summary>
  /// <param name="skip">The number of tools to skip</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolSearchBuilder WithSkip(int skip)
  {
    Skip = skip;
    return this;
  }

  /// <summary>
  /// Specifies the number of tools to take.
  /// </summary>
  /// <param name="take">The number of tools to take</param>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolSearchBuilder WithTake(int take)
  {
    Take = take;
    return this;
  }

  /// <summary>
  /// Includes prerelease tools in the search.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public DotNetToolSearchBuilder WithPrerelease()
  {
    Prerelease = true;
    return this;
  }

  /// <summary>
  /// Assembles the arguments for <c>dotnet tool search</c> and returns a command result that has not run yet.
  /// </summary>
  /// <returns>A command result ready to run, capture, or pass through.</returns>
  public CommandResult Build()
  {
    List<string> arguments = new() { "tool", "search", SearchTerm };

    if (Detail)
    {
      arguments.Add("--detail");
    }

    if (Skip.HasValue)
    {
      arguments.Add("--skip");
      arguments.Add(Skip.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    if (Take.HasValue)
    {
      arguments.Add("--take");
      arguments.Add(Take.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    if (Prerelease)
    {
      arguments.Add("--prerelease");
    }

    return Shell.Run("dotnet", arguments.ToArray(), Options);
  }

  /// <summary>
  /// Runs <c>dotnet tool search</c> and streams its output to the console.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The process exit code.</returns>
  public async Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return await Build().RunAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet tool search</c> without writing to the console and returns the captured output.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>Stdout, stderr, and the exit code.</returns>
  public async Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return await Build().CaptureAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet tool search</c> with the console streams attached so the process can read input and draw on the terminal.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process wrote to the console.</returns>
  public async Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().PassthroughAsync(cancellationToken).ConfigureAwait(false);
  }

  /// <summary>
  /// Runs <c>dotnet tool search</c> with the terminal inherited so a text UI can use the real TTY instead of piped console streams.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>A command output whose text is empty because the process inherited the terminal.</returns>
  public async Task<CommandOutput> TtyPassthroughAsync(CancellationToken cancellationToken = default)
  {
    return await Build().TtyPassthroughAsync(cancellationToken).ConfigureAwait(false);
  }
  
  /// <summary>
  /// Runs <c>dotnet tool search</c> as an interactive selection and returns the value written to stdout.
  /// </summary>
  /// <param name="cancellationToken">Token that cancels the running command.</param>
  /// <returns>The selected value.</returns>
  public async Task<string> SelectAsync(CancellationToken cancellationToken = default)
  {
    return await Build().SelectAsync(cancellationToken).ConfigureAwait(false);
  }
}
