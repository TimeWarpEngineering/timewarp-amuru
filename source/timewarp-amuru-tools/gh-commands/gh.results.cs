#region Purpose
// Typed results for the gh JSON shapes that are stable: run list, pr view, and auth status. Task 152.
#endregion

#region Design
// Data records mirror gh's --json field names (camelCase via GhJsonContext). Every field is
// nullable or defaulted: gh omits or nulls fields that do not apply (conclusion while a run is in
// progress, mergedAt on an open pull request). The result wrappers carry the CommandOutput so a
// caller can read stderr when Success is false. GhAuthStatus never holds a token: it is read from
// `gh auth status --json hosts`, which has no token field.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// One workflow run from <c>gh run list --json</c>.
/// </summary>
public sealed record GhWorkflowRun
{
  /// <summary>
  /// The <c>--json</c> fields <see cref="GhRunListBuilder.ListRunsAsync"/> requests.
  /// </summary>
  public static IReadOnlyList<string> JsonFields { get; } =
  [
    "attempt",
    "conclusion",
    "createdAt",
    "databaseId",
    "displayTitle",
    "event",
    "headBranch",
    "headSha",
    "name",
    "number",
    "startedAt",
    "status",
    "updatedAt",
    "url",
    "workflowDatabaseId",
    "workflowName",
  ];

  /// <summary>The run id used by <c>gh run view</c> and <c>gh run download</c>.</summary>
  public long DatabaseId { get; init; }

  /// <summary>The run number within its workflow.</summary>
  public int Number { get; init; }

  /// <summary>The attempt number.</summary>
  public int Attempt { get; init; }

  /// <summary>The run name.</summary>
  public string? Name { get; init; }

  /// <summary>The title shown in the Actions list.</summary>
  public string? DisplayTitle { get; init; }

  /// <summary>queued, in_progress, completed, and so on.</summary>
  public string? Status { get; init; }

  /// <summary>success, failure, cancelled, and so on. Empty while the run is not completed.</summary>
  public string? Conclusion { get; init; }

  /// <summary>The triggering event, for example push.</summary>
  public string? Event { get; init; }

  /// <summary>The head branch.</summary>
  public string? HeadBranch { get; init; }

  /// <summary>The head commit SHA.</summary>
  public string? HeadSha { get; init; }

  /// <summary>The workflow name.</summary>
  public string? WorkflowName { get; init; }

  /// <summary>The workflow id.</summary>
  public long WorkflowDatabaseId { get; init; }

  /// <summary>The run's web URL.</summary>
  public Uri? Url { get; init; }

  /// <summary>When the run was created.</summary>
  public DateTimeOffset CreatedAt { get; init; }

  /// <summary>When the run started.</summary>
  public DateTimeOffset StartedAt { get; init; }

  /// <summary>When the run was last updated.</summary>
  public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// The result of <see cref="GhRunListBuilder.ListRunsAsync"/>.
/// </summary>
public sealed class GhRunListResult
{
  internal GhRunListResult(bool success, IReadOnlyList<GhWorkflowRun> runs, CommandOutput output)
  {
    Success = success;
    Runs = runs;
    Output = output;
  }

  /// <summary>True when gh exited zero.</summary>
  public bool Success { get; }

  /// <summary>The runs, newest first as gh returns them. Empty when <see cref="Success"/> is false.</summary>
  public IReadOnlyList<GhWorkflowRun> Runs { get; }

  /// <summary>The raw gh output.</summary>
  public CommandOutput Output { get; }
}

/// <summary>
/// A GitHub user or bot as gh reports it in <c>author</c>.
/// </summary>
public sealed record GhActor
{
  /// <summary>The login.</summary>
  public string? Login { get; init; }

  /// <summary>The display name, when set.</summary>
  public string? Name { get; init; }
}

/// <summary>
/// One pull request from <c>gh pr view --json</c>.
/// </summary>
public sealed record GhPullRequest
{
  /// <summary>
  /// The <c>--json</c> fields <see cref="GhPrViewBuilder.ViewPullRequestAsync"/> requests.
  /// </summary>
  public static IReadOnlyList<string> JsonFields { get; } =
  [
    "author",
    "baseRefName",
    "createdAt",
    "headRefName",
    "headRefOid",
    "isDraft",
    "mergeStateStatus",
    "mergeable",
    "mergedAt",
    "number",
    "reviewDecision",
    "state",
    "title",
    "url",
  ];

  /// <summary>The pull request number.</summary>
  public int Number { get; init; }

  /// <summary>The title.</summary>
  public string? Title { get; init; }

  /// <summary>OPEN, CLOSED, or MERGED.</summary>
  public string? State { get; init; }

  /// <summary>The web URL.</summary>
  public Uri? Url { get; init; }

  /// <summary>True for a draft.</summary>
  public bool IsDraft { get; init; }

  /// <summary>The head branch.</summary>
  public string? HeadRefName { get; init; }

  /// <summary>The head commit SHA.</summary>
  public string? HeadRefOid { get; init; }

  /// <summary>The base branch.</summary>
  public string? BaseRefName { get; init; }

  /// <summary>MERGEABLE, CONFLICTING, or UNKNOWN.</summary>
  public string? Mergeable { get; init; }

  /// <summary>CLEAN, BLOCKED, BEHIND, DIRTY, and so on.</summary>
  public string? MergeStateStatus { get; init; }

  /// <summary>APPROVED, CHANGES_REQUESTED, REVIEW_REQUIRED, or empty.</summary>
  public string? ReviewDecision { get; init; }

  /// <summary>The author.</summary>
  public GhActor? Author { get; init; }

  /// <summary>When the pull request was opened.</summary>
  public DateTimeOffset CreatedAt { get; init; }

  /// <summary>When it merged, or null.</summary>
  public DateTimeOffset? MergedAt { get; init; }
}

/// <summary>
/// The result of <see cref="GhPrViewBuilder.ViewPullRequestAsync"/>.
/// </summary>
public sealed class GhPullRequestViewResult
{
  internal GhPullRequestViewResult(bool success, GhPullRequest? pullRequest, CommandOutput output)
  {
    Success = success;
    PullRequest = pullRequest;
    Output = output;
  }

  /// <summary>True when gh exited zero.</summary>
  public bool Success { get; }

  /// <summary>The pull request, or null when <see cref="Success"/> is false.</summary>
  public GhPullRequest? PullRequest { get; }

  /// <summary>The raw gh output.</summary>
  public CommandOutput Output { get; }
}

/// <summary>
/// The result of <see cref="GhAuthStatusBuilder.GetStatusAsync"/>: whether gh is signed in, and where.
/// </summary>
public sealed class GhAuthStatus
{
  internal GhAuthStatus(bool signedIn, string? host, string? login, CommandOutput output)
  {
    SignedIn = signedIn;
    Host = host;
    Login = login;
    Output = output;
  }

  /// <summary>True when the active account on <see cref="Host"/> has a working token.</summary>
  public bool SignedIn { get; }

  /// <summary>The host checked, for example github.com. Null when gh knows no host.</summary>
  public string? Host { get; }

  /// <summary>The active account's login, when gh reports one.</summary>
  public string? Login { get; }

  /// <summary>The raw gh output (the <c>--json hosts</c> document; it holds no token).</summary>
  public CommandOutput Output { get; }
}
