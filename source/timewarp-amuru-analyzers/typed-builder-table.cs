namespace TimeWarp.Amuru.Analyzers;

using System.Collections.Immutable;

#region Purpose
// The one place that maps a dotnet or gh command line to the typed DotNet or Gh builder that
// replaces it. git rows live in git-helper-table.cs, which matches whole argument shapes.
#endregion

#region Design
// The analyzer cannot reference TimeWarp.Amuru.Tools (netstandard2.0, and a dependency cycle), so
// the rows name builders as text. The analyzer test reflects over the public DotNet, Gh, and Git
// types: every public static DotNet or Gh builder method needs a row, and every row's method must
// exist. That test is what keeps this table from drifting from the library.
//
// A row matches when the first readable argument equals Verb and, when SubVerb is set, SubVerb
// appears among the later readable arguments before "--". SubVerb is searched, not read at a fixed
// position, because `dotnet add <project> package X` puts the project between the two words.
// Rows with a SubVerb are tried before the verb-only row.
//
// DotNetGapVerbs are dotnet SDK verbs with no typed builder yet. They raise AMURU0002 and shrink as
// builders land. A verb in neither list is most likely a local tool (`dotnet fixie`, `dotnet ef`),
// which also raises AMURU0002 with a pointer at DotNet.Tool().Run().
//
// GhRows (task 152) are read with FindAtSecond: gh takes `<verb> <sub-verb>` as its first two
// arguments, so the sub-verb must be the second argument, not any later one (`issue create --title
// list` is not `issue list`). A gh verb or sub-verb with no row is not reported: Gh covers only the
// verbs the org calls, and there is no gap list for gh.
#endregion

/// <summary>
/// One command line shape and the typed builder that replaces it.
/// </summary>
public sealed class TypedBuilderRow
{
  /// <summary>
  /// Creates a row.
  /// </summary>
  /// <param name="verb">The first argument, for example <c>run</c> or <c>--version</c>.</param>
  /// <param name="subVerb">A later argument that must also appear, or null.</param>
  /// <param name="replacement">The replacement as shown in the diagnostic, for example <c>DotNet.Run()</c>.</param>
  /// <param name="methods">The public static method names on DotNet that the replacement names.</param>
  public TypedBuilderRow(string verb, string? subVerb, string replacement, params string[] methods)
  {
    Verb = verb;
    SubVerb = subVerb;
    Replacement = replacement;
    Methods = [.. methods];
  }

  /// <summary>The first argument.</summary>
  public string Verb { get; }

  /// <summary>A later argument that must also appear, or null for a verb-only row.</summary>
  public string? SubVerb { get; }

  /// <summary>The replacement text used in the diagnostic message.</summary>
  public string Replacement { get; }

  /// <summary>The method names on DotNet that the replacement names.</summary>
  public ImmutableArray<string> Methods { get; }
}

/// <summary>
/// Verb-to-builder tables for <c>dotnet</c> and <c>gh</c>.
/// </summary>
public static class TypedBuilderTable
{
  /// <summary>
  /// The argument that ends option parsing. Sub-verbs after it belong to the child program.
  /// </summary>
  private const string EndOfOptions = "--";

  /// <summary>
  /// dotnet verbs that have a typed builder on <c>DotNet</c> in TimeWarp.Amuru.Tools.
  /// </summary>
  public static ImmutableArray<TypedBuilderRow> DotNetRows { get; } =
  [
    new("add", "package", "DotNet.AddPackage()", "AddPackage"),
    new("add", "reference", "DotNet.Reference().Add()", "Reference"),
    new("build", null, "DotNet.Build()", "Build"),
    new("clean", null, "DotNet.Clean()", "Clean"),
    new("dev-certs", null, "DotNet.DevCerts()", "DevCerts"),
    new("format", null, "DotNet.Format()", "Format"),
    new("list", "package", "DotNet.ListPackages()", "ListPackages"),
    new("list", "reference", "DotNet.Reference().List()", "Reference"),
    new("msbuild", null, "DotNet.MsBuild()", "MsBuild"),
    new("new", null, "DotNet.New()", "New"),
    new("nuget", null, "DotNet.NuGet()", "NuGet"),
    new("pack", null, "DotNet.Pack()", "Pack"),
    new("package", "add", "DotNet.AddPackage()", "AddPackage"),
    new("package", "list", "DotNet.ListPackages()", "ListPackages"),
    new("package", "remove", "DotNet.RemovePackage()", "RemovePackage"),
    new("package", "search", "DotNet.PackageSearch()", "PackageSearch"),
    new("publish", null, "DotNet.Publish()", "Publish"),
    new("reference", null, "DotNet.Reference()", "Reference"),
    new("remove", "package", "DotNet.RemovePackage()", "RemovePackage"),
    new("remove", "reference", "DotNet.Reference().Remove()", "Reference"),
    new("restore", null, "DotNet.Restore()", "Restore"),
    new("run", null, "DotNet.Run()", "Run"),
    new("sln", null, "DotNet.Sln()", "Sln"),
    new("solution", null, "DotNet.Sln()", "Sln"),
    new("test", null, "DotNet.Test()", "Test"),
    new("tool", null, "DotNet.Tool()", "Tool"),
    new("user-secrets", null, "DotNet.UserSecrets()", "UserSecrets"),
    new("watch", null, "DotNet.Watch()", "Watch"),
    new("workload", null, "DotNet.Workload()", "Workload"),
    new("--info", null, "DotNet.WithInfo()", "WithInfo"),
    new("--list-runtimes", null, "DotNet.WithListRuntimes()", "WithListRuntimes"),
    new("--list-sdks", null, "DotNet.WithListSdks()", "WithListSdks"),
    new("--version", null, "DotNet.WithVersion()", "WithVersion"),
  ];

