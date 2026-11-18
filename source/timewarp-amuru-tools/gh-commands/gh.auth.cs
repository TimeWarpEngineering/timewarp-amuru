#region Purpose
// Entry point for `gh auth` builders: token and status. Task 152.
#endregion

namespace TimeWarp.Amuru;

public static partial class Gh
{
  /// <summary>
  /// Starts a <c>gh auth</c> command.
  /// </summary>
  /// <returns>The auth sub-commands</returns>
  public static GhAuthCommand Auth() => new("auth");
}

/// <summary>
/// The <c>gh auth</c> sub-commands.
/// </summary>
public sealed class GhAuthCommand
{
  private readonly string Verb;

  internal GhAuthCommand(string verb)
  {
    Verb = verb;
  }

  /// <summary>
  /// Creates a builder for <c>gh auth token</c>. Its stdout is the token; it is never tagged or logged.
  /// </summary>
  /// <returns>A builder that prints the active token</returns>
  public GhAuthTokenBuilder Token() => new($"{Verb} token");

  /// <summary>
  /// Creates a builder for <c>gh auth status</c>.
  /// </summary>
  /// <returns>A builder that checks the authentication state</returns>
  public GhAuthStatusBuilder Status() => new($"{Verb} status");
}
