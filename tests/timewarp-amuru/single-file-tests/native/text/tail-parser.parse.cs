#!/usr/bin/env -S dotnet --

#region Purpose
// TailParser accepts each tail form the command owns and rejects the rest.
#endregion

#region Design
// One method covers one form. A bare -N or +N is digits only. A + token that is
// not a count is a file name. -F, --pid, and -s fail in words and do not fall through.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace TextTailParser_
{
using Text = TimeWarp.Amuru.Native.Text;

[TestTag("Native")]
public class Parse_Given_
{
  private const string Supported =
    "Supported forms: -N, -n N, -nN, --lines N, --lines=N, +N, -c N, -cN, --bytes N, --bytes=N, -q, -v, -z, -f, --, --help, --version.";

  [ModuleInitializer]
  internal static void Register() => RegisterTests<Parse_Given_>();

  public static async Task NoArguments_Should_DefaultToLastTenLines()
  {
    Text.TailParseResult parsed = Text.TailParser.Parse([]);
    parsed.Success.ShouldBeTrue();
    parsed.Options.Measure.ShouldBe(Text.TailMeasure.LastLines(10));
    parsed.Operands.Count.ShouldBe(0);
    parsed.Options.SaveStream.ShouldBeTrue();
    await Task.CompletedTask;
  }

  public static async Task BareNumber_Should_KeepThatManyLines()
  {
    AssertLines(["-40"], 40, false);
    await Task.CompletedTask;
  }

  public static async Task SpacedLines_Should_KeepThatManyLines()
  {
    AssertLines(["-n", "3"], 3, false);
    await Task.CompletedTask;
  }

  public static async Task AttachedLines_Should_KeepThatManyLines()
  {
    AssertLines(["-n3"], 3, false);
    await Task.CompletedTask;
  }

  public static async Task LongLines_Should_KeepThatManyLines()
  {
    AssertLines(["--lines", "4"], 4, false);
    await Task.CompletedTask;
  }

  public static async Task LongLinesEquals_Should_KeepThatManyLines()
  {
    AssertLines(["--lines=4"], 4, false);
    await Task.CompletedTask;
  }

  public static async Task LinesFromStart_Should_UseOneBasedCount()
  {
    AssertLines(["-n", "+8"], 8, true);
    await Task.CompletedTask;
  }

  public static async Task LongLinesFromStart_Should_UseOneBasedCount()
  {
    AssertLines(["--lines=+3"], 3, true);
    await Task.CompletedTask;
  }

  public static async Task BareFromStart_Should_UseOneBasedCount()
  {
    AssertLines(["+8"], 8, true);
    await Task.CompletedTask;
  }

  public static async Task FromStartZero_Should_StartAtTheFirstLine()
  {
    AssertLines(["-n", "+0"], 0, true);
    AssertLines(["+0"], 0, true);
    await Task.CompletedTask;
  }

  public static async Task NegativeLines_Should_CountFromTheEnd()
  {
    AssertLines(["-n", "-3"], 3, false);
    AssertLines(["--lines=-3"], 3, false);
    await Task.CompletedTask;
  }

  public static async Task SpacedBytes_Should_KeepThatManyBytes()
  {
    AssertBytes(["-c", "4"], 4, false);
    await Task.CompletedTask;
  }

  public static async Task AttachedBytes_Should_KeepThatManyBytes()
  {
    AssertBytes(["-c4"], 4, false);
    await Task.CompletedTask;
  }

  public static async Task LongBytes_Should_KeepThatManyBytes()
  {
    AssertBytes(["--bytes", "4"], 4, false);
    await Task.CompletedTask;
  }

  public static async Task LongBytesEquals_Should_KeepThatManyBytes()
  {
    AssertBytes(["--bytes=4"], 4, false);
    await Task.CompletedTask;
  }

  public static async Task BytesFromStart_Should_UseOneBasedOffset()
  {
    AssertBytes(["-c", "+4"], 4, true);
    AssertBytes(["--bytes=+4"], 4, true);
    await Task.CompletedTask;
  }

  public static async Task BlockSuffix_Should_Mean512()
  {
    AssertBytes(["-c", "2b"], 1024, false);
    await Task.CompletedTask;
  }

  public static async Task KibibyteSuffix_Should_Mean1024()
  {
    AssertBytes(["-c", "1K"], 1024, false);
    AssertBytes(["-c", "1KiB"], 1024, false);
    AssertBytes(["--bytes=1k"], 1024, false);
    await Task.CompletedTask;
  }

  public static async Task KilobyteSuffix_Should_Mean1000()
  {
    AssertBytes(["-c", "1KB"], 1000, false);
    AssertBytes(["-c", "1mB"], 1_000_000, false);
    await Task.CompletedTask;
  }

  public static async Task MebibyteSuffix_Should_Mean1024Squared()
  {
    AssertBytes(["-c", "1M"], 1024L * 1024L, false);
    AssertBytes(["-c", "1MiB"], 1024L * 1024L, false);
    await Task.CompletedTask;
  }

  public static async Task OverflowingSuffix_Should_Saturate()
  {
    AssertBytes(["-c", "999Y"], long.MaxValue, false);
    await Task.CompletedTask;
  }

  public static async Task Quiet_Should_SuppressHeaders()
  {
    AssertHeaders(["-q"], Text.TailHeaders.Never);
    AssertHeaders(["--quiet"], Text.TailHeaders.Never);
    AssertHeaders(["--silent"], Text.TailHeaders.Never);
    await Task.CompletedTask;
  }

  public static async Task Verbose_Should_ForceHeaders()
  {
    AssertHeaders(["-v"], Text.TailHeaders.Always);
    AssertHeaders(["--verbose"], Text.TailHeaders.Always);
    await Task.CompletedTask;
  }

  public static async Task ZeroTerminated_Should_UseNul()
  {
    Text.TailParseResult parsed = Text.TailParser.Parse(["-z"]);
    parsed.Success.ShouldBeTrue();
    parsed.Options.ZeroTerminated.ShouldBeTrue();
    parsed = Text.TailParser.Parse(["--zero-terminated"]);
    parsed.Options.ZeroTerminated.ShouldBeTrue();
    await Task.CompletedTask;
  }

  public static async Task Follow_Should_PollFiles()
  {
    AssertFollow(["-f"]);
    AssertFollow(["--follow"]);
    AssertFollow(["--follow=descriptor"]);
    await Task.CompletedTask;
  }

  public static async Task BundledFollowAndLines_Should_ApplyBoth()
  {
    Text.TailParseResult parsed = Text.TailParser.Parse(["-fn", "20"]);
    parsed.Success.ShouldBeTrue();
    parsed.Options.Follow.ShouldBeTrue();
    parsed.Options.Measure.ShouldBe(Text.TailMeasure.LastLines(20));
    await Task.CompletedTask;
  }

  public static async Task BundledQuietAndLines_Should_ApplyBoth()
  {
    Text.TailParseResult parsed = Text.TailParser.Parse(["-qn", "5"]);
    parsed.Success.ShouldBeTrue();
    parsed.Options.Headers.ShouldBe(Text.TailHeaders.Never);
    parsed.Options.Measure.ShouldBe(Text.TailMeasure.LastLines(5));
    await Task.CompletedTask;
  }

  public static async Task BundledBytesSuffix_Should_ApplyTheSuffix()
  {
    AssertBytes(["-c1K"], 1024, false);
    await Task.CompletedTask;
  }

  public static async Task Operands_Should_BeAcceptedBeforeAndAfterOptions()
  {
    AssertLines(["notes.txt", "-n", "1"], 1, false, "notes.txt");
    AssertLines(["-n", "1", "a.txt", "b.txt"], 1, false, "a.txt", "b.txt");
    await Task.CompletedTask;
  }

  public static async Task DashOperand_Should_MeanStandardInput()
  {
    AssertLines(["-n", "1", "-"], 1, false, "-");
    await Task.CompletedTask;
  }

  public static async Task EndOfOptions_Should_LeaveTheRestAsOperands()
  {
    AssertLines(["--", "-n"], 10, false, "-n");
    AssertLines(["-n", "1", "--", "-v"], 1, false, "-v");
    await Task.CompletedTask;
  }

  public static async Task PlusThatIsNotACount_Should_BeAFileName()
  {
    AssertLines(["+1a"], 10, false, "+1a");
    AssertLines(["+foo"], 10, false, "+foo");
    AssertLines(["+"], 10, false, "+");
    AssertLines(["+1K"], 10, false, "+1K");
    await Task.CompletedTask;
  }

  public static async Task EmptyToken_Should_BeAnOperand()
  {
    AssertLines([""], 10, false, "");
    await Task.CompletedTask;
  }

  public static async Task RepeatedCount_Should_KeepTheLastValue()
  {
    AssertLines(["-n", "2", "-n", "9"], 9, false);
    AssertBytes(["-n", "4", "-c", "3"], 3, false);
    await Task.CompletedTask;
  }

  public static async Task RepeatedHeaderFlag_Should_KeepTheLastValue()
  {
    AssertHeaders(["-v", "-q"], Text.TailHeaders.Never);
    AssertHeaders(["-q", "-v"], Text.TailHeaders.Always);
    await Task.CompletedTask;
  }

  public static async Task Help_Should_StopTheParse()
  {
    Text.TailParseResult parsed = Text.TailParser.Parse(["--help", "-x"]);
    parsed.Success.ShouldBeTrue();
    parsed.Options.Help.ShouldBeTrue();
    parsed.Options.Version.ShouldBeFalse();
    parsed.Operands.Count.ShouldBe(0);
    await Task.CompletedTask;
  }

  public static async Task Version_Should_StopTheParse()
  {
    Text.TailParseResult parsed = Text.TailParser.Parse(["--version", "-x"]);
    parsed.Success.ShouldBeTrue();
    parsed.Options.Version.ShouldBeTrue();
    parsed.Options.Help.ShouldBeFalse();
    await Task.CompletedTask;
  }

  public static async Task UnknownShort_Should_NameTheSupportedForms()
  {
    AssertRejected(["-x"], "invalid option -- 'x'", namesForms: true);
    await Task.CompletedTask;
  }

  public static async Task UnknownLong_Should_NameTheSupportedForms()
  {
    AssertRejected(["--nope"], "invalid option -- '--nope'", namesForms: true);
    await Task.CompletedTask;
  }

  public static async Task HelpAfterAnError_Should_StayAnError()
  {
    Text.TailParseResult parsed = Text.TailParser.Parse(["-x", "--help"]);
    parsed.Success.ShouldBeFalse();
    parsed.Options.Help.ShouldBeFalse();
    await Task.CompletedTask;
  }

  public static async Task BareSuffix_Should_BeRejected()
  {
    AssertRejected(["-2c"], "invalid number of lines: '-2c'");
    AssertRejected(["-1K"], "invalid number of lines: '-1K'");
    await Task.CompletedTask;
  }

  public static async Task MissingCount_Should_SayTheOptionNeedsAValue()
  {
    AssertRejected(["-n"], "option '-n' requires an argument", namesForms: true);
    AssertRejected(["--lines"], "option '--lines' requires an argument", namesForms: true);
    AssertRejected(["-c"], "option '-c' requires an argument", namesForms: true);
    await Task.CompletedTask;
  }

  public static async Task InvalidNumber_Should_NameTheKind()
  {
    AssertRejected(["-n", "foo"], "invalid number of lines: 'foo'");
    AssertRejected(["-c", "1.5K"], "invalid number of bytes: '1.5K'");
    AssertRejected(["-c", "1mb"], "invalid number of bytes: '1mb'");
    AssertRejected(["-n", "2c"], "invalid number of lines: '2c'");
    await Task.CompletedTask;
  }

  public static async Task SleepInterval_Should_BeRejected()
  {
    AssertRejected(["-s"], "tail: -s is not supported; the sleep interval is not set from the command line");
    AssertRejected(["--sleep-interval", "1"], "tail: -s is not supported");
    await Task.CompletedTask;
  }

  public static async Task Rotation_Should_BeRejected()
  {
    AssertRejected(["-F"], "tail: -F is not supported; rotation following is not available");
    AssertRejected(["--follow=name"], "tail: -F is not supported; rotation following is not available");
    await Task.CompletedTask;
  }

  public static async Task Pid_Should_BeRejected()
  {
    AssertRejected(["--pid"], "tail: --pid is not supported");
    AssertRejected(["--pid=4"], "tail: --pid is not supported");
    await Task.CompletedTask;
  }

  public static async Task UnsupportedLongOptions_Should_BeRejected()
  {
    AssertRejected(["--retry"], "tail: --retry is not supported");
    AssertRejected(["--max-unchanged-stats", "2"], "tail: --max-unchanged-stats is not supported");
    AssertRejected(["--use-polling"], "tail: --use-polling is not supported");
    AssertRejected(["--debug"], "tail: --debug is not supported");
    await Task.CompletedTask;
  }

  public static async Task NullArguments_Should_Throw()
  {
    Should.Throw<ArgumentNullException>(() => Text.TailParser.Parse(null!));
    Should.Throw<ArgumentNullException>(() => Text.TailParser.Parse(new string?[] { null! }!));
    await Task.CompletedTask;
  }

  private static void AssertLines(string[] arguments, long count, bool fromStart, params string[] operands)
  {
    Text.TailParseResult parsed = Text.TailParser.Parse(arguments);
    parsed.Success.ShouldBeTrue(parsed.Error);
    parsed.Options.Measure.ShouldBe(new Text.TailMeasure(Text.TailUnit.Lines, count, fromStart));
    parsed.Operands.ShouldBe(operands);
  }

  private static void AssertBytes(string[] arguments, long count, bool fromStart, params string[] operands)
  {
    Text.TailParseResult parsed = Text.TailParser.Parse(arguments);
    parsed.Success.ShouldBeTrue(parsed.Error);
    parsed.Options.Measure.ShouldBe(new Text.TailMeasure(Text.TailUnit.Bytes, count, fromStart));
    parsed.Operands.ShouldBe(operands);
  }

  private static void AssertHeaders(string[] arguments, Text.TailHeaders headers)
  {
    Text.TailParseResult parsed = Text.TailParser.Parse(arguments);
    parsed.Success.ShouldBeTrue(parsed.Error);
    parsed.Options.Headers.ShouldBe(headers);
  }

  private static void AssertFollow(string[] arguments)
  {
    Text.TailParseResult parsed = Text.TailParser.Parse(arguments);
    parsed.Success.ShouldBeTrue(parsed.Error);
    parsed.Options.Follow.ShouldBeTrue();
  }

  private static void AssertRejected(string[] arguments, string fragment, bool namesForms = false)
  {
    Text.TailParseResult parsed = Text.TailParser.Parse(arguments);
    parsed.Success.ShouldBeFalse();
    parsed.Error.ShouldNotBeNull();
    parsed.Error.ShouldContain(fragment);
    if (namesForms)
    {
      parsed.Error.ShouldContain(Supported);
    }
  }
}
}
