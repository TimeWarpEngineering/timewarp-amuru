namespace TimeWarp.Amuru.Analyzers;

using System.Collections.Immutable;

#region Purpose
// Maps a git command line to the typed Git helper that runs that exact command line.
#endregion

#region Design
// AMURU0003 names a helper, so it must only fire when the helper performs the same operation.
// Each row therefore lists the full argument shapes its helper runs (git.*.cs in
// TimeWarp.Amuru.Tools), and a call matches only when it has exactly that many arguments and each
// one fits its slot. An extra argument, a different flag, another remote, or another ref is a
// different operation and is not reported.
//
// The analyzer only knows the whole argument list when the chain cannot add more later (see
// ShellBuilderAnalyzer). Each argument is a ShellArgument: a constant, or a non-constant whose
// literal start and end are known from an interpolated string or a concatenation.
//
// Slots: Literal(text) needs that constant. Value() takes a constant or non-constant that does not
// start with '-' (a path, branch, URL or remote). Shaped(prefix, suffix) needs the literal start
// and end, for example refs/heads/{branch} or {branch}..HEAD. A plain variable has no known start
// or end, so it never fills a Shaped slot.
//
// Rows left out on purpose (review round 1, M1):
// - `rev-parse --show-toplevel`: Git.FindRoot() walks the file system for .git and runs no git.
// - `remote get-url origin`: Git.GetRepositoryNameAsync() returns a repository name, not the URL.
#endregion

/// <summary>
/// One argument of a shell call as the analyzer can see it.
/// </summary>
public sealed class ShellArgument
{
  private ShellArgument(string? text, string prefix, string suffix)
  {
    Text = text;
    Prefix = prefix;
    Suffix = suffix;
  }

  /// <summary>The full text when the argument is a compile-time constant, otherwise null.</summary>
  public string? Text { get; }

  /// <summary>The literal text the argument starts with. The full text for a constant.</summary>
  public string Prefix { get; }

  /// <summary>The literal text the argument ends with. The full text for a constant.</summary>
  public string Suffix { get; }

  /// <summary>
  /// A compile-time constant argument.
  /// </summary>
  /// <param name="text">The constant text.</param>
  /// <returns>The argument.</returns>
  public static ShellArgument Constant(string text) => new(text, text, text);

  /// <summary>
  /// A non-constant argument whose literal start and end are known, or empty when not known.
  /// </summary>
  /// <param name="prefix">The literal text it starts with.</param>
  /// <param name="suffix">The literal text it ends with.</param>
  /// <returns>The argument.</returns>
  public static ShellArgument Computed(string prefix, string suffix) => new(null, prefix, suffix);
}

/// <summary>
/// One position in a git helper's argument shape.
/// </summary>
public sealed class ArgumentSlot
{
  private readonly string? literal;
  private readonly string? prefix;
  private readonly string? suffix;

  private ArgumentSlot(string? literal, string? prefix, string? suffix)
  {
    this.literal = literal;
    this.prefix = prefix;
    this.suffix = suffix;
  }

  /// <summary>
  /// A slot that needs this exact constant.
  /// </summary>
  /// <param name="text">The constant text.</param>
  /// <returns>The slot.</returns>
  public static ArgumentSlot Literal(string text) => new(text, null, null);

  /// <summary>
  /// A slot for a value that is not an option: a path, branch, URL, or remote.
  /// </summary>
  /// <returns>The slot.</returns>
  public static ArgumentSlot Value() => new(null, null, null);

  /// <summary>
  /// A slot for a value with a known literal start and end, for example <c>refs/heads/{branch}</c>.
  /// </summary>
  /// <param name="prefix">The literal start.</param>
  /// <param name="suffix">The literal end.</param>
  /// <returns>The slot.</returns>
  public static ArgumentSlot Shaped(string prefix, string suffix) => new(null, prefix, suffix);

  /// <summary>
  /// True when the argument fits this slot.
  /// </summary>
  /// <param name="argument">The argument.</param>
  /// <returns>True when the argument fits.</returns>
  public bool Matches(ShellArgument argument)
  {
    if (argument is null)
    {
      return false;
    }

    if (literal is not null)
    {
      return string.Equals(argument.Text, literal, StringComparison.Ordinal);
    }

    if (argument.Prefix.StartsWith("-", StringComparison.Ordinal))
    {
      return false;
    }

    if (prefix is null || suffix is null)
    {
      return argument.Text is null || argument.Text.Length > 0;
    }

    bool fits = argument.Prefix.StartsWith(prefix, StringComparison.Ordinal) &&
      argument.Suffix.EndsWith(suffix, StringComparison.Ordinal);
    return fits && (argument.Text is null || argument.Text.Length > prefix.Length + suffix.Length);
  }
}

