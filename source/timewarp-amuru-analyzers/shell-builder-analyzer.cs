namespace TimeWarp.Amuru.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

#region Purpose
// Reports Shell.Builder and Shell.Run calls on dotnet, gh, or git where a typed Amuru builder exists.
#endregion

#region Design
// The trigger is an invocation of TimeWarp.Amuru.Shell.Builder or Shell.Run whose executable
// argument is a compile-time constant. A non-constant executable is never reported: the analyzer
// cannot know what runs.
//
// The arguments come from every WithArguments call on the same fluent chain, in order
// (ShellBuilder.WithArguments appends), or from the arguments array (Shell.Run). Each element is a
// ShellArgument: a constant, or a non-constant with its known literal start and end (plus its
// variable name when it is a plain reference, and the final hole's name for `{head}/{name}`, which
// SetUpstreamAsync's row compares). A spread or
// an array that is not written out ends the read. The list is complete only when nothing can add
// to it later: every array was written out and, for Shell.Builder, the chain ends in a call that
// does not return ShellBuilder (Build(), CaptureAsync(), ...). A builder stored in a variable and
// given arguments later reads as incomplete, so dotnet gets the generic AMURU0001 message.
//
// dotnet reads only the leading constants. A table row gives AMURU0001 naming the builder. A verb the table knows with a sub-verb it
// does not, or no readable verb at all, gives the generic AMURU0001 message. A known-gap SDK verb or
// an unknown verb (most likely a local tool) gives AMURU0002. A `.cs` first argument is a
// file-based app run and maps to DotNet.Run().WithFile().
// gh (task 152): only a GhRows match reports, as AMURU0001 naming the Gh builder. A gh command no
// row covers, or one whose first two arguments are not constants, is left alone.
// git: only a complete argument list that has exactly the shape a Git helper runs reports, as
// AMURU0003 (info). Every other git call is left alone (GitHelperTable).
#endregion

