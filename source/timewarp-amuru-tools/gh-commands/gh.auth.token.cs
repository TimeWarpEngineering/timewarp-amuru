#region Purpose
// Fluent builder for `gh auth token`. Task 152.
#endregion

#region Design
// The token is stdout. The span records only the command, exit code, and elapsed time; the
// CommandOutput goes back to the caller and nowhere else.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for <c>gh auth token</c>.
/// </summary>
public sealed class GhAuthTokenBuilder : GhCommandBuilder<GhAuthTokenBuilder>
{
  private string? Hostname;
  private string? User;

  internal GhAuthTokenBuilder(string command) : base(command)
  {
  }

  /// <summary>
  /// Reads the token for another host with <c>--hostname</c>.
  /// </summary>
  /// <param name="hostname">The host, for example <c>github.com</c></param>
  /// <returns>The builder instance for method chaining</returns>
  public GhAuthTokenBuilder WithHostname(string hostname)
  {
    Hostname = hostname;
    return this;
  }

  /// <summary>
  /// Reads the token of another signed-in account with <c>--user</c>.
  /// </summary>
  /// <param name="user">The account login</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhAuthTokenBuilder WithUser(string user)
  {
    User = user;
    return this;
  }

  private protected override void AppendArguments(List<string> arguments)
  {
    AddOption(arguments, "--hostname", Hostname);
    AddOption(arguments, "--user", User);
  }
}