  /// <summary>
  /// gh command shapes that have a typed builder on <c>Gh</c> in TimeWarp.Amuru.Tools.
  /// </summary>
  public static ImmutableArray<TypedBuilderRow> GhRows { get; } =
  [
    new("api", null, "Gh.Api()", "Api"),
    new("auth", "status", "Gh.Auth().Status()", "Auth"),
    new("auth", "token", "Gh.Auth().Token()", "Auth"),
    new("issue", "create", "Gh.Issue().Create()", "Issue"),
    new("issue", "list", "Gh.Issue().List()", "Issue"),
    new("pr", "checks", "Gh.Pr().Checks()", "Pr"),
    new("pr", "create", "Gh.Pr().Create()", "Pr"),
    new("pr", "merge", "Gh.Pr().Merge()", "Pr"),
    new("pr", "view", "Gh.Pr().View()", "Pr"),
    new("release", "create", "Gh.Release().Create()", "Release"),
    new("release", "list", "Gh.Release().List()", "Release"),
    new("run", "download", "Gh.Run().Download()", "Run"),
    new("run", "list", "Gh.Run().List()", "Run"),
  ];

  /// <summary>
  /// dotnet SDK verbs with no typed builder yet. Remove a verb when its builder lands.
  /// </summary>
  public static ImmutableArray<string> DotNetGapVerbs { get; } =
  [
    "--diagnostics",
    "--help",
    "-?",
    "-d",
    "-h",
    "build-server",
    "complete",
    "exec",
    "fsi",
    "help",
    "project",
    "sdk",
    "store",
    "user-jwts",
    "vstest",
  ];

  /// <summary>
  /// Finds the row for a command line, or null when no row matches.
  /// </summary>
  /// <param name="rows">The table to search.</param>
  /// <param name="arguments">The leading arguments that are compile-time constants, in order.</param>
  /// <returns>The matching row, or null.</returns>
  public static TypedBuilderRow? Find(ImmutableArray<TypedBuilderRow> rows, IReadOnlyList<string> arguments)
  {
    if (arguments is null || arguments.Count == 0)
    {
      return null;
    }

    string verb = arguments[0];
    TypedBuilderRow? verbOnly = null;
    foreach (TypedBuilderRow row in rows)
    {
      if (!string.Equals(row.Verb, verb, StringComparison.Ordinal))
      {
        continue;
      }

      if (row.SubVerb is null)
      {
        verbOnly = row;
      }
      else if (HasSubVerb(arguments, row.SubVerb))
      {
        return row;
      }
    }

    return verbOnly;
  }

  /// <summary>
  /// Finds the row whose verb is the first argument and whose sub-verb, when set, is the second.
  /// Used for gh, where the sub-verb has a fixed position.
  /// </summary>
  /// <param name="rows">The table to search.</param>
  /// <param name="arguments">The leading arguments that are compile-time constants, in order.</param>
  /// <returns>The matching row, or null.</returns>
  public static TypedBuilderRow? FindAtSecond(ImmutableArray<TypedBuilderRow> rows, IReadOnlyList<string> arguments)
  {
    if (arguments is null || arguments.Count == 0)
    {
      return null;
    }

    string verb = arguments[0];
    string? second = arguments.Count > 1 ? arguments[1] : null;
    foreach (TypedBuilderRow row in rows)
    {
      if (!string.Equals(row.Verb, verb, StringComparison.Ordinal))
      {
        continue;
      }

      if (row.SubVerb is null || string.Equals(row.SubVerb, second, StringComparison.Ordinal))
      {
        return row;
      }
    }

    return null;
  }

  /// <summary>
  /// True when some row in the table starts with this verb.
  /// </summary>
  /// <param name="rows">The table to search.</param>
  /// <param name="verb">The first argument.</param>
  /// <returns>True when the verb is known to the table.</returns>
  public static bool HasVerb(ImmutableArray<TypedBuilderRow> rows, string verb)
  {
    foreach (TypedBuilderRow row in rows)
    {
      if (string.Equals(row.Verb, verb, StringComparison.Ordinal))
      {
        return true;
      }
    }

    return false;
  }

  /// <summary>
  /// True when the verb is a dotnet SDK verb with no typed builder yet.
  /// </summary>
  /// <param name="verb">The first argument.</param>
  /// <returns>True when the verb is on the known-gap list.</returns>
  public static bool IsDotNetGap(string verb) => DotNetGapVerbs.Contains(verb, StringComparer.Ordinal);

  private static bool HasSubVerb(IReadOnlyList<string> arguments, string subVerb)
  {
    for (int index = 1; index < arguments.Count; index++)
    {
      string argument = arguments[index];
      if (string.Equals(argument, EndOfOptions, StringComparison.Ordinal))
      {
        return false;
      }

      if (string.Equals(argument, subVerb, StringComparison.Ordinal))
      {
        return true;
      }
    }

    return false;
  }
}
