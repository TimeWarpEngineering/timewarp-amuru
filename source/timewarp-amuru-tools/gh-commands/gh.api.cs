#region Purpose
// Fluent builder for `gh api <endpoint>`. Task 152.
#endregion

#region Design
// The endpoint is the only api value that reaches the span, as gh.api.endpoint, path only
// (GhTelemetry.EndpointPath). Fields, headers, and the input body are passed to gh and never tagged.
// gh api has no --repo; {owner}/{repo} placeholders resolve from the working directory's repository.
#endregion

namespace TimeWarp.Amuru;

public static partial class Gh
{
  /// <summary>
  /// Creates a builder for <c>gh api &lt;endpoint&gt;</c>.
  /// </summary>
  /// <param name="endpoint">The REST path (for example <c>repos/{owner}/{repo}/actions/runs/1/artifacts</c>) or <c>graphql</c></param>
  /// <returns>A builder for an authenticated GitHub API request</returns>
  public static GhApiBuilder Api(string endpoint) => new(endpoint);
}

/// <summary>
/// Fluent builder for <c>gh api</c>.
/// </summary>
public sealed class GhApiBuilder : GhCommandBuilder<GhApiBuilder>
{
  private readonly string Endpoint;
  private readonly List<string> Fields = [];
  private readonly List<string> Headers = [];
  private string? Method;
  private string? Input;
  private string? JqExpression;
  private string? Hostname;
  private bool Paginate;
  private bool Slurp;
  private bool Silent;
  private bool Include;

  internal GhApiBuilder(string endpoint) : base("api")
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
    Endpoint = endpoint;
  }

  private protected override string? ApiEndpoint => Endpoint;

  /// <summary>
  /// Sets the HTTP method with <c>--method</c>. gh defaults to GET, or POST when fields are given.
  /// </summary>
  /// <param name="method">The HTTP method, for example <c>POST</c></param>
  /// <returns>The builder instance for method chaining</returns>
  public GhApiBuilder WithMethod(string method)
  {
    Method = method;
    return this;
  }

  /// <summary>
  /// Adds a string parameter with <c>-f key=value</c>. The value is never tagged or logged.
  /// </summary>
  /// <param name="key">The parameter name</param>
  /// <param name="value">The string value</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhApiBuilder WithRawField(string key, string value)
  {
    Fields.Add("-f");
    Fields.Add($"{key}={value}");
    return this;
  }

  /// <summary>
  /// Adds a typed parameter with <c>-F key=value</c> (true, false, null, integers, or <c>@file</c>).
  /// The value is never tagged or logged.
  /// </summary>
  /// <param name="key">The parameter name</param>
  /// <param name="value">The typed value</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhApiBuilder WithField(string key, string value)
  {
    Fields.Add("-F");
    Fields.Add($"{key}={value}");
    return this;
  }

  /// <summary>
  /// Adds a request header with <c>-H key:value</c>.
  /// </summary>
  /// <param name="header">The header, for example <c>Accept: application/vnd.github+json</c></param>
  /// <returns>The builder instance for method chaining</returns>
  public GhApiBuilder WithHeader(string header)
  {
    Headers.Add(header);
    return this;
  }

  /// <summary>
  /// Sends a file as the request body with <c>--input</c> (<c>-</c> reads standard input).
  /// </summary>
  /// <param name="path">The body file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhApiBuilder WithInput(string path)
  {
    Input = path;
    return this;
  }

  /// <summary>
  /// Filters the response with <c>--jq</c>.
  /// </summary>
  /// <param name="expression">The jq expression</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhApiBuilder WithJq(string expression)
  {
    JqExpression = expression;
    return this;
  }

  /// <summary>
  /// Targets another GitHub host with <c>--hostname</c>.
  /// </summary>
  /// <param name="hostname">The host, for example <c>github.example.com</c></param>
  /// <returns>The builder instance for method chaining</returns>
  public GhApiBuilder WithHostname(string hostname)
  {
    Hostname = hostname;
    return this;
  }

  /// <summary>
  /// Fetches every page with <c>--paginate</c>.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhApiBuilder WithPaginate()
  {
    Paginate = true;
    return this;
  }

  /// <summary>
  /// With <see cref="WithPaginate"/>, returns one array of all pages (<c>--slurp</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhApiBuilder WithSlurp()
  {
    Slurp = true;
    return this;
  }

  /// <summary>
  /// Suppresses the response body with <c>--silent</c>.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhApiBuilder WithSilent()
  {
    Silent = true;
    return this;
  }

  /// <summary>
  /// Includes the status line and headers with <c>--include</c>.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhApiBuilder WithInclude()
  {
    Include = true;
    return this;
  }

  private protected override void AppendArguments(List<string> arguments)
  {
    arguments.Add(Endpoint);
    AddOption(arguments, "--method", Method);
    AddOption(arguments, "--hostname", Hostname);
    arguments.AddRange(Fields);
    AddOptions(arguments, "-H", Headers);
    AddOption(arguments, "--input", Input);
    AddFlag(arguments, "--paginate", Paginate);
    AddFlag(arguments, "--slurp", Slurp);
    AddFlag(arguments, "--silent", Silent);
    AddFlag(arguments, "--include", Include);
    AddOption(arguments, "--jq", JqExpression);
  }
}
