#region Purpose
// Shared plumbing for every Gh builder: options, the WithArgument escape hatch, and execution
// wrapped in a gh span. Task 152.
#endregion

#region Design
// Three layers keep the common flags where gh accepts them:
//   GhCommandBuilder      options, WithArgument, execution with a span
//   GhRepoCommandBuilder  adds --repo (every repo-scoped verb)
//   GhJsonCommandBuilder  adds --json and --jq (verbs with JSON output)
//   GhListCommandBuilder  adds --limit (list verbs)
// The constructors are private protected, so only the builders in this assembly derive.
// Argument order: command words, the verb's own arguments, the shared flags, then WithArgument
// tokens. gh parses flags anywhere, so the order only matters for reading.
// Build() returns the CommandResult without a span: a caller that executes it directly gets
// TRACEPARENT from task 149 but no gh span. The execution methods here open the span first, so it
// is Activity.Current when CommandResult resolves TRACEPARENT at start.
#endregion

namespace TimeWarp.Amuru;

using System.Diagnostics;

/// <summary>
/// Base for the typed <c>gh</c> builders: options, extra arguments, and execution inside a gh span.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type, returned from each fluent method.</typeparam>
public abstract class GhCommandBuilder<TBuilder> : ICommandBuilder<TBuilder>
  where TBuilder : GhCommandBuilder<TBuilder>
{
  private readonly List<string> ExtraArguments = [];
  private CommandOptions Options = new();

  /// <summary>
  /// Creates a builder for one gh command.
  /// </summary>
  /// <param name="command">The command words, for example <c>run list</c>.</param>
  private protected GhCommandBuilder(string command)
  {
    Command = command;
  }

  /// <summary>
  /// The command words, for example <c>run list</c>. Also the <c>gh.command</c> span tag.
  /// </summary>
  public string Command { get; }

  /// <summary>
  /// The --repo value, when the builder has one. Tagged as <c>gh.repo</c>.
  /// </summary>
  private protected virtual string? Repository => null;

  /// <summary>
  /// The api endpoint for <c>gh api</c>. Tagged as <c>gh.api.endpoint</c>, path only.
  /// </summary>
  private protected virtual string? ApiEndpoint => null;

  private TBuilder Self => (TBuilder)this;

  /// <summary>
  /// Appends one argument the builder does not model, after every modeled argument.
  /// </summary>
  /// <param name="argument">The argument token</param>
  /// <returns>The builder instance for method chaining</returns>
  public TBuilder WithArgument(string argument)
  {
    ArgumentNullException.ThrowIfNull(argument);
    ExtraArguments.Add(argument);
    return Self;
  }

  /// <summary>
  /// Appends arguments the builder does not model, after every modeled argument.
  /// </summary>
  /// <param name="arguments">The argument tokens</param>
  /// <returns>The builder instance for method chaining</returns>
  public TBuilder WithArguments(params string[] arguments)
  {
    ArgumentNullException.ThrowIfNull(arguments);
    foreach (string argument in arguments)
    {
      WithArgument(argument);
    }

    return Self;
  }

  /// <summary>
  /// Specifies the working directory for the command. gh resolves the current repository from it.
  /// </summary>
  /// <param name="directory">The working directory path</param>
  /// <returns>The builder instance for method chaining</returns>
  public TBuilder WithWorkingDirectory(string directory)
  {
    Options = Options.WithWorkingDirectory(directory);
    return Self;
  }

  /// <summary>
  /// Adds an environment variable for the command execution.
  /// </summary>
  /// <param name="key">The environment variable name</param>
  /// <param name="value">The environment variable value</param>
  /// <returns>The builder instance for method chaining</returns>
  public TBuilder WithEnvironmentVariable(string key, string? value)
  {
    Options = Options.WithEnvironmentVariable(key, value);
    return Self;
  }

  /// <summary>
  /// Disables command validation, so a non-zero exit is reported through the exit code, not thrown.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public TBuilder WithNoValidation()
  {
    Options = Options.WithNoValidation();
    return Self;
  }

  /// <summary>
  /// Enables strict validation: a non-zero exit code causes the execution to throw
  /// instead of reporting the failure via the result's exit code.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public TBuilder WithZeroExitCodeValidation()
  {
    Options = Options.WithZeroExitCodeValidation();
    return Self;
  }

  /// <summary>
  /// Sets the maximum time this gh command may run. The caller's cancellation token still applies.
  /// </summary>
  /// <param name="timeout">Positive duration</param>
  /// <returns>The builder instance for method chaining</returns>
  public TBuilder WithTimeout(TimeSpan timeout)
  {
    Options = Options.WithTimeout(timeout);
    return Self;
  }

  /// <summary>
  /// The full argument list after <c>gh</c>, in the order it is passed.
  /// </summary>
  /// <returns>The arguments</returns>
  public IReadOnlyList<string> ToArguments()
  {
    List<string> arguments = [.. Command.Split(' ', StringSplitOptions.RemoveEmptyEntries)];
    AppendArguments(arguments);
    arguments.AddRange(ExtraArguments);
    return arguments;
  }

  /// <summary>
  /// Builds the command without running it. Executing the result directly opens no gh span; use the
  /// execution methods on this builder for that.
  /// </summary>
  /// <returns>A CommandResult for further processing</returns>
  public CommandResult Build()
  {
    return Shell.Run(Gh.Executable, [.. ToArguments()], Options);
  }

  /// <summary>
  /// Executes the command inside a gh span and streams output to the console.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The exit code of the command</returns>
  public Task<int> RunAsync(CancellationToken cancellationToken = default)
  {
    return ExecuteAsync(static (command, token) => command.RunAsync(token), static exitCode => exitCode, cancellationToken);
  }

  /// <summary>
  /// Executes the command inside a gh span and captures all output. Nothing is written to the console.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>CommandOutput with stdout, stderr, combined output and exit code</returns>
  public Task<CommandOutput> CaptureAsync(CancellationToken cancellationToken = default)
  {
    return ExecuteAsync(static (command, token) => command.CaptureAsync(token), static output => output.ExitCode, cancellationToken);
  }

  /// <summary>
  /// Executes the command inside a gh span, streams output to the console, and captures it.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>CommandOutput with stdout, stderr, combined output and exit code</returns>
  public Task<CommandOutput> RunAndCaptureAsync(CancellationToken cancellationToken = default)
  {
    return ExecuteAsync(static (command, token) => command.RunAndCaptureAsync(token), static output => output.ExitCode, cancellationToken);
  }

  /// <summary>
  /// Executes the command inside a gh span with the terminal attached, for prompts gh shows.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The execution result (output strings are empty since output goes to the console)</returns>
  public Task<CommandOutput> PassthroughAsync(CancellationToken cancellationToken = default)
  {
    return ExecuteAsync(static (command, token) => command.PassthroughAsync(token), static output => output.ExitCode, cancellationToken);
  }

  /// <summary>
  /// Appends the verb's own arguments and shared flags after the command words.
  /// </summary>
  /// <param name="arguments">The list to append to</param>
  private protected abstract void AppendArguments(List<string> arguments);

  /// <summary>
  /// Adds <paramref name="flag"/> and <paramref name="value"/> when the value is set.
  /// </summary>
  private protected static void AddOption(List<string> arguments, string flag, string? value)
  {
    if (!string.IsNullOrWhiteSpace(value))
    {
      arguments.Add(flag);
      arguments.Add(value);
    }
  }

  /// <summary>
  /// Adds <paramref name="flag"/> once per value.
  /// </summary>
  private protected static void AddOptions(List<string> arguments, string flag, IEnumerable<string> values)
  {
    foreach (string value in values)
    {
      AddOption(arguments, flag, value);
    }
  }

  /// <summary>
  /// Adds <paramref name="flag"/> when <paramref name="enabled"/> is true.
  /// </summary>
  private protected static void AddFlag(List<string> arguments, string flag, bool enabled)
  {
    if (enabled)
    {
      arguments.Add(flag);
    }
  }

  private async Task<TResult> ExecuteAsync<TResult>(
    Func<CommandResult, CancellationToken, Task<TResult>> execute,
    Func<TResult, int> exitCode,
    CancellationToken cancellationToken)
  {
    long started = Stopwatch.GetTimestamp();
    using Activity? activity = GhTelemetry.Start(Command, Repository, ApiEndpoint);
    try
    {
      TResult result = await execute(Build(), cancellationToken).ConfigureAwait(false);
      GhTelemetry.Complete(activity, exitCode(result), Stopwatch.GetElapsedTime(started));
      return result;
    }
    catch (Exception exception)
    {
      GhTelemetry.Fail(activity, exception, Stopwatch.GetElapsedTime(started));
      throw;
    }
  }
}

