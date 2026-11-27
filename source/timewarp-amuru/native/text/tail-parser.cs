#region Purpose
// Turns tail argv into TailOptions and file operands.
#endregion

#region Design
// Tail owns this parser. Unix tools do not share one option grammar, and an unknown
// option must not fall through to another binary.
// A repeated count or header flag keeps the last value. --help and --version stop
// the parse and are reported as flags; this type does not print either.
// -F, --pid, and -s are rejected in words. Rotation, pid, and the sleep interval
// are not implemented.
// A bare -N or +N is digits only. The obsolete [bcl][f] suffix on that form is
// not accepted; -c and -n own suffixes. A + token that is not a count is a file name.
#endregion

namespace TimeWarp.Amuru.Native.Text;

/// <summary>
/// Parses <c>tail</c> arguments into <see cref="TailOptions"/>.
/// </summary>
public static class TailParser
{
  private const string Supported =
    "Supported forms: -N, -n N, -nN, --lines N, --lines=N, +N, -c N, -cN, --bytes N, --bytes=N, -q, -v, -z, -f, --, --help, --version.\n";

  /// <summary>
  /// Parses <paramref name="arguments"/>.
  /// </summary>
  /// <param name="arguments">The argument vector, without the program name.</param>
  /// <returns>Options and operands, or a usage error.</returns>
  /// <exception cref="ArgumentNullException">When <paramref name="arguments"/> or an entry is null.</exception>
  public static TailParseResult Parse(IReadOnlyList<string> arguments)
  {
    ArgumentNullException.ThrowIfNull(arguments);
    TailDraft draft = new();
    List<string> operands = [];
    bool optionsOpen = true;
    for (int index = 0; index < arguments.Count; index++)
    {
      string token = arguments[index];
      ArgumentNullException.ThrowIfNull(token);
      if (!optionsOpen || token.Length == 0 || token == "-" || !IsOption(token))
      {
        operands.Add(token);
        continue;
      }

      if (token == "--")
      {
        optionsOpen = false;
        continue;
      }

      if (token == "--help")
      {
        draft.Help = true;
        return Success(draft, []);
      }

      if (token == "--version")
      {
        draft.Version = true;
        return Success(draft, []);
      }

      string? error = token.StartsWith("--", StringComparison.Ordinal)
        ? ParseLong(token, arguments, ref index, draft)
        : ParseShort(token, arguments, ref index, draft);
      if (error is not null)
      {
        return Failure(error);
      }
    }

    return Success(draft, operands);
  }

  private static bool IsBareCount(string token)
  {
    for (int index = 1; index < token.Length; index++)
    {
      if (!char.IsAsciiDigit(token[index]))
      {
        return false;
      }
    }

    return token.Length >= 2;
  }

  private static bool IsOption(string token)
  {
    if (token.StartsWith("--", StringComparison.Ordinal))
    {
      return true;
    }

    if (token.Length >= 2 && token[0] == '-' && char.IsAsciiDigit(token[1]))
    {
      return true;
    }

    if (token.Length >= 2 && token[0] == '+' && char.IsAsciiDigit(token[1]))
    {
      return IsBareCount(token);
    }

    return token.Length >= 2 && token[0] == '-' && !char.IsAsciiDigit(token[1]);
  }

  private static string? ParseShort(string token, IReadOnlyList<string> arguments, ref int index, TailDraft draft)
  {
    if (token.Length >= 2 && char.IsAsciiDigit(token[1]))
    {
      if (!IsBareCount(token))
      {
        return $"tail: invalid number of lines: '{token}'\n";
      }

      return draft.SetMeasure(TailUnit.Lines, token, "lines");
    }

    for (int cursor = 1; cursor < token.Length; cursor++)
    {
      char flag = token[cursor];
      switch (flag)
      {
        case 'q':
          draft.Headers = TailHeaders.Never;
          break;
        case 'v':
          draft.Headers = TailHeaders.Always;
          break;
        case 'z':
          draft.ZeroTerminated = true;
          break;
        case 'f':
          draft.Follow = true;
          break;
        case 'n':
        case 'c':
          string? value = ReadAttachedOrNext(token, ref cursor, arguments, ref index);
          if (value is null)
          {
            return $"tail: option '-{flag}' requires an argument\n{Supported}";
          }

          TailUnit unit = flag == 'n' ? TailUnit.Lines : TailUnit.Bytes;
          string kind = flag == 'n' ? "lines" : "bytes";
          return draft.SetMeasure(unit, value, kind);
        case 's':
          return "tail: -s is not supported; the sleep interval is not set from the command line\n";
        case 'F':
          return "tail: -F is not supported; rotation following is not available\n";
        default:
          return $"tail: invalid option -- '{flag}'\n{Supported}";
      }
    }

    return null;
  }

