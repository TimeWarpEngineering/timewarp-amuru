#region Purpose
// Fluent builder for `gh auth status`, with a typed signed-in result. Task 152.
#endregion

#region Design
// GetStatusAsync runs `gh auth status --json hosts`, which exits zero whatever the auth state and
// holds no token. SignedIn is true when the active account on the chosen host has state "success".
// The host is the --hostname value when set, otherwise the first host with an active account,
// otherwise the first host gh lists. The text form (no --json) is left to CaptureAsync.
// --show-token is not modeled; WithArgument can still pass it, and the output then holds the token.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for <c>gh auth status</c>.
/// </summary>
public sealed class GhAuthStatusBuilder : GhCommandBuilder<GhAuthStatusBuilder>
{
  private const string SuccessState = "success";

  private string? Hostname;
  private bool Active;
  private bool Json;

  internal GhAuthStatusBuilder(string command) : base(command)
  {
  }

  /// <summary>
  /// Checks only one host with <c>--hostname</c>.
  /// </summary>
  /// <param name="hostname">The host, for example <c>github.com</c></param>
  /// <returns>The builder instance for method chaining</returns>
  public GhAuthStatusBuilder WithHostname(string hostname)
  {
    Hostname = hostname;
    return this;
  }

  /// <summary>
  /// Shows only the active account with <c>--active</c>.
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhAuthStatusBuilder WithActive()
  {
    Active = true;
    return this;
  }

  /// <summary>
  /// Runs <c>gh auth status --json hosts</c> inside a gh span and reports whether the active
  /// account is signed in.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token for the operation</param>
  /// <returns>The signed-in flag, host, and login</returns>
  public async Task<GhAuthStatus> GetStatusAsync(CancellationToken cancellationToken = default)
  {
    Json = true;
    CommandOutput output = await CaptureAsync(cancellationToken).ConfigureAwait(false);
    if (!output.Success)
    {
      return new GhAuthStatus(false, Hostname, null, output);
    }

    GhAuthStatusDocument? document = GhJson.Deserialize(output.Stdout, GhJsonContext.Default.GhAuthStatusDocument);
    return Read(document, Hostname, output);
  }

  private protected override void AppendArguments(List<string> arguments)
  {
    AddOption(arguments, "--hostname", Hostname);
    AddFlag(arguments, "--active", Active);
    if (Json)
    {
      arguments.Add("--json");
      arguments.Add("hosts");
    }
  }

  private static GhAuthStatus Read(GhAuthStatusDocument? document, string? hostname, CommandOutput output)
  {
    Dictionary<string, List<GhAuthAccount>> hosts = document?.Hosts ?? [];
    string? host = hostname;
    if (host is null)
    {
      foreach (KeyValuePair<string, List<GhAuthAccount>> entry in hosts)
      {
        if (entry.Value.Exists(account => account.Active))
        {
          host = entry.Key;
          break;
        }
      }

      host ??= hosts.Keys.FirstOrDefault();
    }

    if (host is null || !hosts.TryGetValue(host, out List<GhAuthAccount>? accounts))
    {
      return new GhAuthStatus(false, host, null, output);
    }

    GhAuthAccount? active = accounts.Find(account => account.Active);
    bool signedIn = active is not null && string.Equals(active.State, SuccessState, StringComparison.OrdinalIgnoreCase);
    return new GhAuthStatus(signedIn, host, active?.Login, output);
  }
}