/// <summary>
/// Base for gh builders that take <c>--repo</c>.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type, returned from each fluent method.</typeparam>
public abstract class GhRepoCommandBuilder<TBuilder> : GhCommandBuilder<TBuilder>
  where TBuilder : GhRepoCommandBuilder<TBuilder>
{
  private string? Repo;

  private protected GhRepoCommandBuilder(string command) : base(command)
  {
  }

  private protected override string? Repository => Repo;

  /// <summary>
  /// Selects the repository with <c>--repo [HOST/]OWNER/REPO</c>. Tagged as <c>gh.repo</c>.
  /// </summary>
  /// <param name="repository">The repository, for example <c>TimeWarpEngineering/timewarp-amuru</c></param>
  /// <returns>The builder instance for method chaining</returns>
  public TBuilder WithRepo(string repository)
  {
    Repo = repository;
    return (TBuilder)this;
  }

  private protected sealed override void AppendArguments(List<string> arguments)
  {
    AppendVerbArguments(arguments);
    AddOption(arguments, "--repo", Repo);
    AppendSharedArguments(arguments);
  }

  /// <summary>
  /// Appends the verb's own positional arguments and flags.
  /// </summary>
  private protected abstract void AppendVerbArguments(List<string> arguments);

  /// <summary>
  /// Appends the shared flags of a derived layer, after --repo.
  /// </summary>
  private protected virtual void AppendSharedArguments(List<string> arguments)
  {
  }
}

