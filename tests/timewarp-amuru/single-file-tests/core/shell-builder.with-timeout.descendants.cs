#!/usr/bin/env -S dotnet --

#region Purpose
// Timeout coverage for a descendant that keeps a pipe open after the direct child exits.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// These cases share the classification in shell-builder.with-timeout.cs.
// The capture file stays under the 500-line cap, so the descendant probes live here.
// Signal-specific cases run on Linux and macOS.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class WithTimeoutDescendants_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithTimeoutDescendants_Given_>();

    [Timeout(15000)]
    public static async Task Capture_DescendantHoldsStdoutBeforeTimeout_Should_EndAtTheOriginalDeadline()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      var timeout = TimeSpan.FromSeconds(1);
      var grace = TimeSpan.FromSeconds(1);
      var stopwatch = Stopwatch.StartNew();
      CommandOutput output = await Shell.Builder("sh")
        .WithArguments("-c", "sleep 15 & echo started")
        .WithNoValidation()
        .WithTimeout(timeout)
        .WithTimeoutGracePeriod(grace)
        .CaptureAsync();
      stopwatch.Stop();

      output.ExitCode.ShouldBe(0);
      output.TimedOut.ShouldBeFalse();
      output.Stdout.ShouldContain("started");
      stopwatch.Elapsed.ShouldBeGreaterThan(timeout);
      stopwatch.Elapsed.ShouldBeLessThan(timeout + grace + TimeSpan.FromSeconds(2));
    }

    [Timeout(15000)]
    public static async Task Capture_DescendantHoldsStdoutWithZeroGrace_Should_KeepTheChildCodeAtTimeout()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      var timeout = TimeSpan.FromSeconds(1);
      var stopwatch = Stopwatch.StartNew();
      CommandOutput output = await Shell.Builder("sh")
        .WithArguments("-c", "sleep 15 & echo started")
        .WithNoValidation()
        .WithTimeout(timeout)
        .WithTimeoutGracePeriod(TimeSpan.Zero)
        .CaptureAsync();
      stopwatch.Stop();

      output.ExitCode.ShouldBe(0);
      output.TimedOut.ShouldBeFalse();
      output.Stdout.ShouldContain("started");
      stopwatch.Elapsed.ShouldBeGreaterThan(timeout);
      stopwatch.Elapsed.ShouldBeLessThan(timeout + TimeSpan.FromSeconds(2));
    }

    [Timeout(15000)]
    public static async Task Capture_DescendantHoldsStdoutDuringGrace_Should_EndAtTheDrainBound()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      const string script =
        """
        import os, signal, time
        def handle(signum, frame):
            if os.fork() == 0:
                time.sleep(15); os._exit(0)
            raise SystemExit(0)
        signal.signal(signal.SIGINT, handle)
        while True: time.sleep(0.05)
        """;

      var timeout = TimeSpan.FromSeconds(1);
      var grace = TimeSpan.FromSeconds(1);
      var stopwatch = Stopwatch.StartNew();
      CommandOutput output = await Python(script, timeout, grace).CaptureAsync();
      stopwatch.Stop();

      output.ExitCode.ShouldBe(0);
      output.TimedOut.ShouldBeFalse();
      stopwatch.Elapsed.ShouldBeGreaterThan(timeout + grace);
      stopwatch.Elapsed.ShouldBeLessThan(timeout + grace + grace + TimeSpan.FromSeconds(2));
    }

    private static bool CanDeliverSigInt() =>
      OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();

    private static CommandResult Python(string script, TimeSpan timeout, TimeSpan grace)
    {
      return Shell.Builder("python3")
        .WithArguments("-c", script)
        .WithTimeout(timeout)
        .WithTimeoutGracePeriod(grace)
        .Build();
    }
  }
}
