namespace TimeWarp.Amuru.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

#region Purpose
// Reports Shell.Builder and Shell.Run calls on dotnet or git where a typed Amuru builder exists.
#endregion

#region Design
// The trigger is an invocation of TimeWarp.Amuru.Shell.Builder or Shell.Run whose executable
// argument is a compile-time constant. A non-constant executable is never reported: the analyzer
// cannot know what runs.
//
// The arguments come from the first WithArguments call on the same fluent chain (Shell.Builder) or
// from the arguments array (Shell.Run). Only the leading constant elements are read; the first
// non-constant element or spread ends the read. A builder stored in a variable and given arguments
// later reads as "no arguments", which gives the generic AMURU0001 message.
//
// dotnet: a table row gives AMURU0001 naming the builder. A verb the table knows with a sub-verb it
// does not, or no readable verb at all, gives the generic AMURU0001 message. A known-gap SDK verb or
// an unknown verb (most likely a local tool) gives AMURU0002. A `.cs` first argument is a
// file-based app run and maps to DotNet.Run().WithFile().
// git: only a table row reports, as AMURU0003 (info). Every other git call is left alone.
#endregion

/// <summary>
/// Points raw <c>Shell.Builder("dotnet")</c> and <c>Shell.Builder("git")</c> calls at the typed
/// builders in TimeWarp.Amuru.Tools.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ShellBuilderAnalyzer : DiagnosticAnalyzer
{
  /// <summary>dotnet verb with a typed builder.</summary>
  public const string DotNetBuilderId = "AMURU0001";

  /// <summary>dotnet verb with no typed builder yet.</summary>
  public const string DotNetGapId = "AMURU0002";

  /// <summary>git call with a typed Git helper.</summary>
  public const string GitHelperId = "AMURU0003";

  private const string Category = "Usage";
  private const string ToolsPackage = "TimeWarp.Amuru.Tools";
  private const string HelpLink = "https://github.com/TimeWarpEngineering/timewarp-amuru#analyzers";
  private const string ShellNamespace = "TimeWarp.Amuru";

  private static readonly DiagnosticDescriptor DotNetBuilderRule = new(
    DotNetBuilderId,
    "Use the typed DotNet builder",
    "Use {0} (" + ToolsPackage + ") instead of {1}{2}",
    Category,
    DiagnosticSeverity.Error,
    isEnabledByDefault: true,
    description: "The typed DotNet builders carry telemetry tags, TRACEPARENT propagation, and argument validation. A raw Shell call on dotnet bypasses all of it.",
    helpLinkUri: HelpLink);

  private static readonly DiagnosticDescriptor DotNetGapRule = new(
    DotNetGapId,
    "dotnet verb has no typed builder",
    "{0} with \"{1}\" has no typed builder: {2}",
    Category,
    DiagnosticSeverity.Warning,
    isEnabledByDefault: true,
    description: "This dotnet verb has no DotNet builder yet. File a task in timewarp-amuru so the call can move to one.",
    helpLinkUri: HelpLink);

  private static readonly DiagnosticDescriptor GitHelperRule = new(
    GitHelperId,
    "Use the typed Git helper",
    "Use {0} (" + ToolsPackage + ") instead of {1} with \"{2}\"",
    Category,
    DiagnosticSeverity.Info,
    isEnabledByDefault: true,
    description: "A typed Git helper performs this git command. Raise this rule per repository in .editorconfig.",
    helpLinkUri: HelpLink);

  /// <inheritdoc />
  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [DotNetBuilderRule, DotNetGapRule, GitHelperRule];

  /// <inheritdoc />
  public override void Initialize(AnalysisContext context)
  {
    if (context is null)
    {
      throw new ArgumentNullException(nameof(context));
    }

    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
  }

  private static void AnalyzeInvocation(OperationAnalysisContext context)
  {
    var invocation = (IInvocationOperation)context.Operation;
    IMethodSymbol method = invocation.TargetMethod;
    if (!IsShellEntry(method))
    {
      return;
    }

    string? executable = ConstantString(ArgumentFor(invocation, 0));
    if (executable is null)
    {
      return;
    }

    string call = $"Shell.{method.Name}(\"{executable}\")";
    List<string> arguments = method.Name == "Run"
      ? ReadConstants(ArgumentFor(invocation, 1))
      : ReadConstants(FindWithArguments(invocation));

    string tool = ToolName(executable);
    if (string.Equals(tool, "dotnet", StringComparison.OrdinalIgnoreCase))
    {
      ReportDotNet(context, invocation, call, arguments);
    }
    else if (string.Equals(tool, "git", StringComparison.OrdinalIgnoreCase))
    {
      TypedBuilderRow? row = TypedBuilderTable.Find(TypedBuilderTable.GitRows, arguments);
      if (row is not null)
      {
        context.ReportDiagnostic(Diagnostic.Create(
          GitHelperRule,
          invocation.Syntax.GetLocation(),
          row.Replacement,
          call,
          Describe(row)));
      }
    }
  }

  private static void ReportDotNet(
    OperationAnalysisContext context,
    IInvocationOperation invocation,
    string call,
    List<string> arguments)
  {
    Location location = invocation.Syntax.GetLocation();
    if (arguments.Count == 0)
    {
      context.ReportDiagnostic(Diagnostic.Create(DotNetBuilderRule, location, "the DotNet.* builders", call, string.Empty));
      return;
    }

    string verb = arguments[0];
    TypedBuilderRow? row = TypedBuilderTable.Find(TypedBuilderTable.DotNetRows, arguments);
    if (row is not null)
    {
      context.ReportDiagnostic(Diagnostic.Create(DotNetBuilderRule, location, row.Replacement, call, $" with \"{Describe(row)}\""));
      return;
    }

    if (verb.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
    {
      context.ReportDiagnostic(Diagnostic.Create(DotNetBuilderRule, location, "DotNet.Run().WithFile()", call, $" with \"{verb}\""));
      return;
    }

    if (TypedBuilderTable.HasVerb(TypedBuilderTable.DotNetRows, verb))
    {
      context.ReportDiagnostic(Diagnostic.Create(DotNetBuilderRule, location, "the DotNet.* builders", call, $" with \"{verb}\""));
      return;
    }

    string advice = TypedBuilderTable.IsDotNetGap(verb)
      ? "file a task in timewarp-amuru for a DotNet builder"
      : $"for a local tool use DotNet.Tool().Run(\"{verb}\") ({ToolsPackage}), otherwise file a task in timewarp-amuru for a DotNet builder";
    context.ReportDiagnostic(Diagnostic.Create(DotNetGapRule, location, call, verb, advice));
  }

  private static string Describe(TypedBuilderRow row) =>
    row.SubVerb is null ? row.Verb : $"{row.Verb} {row.SubVerb}";

  private static bool IsShellEntry(IMethodSymbol method)
  {
    if (!method.IsStatic || method.Name is not ("Builder" or "Run") || method.Parameters.Length == 0)
    {
      return false;
    }

    INamedTypeSymbol type = method.ContainingType;
    return type.Name == "Shell" &&
      type.ContainingType is null &&
      type.ContainingNamespace.ToDisplayString() == ShellNamespace &&
      method.Parameters[0].Type.SpecialType == SpecialType.System_String;
  }

  private static string ToolName(string executable)
  {
    int slash = Math.Max(executable.LastIndexOf('/'), executable.LastIndexOf('\\'));
    string name = slash >= 0 ? executable.Substring(slash + 1) : executable;
    if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
    {
      name = name.Substring(0, name.Length - 4);
    }

    return name;
  }

  private static IOperation? ArgumentFor(IInvocationOperation invocation, int ordinal)
  {
    foreach (IArgumentOperation argument in invocation.Arguments)
    {
      if (argument.Parameter?.Ordinal == ordinal)
      {
        return argument.Value;
      }
    }

    return null;
  }

  // Walks up the fluent chain from Shell.Builder(...) to the first ShellBuilder.WithArguments call.
  private static IOperation? FindWithArguments(IInvocationOperation builder)
  {
    IOperation current = builder;
    while (current.Parent is IInvocationOperation next && ReferenceEquals(Unwrap(next.Instance), current))
    {
      if (next.TargetMethod.Name == "WithArguments" && next.TargetMethod.ContainingType.Name == "ShellBuilder")
      {
        return ArgumentFor(next, 0);
      }

      current = next;
    }

    return null;
  }

  private static List<string> ReadConstants(IOperation? value)
  {
    IOperation? unwrapped = Unwrap(value);
    if (unwrapped is IArrayCreationOperation { Initializer: { } initializer })
    {
      return LeadingConstants(initializer.ElementValues);
    }

    if (unwrapped is ICollectionExpressionOperation collection)
    {
      return LeadingConstants(collection.Elements);
    }

    return [];
  }

  private static List<string> LeadingConstants(ImmutableArray<IOperation> elements)
  {
    List<string> result = [];
    foreach (IOperation element in elements)
    {
      string? text = ConstantString(element);
      if (text is null)
      {
        break;
      }

      result.Add(text);
    }

    return result;
  }

  private static string? ConstantString(IOperation? operation)
  {
    IOperation? unwrapped = Unwrap(operation);
    return unwrapped is not null && unwrapped.ConstantValue.HasValue && unwrapped.ConstantValue.Value is string text
      ? text
      : null;
  }

  private static IOperation? Unwrap(IOperation? operation)
  {
    while (operation is IConversionOperation conversion && conversion.IsImplicit)
    {
      operation = conversion.Operand;
    }

    return operation;
  }
}
