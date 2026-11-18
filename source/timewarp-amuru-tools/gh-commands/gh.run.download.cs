#region Purpose
// Fluent builder for `gh run download`. Task 152.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for <c>gh run download</c>.
/// </summary>
public sealed class GhRunDownloadBuilder : GhRepoCommandBuilder<GhRunDownloadBuilder>
{
  private readonly string? RunId;
  private readonly List<string> Names = [];
  private readonly List<string> Patterns = [];
  private string? Directory;

  internal GhRunDownloadBuilder(string command, string? runId) : base(command)
  {
    RunId = runId;
  }

  /// <summary>
  /// Downloads only artifacts with this name (<c>--name</c>). Repeat for more names.
  /// </summary>
  /// <param name="name">The artifact name</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhRunDownloadBuilder WithName(string name)
  {
    Names.Add(name);
    return this;
  }

  /// <summary>
  /// Downloads artifacts that match a glob (<c>--pattern</c>). Repeat for more patterns.
  /// </summary>
  /// <param name="pattern">The glob pattern</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhRunDownloadBuilder WithPattern(string pattern)
  {
    Patterns.Add(pattern);
    return this;
  }

  /// <summary>
  /// Sets the directory to download into (<c>--dir</c>).
  /// </summary>
  /// <param name="directory">The target directory</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhRunDownloadBuilder WithDirectory(string directory)
  {
    Directory = directory;
    return this;
  }

  private protected override void AppendVerbArguments(List<string> arguments)
  {
    if (!string.IsNullOrWhiteSpace(RunId))
    {
      arguments.Add(RunId);
    }

    AddOptions(arguments, "--name", Names);
    AddOptions(arguments, "--pattern", Patterns);
    AddOption(arguments, "--dir", Directory);
  }
}
