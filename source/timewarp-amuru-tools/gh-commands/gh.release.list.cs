#region Purpose
// Fluent builder for `gh release list`. Task 152.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for <c>gh release list</c>.
/// </summary>
public sealed class GhReleaseListBuilder : GhListCommandBuilder<GhReleaseListBuilder>
{
  private bool ExcludeDrafts;
  private bool ExcludePreReleases;

  internal GhReleaseListBuilder(string command) : base(command)
  {
  }

  /// <summary>
  /// Leaves out draft releases (<c>--exclude-drafts</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhReleaseListBuilder WithExcludeDrafts()
  {
    ExcludeDrafts = true;
    return this;
  }

  /// <summary>
  /// Leaves out prereleases (<c>--exclude-pre-releases</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhReleaseListBuilder WithExcludePreReleases()
  {
    ExcludePreReleases = true;
    return this;
  }

  private protected override void AppendVerbArguments(List<string> arguments)
  {
    AddFlag(arguments, "--exclude-drafts", ExcludeDrafts);
    AddFlag(arguments, "--exclude-pre-releases", ExcludePreReleases);
  }
}