/// <summary>
/// A typed Git helper and the argument shapes it runs.
/// </summary>
public sealed class GitHelperRow
{
  /// <summary>
  /// Creates a row.
  /// </summary>
  /// <param name="method">The public static method name on Git.</param>
  /// <param name="label">The command line as shown in the diagnostic, for example <c>worktree add &lt;path&gt; &lt;branch&gt;</c>.</param>
  /// <param name="shapes">The full argument shapes the helper runs.</param>
  public GitHelperRow(string method, string label, params ArgumentSlot[][] shapes)
  {
    Method = method;
    Label = label;
    Shapes = [.. shapes.Select(shape => shape.ToImmutableArray())];
  }

  /// <summary>The public static method name on Git.</summary>
  public string Method { get; }

  /// <summary>The replacement text used in the diagnostic message.</summary>
  public string Replacement => $"Git.{Method}()";

  /// <summary>The command line shown in the diagnostic message.</summary>
  public string Label { get; }

  /// <summary>The full argument shapes the helper runs.</summary>
  public ImmutableArray<ImmutableArray<ArgumentSlot>> Shapes { get; }

  /// <summary>
  /// True when the arguments have one of this row's shapes exactly.
  /// </summary>
  /// <param name="arguments">Every argument of the call, in order.</param>
  /// <returns>True when a shape matches.</returns>
  public bool Matches(IReadOnlyList<ShellArgument> arguments)
  {
    if (arguments is null)
    {
      return false;
    }

    foreach (ImmutableArray<ArgumentSlot> shape in Shapes)
    {
      if (shape.Length != arguments.Count)
      {
        continue;
      }

      bool all = true;
      for (int index = 0; index < shape.Length && all; index++)
      {
        all = shape[index].Matches(arguments[index]);
      }

      if (all)
      {
        return true;
      }
    }

    return false;
  }
}

/// <summary>
/// git command lines that an existing <c>Git</c> helper in TimeWarp.Amuru.Tools performs.
/// </summary>
public static class GitHelperTable
{
  private static readonly ArgumentSlot AnyValue = ArgumentSlot.Value();

  /// <summary>
  /// The rows, one per helper.
  /// </summary>
  public static ImmutableArray<GitHelperRow> Rows { get; } =
  [
    new(
      "SetUpstreamAsync",
      "branch --set-upstream-to <remote>/<branch> <branch>",
      [L("branch"), L("--set-upstream-to"), AnyValue, AnyValue]),
    new(
      "CloneBareAsync",
      "clone --bare <url> <path>",
      [L("clone"), L("--bare"), AnyValue, AnyValue]),
    new(
      "ConfigureFetchRefspecAsync",
      "config remote.origin.fetch +refs/heads/*:refs/remotes/origin/*",
      [L("config"), L("remote.origin.fetch"), L("+refs/heads/*:refs/remotes/origin/*")]),
    new(
      "FetchAsync",
      "fetch <remote>",
      [L("fetch"), AnyValue]),
    new(
      "SetRemoteHeadAutoAsync",
      "remote set-head origin --auto",
      [L("remote"), L("set-head"), L("origin"), L("--auto")]),
    new(
      "GetCommitsAheadAsync",
      "rev-list --count <branch>..HEAD",
      [L("rev-list"), L("--count"), ArgumentSlot.Shaped(string.Empty, "..HEAD")]),
    new(
      "BranchExistsAsync",
      "show-ref --verify --quiet refs/heads/<branch>",
      [L("show-ref"), L("--verify"), L("--quiet"), ArgumentSlot.Shaped("refs/heads/", string.Empty)]),
    new(
      "GetDefaultBranchAsync",
      "symbolic-ref refs/remotes/origin/HEAD",
      [L("symbolic-ref"), L("refs/remotes/origin/HEAD"), L("--short")],
      [L("symbolic-ref"), L("--short"), L("refs/remotes/origin/HEAD")],
      [L("symbolic-ref"), L("refs/remotes/origin/HEAD")]),
    new(
      "WorktreeAddAsync",
      "worktree add <path> <branch>",
      [L("worktree"), L("add"), AnyValue, AnyValue]),
    new(
      "WorktreeAddNewBranchAsync",
      "worktree add -b <branch> <path>",
      [L("worktree"), L("add"), L("-b"), AnyValue, AnyValue],
      [L("worktree"), L("add"), L("-b"), AnyValue, AnyValue, AnyValue]),
    new(
      "WorktreeListPorcelainAsync",
      "worktree list --porcelain",
      [L("worktree"), L("list"), L("--porcelain")]),
    new(
      "WorktreeRemoveAsync",
      "worktree remove <path>",
      [L("worktree"), L("remove"), AnyValue],
      [L("worktree"), L("remove"), L("--force"), AnyValue]),
  ];

  /// <summary>
  /// Finds the row whose helper runs exactly this command line, or null.
  /// </summary>
  /// <param name="arguments">Every argument of the call, in order.</param>
  /// <returns>The matching row, or null.</returns>
  public static GitHelperRow? Find(IReadOnlyList<ShellArgument> arguments)
  {
    foreach (GitHelperRow row in Rows)
    {
      if (row.Matches(arguments))
      {
        return row;
      }
    }

    return null;
  }

  private static ArgumentSlot L(string text) => ArgumentSlot.Literal(text);
}
