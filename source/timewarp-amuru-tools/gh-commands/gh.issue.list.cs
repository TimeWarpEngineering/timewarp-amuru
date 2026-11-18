#region Purpose
// Fluent builder for `gh issue list`. Task 152.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for <c>gh issue list</c>.
/// </summary>
public sealed class GhIssueListBuilder : GhListCommandBuilder<GhIssueListBuilder>
{
  private readonly List<string> Labels = [];
  private string? State;
  private string? Assignee;
  private string? Author;
  private string? Search;

  internal GhIssueListBuilder(string command) : base(command)
  {
  }

  /// <summary>
  /// Filters by state with <c>--state</c> (open, closed, or all).
  /// </summary>
  /// <param name="state">The state</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhIssueListBuilder WithState(string state)
  {
    State = state;
    return this;
  }

  /// <summary>
  /// Filters by label with <c>--label</c>. Repeat for more labels.
  /// </summary>
  /// <param name="label">The label name</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhIssueListBuilder WithLabel(string label)
  {
    Labels.Add(label);
    return this;
  }

  /// <summary>
  /// Filters by assignee with <c>--assignee</c>.
  /// </summary>
  /// <param name="assignee">The assignee login</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhIssueListBuilder WithAssignee(string assignee)
  {
    Assignee = assignee;
    return this;
  }

  /// <summary>
  /// Filters by author with <c>--author</c>.
  /// </summary>
  /// <param name="author">The author login</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhIssueListBuilder WithAuthor(string author)
  {
    Author = author;
    return this;
  }

  /// <summary>
  /// Filters with a GitHub search query (<c>--search</c>).
  /// </summary>
  /// <param name="query">The search query</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhIssueListBuilder WithSearch(string query)
  {
    Search = query;
    return this;
  }

  private protected override void AppendVerbArguments(List<string> arguments)
  {
    AddOption(arguments, "--state", State);
    AddOptions(arguments, "--label", Labels);
    AddOption(arguments, "--assignee", Assignee);
    AddOption(arguments, "--author", Author);
    AddOption(arguments, "--search", Search);
  }
}
