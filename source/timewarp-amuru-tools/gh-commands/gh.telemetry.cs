#region Purpose
// Opens and closes the span for one gh execution. Task 152.
#endregion

#region Design
// One ActivitySource for all gh builders. StartActivity returns null when nothing listens, and then
// nothing is tagged; Activity.Current stays the caller's span, so TRACEPARENT still names it.
// The span is current while the command runs, so CommandResult hands gh a TRACEPARENT naming it.
// Tags follow dot-net.build-telemetry.cs: only fixed, known-safe values. The command words, the
// --repo value, the api endpoint path (query cut off), the exit code, and the elapsed time.
// Never argv, stdout, stderr, field values, bodies, tokens, or exception text. A failed run sets
// Error status with the exit code; a thrown exception sets Error with the exception type name only.
#endregion

namespace TimeWarp.Amuru;

using System.Diagnostics;

/// <summary>
/// Span creation and tagging for <see cref="Gh"/> builders.
/// </summary>
internal static class GhTelemetry
{
  internal const string CommandTag = "gh.command";
  internal const string RepoTag = "gh.repo";
  internal const string ApiEndpointTag = "gh.api.endpoint";
  internal const string ExitCodeTag = "gh.exit_code";
  internal const string ElapsedTag = "gh.elapsed_ms";

  private static readonly ActivitySource Source = new(Gh.ActivitySourceName);

  /// <summary>
  /// Starts the span for one gh execution, or returns null when no listener samples it.
  /// </summary>
  /// <param name="command">The command words, for example <c>run list</c>.</param>
  /// <param name="repository">The --repo value, if any.</param>
  /// <param name="apiEndpoint">The api endpoint, if this is <c>gh api</c>.</param>
  internal static Activity? Start(string command, string? repository, string? apiEndpoint)
  {
    Activity? activity = Source.StartActivity($"{Gh.Executable} {command}", ActivityKind.Client);
    if (activity is null)
    {
      return null;
    }

    activity.SetTag(CommandTag, command);
    if (!string.IsNullOrWhiteSpace(repository))
    {
      activity.SetTag(RepoTag, repository);
    }

    string? endpoint = EndpointPath(apiEndpoint);
    if (endpoint is not null)
    {
      activity.SetTag(ApiEndpointTag, endpoint);
    }

    return activity;
  }

  /// <summary>
  /// Records the exit code and elapsed time of a finished run.
  /// </summary>
  internal static void Complete(Activity? activity, int exitCode, TimeSpan elapsed)
  {
    if (activity is null)
    {
      return;
    }

    activity.SetTag(ExitCodeTag, exitCode);
    activity.SetTag(ElapsedTag, elapsed.TotalMilliseconds);
    if (exitCode != 0)
    {
      activity.SetStatus(ActivityStatusCode.Error, $"exit code {exitCode.ToString(CultureInfo.InvariantCulture)}");
    }
  }

  /// <summary>
  /// Records a run that threw. Only the exception type name is exported.
  /// </summary>
  internal static void Fail(Activity? activity, Exception exception, TimeSpan elapsed)
  {
    if (activity is null)
    {
      return;
    }

    activity.SetTag(ElapsedTag, elapsed.TotalMilliseconds);
    activity.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
  }

  /// <summary>
  /// The endpoint path: no query string, fragment, scheme, host, or user info. Null when there is no endpoint.
  /// </summary>
  internal static string? EndpointPath(string? endpoint)
  {
    if (string.IsNullOrWhiteSpace(endpoint))
    {
      return null;
    }

    string trimmed = endpoint.Trim();

    // A full URL keeps only its path, so user info, host, and query never reach a tag.
    if (Uri.TryCreate(trimmed, UriKind.Absolute, out Uri? uri) && uri.Scheme is "http" or "https")
    {
      return uri.AbsolutePath;
    }

    int cut = trimmed.IndexOfAny(['?', '#']);
    return cut >= 0 ? trimmed[..cut] : trimmed;
  }
}
