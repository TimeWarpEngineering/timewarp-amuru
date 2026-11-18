#region Purpose
// Fluent builder for `gh run list`, with a typed result for the JSON listing. Task 152.
#endregion

#region Design
// ListRunsAsync replaces any --json fields and --jq with GhWorkflowRun.JsonFields, so the output
// always deserializes. A non-zero exit returns Success false and no runs; it does not throw unless
// strict validation was set.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for <c>gh run list</c>.
/// </summary>
public sealed class GhRunListBuilder : GhListCommandBuilder<GhRunListBuilder>
{
  private string? Workflow;
  private string? Branch;
  private string? Status;
  private string? Event;
  private string? Commit;
  private string? User;

  internal GhRunListBuilder(string command) : base(command)
  {
  }

  /// <summary>
  /// Filters by workflow name or file with <c>--workflow</c>.
  /// </summary>
  /// <param name="workflow">The workflow name or file name</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhRunListBuilder WithWorkflow(string workflow)
  {
    Workflow = workflow;
    return this;
  }

  /// <summary>
  /// Filters by branch with <c>--branch</c>.
  /// </summary>
  /// <param name="branch">The branch name</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhRunListBuilder WithBranch(string branch)
  {
    Branch = branch;
    return this;
  }

  /// <summary>
  /// Filters by status or conclusion with <c>--status</c> (for example <c>completed</c> or <c>success</c>).
  /// </summary>
  /// <param name="status">The status value</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhRunListBuilder WithStatus(string status)
  {
    Status = status;
    return this;
  }

  /// <summary>
  /// Filters by triggering event with <c>--event</c>.
  /// </summary>
  /// <param name="eventName">The event name, for example <c>push</c></param>
  /// <returns>The builder instance for method chaining</returns>
  public GhRunListBuilder WithEvent(string eventName)
  {
    Event = eventName;
    return this;
  }

  /// <summary>
  /// Filters by head commit SHA with <c>--commit</c>.
  /// </summary>
  /// <param name="sha">The commit SHA</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhRunListBuilder WithCommit(string sha)
  {
    Commit = sha;
    return this;
  }

  /// <summary>
  /// Filters by the user who triggered the run with <c>--user</c>.
  /// </summary>
  /// <param name="user">The user login</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhRunListBuilder WithUser(string user)
  {
    User = user;
    return this;
  }

  /// <summary>
  /// Runs <c>gh run list --json</c> with <see cref="GhWorkflowRun.JsonFields"/> inside a gh span and
  /// returns the runs. Any <c>--json</c> fields or <c>--jq</c> set earlier are replaced.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The runs, or Success false with no runs when gh failed</returns>
  public async Task<GhRunListResult> ListRunsAsync(CancellationToken cancellationToken = default)
  {
    UseJsonFields(GhWorkflowRun.JsonFields);
    CommandOutput output = await CaptureAsync(cancellationToken).ConfigureAwait(false);
    if (!output.Success)
    {
      return new GhRunListResult(false, [], output);
    }

    List<GhWorkflowRun> runs = GhJson.Deserialize(output.Stdout, GhJsonContext.Default.ListGhWorkflowRun) ?? [];
    return new GhRunListResult(true, runs, output);
  }

  private protected override void AppendVerbArguments(List<string> arguments)
  {
    AddOption(arguments, "--workflow", Workflow);
    AddOption(arguments, "--branch", Branch);
    AddOption(arguments, "--status", Status);
    AddOption(arguments, "--event", Event);
    AddOption(arguments, "--commit", Commit);
    AddOption(arguments, "--user", User);
  }
}