/// <summary>
/// Points raw <c>Shell.Builder("dotnet")</c>, <c>Shell.Builder("gh")</c>, and <c>Shell.Builder("git")</c> calls at the typed
/// builders in TimeWarp.Amuru.Tools.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ShellBuilderAnalyzer : DiagnosticAnalyzer
{
  /// <summary>dotnet or gh verb with a typed builder.</summary>
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
    "Use the typed DotNet or Gh builder",
    "Use {0} (" + ToolsPackage + ") instead of {1}{2}",
    Category,
    DiagnosticSeverity.Error,
    isEnabledByDefault: true,
    description: "The typed DotNet and Gh builders carry telemetry (build tags, gh spans), TRACEPARENT propagation, and argument validation. A raw Shell call on dotnet or gh bypasses all of it.",
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
    List<ShellArgument> arguments = [];
    bool complete = method.Name == "Run"
      ? AppendArguments(ArgumentFor(invocation, 1), arguments)
      : ReadChain(invocation, arguments);

    string tool = ToolName(executable);
    if (string.Equals(tool, "dotnet", StringComparison.OrdinalIgnoreCase))
    {
      ReportDotNet(context, invocation, call, LeadingConstants(arguments));
    }
    else if (string.Equals(tool, "gh", StringComparison.OrdinalIgnoreCase))
    {
      TypedBuilderRow? row = TypedBuilderTable.FindAtSecond(TypedBuilderTable.GhRows, LeadingConstants(arguments));
      if (row is not null)
      {
        context.ReportDiagnostic(Diagnostic.Create(
          DotNetBuilderRule,
          invocation.Syntax.GetLocation(),
          row.Replacement,
          call,
          $" with \"{Describe(row)}\""));
      }
    }
    else if (string.Equals(tool, "git", StringComparison.OrdinalIgnoreCase) && complete)
    {
      GitHelperRow? row = GitHelperTable.Find(arguments);
      if (row is not null)
      {
        context.ReportDiagnostic(Diagnostic.Create(
          GitHelperRule,
          invocation.Syntax.GetLocation(),
          row.Replacement,
          call,
          row.Label));
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

  // Walks up the fluent chain from Shell.Builder(...) and appends the arguments of every
  // ShellBuilder.WithArguments call. Returns true when the list is complete.
  private static bool ReadChain(IInvocationOperation builder, List<ShellArgument> arguments)
  {
    bool complete = true;
    IOperation current = builder;
    while (current.Parent is IInvocationOperation next && ReferenceEquals(Unwrap(next.Instance), current))
    {
      if (complete && next.TargetMethod.Name == "WithArguments" && next.TargetMethod.ContainingType.Name == "ShellBuilder")
      {
        complete = AppendArguments(ArgumentFor(next, 0), arguments);
      }

      current = next;
    }

    // A chain that still yields a ShellBuilder can be given more arguments later.
    return complete && current.Type?.Name != "ShellBuilder";
  }

  // Appends the elements of a written-out array. Returns false when the array is not written out
  // or holds a spread, which leaves the argument list incomplete.
  private static bool AppendArguments(IOperation? value, List<ShellArgument> arguments)
  {
    IOperation? unwrapped = Unwrap(value);
    ImmutableArray<IOperation> elements;
    if (unwrapped is IArrayCreationOperation { Initializer: { } initializer })
    {
      elements = initializer.ElementValues;
    }
    else if (unwrapped is ICollectionExpressionOperation collection)
    {
      elements = collection.Elements;
    }
    else
    {
      return false;
    }

    foreach (IOperation element in elements)
    {
      if (element is ISpreadOperation)
      {
        return false;
      }

      arguments.Add(ReadArgument(element));
    }

    return true;
  }

  private static List<string> LeadingConstants(List<ShellArgument> arguments)
  {
    List<string> result = [];
    foreach (ShellArgument argument in arguments)
    {
      if (argument.Text is null)
      {
        break;
      }

      result.Add(argument.Text);
    }

    return result;
  }

  private static ShellArgument ReadArgument(IOperation element)
  {
    string? text = ConstantString(element);
    return text is not null
      ? ShellArgument.Constant(text)
      : ShellArgument.Computed(KnownPrefix(element), KnownSuffix(element), ReferenceName(element), SlashTail(element));
  }

  // The name of a plain local, parameter, field or property reference, otherwise null. Two
  // arguments with the same name read the same value.
  private static string? ReferenceName(IOperation? operation) =>
    Unwrap(operation) switch
    {
      ILocalReferenceOperation local => local.Syntax.ToString(),
      IParameterReferenceOperation parameter => parameter.Syntax.ToString(),
      IFieldReferenceOperation field => field.Syntax.ToString(),
      IPropertyReferenceOperation { Arguments.Length: 0 } property => property.Syntax.ToString(),
      _ => null,
    };

  // For an interpolation {head}/{name} with a non-empty head and no alignment or format on the
  // final hole: the final hole's reference name. Otherwise null.
  private static string? SlashTail(IOperation? operation)
  {
    if (Unwrap(operation) is not IInterpolatedStringOperation interpolated || interpolated.Parts.Length < 2)
    {
      return null;
    }

    int count = interpolated.Parts.Length;
    if (interpolated.Parts[count - 1] is not IInterpolationOperation { Alignment: null, FormatString: null } last ||
      interpolated.Parts[count - 2] is not IInterpolatedStringTextOperation separator)
    {
      return null;
    }

    string? text = ConstantString(separator.Text);
    bool headPresent = count > 2 || text?.Length > 1;
    return text is not null && text.EndsWith("/", StringComparison.Ordinal) && headPresent
      ? ReferenceName(last.Expression)
      : null;
  }

  // The literal text a string expression starts with: the first part of an interpolated string, or
  // the left operand of a concatenation. Empty when not known.
  private static string KnownPrefix(IOperation? operation)
  {
    IOperation? unwrapped = Unwrap(operation);
    string? text = ConstantString(unwrapped);
    if (text is not null)
    {
      return text;
    }

    return unwrapped switch
    {
      IInterpolatedStringOperation interpolated when interpolated.Parts.Length > 0 &&
        interpolated.Parts[0] is IInterpolatedStringTextOperation first => ConstantString(first.Text) ?? string.Empty,
      IBinaryOperation { OperatorKind: BinaryOperatorKind.Add } binary => KnownPrefix(binary.LeftOperand),
      _ => string.Empty,
    };
  }

  // The literal text a string expression ends with. Empty when not known.
  private static string KnownSuffix(IOperation? operation)
  {
    IOperation? unwrapped = Unwrap(operation);
    string? text = ConstantString(unwrapped);
    if (text is not null)
    {
      return text;
    }

    return unwrapped switch
    {
      IInterpolatedStringOperation interpolated when interpolated.Parts.Length > 0 &&
        interpolated.Parts[interpolated.Parts.Length - 1] is IInterpolatedStringTextOperation last => ConstantString(last.Text) ?? string.Empty,
      IBinaryOperation { OperatorKind: BinaryOperatorKind.Add } binary => KnownSuffix(binary.RightOperand),
      _ => string.Empty,
    };
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
