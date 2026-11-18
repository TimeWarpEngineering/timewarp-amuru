#region Purpose
// Fluent builder for `gh pr view`, with a typed result for the JSON view. Task 152.
#endregion

#region Design
// ViewPullRequestAsync replaces any --json fields and --jq with GhPullRequest.JsonFields.
// A non-zero exit (no pull request for the branch, for one) returns Success false.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for <c>gh pr view</c>.
/// </summary>
public sealed class GhPrViewBuilder : GhJsonCommandBuilder<GhPrViewBuilder>
{
  private readonly string? Selector;
  private bool Comments;

  internal GhPrViewBuilder(string command, string? selector) : base(command)
  {
    Selector = selector;
  }

  /// <summary>
  /// Includes comments with <c>--comments</c>.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrViewBuilder WithComments()
  {
    Comments = true;
    return this;
  }

  /// <summary>
  /// Runs <c>gh pr view --json</c> with <see cref="GhPullRequest.JsonFields"/> inside a gh span and
  /// returns the pull request. Any <c>--json</c> fields or <c>--jq</c> set earlier are replaced.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The pull request, or Success false when gh failed</returns>
  public async Task<GhPullRequestViewResult> ViewPullRequestAsync(CancellationToken cancellationToken = default)
  {
    UseJsonFields(GhPullRequest.JsonFields);
    CommandOutput output = await CaptureAsync(cancellationToken).ConfigureAwait(false);
    if (!output.Success)
    {
      return new GhPullRequestViewResult(false, null, output);
    }

    GhPullRequest? pullRequest = GhJson.Deserialize(output.Stdout, GhJsonContext.Default.GhPullRequest);
    return new GhPullRequestViewResult(pullRequest is not null, pullRequest, output);
  }

  private protected override void AppendVerbArguments(List<string> arguments)
  {
    if (!string.IsNullOrWhiteSpace(Selector))
    {
      arguments.Add(Selector);
    }

    AddFlag(arguments, "--comments", Comments);
  }
}
