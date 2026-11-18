#!/usr/bin/env -S dotnet --
#:package Microsoft.CodeAnalysis.CSharp
#:project $(SourceDirectory)timewarp-amuru-analyzers/timewarp-amuru-analyzers.csproj

#region Purpose
// Keeps the analyzer's verb tables (TypedBuilderTable, GitHelperTable) in step with the public
// DotNet, Gh, and Git surface of TimeWarp.Amuru.Tools. Split from shell-builder-analyzer.cs (task 152).
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// The cases reflect over DotNet, Gh, and Git. A new DotNet or Gh builder with no table row fails
// here, and so does a row whose method was removed.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace TypedBuilderTable_
{
  using System.Diagnostics.CodeAnalysis;
  using TimeWarp.Amuru.Analyzers;

  [TestTag("Analyzers")]
  public class VerbTable_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<VerbTable_Given_>();

    // DotNet.Builder() is the raw entry point for arguments no typed builder covers. It has no verb.
    private static readonly string[] EntryPoints = ["Builder"];

    public static async Task EveryDotNetBuilder_Should_HaveATableRow()
    {
      HashSet<string> rowMethods = [.. TypedBuilderTable.DotNetRows.SelectMany(row => row.Methods)];

      string[] missing = [.. PublicStaticMethodNames(typeof(DotNet))
        .Where(name => !EntryPoints.Contains(name) && !rowMethods.Contains(name))];

      missing.ShouldBeEmpty($"DotNet builders with no row in TypedBuilderTable.DotNetRows: {string.Join(", ", missing)}");
      await Task.CompletedTask;
    }

    public static async Task EveryDotNetRow_Should_NameAnExistingBuilder()
    {
      HashSet<string> builders = [.. PublicStaticMethodNames(typeof(DotNet))];

      string[] stale = [.. TypedBuilderTable.DotNetRows
        .SelectMany(row => row.Methods)
        .Where(method => !builders.Contains(method))
        .Distinct()];

      stale.ShouldBeEmpty($"TypedBuilderTable.DotNetRows names missing DotNet methods: {string.Join(", ", stale)}");
      await Task.CompletedTask;
    }

    public static async Task EveryGitRow_Should_NameAnExistingHelper()
    {
      HashSet<string> helpers = [.. PublicStaticMethodNames(typeof(Git))];

      string[] stale = [.. GitHelperTable.Rows
        .Select(row => row.Method)
        .Where(method => !helpers.Contains(method))
        .Distinct()];

      stale.ShouldBeEmpty($"GitHelperTable.Rows names missing Git methods: {string.Join(", ", stale)}");
      await Task.CompletedTask;
    }

    public static async Task EveryGhBuilder_Should_HaveATableRow()
    {
      HashSet<string> rowMethods = [.. TypedBuilderTable.GhRows.SelectMany(row => row.Methods)];

      string[] missing = [.. PublicStaticMethodNames(typeof(Gh)).Where(name => !rowMethods.Contains(name))];

      missing.ShouldBeEmpty($"Gh builders with no row in TypedBuilderTable.GhRows: {string.Join(", ", missing)}");
      await Task.CompletedTask;
    }

    public static async Task EveryGhRow_Should_NameAnExistingBuilder()
    {
      HashSet<string> builders = [.. PublicStaticMethodNames(typeof(Gh))];

      string[] stale = [.. TypedBuilderTable.GhRows
        .SelectMany(row => row.Methods)
        .Where(method => !builders.Contains(method))
        .Distinct()];

      stale.ShouldBeEmpty($"TypedBuilderTable.GhRows names missing Gh methods: {string.Join(", ", stale)}");
      await Task.CompletedTask;
    }

    // Each GhRows sub-verb is a method on the group the row's builder returns, so the replacement
    // text names a real chain: Gh.Run().List() needs GhRunCommand.List.
    public static async Task EveryGhSubVerb_Should_BeAMethodOnItsGroup()
    {
      string[] missing = [.. TypedBuilderTable.GhRows
        .Where(row => row.SubVerb is not null)
        .Where(row => !HasGroupMethod(row.Methods[0], row.SubVerb!))
        .Select(row => row.Replacement)];

      missing.ShouldBeEmpty($"GhRows whose sub-verb has no group method: {string.Join(", ", missing)}");
      await Task.CompletedTask;
    }

    private static bool HasGroupMethod(string entry, string subVerb)
    {
      Type group = entry switch
      {
        "Run" => typeof(GhRunCommand),
        "Auth" => typeof(GhAuthCommand),
        "Issue" => typeof(GhIssueCommand),
        "Pr" => typeof(GhPrCommand),
        "Release" => typeof(GhReleaseCommand),
        _ => throw new InvalidOperationException($"No group type for Gh.{entry}()"),
      };

      return PublicInstanceMethodNames(group).Contains(subVerb, StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> PublicInstanceMethodNames(
      [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type) =>
      type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(method => method.Name);

    public static async Task GapVerbs_Should_NotOverlapTheBuilderRows()
    {
      string[] overlap = [.. TypedBuilderTable.DotNetGapVerbs
        .Where(verb => TypedBuilderTable.HasVerb(TypedBuilderTable.DotNetRows, verb))];

      overlap.ShouldBeEmpty();
      TypedBuilderTable.IsDotNetGap("msbuild").ShouldBeFalse();
      TypedBuilderTable.IsDotNetGap("nuget").ShouldBeFalse();
      TypedBuilderTable.IsDotNetGap("format").ShouldBeFalse();
      await Task.CompletedTask;
    }

    private static IEnumerable<string> PublicStaticMethodNames(
      [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type) =>
      type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(method => !method.IsSpecialName)
        .Select(method => method.Name)
        .Distinct();
  }
}
