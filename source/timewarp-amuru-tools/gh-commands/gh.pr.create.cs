#region Purpose
// Fluent builder for `gh pr create`. Task 152.
#endregion

#region Design
// Title and body are passed to gh only; they never reach a span tag.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for <c>gh pr create</c>.
/// </summary>
public sealed class GhPrCreateBuilder : GhRepoCommandBuilder<GhPrCreateBuilder>
{
  private readonly List<string> Labels = [];
  private readonly List<string> Reviewers = [];
  private readonly List<string> Assignees = [];
  private string? Title;
  private string? Body;
  private string? BodyFile;
  private string? Base;
  private string? Head;
  private bool Draft;
  private bool Fill;

  internal GhPrCreateBuilder(string command) : base(command)
  {
  }

  /// <summary>
  /// Sets the title with <c>--title</c>.
  /// </summary>
  /// <param name="title">The pull request title</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrCreateBuilder WithTitle(string title)
  {
    Title = title;
    return this;
  }

  /// <summary>
  /// Sets the body with <c>--body</c>.
  /// </summary>
  /// <param name="body">The pull request body</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrCreateBuilder WithBody(string body)
  {
    Body = body;
    return this;
  }

  /// <summary>
  /// Reads the body from a file with <c>--body-file</c> (<c>-</c> reads standard input).
  /// </summary>
  /// <param name="path">The body file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrCreateBuilder WithBodyFile(string path)
  {
    BodyFile = path;
    return this;
  }

  /// <summary>
  /// Sets the branch to merge into with <c>--base</c>.
  /// </summary>
  /// <param name="branch">The base branch</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrCreateBuilder WithBase(string branch)
  {
    Base = branch;
    return this;
  }

  /// <summary>
  /// Sets the branch that holds the commits with <c>--head</c>.
  /// </summary>
  /// <param name="branch">The head branch</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrCreateBuilder WithHead(string branch)
  {
    Head = branch;
    return this;
  }

  /// <summary>
  /// Opens the pull request as a draft (<c>--draft</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrCreateBuilder WithDraft()
  {
    Draft = true;
    return this;
  }

  /// <summary>
  /// Fills title and body from the commits (<c>--fill</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrCreateBuilder WithFill()
  {
    Fill = true;
    return this;
  }

  /// <summary>
  /// Adds a label with <c>--label</c>. Repeat for more labels.
  /// </summary>
  /// <param name="label">The label name</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrCreateBuilder WithLabel(string label)
  {
    Labels.Add(label);
    return this;
  }

  /// <summary>
  /// Requests a review with <c>--reviewer</c>. Repeat for more reviewers.
  /// </summary>
  /// <param name="reviewer">A login or <c>org/team</c></param>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrCreateBuilder WithReviewer(string reviewer)
  {
    Reviewers.Add(reviewer);
    return this;
  }

  /// <summary>
  /// Assigns a user with <c>--assignee</c>. Repeat for more users.
  /// </summary>
  /// <param name="assignee">The login, or <c>@me</c></param>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrCreateBuilder WithAssignee(string assignee)
  {
    Assignees.Add(assignee);
    return this;
  }

  private protected override void AppendVerbArguments(List<string> arguments)
  {
    AddOption(arguments, "--title", Title);
    AddOption(arguments, "--body", Body);
    AddOption(arguments, "--body-file", BodyFile);
    AddOption(arguments, "--base", Base);
    AddOption(arguments, "--head", Head);
    AddFlag(arguments, "--draft", Draft);
    AddFlag(arguments, "--fill", Fill);
    AddOptions(arguments, "--label", Labels);
    AddOptions(arguments, "--reviewer", Reviewers);
    AddOptions(arguments, "--assignee", Assignees);
  }
}
