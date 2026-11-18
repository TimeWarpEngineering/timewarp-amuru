#region Purpose
// Entry point for `gh issue` builders: list and create. Task 152.
#endregion

namespace TimeWarp.Amuru;

public static partial class Gh
{
  /// <summary>
  /// Starts a <c>gh issue</c> command.
  /// </summary>
  /// <returns>The issue sub-commands</returns>
  public static GhIssueCommand Issue() => new("issue");
}

/// <summary>
/// The <c>gh issue</c> sub-commands.
/// </summary>
public sealed class GhIssueCommand
{
  private readonly string Verb;

  internal GhIssueCommand(string verb)
  {
    Verb = verb;
  }

  /// <summary>
  /// Creates a builder for <c>gh issue list</c>.
  /// </summary>
  /// <returns>A builder for listing issues</returns>
  public GhIssueListBuilder List() => new($"{Verb} list");

  /// <summary>
  /// Creates a builder for <c>gh issue create</c>.
  /// </summary>
  /// <returns>A builder for creating an issue</returns>
  public GhIssueCreateBuilder Create() => new($"{Verb} create");
}
