#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for per-command timeout on ShellBuilder and CommandResult.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// The child is python3 so SIGINT hits that process. A nested sleep would swallow the signal.
// Signal-specific cases run on Linux and macOS. Other platforms still assert the exit-124 result.
// Temp directories come from CreateTempSubdirectory, not a hardcoded /tmp path.
// Stream, passthrough, TTY, pipe, select, and run live in shell-builder.with-timeout.modes.cs.
// A timed stream keeps one unread line. A slow consumer must stall the child before the burst finishes.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class WithTimeout_Given_
  {
    private const string GracefulScript =
      """
      import signal, sys, time
      def handle(signum, frame):
          print("GRACEFUL", flush=True)
          print("GRACEFUL-ERR", file=sys.stderr, flush=True)
          raise SystemExit(0)
      signal.signal(signal.SIGINT, handle)
      while True:
          time.sleep(0.05)
      """;

    private const string IgnoreScript =
      """
      import signal, time
      signal.signal(signal.SIGINT, signal.SIG_IGN)
      while True:
          time.sleep(0.05)
      """;

    private const string ExitSevenScript =
      """
      import signal, time
      def handle(signum, frame):
          raise SystemExit(7)
      signal.signal(signal.SIGINT, handle)
      while True:
          time.sleep(0.05)
      """;

    private const string DefaultDispositionScript =
      """
      import signal, time
      signal.signal(signal.SIGINT, signal.SIG_DFL)
      while True:
          time.sleep(0.05)
      """;

    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithTimeout_Given_>();

    public static async Task NoTimeout_Should_LeaveTimedOutFalse()
    {
      CommandResult command = Shell.Builder("echo").WithArguments("still-here").Build();

      CommandOutput output = await command.CaptureAsync();

      output.TimedOut.ShouldBeFalse();
      output.ExitCode.ShouldBe(0);
      output.Success.ShouldBeTrue();
      output.Stdout.Trim().ShouldBe("still-here");
      command.LastOutput.ShouldBeSameAs(output);
    }

    [Timeout(20000)]
    public static async Task GracefulExitZero_Should_ReportTheChildCode()
    {
      if (!CanDeliverSigInt())
      {
        CommandOutput forced = await ForceTimeoutOnThisPlatform();
        forced.TimedOut.ShouldBeTrue();
        forced.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
        forced.Success.ShouldBeFalse();
        return;
      }

      var timeout = TimeSpan.FromSeconds(1);
      var grace = TimeSpan.FromSeconds(3);
      var stopwatch = Stopwatch.StartNew();
      CommandOutput output = await Python(GracefulScript, timeout, grace).CaptureAsync();
      stopwatch.Stop();

      output.TimedOut.ShouldBeFalse();
      output.ExitCode.ShouldBe(0);
      output.Success.ShouldBeTrue();
      output.Stdout.ShouldContain("GRACEFUL");
      stopwatch.Elapsed.ShouldBeLessThan(timeout + TimeSpan.FromMilliseconds(1500));
    }

    [Timeout(20000)]
    public static async Task GracefulExitZero_Strict_Should_NotThrow()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      CommandResult command = Python(GracefulScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), strict: true);

      CommandOutput output = await command.CaptureAsync();

      output.TimedOut.ShouldBeFalse();
      output.ExitCode.ShouldBe(0);
      output.Success.ShouldBeTrue();
      command.LastOutput.ShouldBeSameAs(output);
    }

    [Timeout(20000)]
    public static async Task GracefulExitSeven_Should_ReportSeven()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      CommandOutput output = await Python(ExitSevenScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3))
        .CaptureAsync();

      output.TimedOut.ShouldBeFalse();
      output.ExitCode.ShouldBe(7);
      output.Success.ShouldBeFalse();
    }

    [Timeout(20000)]
    public static async Task GracefulExitSeven_Strict_Should_ThrowCommandExecutionException()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      CommandResult command = Python(ExitSevenScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), strict: true);

      CliWrap.Exceptions.CommandExecutionException exception = await Should.ThrowAsync<CliWrap.Exceptions.CommandExecutionException>(
        async () => await command.CaptureAsync());

      exception.ExitCode.ShouldBe(7);
      command.LastOutput.ShouldBeNull();
    }

    [Timeout(20000)]
    public static async Task UnhandledSigInt_Should_ReportExit130()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      CommandOutput output = await Python(DefaultDispositionScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3))
        .CaptureAsync();

      output.TimedOut.ShouldBeFalse();
      output.ExitCode.ShouldBe(130);
    }

    [Timeout(20000)]
    public static async Task StrictValidation_Should_ThrowTimeoutException()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      TimeoutException exception = await Should.ThrowAsync<TimeoutException>(async () =>
        await Python(IgnoreScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), strict: true)
          .CaptureAsync());

      exception.Message.ShouldContain("timed out");
      exception.Message.ShouldContain("terminated");
    }

    [Timeout(20000)]
    public static async Task StrictCapture_Should_RecordLastOutputBeforeThrowing()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      CommandResult command = Python(IgnoreScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), strict: true);

      await Should.ThrowAsync<TimeoutException>(async () => await command.CaptureAsync());

      command.LastOutput.ShouldNotBeNull();
      command.LastOutput.TimedOut.ShouldBeTrue();
      command.LastOutput.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
    }

    [Timeout(20000)]
    public static async Task CallerCancellation_Should_PropagateAndLeaveLastOutputUnset()
    {
      CommandResult command = Python(IgnoreScript, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));
      using CancellationTokenSource source = new(TimeSpan.FromMilliseconds(300));

      await Should.ThrowAsync<OperationCanceledException>(async () => await command.CaptureAsync(source.Token));

      command.LastOutput.ShouldBeNull();
    }

    [Timeout(20000)]
    public static async Task IgnoredSigInt_Should_BeForceKilledAfterGrace()
    {
      if (!CanDeliverSigInt())
      {
        CommandOutput forced = await ForceTimeoutOnThisPlatform();
        forced.TimedOut.ShouldBeTrue();
        forced.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
        return;
      }

      var timeout = TimeSpan.FromSeconds(1);
      var grace = TimeSpan.FromSeconds(1);
      var stopwatch = Stopwatch.StartNew();
      CommandOutput output = await Python(IgnoreScript, timeout, grace).CaptureAsync();
      stopwatch.Stop();

      output.TimedOut.ShouldBeTrue();
      output.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
      output.Stdout.ShouldNotContain("GRACEFUL");
      stopwatch.Elapsed.ShouldBeGreaterThanOrEqualTo(timeout + grace - TimeSpan.FromMilliseconds(400));
      stopwatch.Elapsed.ShouldBeLessThan(timeout + grace + TimeSpan.FromSeconds(4));
    }

    [Timeout(20000)]
    public static async Task StreamStdout_GracefulExit_Should_ReportTheChildCode()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      CommandResult command = Python(GracefulScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
      List<string> lines = [];
      await foreach (string line in command.StreamStdoutAsync())
      {
        lines.Add(line);
      }

      string.Join('\n', lines).ShouldContain("GRACEFUL");
      command.LastOutput.ShouldNotBeNull();
      command.LastOutput.TimedOut.ShouldBeFalse();
      command.LastOutput.ExitCode.ShouldBe(0);
    }

    [Timeout(20000)]
    public static async Task StreamStderr_GracefulExit_Should_ReportTheChildCode()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      CommandResult command = Python(GracefulScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
      List<string> lines = [];
      await foreach (string line in command.StreamStderrAsync())
      {
        lines.Add(line);
      }

      string.Join('\n', lines).ShouldContain("GRACEFUL-ERR");
      command.LastOutput.ShouldNotBeNull();
      command.LastOutput.TimedOut.ShouldBeFalse();
      command.LastOutput.ExitCode.ShouldBe(0);
    }

    [Timeout(20000)]
    public static async Task StrictStream_Should_ThrowAfterRecordingLastOutput()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      CommandResult command = Python(
        IgnoreScript,
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(1),
        strict: true);

      await Should.ThrowAsync<TimeoutException>(async () =>
      {
        await foreach (string line in command.StreamStdoutAsync())
        {
          _ = line;
        }
      });

      command.LastOutput.ShouldNotBeNull();
      command.LastOutput.TimedOut.ShouldBeTrue();
    }

    public static async Task StreamWithoutTimeout_Should_RecordExitCode()
    {
      CommandResult command = Shell.Builder("bash").WithArguments("-c", "echo hi; exit 3").Build();
      List<string> lines = [];
      await foreach (string line in command.StreamStdoutAsync())
      {
        lines.Add(line);
      }

      lines.ShouldContain("hi");
      command.LastOutput.ShouldNotBeNull();
      command.LastOutput.ExitCode.ShouldBe(3);
      command.LastOutput.TimedOut.ShouldBeFalse();
    }

    [Timeout(20000)]
    public static async Task StreamStdout_WithTimeout_Should_PushBackOnASlowConsumer()
    {
      string directory = Directory.CreateTempSubdirectory("amuru-stream-backpressure-").FullName;
      try
      {
        string marker = Path.Combine(directory, "done");
        const string script =
          """
          import os, pathlib
          path = pathlib.Path(os.environ["AMURU_MARKER"])
          print("START", flush=True)
          line = "x" * 1024
          for _ in range(4096):
              print(line, flush=True)
          path.write_text("done", encoding="utf-8")
          print("DONE", flush=True)
          """;

        CommandResult command = Shell.Builder("python3")
          .WithArguments("-c", script)
          .WithEnvironmentVariable("AMURU_MARKER", marker)
          .WithTimeout(TimeSpan.FromSeconds(15))
          .WithTimeoutGracePeriod(TimeSpan.FromSeconds(2))
          .Build();

        bool held = false;
        await foreach (string line in command.StreamStdoutAsync())
        {
          if (held)
          {
            continue;
          }

          line.ShouldBe("START");
          held = true;
          await Task.Delay(TimeSpan.FromSeconds(2));
          File.Exists(marker).ShouldBeFalse();
        }

        held.ShouldBeTrue();
        File.Exists(marker).ShouldBeTrue();
        command.LastOutput.ShouldNotBeNull();
        command.LastOutput.ExitCode.ShouldBe(0);
        command.LastOutput.TimedOut.ShouldBeFalse();
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    private static bool CanDeliverSigInt() =>
      OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();

    private static CommandResult Python(string script, TimeSpan timeout, TimeSpan grace, bool strict = false)
    {
      ShellBuilder builder = Shell.Builder("python3")
        .WithArguments("-c", script)
        .WithTimeout(timeout)
        .WithTimeoutGracePeriod(grace);
      if (strict)
      {
        builder = builder.WithZeroExitCodeValidation();
      }

      return builder.Build();
    }

    private static async Task<CommandOutput> ForceTimeoutOnThisPlatform()
    {
      return await Shell.Builder("ping")
        .WithArguments("-n", "30", "127.0.0.1")
        .WithTimeout(TimeSpan.FromSeconds(1))
        .WithTimeoutGracePeriod(TimeSpan.Zero)
        .CaptureAsync();
    }
  }
}
