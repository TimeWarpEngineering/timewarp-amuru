#!/usr/bin/env -S dotnet --
#:package Microsoft.CodeAnalysis.CSharp
#:project $(SourceDirectory)timewarp-amuru-analyzers/timewarp-amuru-analyzers.csproj

#region Purpose
// Proves AMURU0001-0003: which Shell.Builder and Shell.Run calls report, at what severity, with
// what message. typed-builder-table.cs checks the verb tables against the public surface.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Each case compiles a small source in process against the built TimeWarp.Amuru assembly and runs
// ShellBuilderAnalyzer through CompilationWithAnalyzers. Pragma-suppressed diagnostics are dropped
// by that API, so the pragma case expects none.
// The editorconfig case parses a real .editorconfig with AnalyzerConfigSet and feeds its tree
// options to the compilation, which is how the compiler applies severity overrides.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilderAnalyzer_
{
  using System.Collections.Immutable;
  using System.Globalization;
  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.CSharp;
  using Microsoft.CodeAnalysis.Diagnostics;
  using TimeWarp.Amuru.Analyzers;

  [TestTag("Analyzers")]
  public class Diagnostics_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Diagnostics_Given_>();

    public static async Task LiteralDotNetRun_Should_ReportErrorNamingDotNetRun()
    {
      Diagnostic diagnostic = (await AnalyzeAsync("""Shell.Builder("dotnet").WithArguments("run", "--project", "x.csproj").Build();""")).ShouldHaveSingleItem();

      diagnostic.Id.ShouldBe("AMURU0001");
      diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
      diagnostic.GetMessage(CultureInfo.InvariantCulture)
        .ShouldBe("""Use DotNet.Run() (TimeWarp.Amuru.Tools) instead of Shell.Builder("dotnet") with "run" """.TrimEnd());
    }

    public static async Task ConstExecutable_Should_ReportLikeALiteral()
    {
      Diagnostic diagnostic = (await AnalyzeAsync(
        """Shell.Builder(DotNetName).WithArguments("build").Build();""",
        """const string DotNetName = "dotnet";""")).ShouldHaveSingleItem();

      diagnostic.Id.ShouldBe("AMURU0001");
      diagnostic.GetMessage(CultureInfo.InvariantCulture).ShouldContain("Use DotNet.Build() (TimeWarp.Amuru.Tools)");
    }

    public static async Task NoArguments_Should_ReportTheDotNetEntryPoint()
    {
      Diagnostic diagnostic = (await AnalyzeAsync("""Shell.Builder("dotnet").WithNoValidation().Build();""")).ShouldHaveSingleItem();

      diagnostic.Id.ShouldBe("AMURU0001");
      diagnostic.GetMessage(CultureInfo.InvariantCulture)
        .ShouldBe("""Use the DotNet.* builders (TimeWarp.Amuru.Tools) instead of Shell.Builder("dotnet")""");
    }

    public static async Task NonConstantFirstArgument_Should_ReportTheDotNetEntryPoint()
    {
      Diagnostic diagnostic = (await AnalyzeAsync(
        """Shell.Builder("dotnet").WithArguments(Verb()).Build();""",
        """static string Verb() => "run";""")).ShouldHaveSingleItem();

      diagnostic.Id.ShouldBe("AMURU0001");
      diagnostic.GetMessage(CultureInfo.InvariantCulture).ShouldContain("the DotNet.* builders");
    }

    public static async Task ProjectBeforeSubVerb_Should_FindAddPackage()
    {
      Diagnostic diagnostic = (await AnalyzeAsync(
        """Shell.Builder("dotnet").WithArguments("add", "app.csproj", "package", "Shouldly").Build();""")).ShouldHaveSingleItem();

      diagnostic.GetMessage(CultureInfo.InvariantCulture).ShouldContain("Use DotNet.AddPackage()");
      diagnostic.GetMessage(CultureInfo.InvariantCulture).ShouldContain("""with "add package" """.TrimEnd());
    }

    public static async Task VersionFlag_Should_ReportWithVersion()
    {
      Diagnostic diagnostic = (await AnalyzeAsync("""Shell.Builder("dotnet").WithArguments("--version").Build();""")).ShouldHaveSingleItem();

      diagnostic.Id.ShouldBe("AMURU0001");
      diagnostic.GetMessage(CultureInfo.InvariantCulture).ShouldContain("Use DotNet.WithVersion()");
    }

    public static async Task MsBuildAndNuGetVerify_Should_ReportTypedBuilders()
    {
      ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
        """
        Shell.Builder("dotnet").WithArguments("msbuild", "app.csproj", "-getProperty:Version").Build();
        Shell.Builder("dotnet").WithArguments("nuget", "verify", "a.nupkg").Build();
        """);

      diagnostics.Length.ShouldBe(2);
      diagnostics.ShouldAllBe(diagnostic => diagnostic.Id == "AMURU0001");
      diagnostics[0].GetMessage(CultureInfo.InvariantCulture).ShouldContain("DotNet.MsBuild()");
      diagnostics[1].GetMessage(CultureInfo.InvariantCulture).ShouldContain("DotNet.NuGet()");
    }

    public static async Task KnownGapVerb_Should_WarnAndAskForATask()
    {
      Diagnostic diagnostic = (await AnalyzeAsync("""Shell.Builder("dotnet").WithArguments("format", "--verify-no-changes").Build();""")).ShouldHaveSingleItem();

      diagnostic.Id.ShouldBe("AMURU0002");
      diagnostic.Severity.ShouldBe(DiagnosticSeverity.Warning);
      diagnostic.GetMessage(CultureInfo.InvariantCulture)
        .ShouldBe("""Shell.Builder("dotnet") with "format" has no typed builder: file a task in timewarp-amuru for a DotNet builder""");
    }

    public static async Task UnknownVerb_Should_WarnAndPointAtToolRun()
    {
      Diagnostic diagnostic = (await AnalyzeAsync("""Shell.Builder("dotnet").WithArguments("fixie").Build();""")).ShouldHaveSingleItem();

      diagnostic.Id.ShouldBe("AMURU0002");
      diagnostic.GetMessage(CultureInfo.InvariantCulture).ShouldContain("""DotNet.Tool().Run("fixie")""");
    }

    public static async Task NonConstantExecutable_Should_NotReport()
    {
      ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
        """Shell.Builder(Tool()).WithArguments("run").Build();""",
        """static string Tool() => "dotnet";""");

      diagnostics.ShouldBeEmpty();
    }

    public static async Task OtherExecutable_Should_NotReport()
    {
      ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""Shell.Builder("docker").WithArguments("ps").Build();""");

      diagnostics.ShouldBeEmpty();
    }

    public static async Task GhCoveredVerbs_Should_ReportTheGhBuilder()
    {
      ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
        """
        Shell.Builder("gh").WithArguments("run", "list", "--limit", "5").Build();
        Shell.Builder("gh").WithArguments("api", "repos/o/r/dispatches").Build();
        Shell.Run("gh", ["auth", "token"], new CommandOptions());
        Shell.Builder("gh.exe").WithArguments("pr", "view", "42").Build();
        """);

      diagnostics.Length.ShouldBe(4);
      diagnostics.ShouldAllBe(d => d.Id == "AMURU0001" && d.Severity == DiagnosticSeverity.Error);
      diagnostics[0].GetMessage(CultureInfo.InvariantCulture)
        .ShouldBe("""Use Gh.Run().List() (TimeWarp.Amuru.Tools) instead of Shell.Builder("gh") with "run list" """.TrimEnd());
      diagnostics[1].GetMessage(CultureInfo.InvariantCulture).ShouldContain("""Use Gh.Api() (TimeWarp.Amuru.Tools) instead of Shell.Builder("gh") with "api" """.TrimEnd());
      diagnostics[2].GetMessage(CultureInfo.InvariantCulture).ShouldContain("Use Gh.Auth().Token()");
      diagnostics[3].GetMessage(CultureInfo.InvariantCulture).ShouldContain("Use Gh.Pr().View()");
    }

    // One call per GhRows shape. A new row needs a line here and a builder on Gh.
    public static async Task GhRowShapes_Should_EachNameTheirBuilder()
    {
      string body = string.Join(
        Environment.NewLine,
        TypedBuilderTable.GhRows.Select(row => row.SubVerb is null
          ? $$"""Shell.Builder("gh").WithArguments("{{row.Verb}}", "x").Build();"""
          : $$"""Shell.Builder("gh").WithArguments("{{row.Verb}}", "{{row.SubVerb}}").Build();"""));

      ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(body);

      diagnostics.Length.ShouldBe(TypedBuilderTable.GhRows.Length);
      for (int index = 0; index < diagnostics.Length; index++)
      {
        diagnostics[index].GetMessage(CultureInfo.InvariantCulture).ShouldStartWith($"Use {TypedBuilderTable.GhRows[index].Replacement} ");
      }
    }

    public static async Task GhUncoveredOrUnreadable_Should_NotReport()
    {
      ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
        """
        Shell.Builder("gh").WithArguments("run", "view", "1", "--log").Build();
        Shell.Builder("gh").WithArguments("repo", "view").Build();
        Shell.Builder("gh").WithArguments("auth", "login").Build();
        Shell.Builder("gh").Build();
        Shell.Builder("gh").WithArguments(Verb(), "list").Build();
        Shell.Builder("gh").WithArguments("run", Verb()).Build();
        """,
        """static string Verb() => "run";""");

      diagnostics.ShouldBeEmpty();
    }

    // The gh sub-verb is the second argument; "list" later in the line is a value.
    public static async Task GhSubVerbLaterInTheLine_Should_MatchOnlyTheSecondArgument()
    {
      Diagnostic diagnostic = (await AnalyzeAsync(
        """Shell.Builder("gh").WithArguments("issue", "create", "--title", "list").Build();""")).ShouldHaveSingleItem();

      diagnostic.GetMessage(CultureInfo.InvariantCulture).ShouldContain("Use Gh.Issue().Create()");
    }

    public static async Task ShellRunWithArrayLiterals_Should_ReportTheVerb()
    {
      ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
        """
        Shell.Run("dotnet", ["test", "--no-build"], new CommandOptions());
        Shell.Run("dotnet", new[] { "pack" }, new CommandOptions(), null);
        Shell.Run("dotnet.exe", new string[] { "user-secrets", "list" }, new CommandOptions());
        """);

      diagnostics.Length.ShouldBe(3);
      diagnostics.ShouldAllBe(diagnostic => diagnostic.Id == "AMURU0001");
      diagnostics[0].GetMessage(CultureInfo.InvariantCulture)
        .ShouldBe("""Use DotNet.Test() (TimeWarp.Amuru.Tools) instead of Shell.Run("dotnet") with "test" """.TrimEnd());
      diagnostics[1].GetMessage(CultureInfo.InvariantCulture).ShouldContain("DotNet.Pack()");
      diagnostics[2].GetMessage(CultureInfo.InvariantCulture).ShouldContain("DotNet.UserSecrets()");
    }

    public static async Task ShellRunWithArrayVariable_Should_ReportTheDotNetEntryPoint()
    {
      Diagnostic diagnostic = (await AnalyzeAsync(
        """
        string[] arguments = ["run"];
        Shell.Run("dotnet", arguments, new CommandOptions());
        """)).ShouldHaveSingleItem();

      diagnostic.Id.ShouldBe("AMURU0001");
      diagnostic.GetMessage(CultureInfo.InvariantCulture).ShouldContain("the DotNet.* builders");
    }

    public static async Task GitWithHelper_Should_ReportInfo()
    {
      Diagnostic diagnostic = (await AnalyzeAsync(
        """
        Shell.Builder("git").WithArguments("worktree", "add", "../wt", "main").Build();
        Shell.Builder("git").WithArguments("status").Build();
        """)).ShouldHaveSingleItem();

      diagnostic.Id.ShouldBe("AMURU0003");
      diagnostic.Severity.ShouldBe(DiagnosticSeverity.Info);
      diagnostic.GetMessage(CultureInfo.InvariantCulture)
        .ShouldBe("""Use Git.WorktreeAddAsync() (TimeWarp.Amuru.Tools) instead of Shell.Builder("git") with "worktree add <path> <branch>" """.TrimEnd());
    }

    // One call per Git helper, in the shape that helper runs (git.*.cs in TimeWarp.Amuru.Tools),
    // then constant forms of the SetUpstreamAsync and GetCommitsAheadAsync argv (review round 2).
    public static async Task GitHelperShapes_Should_EachNameTheirHelper()
    {
      ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
        """
        Shell.Builder("git").WithArguments("branch", "--set-upstream-to", $"{remote}/{branch}", branch).Build();
        Shell.Builder("git").WithArguments("clone", "--bare", "https://x/y.git", path).Build();
        Shell.Builder("git").WithArguments("config", "remote.origin.fetch", "+refs/heads/*:refs/remotes/origin/*").Build();
        Shell.Builder("git").WithArguments("fetch", remote).Build();
        Shell.Builder("git").WithArguments("remote", "set-head", "origin", "--auto").Build();
        Shell.Builder("git").WithArguments("rev-list", "--count", $"{branch}..HEAD").Build();
        Shell.Builder("git").WithArguments("show-ref", "--verify", "--quiet", "refs/heads/" + branch).Build();
        Shell.Builder("git").WithArguments("symbolic-ref", "refs/remotes/origin/HEAD", "--short").Build();
        Shell.Builder("git").WithArguments("worktree", "add", path, branch).Build();
        Shell.Builder("git").WithArguments("worktree", "add", "-b", branch, path, "origin/main").Build();
        Shell.Builder("git").WithArguments("worktree", "list", "--porcelain").Build();
        Shell.Builder("git").WithArguments("worktree", "remove", "--force", path).Build();
        Shell.Run("git", ["fetch", "origin"], new CommandOptions());
        Shell.Builder("git").WithArguments("branch", "--set-upstream-to", "origin/main", "main").Build();
        Shell.Builder("git").WithArguments("branch", "--set-upstream-to", $"origin/{branch}", branch).Build();
        Shell.Builder("git").WithArguments("rev-list", "--count", "main..HEAD").Build();
        """,
        """static string remote = "origin"; static string branch = "main"; static string path = "../wt";""");

      string[] expected =
      [
        "SetUpstreamAsync", "CloneBareAsync", "ConfigureFetchRefspecAsync", "FetchAsync", "SetRemoteHeadAutoAsync",
        "GetCommitsAheadAsync", "BranchExistsAsync", "GetDefaultBranchAsync", "WorktreeAddAsync",
        "WorktreeAddNewBranchAsync", "WorktreeListPorcelainAsync", "WorktreeRemoveAsync", "FetchAsync",
        "SetUpstreamAsync", "SetUpstreamAsync", "GetCommitsAheadAsync",
      ];
      diagnostics.Length.ShouldBe(expected.Length, string.Join(Environment.NewLine, diagnostics));
      diagnostics.ShouldAllBe(diagnostic => diagnostic.Id == "AMURU0003");
      for (int index = 0; index < expected.Length; index++)
      {
        diagnostics[index].GetMessage(CultureInfo.InvariantCulture).ShouldStartWith($"Use Git.{expected[index]}() ");
      }

      GitHelperTable.Rows.Select(row => row.Method).ShouldBe(expected.Distinct(), ignoreOrder: true);
    }

    // Review round 1, M1: each line is a git call whose operation the named helper does not run.
    public static async Task GitOtherOperations_Should_NotReport()
    {
      ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
        """
        Shell.Builder("git").WithArguments("show-ref", "--verify", "refs/tags/v1").Build();
        Shell.Builder("git").WithArguments("show-ref", "--verify", "--quiet", "refs/tags/v1").Build();
        Shell.Builder("git").WithArguments("show-ref", "--verify", "--quiet", name).Build();
        Shell.Builder("git").WithArguments("rev-list", "--count", "HEAD").Build();
        Shell.Builder("git").WithArguments("rev-list", "--count", name).Build();
        Shell.Builder("git").WithArguments("remote", "get-url", "upstream").Build();
        Shell.Builder("git").WithArguments("remote", "get-url", "origin").Build();
        Shell.Builder("git").WithArguments("remote", "set-head", "origin", "main").Build();
        Shell.Builder("git").WithArguments("remote", "set-head", "upstream", "--auto").Build();
        Shell.Builder("git").WithArguments("config", "remote.origin.fetch").Build();
        Shell.Builder("git").WithArguments("config", "remote.origin.fetch", "+refs/heads/main:refs/remotes/origin/main").Build();
        Shell.Builder("git").WithArguments("fetch", "--all").Build();
        Shell.Builder("git").WithArguments("fetch", "origin", "main").Build();
        Shell.Builder("git").WithArguments("fetch").Build();
        Shell.Builder("git").WithArguments("symbolic-ref", "refs/remotes/origin/HEAD", "refs/remotes/origin/main").Build();
        Shell.Builder("git").WithArguments("rev-parse", "--show-toplevel").Build();
        Shell.Builder("git").WithArguments("worktree", "add", "--detach", "../wt").Build();
        Shell.Builder("git").WithArguments("worktree", "add", "-b", "new").Build();
        Shell.Builder("git").WithArguments("worktree", "list").Build();
        Shell.Builder("git").WithArguments("worktree", "remove", "../wt", "--force").Build();
        Shell.Builder("git").WithArguments("fetch", "--", "origin").Build();
        """,
        """static string name = "main";""");

      diagnostics.ShouldBeEmpty(string.Join(Environment.NewLine, diagnostics));
    }

    // Review round 2, M2-M4: argv shapes close to a helper's that the helper does not run.
    public static async Task GitNearHelperShapes_Should_NotReport()
    {
      ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
        """
        Shell.Builder("git").WithArguments("symbolic-ref", "refs/remotes/origin/HEAD").Build();
        Shell.Builder("git").WithArguments("symbolic-ref", "--short", "refs/remotes/origin/HEAD").Build();
        Shell.Builder("git").WithArguments("branch", "--set-upstream-to", "origin/main", "feature").Build();
        Shell.Builder("git").WithArguments("branch", "--set-upstream-to", "origin", "main").Build();
        Shell.Builder("git").WithArguments("branch", "--set-upstream-to", "/main", "main").Build();
        Shell.Builder("git").WithArguments("branch", "--set-upstream-to", $"{remote}/{branch}", other).Build();
        Shell.Builder("git").WithArguments("branch", "--set-upstream-to", $"/{branch}", branch).Build();
        Shell.Builder("git").WithArguments("branch", "--set-upstream-to", $"{remote}-{branch}", branch).Build();
        Shell.Builder("git").WithArguments("branch", "--set-upstream-to", remote, branch).Build();
        Shell.Builder("git").WithArguments("rev-list", "--count", "main...HEAD").Build();
        Shell.Builder("git").WithArguments("rev-list", "--count", $"{branch}...HEAD").Build();
        """,
        """static string remote = "origin"; static string branch = "main"; static string other = "dev";""");

      diagnostics.ShouldBeEmpty(string.Join(Environment.NewLine, diagnostics));
    }

    // Arguments the analyzer cannot see in full: a spread, an array variable, a builder kept for
    // later, or a later WithArguments that extends the call.
    public static async Task GitIncompleteArguments_Should_NotReport()
    {
      ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
        """
        string[] extra = ["main"];
        Shell.Builder("git").WithArguments(["fetch", .. extra]).Build();
        Shell.Run("git", extra, new CommandOptions());
        ShellBuilder kept = Shell.Builder("git").WithArguments("fetch", "origin");
        kept.WithArguments("main").Build();
        Shell.Builder("git").WithArguments("fetch", "origin").WithArguments("main").Build();
        """);

      diagnostics.ShouldBeEmpty(string.Join(Environment.NewLine, diagnostics));
    }

    public static async Task EditorConfigSeverity_Should_OverrideTheDefaults()
    {
      ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
        """
        Shell.Builder("dotnet").WithArguments("run").Build();
        Shell.Builder("git").WithArguments("fetch", "origin").Build();
        """,
        editorConfig: """
        root = true

        [*.cs]
        dotnet_diagnostic.AMURU0001.severity = none
        dotnet_diagnostic.AMURU0003.severity = warning
        """);

      Diagnostic diagnostic = diagnostics.ShouldHaveSingleItem();
      diagnostic.Id.ShouldBe("AMURU0003");
      diagnostic.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    public static async Task PragmaDisable_Should_SuppressTheError()
    {
      // The directive is sample source compiled by AnalyzeAsync, not a suppression in this file.
      // It is assembled from PragmaWarning so the repo's suppression audit does not count it.
      ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
        $"""
        // A raw dotnet shell is the point of this call.
        {PragmaWarning} disable AMURU0001
        Shell.Builder("dotnet").WithArguments("run").Build();
        {PragmaWarning} restore AMURU0001
        """);

      diagnostics.ShouldBeEmpty();
    }

    private const string PragmaWarning = "#" + "pragma warning";

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
      string body,
      string members = "",
      string? editorConfig = null)
    {
      string directory = Path.Combine(Path.GetTempPath(), "timewarp-amuru-150-analyzer");
      string sourcePath = Path.Combine(directory, "sample.cs");
      string source = $$"""
        using TimeWarp.Amuru;

        public static class Sample
        {
          {{members}}

          public static void Go()
          {
            {{body}}
          }
        }
        """;

      SyntaxTree tree = CSharpSyntaxTree.ParseText(
        source,
        new CSharpParseOptions(LanguageVersion.Preview),
        path: sourcePath);

      CSharpCompilationOptions options = new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable);
      if (editorConfig is not null)
      {
        var config = AnalyzerConfig.Parse(editorConfig, Path.Combine(directory, ".editorconfig"));
        AnalyzerConfigOptionsResult result = AnalyzerConfigSet.Create(ImmutableArray.Create(config)).GetOptionsForSourcePath(sourcePath);
        options = options.WithSyntaxTreeOptionsProvider(new TreeOptions(result.TreeOptions));
      }

      var compilation = CSharpCompilation.Create(
        "AnalyzerSample",
        [tree],
        References(),
        options);

      ImmutableArray<Diagnostic> compileErrors = [.. compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error)];
      compileErrors.ShouldBeEmpty(string.Join(Environment.NewLine, compileErrors));

      ImmutableArray<Diagnostic> diagnostics = await compilation
        .WithAnalyzers([new ShellBuilderAnalyzer()])
        .GetAnalyzerDiagnosticsAsync();

      return [.. diagnostics.OrderBy(d => d.Location.SourceSpan.Start)];
    }

    private static List<MetadataReference> References()
    {
      string platform = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
      List<MetadataReference> references = [.. platform
        .Split(Path.PathSeparator)
        .Where(path => Path.GetFileName(path).StartsWith("System.", StringComparison.Ordinal) ||
          Path.GetFileName(path) is "mscorlib.dll" or "netstandard.dll")
        .Select(path => MetadataReference.CreateFromFile(path))];
      // The runfile output folder holds the TimeWarp.Amuru.dll this test was built against.
      string amuru = $"{typeof(Shell).Assembly.GetName().Name}.dll";
      references.Add(MetadataReference.CreateFromFile(Path.Combine(AppContext.BaseDirectory, amuru)));
      return references;
    }

    private sealed class TreeOptions(ImmutableDictionary<string, ReportDiagnostic> severities) : SyntaxTreeOptionsProvider
    {
      public override GeneratedKind IsGenerated(SyntaxTree tree, CancellationToken cancellationToken) =>
        GeneratedKind.NotGenerated;

      public override bool TryGetDiagnosticValue(
        SyntaxTree tree,
        string diagnosticId,
        CancellationToken cancellationToken,
        out ReportDiagnostic severity) =>
        severities.TryGetValue(diagnosticId, out severity);

      public override bool TryGetGlobalDiagnosticValue(
        string diagnosticId,
        CancellationToken cancellationToken,
        out ReportDiagnostic severity)
      {
        severity = ReportDiagnostic.Default;
        return false;
      }
    }
  }
}
