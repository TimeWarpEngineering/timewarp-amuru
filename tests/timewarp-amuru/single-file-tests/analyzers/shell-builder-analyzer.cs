#!/usr/bin/env -S dotnet --
#:package Microsoft.CodeAnalysis.CSharp
#:project $(SourceDirectory)timewarp-amuru-analyzers/timewarp-amuru-analyzers.csproj

#region Purpose
// Proves AMURU0001-0003: which Shell.Builder and Shell.Run calls report, at what severity, with
// what message, and that the verb table matches the public DotNet and Git surface.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Each case compiles a small source in process against the built TimeWarp.Amuru assembly and runs
// ShellBuilderAnalyzer through CompilationWithAnalyzers. Pragma-suppressed diagnostics are dropped
// by that API, so the pragma case expects none.
// The editorconfig case parses a real .editorconfig with AnalyzerConfigSet and feeds its tree
// options to the compilation, which is how the compiler applies severity overrides.
// The surface cases reflect over DotNet and Git (TimeWarp.Amuru.Tools). A new DotNet builder with
// no table row fails here, and so does a row whose method was removed.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilderAnalyzer_
{
  using System.Collections.Immutable;
  using System.Diagnostics.CodeAnalysis;
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
      ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""Shell.Builder("gh").WithArguments("pr", "list").Build();""");

      diagnostics.ShouldBeEmpty();
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
        .ShouldBe("""Use Git.WorktreeAddAsync() or Git.WorktreeAddNewBranchAsync() (TimeWarp.Amuru.Tools) instead of Shell.Builder("git") with "worktree add" """.TrimEnd());
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

      string[] stale = [.. TypedBuilderTable.GitRows
        .SelectMany(row => row.Methods)
        .Where(method => !helpers.Contains(method))
        .Distinct()];

      stale.ShouldBeEmpty($"TypedBuilderTable.GitRows names missing Git methods: {string.Join(", ", stale)}");
      await Task.CompletedTask;
    }

    public static async Task GapVerbs_Should_NotOverlapTheBuilderRows()
    {
      string[] overlap = [.. TypedBuilderTable.DotNetGapVerbs
        .Where(verb => TypedBuilderTable.HasVerb(TypedBuilderTable.DotNetRows, verb))];

      overlap.ShouldBeEmpty();
      TypedBuilderTable.IsDotNetGap("msbuild").ShouldBeFalse();
      TypedBuilderTable.IsDotNetGap("nuget").ShouldBeFalse();
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
