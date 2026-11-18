#region Purpose
// Entry point for `gh pr` builders: view, checks, create, and merge. Task 152.
#endregion

namespace TimeWarp.Amuru;

public static partial class Gh
{
  /// <summary>
  /// Starts a <c>gh pr</c> command.
  /// </summary>
  /// <returns>The pull request sub-commands</returns>
  public static GhPrCommand Pr() => new("pr");
}

/// <summary>
/// The <c>gh pr</c> sub-commands. A selector is a number, URL, or branch; without one gh uses the
/// pull request of the current branch.
/// </summary>
public sealed class GhPrCommand
{
  private readonly string Verb;

  internal GhPrCommand(string verb)
  {
    Verb = verb;
  }

  /// <summary>
  /// Creates a builder for <c>gh pr view</c> on the current branch's pull request.
  /// </summary>
  /// <returns>A builder for viewing a pull request</returns>
  public GhPrViewBuilder View() => new($"{Verb} view", null);

  /// <summary>
  /// Creates a builder for <c>gh pr view &lt;selector&gt;</c>.
  /// </summary>
  /// <param name="selector">A pull request number, URL, or branch</param>
  /// <returns>A builder for viewing a pull request</returns>
  public GhPrViewBuilder View(string selector) => new($"{Verb} view", selector);

  /// <summary>
  /// Creates a builder for <c>gh pr checks</c> on the current branch's pull request.
  /// </summary>
  /// <returns>A builder for pull request checks</returns>
  public GhPrChecksBuilder Checks() => new($"{Verb} checks", null);

  /// <summary>
  /// Creates a builder for <c>gh pr checks &lt;selector&gt;</c>.
  /// </summary>
  /// <param name="selector">A pull request number, URL, or branch</param>
  /// <returns>A builder for pull request checks</returns>
  public GhPrChecksBuilder Checks(string selector) => new($"{Verb} checks", selector);

  /// <summary>
  /// Creates a builder for <c>gh pr create</c>.
  /// </summary>
  /// <returns>A builder for opening a pull request</returns>
  public GhPrCreateBuilder Create() => new($"{Verb} create");

  /// <summary>
  /// Creates a builder for <c>gh pr merge</c> on the current branch's pull request.
  /// </summary>
  /// <returns>A builder for merging a pull request</returns>
  public GhPrMergeBuilder Merge() => new($"{Verb} merge", null);

  /// <summary>
  /// Creates a builder for <c>gh pr merge &lt;selector&gt;</c>.
  /// </summary>
  /// <param name="selector">A pull request number, URL, or branch</param>
  /// <returns>A builder for merging a pull request</returns>
  public GhPrMergeBuilder Merge(string selector) => new($"{Verb} merge", selector);
}
