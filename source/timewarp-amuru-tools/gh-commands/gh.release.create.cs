#region Purpose
// Fluent builder for `gh release create`. Task 152.
#endregion

#region Design
// Asset files are positional arguments after the tag. Notes are passed to gh only; they never
// reach a span tag.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Fluent builder for <c>gh release create</c>.
/// </summary>
public sealed class GhReleaseCreateBuilder : GhRepoCommandBuilder<GhReleaseCreateBuilder>
{
  private readonly string Tag;
  private readonly List<string> Assets = [];
  private string? Title;
  private string? Notes;
  private string? NotesFile;
  private string? Target;
  private bool GenerateNotes;
  private bool Draft;
  private bool Prerelease;
  private bool VerifyTag;
  private bool? Latest;

  internal GhReleaseCreateBuilder(string command, string tag) : base(command)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(tag);
    Tag = tag;
  }

  /// <summary>
  /// Uploads a file as a release asset. Repeat for more files.
  /// </summary>
  /// <param name="path">The asset path, optionally <c>path#label</c></param>
  /// <returns>The builder instance for method chaining</returns>
  public GhReleaseCreateBuilder WithAsset(string path)
  {
    Assets.Add(path);
    return this;
  }

  /// <summary>
  /// Sets the release title with <c>--title</c>.
  /// </summary>
  /// <param name="title">The title</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhReleaseCreateBuilder WithTitle(string title)
  {
    Title = title;
    return this;
  }

  /// <summary>
  /// Sets the release notes with <c>--notes</c>.
  /// </summary>
  /// <param name="notes">The notes</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhReleaseCreateBuilder WithNotes(string notes)
  {
    Notes = notes;
    return this;
  }

  /// <summary>
  /// Reads the release notes from a file with <c>--notes-file</c>.
  /// </summary>
  /// <param name="path">The notes file path</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhReleaseCreateBuilder WithNotesFile(string path)
  {
    NotesFile = path;
    return this;
  }

  /// <summary>
  /// Generates title and notes from the GitHub release notes API (<c>--generate-notes</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhReleaseCreateBuilder WithGenerateNotes()
  {
    GenerateNotes = true;
    return this;
  }

  /// <summary>
  /// Sets the branch or commit the tag is created from with <c>--target</c>.
  /// </summary>
  /// <param name="target">A branch name or full commit SHA</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhReleaseCreateBuilder WithTarget(string target)
  {
    Target = target;
    return this;
  }

  /// <summary>
  /// Saves the release as a draft (<c>--draft</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhReleaseCreateBuilder WithDraft()
  {
    Draft = true;
    return this;
  }

  /// <summary>
  /// Marks the release as a prerelease (<c>--prerelease</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhReleaseCreateBuilder WithPrerelease()
  {
    Prerelease = true;
    return this;
  }

  /// <summary>
  /// Refuses to create the release when the tag is not already on the remote (<c>--verify-tag</c>).
  /// </summary>
  /// <returns>The builder instance for method chaining</returns>
  public GhReleaseCreateBuilder WithVerifyTag()
  {
    VerifyTag = true;
    return this;
  }

  /// <summary>
  /// Marks the release as latest, or explicitly not latest (<c>--latest</c> or <c>--latest=false</c>).
  /// </summary>
  /// <param name="latest">True for latest, false for not latest</param>
  /// <returns>The builder instance for method chaining</returns>
  public GhReleaseCreateBuilder WithLatest(bool latest)
  {
    Latest = latest;
    return this;
  }

  private protected override void AppendVerbArguments(List<string> arguments)
  {
    arguments.Add(Tag);
    arguments.AddRange(Assets);
    AddOption(arguments, "--title", Title);
    AddOption(arguments, "--notes", Notes);
    AddOption(arguments, "--notes-file", NotesFile);
    AddFlag(arguments, "--generate-notes", GenerateNotes);
    AddOption(arguments, "--target", Target);
    AddFlag(arguments, "--draft", Draft);
    AddFlag(arguments, "--prerelease", Prerelease);
    AddFlag(arguments, "--verify-tag", VerifyTag);
    if (Latest is bool latest)
    {
      arguments.Add(latest ? "--latest" : "--latest=false");
    }
  }
}
