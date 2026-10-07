#!/usr/bin/env -S dotnet --

using System.Globalization;

#region Purpose
// Proves capture buffers large output and streaming keeps a bounded retained set.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Children are seq and python3. No fixtures are written to disk.
// Capture of seq 1..2000000 and the stream of that same command share one test so the memory ratio uses one baseline.
// Retained stream memory is sampled with GC.GetTotalMemory(true) so gen0 garbage from line strings is not the peak.
// Unforced samples are printed with the ratio. The assertion is retained bytes under 25% of the capture delta.
// RunAndCaptureAsync is the path whose OutputLines order is all stdout then all stderr.
// RunAsync sends seq to the terminal with stdout and stderr replaced by TextWriter.Null so the CI log stays small.
// Linux commands are skipped on other operating systems. Jaribu has no runtime skip signal; the method returns.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace CommandResult_
{
  [TestTag("Core")]
  public class LargeOutput_Given_
  {
    private const int TwoMillion = 2_000_000;
    private const int InterleavedCount = 200_000;
    private const int LongLineLength = 10_000_000;

    private const string LongLineScript = """
      import sys
      sys.stdout.buffer.write(b"x" * 10000000)
      """;

    private const string InterleavedScript = """
      import sys
      count = 200000
      for index in range(count):
          sys.stdout.write(f"o{index}\n")
      for index in range(count):
          sys.stderr.write(f"e{index}\n")
      """;

    [ModuleInitializer]
    internal static void Register() => RegisterTests<LargeOutput_Given_>();

    [Timeout(60000)]
    public static async Task TwoMillionLines_Should_CaptureExactLinesAndStreamUnderCaptureMemory()
    {
      if (!RequireLinux())
      {
        return;
      }

      long captureDelta = await CaptureTwoMillionAsync();
      captureDelta.ShouldBeGreaterThan(1_000_000);

      Collect();
      long streamBaseline = GC.GetTotalMemory(true);
      long retainedPeak = streamBaseline;
      long unforcedPeak = streamBaseline;
      CommandResult command = Seq(TwoMillion);
      int count = 0;
      string? first = null;
      string? last = null;

      await foreach (string line in command.StreamStdoutAsync())
      {
        if (count == 0)
        {
          first = line;
        }

        last = line;
        count++;
        if ((count & 65_535) == 0)
        {
          long unforced = GC.GetTotalMemory(false);
          if (unforced > unforcedPeak)
          {
            unforcedPeak = unforced;
          }
        }

        if (count % 250_000 == 0)
        {
          long retained = GC.GetTotalMemory(true);
          if (retained > retainedPeak)
          {
            retainedPeak = retained;
          }
        }
      }

      long finalRetained = GC.GetTotalMemory(true);
      if (finalRetained > retainedPeak)
      {
        retainedPeak = finalRetained;
      }

      count.ShouldBe(TwoMillion);
      first.ShouldBe("1");
      last.ShouldBe("2000000");

      long streamDelta = retainedPeak - streamBaseline;
      if (streamDelta < 0)
      {
        streamDelta = 0;
      }

      long unforcedDelta = unforcedPeak - streamBaseline;
      if (unforcedDelta < 0)
      {
        unforcedDelta = 0;
      }

      double retainedRatio = (double)streamDelta / captureDelta;
      double unforcedRatio = (double)unforcedDelta / captureDelta;
      await TimeWarpTerminal.Default.WriteLineAsync(
        string.Create(
          CultureInfo.InvariantCulture,
          $"large-output captureDelta={captureDelta} retainedDelta={streamDelta} retainedRatio={retainedRatio:F4} unforcedDelta={unforcedDelta} unforcedRatio={unforcedRatio:F4}"));

      streamDelta.ShouldBeLessThan(captureDelta / 4);
    }

    [Timeout(60000)]
    public static async Task SingleLongLine_Should_CaptureAndStreamIntact()
    {
      if (!RequireLinux())
      {
        return;
      }

      CommandResult command = Python(LongLineScript);
      CommandOutput output = await command.CaptureAsync();
      output.ExitCode.ShouldBe(0);
      output.Success.ShouldBeTrue();
      string[] lines = output.GetLines();
      lines.Length.ShouldBe(1);
      lines[0].Length.ShouldBe(LongLineLength);
      lines[0].All(character => character == 'x').ShouldBeTrue();

      int streamCount = 0;
      int streamLength = 0;
      bool allX = true;
      await foreach (string line in Python(LongLineScript).StreamStdoutAsync())
      {
        streamCount++;
        streamLength = line.Length;
        allX = line.All(character => character == 'x');
      }

      streamCount.ShouldBe(1);
      streamLength.ShouldBe(LongLineLength);
      allX.ShouldBeTrue();
    }

    [Timeout(60000)]
    public static async Task InterleavedStreams_Should_OrderStdoutBeforeStderr()
    {
      if (!RequireLinux())
      {
        return;
      }

      CommandOutput output = await WithSilentConsoleAsync(() =>
        Python(InterleavedScript).RunAndCaptureAsync());

      output.ExitCode.ShouldBe(0);
      output.Success.ShouldBeTrue();

      string[] stdout = output.GetStdoutLines();
      string[] stderr = output.GetStderrLines();
      stdout.Length.ShouldBe(InterleavedCount);
      stderr.Length.ShouldBe(InterleavedCount);
      stdout[0].ShouldBe("o0");
      stdout[InterleavedCount - 1].ShouldBe("o199999");
      stderr[0].ShouldBe("e0");
      stderr[InterleavedCount - 1].ShouldBe("e199999");

      IReadOnlyList<OutputLine> lines = output.OutputLines;
      lines.Count.ShouldBe(InterleavedCount * 2);
      lines.Take(InterleavedCount).All(line => !line.IsError).ShouldBeTrue();
      lines.Skip(InterleavedCount).All(line => line.IsError).ShouldBeTrue();
    }

    [Timeout(60000)]
    public static async Task TwoMillionLines_Should_RunToConsoleWithoutDeadlock()
    {
      if (!RequireLinux())
      {
        return;
      }

      int exitCode = await WithSilentConsoleAsync(() => Seq(TwoMillion).RunAsync());
      exitCode.ShouldBe(0);
    }

    private static CommandResult Seq(int count) =>
      Shell.Builder("seq").WithArguments("1", count.ToString(CultureInfo.InvariantCulture)).Build();

    private static CommandResult Python(string script) =>
      Shell.Builder("python3").WithArguments("-c", script).Build();

    private static async Task<long> CaptureTwoMillionAsync()
    {
      Collect();
      long before = GC.GetTotalMemory(true);
      CommandOutput output = await Seq(TwoMillion).CaptureAsync();
      string[] lines = output.GetLines();
      lines.Length.ShouldBe(TwoMillion);
      lines[0].ShouldBe("1");
      lines[TwoMillion - 1].ShouldBe("2000000");
      output.ExitCode.ShouldBe(0);
      output.Success.ShouldBeTrue();
      long after = GC.GetTotalMemory(true);
      return after - before;
    }

    private static async Task<T> WithSilentConsoleAsync<T>(Func<Task<T>> action)
    {
      TextWriter stdout = TimeWarpTerminal.Default.Out;
      TextWriter stderr = TimeWarpTerminal.Default.Error;
      try
      {
        TimeWarpTerminal.Default.SetOut(TextWriter.Null);
        TimeWarpTerminal.Default.SetError(TextWriter.Null);
        return await action();
      }
      finally
      {
        TimeWarpTerminal.Default.SetOut(stdout);
        TimeWarpTerminal.Default.SetError(stderr);
      }
    }

    private static void Collect()
    {
      GC.Collect();
      GC.WaitForPendingFinalizers();
      GC.Collect();
    }

    private static bool RequireLinux()
    {
      if (OperatingSystem.IsLinux())
      {
        return true;
      }

      TimeWarp.Jaribu.TestHelpers.TestSkipped("Requires Linux");
      return false;
    }
  }
}
