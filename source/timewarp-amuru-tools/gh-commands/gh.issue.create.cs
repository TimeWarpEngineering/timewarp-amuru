#region Purpose
// Fluent builder for `gh issue create`. Task 152.
#endregion

#region Design
// Title and body are passed to gh only; they never reach a span tag.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for <c>gh issue create</c>.
/// </summary>
public sealed class GhIssueCreateBuilder : GhRepoCommandBuilder<GhIssueCreateBuilder>
{
  private readonly List<string> Labels = [];
  private readonly List<string> Assignees = [];
  private string? Title;
  private string? Body;
  private string? BodyFile;
  private string? Milestone;

  internal GhIssueCreateBuilder(string command) : base(command)
  {
  }

  /// <summary>
  /// Sets the title with <c>--title</c>.
  /// </summary>
  /// <param name="title">The issue title</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhIssueCreateBuilder WithTitle(string title)
  {
    Title = title;
    return this;
  }

  /// <summary>
  /// Sets the body with <c>--body</c>.
  /// </summary>
  /// <param name="body">The issue body</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhIssueCreateBuilder WithBody(string body)
  {
    Body = body;
    return this;
  }

  /// <summary>
  /// Reads the body from a file with <c>--body-file</c> (<c>-</c> reads standard input).
  /// </summary>
  /// <param name="path">The body file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhIssueCreateBuilder WithBodyFile(string path)
  {
    BodyFile = path;
    return this;
  }

  /// <summary>
  /// Adds a label with <c>--label</c>. Repeat for more labels.
  /// </summary>
  /// <param name="label">The label name</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhIssueCreateBuilder WithLabel(string label)
  {
    Labels.Add(label);
    return this;
  }

  /// <summary>
  /// Assigns a user with <c>--assignee</c>. Repeat for more users.
  /// </summary>
  /// <param name="assignee">The login, or <c>@me</c></param>
  /// <returns>The builder instance for method chaining</returns>
  public GhIssueCreateBuilder WithAssignee(string assignee)
  {
    Assignees.Add(assignee);
    return this;
  }

  /// <summary>
  /// Adds the issue to a milestone with <c>--milestone</c>.
  /// </summary>
  /// <param name="milestone">The milestone name</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhIssueCreateBuilder WithMilestone(string milestone)
  {
    Milestone = milestone;
    return this;
  }

  private protected override void AppendVerbArguments(List<string> arguments)
  {
    AddOption(arguments, "--title", Title);
    AddOption(arguments, "--body", Body);
    AddOption(arguments, "--body-file", BodyFile);
    AddOptions(arguments, "--label", Labels);
    AddOptions(arguments, "--assignee", Assignees);
    AddOption(arguments, "--milestone", Milestone);
  }
}