/// <summary>
/// Base for gh builders with JSON output: <c>--repo</c>, <c>--json</c>, and <c>--jq</c>.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type, returned from each fluent method.</typeparam>
public abstract class GhJsonCommandBuilder<TBuilder> : GhRepoCommandBuilder<TBuilder>
  where TBuilder : GhJsonCommandBuilder<TBuilder>
{
  private readonly List<string> JsonFields = [];
  private string? JqExpression;

  private protected GhJsonCommandBuilder(string command) : base(command)
  {
  }

  /// <summary>
  /// Requests JSON output with <c>--json</c> and the given fields. Repeated calls add fields.
  /// </summary>
  /// <param name="fields">The JSON field names</param>
  /// <returns>The builder instance for method chaining</returns>
  public TBuilder WithJson(params string[] fields)
  {
    ArgumentNullException.ThrowIfNull(fields);
    foreach (string field in fields)
    {
      if (!string.IsNullOrWhiteSpace(field) && !JsonFields.Contains(field, StringComparer.Ordinal))
      {
        JsonFields.Add(field);
      }
    }

    return (TBuilder)this;
  }

  /// <summary>
  /// Filters the JSON output with <c>--jq</c>.
  /// </summary>
  /// <param name="expression">The jq expression</param>
  /// <returns>The builder instance for method chaining</returns>
  public TBuilder WithJq(string expression)
  {
    JqExpression = expression;
    return (TBuilder)this;
  }

  /// <summary>
  /// Replaces the JSON fields and clears --jq, for the typed result methods.
  /// </summary>
  private protected void UseJsonFields(IEnumerable<string> fields)
  {
    JsonFields.Clear();
    JsonFields.AddRange(fields);
    JqExpression = null;
  }

  private protected override void AppendSharedArguments(List<string> arguments)
  {
    if (JsonFields.Count > 0)
    {
      arguments.Add("--json");
      arguments.Add(string.Join(',', JsonFields));
    }

    AddOption(arguments, "--jq", JqExpression);
  }
}

/// <summary>
/// Base for gh list builders: <c>--repo</c>, <c>--json</c>, <c>--jq</c>, and <c>--limit</c>.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type, returned from each fluent method.</typeparam>
public abstract class GhListCommandBuilder<TBuilder> : GhJsonCommandBuilder<TBuilder>
  where TBuilder : GhListCommandBuilder<TBuilder>
{
  private int? Limit;

  private protected GhListCommandBuilder(string command) : base(command)
  {
  }

  /// <summary>
  /// Sets the maximum number of items with <c>--limit</c>.
  /// </summary>
  /// <param name="limit">A positive item count</param>
  /// <returns>The builder instance for method chaining</returns>
  public TBuilder WithLimit(int limit)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);
    Limit = limit;
    return (TBuilder)this;
  }

  private protected override void AppendSharedArguments(List<string> arguments)
  {
    base.AppendSharedArguments(arguments);
    if (Limit is int limit)
    {
      arguments.Add("--limit");
      arguments.Add(limit.ToString(CultureInfo.InvariantCulture));
    }
  }
}