  private static string? ReadAttachedOrNext(
    string token,
    ref int cursor,
    IReadOnlyList<string> arguments,
    ref int index)
  {
    if (cursor + 1 < token.Length)
    {
      string attached = token[(cursor + 1)..];
      cursor = token.Length;
      return attached;
    }

    if (index + 1 >= arguments.Count)
    {
      return null;
    }

    index++;
    return arguments[index];
  }

  private static string? ParseLong(string token, IReadOnlyList<string> arguments, ref int index, TailDraft draft)
  {
    string body = token[2..];
    string name = body;
    string? inline = null;
    bool hasInline = false;
    int split = body.IndexOf('=', StringComparison.Ordinal);
    if (split >= 0)
    {
      name = body[..split];
      inline = body[(split + 1)..];
      hasInline = true;
    }

    switch (name)
    {
      case "lines":
      case "bytes":
        string? value = inline;
        if (!hasInline)
        {
          if (index + 1 >= arguments.Count)
          {
            return $"tail: option '--{name}' requires an argument\n{Supported}";
          }

          index++;
          value = arguments[index];
        }

        TailUnit unit = name == "lines" ? TailUnit.Lines : TailUnit.Bytes;
        return draft.SetMeasure(unit, value ?? string.Empty, name);
      case "quiet":
      case "silent":
        if (hasInline)
        {
          return $"tail: option '--{name}' does not take a value\n{Supported}";
        }

        draft.Headers = TailHeaders.Never;
        return null;
      case "verbose":
        if (hasInline)
        {
          return $"tail: option '--verbose' does not take a value\n{Supported}";
        }

        draft.Headers = TailHeaders.Always;
        return null;
      case "zero-terminated":
        if (hasInline)
        {
          return $"tail: option '--zero-terminated' does not take a value\n{Supported}";
        }

        draft.ZeroTerminated = true;
        return null;
      case "follow":
        if (!hasInline || inline == "descriptor")
        {
          draft.Follow = true;
          return null;
        }

        if (inline == "name")
        {
          return "tail: -F is not supported; rotation following is not available\n";
        }

        return $"tail: invalid option -- '--follow={inline}'\n{Supported}";
      case "pid":
        return "tail: --pid is not supported\n";
      case "sleep-interval":
        return "tail: -s is not supported; the sleep interval is not set from the command line\n";
      case "retry":
      case "max-unchanged-stats":
      case "use-polling":
      case "debug":
        return $"tail: --{name} is not supported\n";
      default:
        return $"tail: invalid option -- '{token}'\n{Supported}";
    }
  }

  private static TailParseResult Success(TailDraft draft, IReadOnlyList<string> operands)
  {
    return new TailParseResult(draft.Build(), operands, null);
  }

  private static TailParseResult Failure(string error)
  {
    return new TailParseResult(new TailOptions(), [], error);
  }

  private sealed class TailDraft
  {
    public TailMeasure Measure { get; private set; } = TailMeasure.LastLines(10);

    public TailHeaders Headers { get; set; } = TailHeaders.Auto;

    public bool ZeroTerminated { get; set; }

    public bool Follow { get; set; }

    public bool Help { get; set; }

    public bool Version { get; set; }

    public string? SetMeasure(TailUnit unit, string text, string kind)
    {
      if (!TailNumbers.TryParse(text, out long count, out bool fromStart))
      {
        return $"tail: invalid number of {kind}: '{text}'\n";
      }

      Measure = new TailMeasure(unit, count, fromStart);
      return null;
    }

    public TailOptions Build()
    {
      return new TailOptions
      {
        Measure = Measure,
        Headers = Headers,
        ZeroTerminated = ZeroTerminated,
        Follow = Follow,
        Help = Help,
        Version = Version
      };
    }
  }
}
