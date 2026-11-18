#region Purpose
// Fluent builder for `gh pr merge`. Task 152.
#endregion

#region Design
// The merge strategy is one of --merge, --squash, --rebase. The last strategy method called wins,
// so the builder never passes two.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for <c>gh pr merge</c>.
/// </summary>
public sealed class GhPrMergeBuilder : GhRepoCommandBuilder<GhPrMergeBuilder>
{
  private readonly string? Selector;
  private string? Strategy;
  private string? Subject;
  private string? Body;
  private string? MatchHeadCommit;
  private bool Auto;
  private bool DeleteBranch;
  private bool Admin;

  internal GhPrMergeBuilder(string command, string? selector) : base(command)
  {
    Selector = selector;
  }

  /// <summary>
  /// Merges with a merge commit (<c>--merge</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrMergeBuilder WithMergeCommit()
  {
    Strategy = "--merge";
    return this;
  }

  /// <summary>
  /// Squashes the commits into one (<c>--squash</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrMergeBuilder WithSquash()
  {
    Strategy = "--squash";
    return this;
  }

  /// <summary>
  /// Rebases the commits onto the base branch (<c>--rebase</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrMergeBuilder WithRebase()
  {
    Strategy = "--rebase";
    return this;
  }

  /// <summary>
  /// Merges once the requirements are met (<c>--auto</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrMergeBuilder WithAuto()
  {
    Auto = true;
    return this;
  }

  /// <summary>
  /// Deletes the local and remote branch after the merge (<c>--delete-branch</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrMergeBuilder WithDeleteBranch()
  {
    DeleteBranch = true;
    return this;
  }

  /// <summary>
  /// Merges with administrator privileges past unmet requirements (<c>--admin</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrMergeBuilder WithAdmin()
  {
    Admin = true;
    return this;
  }

  /// <summary>
  /// Sets the merge commit subject with <c>--subject</c>.
  /// </summary>
  /// <param name="subject">The subject line</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrMergeBuilder WithSubject(string subject)
  {
    Subject = subject;
    return this;
  }

  /// <summary>
  /// Sets the merge commit body with <c>--body</c>.
  /// </summary>
  /// <param name="body">The commit body</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrMergeBuilder WithBody(string body)
  {
    Body = body;
    return this;
  }

  /// <summary>
  /// Merges only when the head is this commit (<c>--match-head-commit</c>).
  /// </summary>
  /// <param name="sha">The expected head commit SHA</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhPrMergeBuilder WithMatchHeadCommit(string sha)
  {
    MatchHeadCommit = sha;
    return this;
  }

  private protected override void AppendVerbArguments(List<string> arguments)
  {
    if (!string.IsNullOrWhiteSpace(Selector))
    {
      arguments.Add(Selector);
    }

    if (Strategy is not null)
    {
      arguments.Add(Strategy);
    }

    AddFlag(arguments, "--auto", Auto);
    AddFlag(arguments, "--delete-branch", DeleteBranch);
    AddFlag(arguments, "--admin", Admin);
    AddOption(arguments, "--subject", Subject);
    AddOption(arguments, "--body", Body);
    AddOption(arguments, "--match-head-commit", MatchHeadCommit);
  }
}
