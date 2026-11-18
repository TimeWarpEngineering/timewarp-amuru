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
// and end, for example refs/heads/{branch} or {branch}..HEAD; an excluded suffix rejects a longer
// end that means something else ({branch}...HEAD is a symmetric range). A plain variable has no
// known start or end, so it never fills a Shaped slot.
//
// A row may also check how its arguments relate. SetUpstreamAsync passes {remote}/{branch} and
// then that same branch, so the third argument must end with '/' plus the fourth: both constants,
// or an interpolation whose final hole is the same variable as the fourth argument (review round
// 2, M3). Same variable means the same local, parameter, field or property name; anything else is
// not known to be equal and is not reported.
//
// Rows left out on purpose (review round 1, M1):
// - `rev-parse --show-toplevel`: Git.FindRoot() walks the file system for .git and runs no git.
// - `remote get-url origin`: Git.GetRepositoryNameAsync() returns a repository name, not the URL.
// GetDefaultBranchAsync has one shape, `symbolic-ref refs/remotes/origin/HEAD --short` (review
// round 2, M2): without --short git prints the full ref, which the helper does not parse.
#endregion

/// <summary>
/// One argument of a shell call as the analyzer can see it.
/// </summary>
public sealed class ShellArgument
{
  private ShellArgument(string? text, string prefix, string suffix, string? reference, string? slashTail)
  {
    Text = text;
    Prefix = prefix;
    Suffix = suffix;
    Reference = reference;
    SlashTail = slashTail;
  }

  /// <summary>The full text when the argument is a compile-time constant, otherwise null.</summary>
  public string? Text { get; }

  /// <summary>The literal text the argument starts with. The full text for a constant.</summary>
  public string Prefix { get; }

  /// <summary>The literal text the argument ends with. The full text for a constant.</summary>
  public string Suffix { get; }

  /// <summary>The variable name when the argument is a plain local, parameter, field or property reference.</summary>
  public string? Reference { get; }

  /// <summary>
  /// The variable name when the argument is an interpolation <c>{head}/{name}</c> with a non-empty
  /// head and a plain variable as the final hole, otherwise null.
  /// </summary>
  public string? SlashTail { get; }

  /// <summary>
  /// A compile-time constant argument.
  /// </summary>
  /// <param name="text">The constant text.</param>
  /// <returns>The argument.</returns>
  public static ShellArgument Constant(string text) => new(text, text, text, null, null);

  /// <summary>
  /// A non-constant argument whose literal start and end are known, or empty when not known.
  /// </summary>
  /// <param name="prefix">The literal text it starts with.</param>
  /// <param name="suffix">The literal text it ends with.</param>
  /// <param name="reference">The variable name when the argument is a plain variable reference.</param>
  /// <param name="slashTail">The final hole's variable name for an interpolation <c>{head}/{name}</c>.</param>
  /// <returns>The argument.</returns>
  public static ShellArgument Computed(string prefix, string suffix, string? reference = null, string? slashTail = null) =>
    new(null, prefix, suffix, reference, slashTail);
}

/// <summary>
/// One position in a git helper's argument shape.
/// </summary>
public sealed class ArgumentSlot
{
  private readonly string? literal;
  private readonly string? prefix;
  private readonly string? suffix;
  private readonly string? excludedSuffix;

  private ArgumentSlot(string? literal, string? prefix, string? suffix, string? excludedSuffix)
  {
    this.literal = literal;
    this.prefix = prefix;
    this.suffix = suffix;
    this.excludedSuffix = excludedSuffix;
  }

  /// <summary>
  /// A slot that needs this exact constant.
  /// </summary>
  /// <param name="text">The constant text.</param>
  /// <returns>The slot.</returns>
  public static ArgumentSlot Literal(string text) => new(text, null, null, null);

  /// <summary>
  /// A slot for a value that is not an option: a path, branch, URL, or remote.
  /// </summary>
  /// <returns>The slot.</returns>
  public static ArgumentSlot Value() => new(null, null, null, null);

  /// <summary>
  /// A slot for a value with a known literal start and end, for example <c>refs/heads/{branch}</c>.
  /// </summary>
  /// <param name="prefix">The literal start.</param>
  /// <param name="suffix">The literal end.</param>
  /// <param name="excludedSuffix">A longer literal end that is a different value, for example <c>...HEAD</c>.</param>
  /// <returns>The slot.</returns>
  public static ArgumentSlot Shaped(string prefix, string suffix, string? excludedSuffix = null) =>
    new(null, prefix, suffix, excludedSuffix);

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
      argument.Suffix.EndsWith(suffix, StringComparison.Ordinal) &&
      (excludedSuffix is null || !argument.Suffix.EndsWith(excludedSuffix, StringComparison.Ordinal));
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
    : this(method, label, null, shapes)
  {
  }

  /// <summary>
  /// Creates a row whose arguments must also pass a check on how they relate.
  /// </summary>
  /// <param name="method">The public static method name on Git.</param>
  /// <param name="label">The command line as shown in the diagnostic.</param>
  /// <param name="check">A check on the whole argument list, run after a shape matches.</param>
  /// <param name="shapes">The full argument shapes the helper runs.</param>
  public GitHelperRow(string method, string label, Func<IReadOnlyList<ShellArgument>, bool>? check, params ArgumentSlot[][] shapes)
  {
    Method = method;
    Label = label;
    Check = check;
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

  /// <summary>A check on how the arguments relate, or null.</summary>
  public Func<IReadOnlyList<ShellArgument>, bool>? Check { get; }

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

      if (all && (Check is null || Check(arguments)))
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
      UpstreamNamesBranch,
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
      [L("rev-list"), L("--count"), ArgumentSlot.Shaped(string.Empty, "..HEAD", "...HEAD")]),
    new(
      "BranchExistsAsync",
      "show-ref --verify --quiet refs/heads/<branch>",
      [L("show-ref"), L("--verify"), L("--quiet"), ArgumentSlot.Shaped("refs/heads/", string.Empty)]),
    new(
      "GetDefaultBranchAsync",
      "symbolic-ref refs/remotes/origin/HEAD --short",
      [L("symbolic-ref"), L("refs/remotes/origin/HEAD"), L("--short")]),
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

  // branch --set-upstream-to {remote}/{branch} {branch}: the third argument is a non-empty remote,
  // '/', and then the fourth argument.
  private static bool UpstreamNamesBranch(IReadOnlyList<ShellArgument> arguments)
  {
    ShellArgument upstream = arguments[2];
    ShellArgument branch = arguments[3];
    if (upstream.Text is not null && branch.Text is not null)
    {
      string tail = "/" + branch.Text;
      return upstream.Text.Length > tail.Length && upstream.Text.EndsWith(tail, StringComparison.Ordinal);
    }

    return upstream.SlashTail is not null &&
      string.Equals(upstream.SlashTail, branch.Reference, StringComparison.Ordinal);
  }
}
