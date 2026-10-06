#region Purpose
// Compiles SelectString and ReplaceInFiles patterns and checks option bounds.
#endregion

#region Design
// RegexOptions.Compiled is intentional; native AOT ignores it and runs the interpreter.
// CultureInvariant keeps case folding stable across machines. Literal patterns are
// Regex.Escape'd so SimpleMatch cannot inject regex syntax.
#endregion

namespace TimeWarp.Amuru.Native.Text;

internal static class TextPatterns
{
  public static SelectStringOptions Select(SelectStringOptions? options)
  {
    SelectStringOptions resolved = options ?? new SelectStringOptions();
    if (resolved.ContextBefore < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(options),
        resolved.ContextBefore,
        "ContextBefore must be greater than or equal to zero.");
    }

    if (resolved.ContextAfter < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(options),
        resolved.ContextAfter,
        "ContextAfter must be greater than or equal to zero.");
    }

    if (resolved.MaxMatches is < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(options),
        resolved.MaxMatches,
        "MaxMatches must be greater than or equal to zero.");
    }

    return resolved;
  }

  public static ReplaceInFilesOptions Replace(ReplaceInFilesOptions? options)
  {
    ReplaceInFilesOptions resolved = options ?? new ReplaceInFilesOptions();
    if (resolved.MaxReplacements is < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(options),
        resolved.MaxReplacements,
        "MaxReplacements must be greater than or equal to zero.");
    }

    return resolved;
  }

  public static Regex Compile(string pattern, SelectStringOptions options)
  {
    ArgumentNullException.ThrowIfNull(pattern);
    ArgumentNullException.ThrowIfNull(options);
    return CompileCore(
      pattern,
      options.SimpleMatch,
      options.CaseInsensitive,
      multiline: false,
      singleline: false);
  }

  public static Regex Compile(string pattern, ReplaceInFilesOptions options)
  {
    ArgumentNullException.ThrowIfNull(pattern);
    ArgumentNullException.ThrowIfNull(options);
    return CompileCore(
      pattern,
      options.SimpleMatch,
      options.CaseInsensitive,
      options.Multiline,
      options.Singleline);
  }

  private static Regex CompileCore(
    string pattern,
    bool simpleMatch,
    bool caseInsensitive,
    bool multiline,
    bool singleline)
  {
    RegexOptions flags = RegexOptions.Compiled | RegexOptions.CultureInvariant;
    if (caseInsensitive)
    {
      flags |= RegexOptions.IgnoreCase;
    }

    if (multiline)
    {
      flags |= RegexOptions.Multiline;
    }

    if (singleline)
    {
      flags |= RegexOptions.Singleline;
    }

    string body = simpleMatch ? Regex.Escape(pattern) : pattern;
    return new Regex(body, flags);
  }
}
