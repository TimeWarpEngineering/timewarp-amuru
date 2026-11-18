#region Purpose
// Entry point for `gh release` builders: list and create. Task 152.
#endregion

namespace TimeWarp.Amuru;

public static partial class Gh
{
  /// <summary>
  /// Starts a <c>gh release</c> command.
  /// </summary>
  /// <returns>The release sub-commands</returns>
  public static GhReleaseCommand Release() => new("release");
}

/// <summary>
/// The <c>gh release</c> sub-commands.
/// </summary>
public sealed class GhReleaseCommand
{
  private readonly string Verb;

  internal GhReleaseCommand(string verb)
  {
    Verb = verb;
  }

  /// <summary>
  /// Creates a builder for <c>gh release list</c>.
  /// </summary>
  /// <returns>A builder for listing releases</returns>
  public GhReleaseListBuilder List() => new($"{Verb} list");

  /// <summary>
  /// Creates a builder for <c>gh release create &lt;tag&gt;</c>.
  /// </summary>
  /// <param name="tag">The release tag, for example <c>v3.0.0</c></param>
  /// <returns>A builder for creating a release</returns>
  public GhReleaseCreateBuilder Create(string tag) => new($"{Verb} create", tag);
}
