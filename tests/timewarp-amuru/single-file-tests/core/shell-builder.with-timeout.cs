#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for per-command timeout on ShellBuilder and CommandResult.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// The child is python3 so SIGINT hits that process. A nested sleep would swallow the signal.
// Signal-specific cases run on Linux and macOS. Other platforms still assert the exit-124 result.
// Temp directories come from CreateTempSubdirectory, not a hardcoded /tmp path.
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

    private const string GracefulMarkerScript =
      """
      import os, signal, time
      def handle(signum, frame):
          marker = os.environ["MARKER"]
          with open(marker, "w", encoding="utf-8") as marker_file:
              marker_file.write("GRACEFUL")
          raise SystemExit(0)
      signal.signal(signal.SIGINT, handle)
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
    public static async Task DefaultValidation_Should_ReportExit124WithoutThrowing()
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

      output.TimedOut.ShouldBeTrue();
      output.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
      output.Success.ShouldBeFalse();
      output.Stdout.ShouldContain("GRACEFUL");
      stopwatch.Elapsed.ShouldBeLessThan(timeout + TimeSpan.FromMilliseconds(1500));
    }

    [Timeout(20000)]
    public static async Task StrictValidation_Should_ThrowTimeoutException()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      TimeoutException exception = await Should.ThrowAsync<TimeoutException>(async () =>
        await Python(GracefulScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), strict: true)
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

      CommandResult command = Python(GracefulScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), strict: true);

      await Should.ThrowAsync<TimeoutException>(async () => await command.CaptureAsync());

      command.LastOutput.ShouldNotBeNull();
      command.LastOutput.TimedOut.ShouldBeTrue();
      command.LastOutput.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
      command.LastOutput.Stdout.ShouldContain("GRACEFUL");
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
    public static async Task StreamStdout_Should_EndWithTimeoutState()
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
      command.LastOutput.TimedOut.ShouldBeTrue();
      command.LastOutput.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
    }

    [Timeout(20000)]
    public static async Task StreamStderr_Should_EndWithTimeoutState()
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
      command.LastOutput.TimedOut.ShouldBeTrue();
      command.LastOutput.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
    }

    [Timeout(20000)]
    public static async Task StrictStream_Should_ThrowAfterRecordingLastOutput()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      CommandResult command = Python(
        GracefulScript,
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(3),
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
    public static async Task Passthrough_Should_ReportTimeout()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      string directory = Directory.CreateTempSubdirectory("amuru-timeout-pass-").FullName;
      try
      {
        string marker = Path.Combine(directory, "graceful.txt");
        CommandOutput output = await Shell.Builder("python3")
          .WithArguments("-c", GracefulMarkerScript)
          .WithEnvironmentVariable("MARKER", marker)
          .WithStandardInput("")
          .WithTimeout(TimeSpan.FromSeconds(1))
          .WithTimeoutGracePeriod(TimeSpan.FromSeconds(3))
          .Build()
          .PassthroughAsync();

        output.TimedOut.ShouldBeTrue();
        output.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
        output.Stdout.ShouldBeEmpty();
        (await File.ReadAllTextAsync(marker)).ShouldContain("GRACEFUL");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Timeout(20000)]
    public static async Task TtyPassthrough_Should_ReportTimeoutWithoutThrowing()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      string directory = Directory.CreateTempSubdirectory("amuru-timeout-tty-").FullName;
      try
      {
        string marker = Path.Combine(directory, "graceful.txt");
        CommandOutput output = await Shell.Builder("python3")
          .WithArguments("-c", GracefulMarkerScript)
          .WithEnvironmentVariable("MARKER", marker)
          .WithTimeout(TimeSpan.FromSeconds(1))
          .WithTimeoutGracePeriod(TimeSpan.FromSeconds(3))
          .WithZeroExitCodeValidation()
          .Build()
          .TtyPassthroughAsync();

        output.TimedOut.ShouldBeTrue();
        output.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
        (await File.ReadAllTextAsync(marker)).ShouldContain("GRACEFUL");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Timeout(20000)]
    public static async Task TtyPassthrough_Should_ForceKillAChildThatIgnoresSigInt()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      var timeout = TimeSpan.FromMilliseconds(500);
      var grace = TimeSpan.FromMilliseconds(500);
      var stopwatch = Stopwatch.StartNew();
      CommandOutput output = await Python(IgnoreScript, timeout, grace).TtyPassthroughAsync();
      stopwatch.Stop();

      output.TimedOut.ShouldBeTrue();
      output.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
      stopwatch.Elapsed.ShouldBeGreaterThanOrEqualTo(timeout + grace - TimeSpan.FromMilliseconds(300));
      stopwatch.Elapsed.ShouldBeLessThan(timeout + grace + TimeSpan.FromSeconds(4));
    }

    [Timeout(20000)]
    public static async Task StreamStdout_Should_PropagateCallerCancellationAndLeaveLastOutputUnset()
    {
      CommandResult command = Python(IgnoreScript, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));
      using CancellationTokenSource source = new(TimeSpan.FromMilliseconds(300));

      await Should.ThrowAsync<OperationCanceledException>(async () =>
      {
        await foreach (string line in command.StreamStdoutAsync(source.Token))
        {
          _ = line;
        }
      });

      command.LastOutput.ShouldBeNull();
    }

    [Timeout(20000)]
    public static async Task Pipe_Should_ShareTheUpstreamWindow()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      var timeout = TimeSpan.FromSeconds(1);
      var stopwatch = Stopwatch.StartNew();
      CommandOutput output = await Python(IgnoreScript, timeout, TimeSpan.Zero)
        .Pipe("cat")
        .CaptureAsync();
      stopwatch.Stop();

      output.TimedOut.ShouldBeTrue();
      output.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
      stopwatch.Elapsed.ShouldBeLessThan(timeout + TimeSpan.FromSeconds(3));
    }

    [Timeout(20000)]
    public static async Task Pipe_Should_UseTheShorterStageTimeoutAndItsGrace()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      CommandOptions stage = new CommandOptions()
        .WithTimeout(TimeSpan.FromSeconds(1))
        .WithTimeoutGracePeriod(TimeSpan.Zero);
      var stopwatch = Stopwatch.StartNew();
      CommandOutput output = await Python(IgnoreScript, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5))
        .Pipe("python3", stage, "-c", IgnoreScript)
        .CaptureAsync();
      stopwatch.Stop();

      output.TimedOut.ShouldBeTrue();
      output.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
      stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(4));
    }

    [Timeout(20000)]
    public static async Task Select_Should_ReturnCapturedTextAndRecordTimeout()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      CommandResult command = Python(GracefulScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));

      string selected = await command.SelectAsync();

      selected.ShouldContain("GRACEFUL");
      command.LastOutput.ShouldNotBeNull();
      command.LastOutput.TimedOut.ShouldBeTrue();
      command.LastOutput.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
    }

    [Timeout(20000)]
    public static async Task Run_Should_Return124()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      int exitCode = await Python(GracefulScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)).RunAsync();

      exitCode.ShouldBe(CommandResult.TimeoutExitCode);
    }

    [Timeout(20000)]
    public static async Task RunAndCapture_Should_Report124AndKeepStdout()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      CommandOutput output = await Python(GracefulScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3))
        .RunAndCaptureAsync();

      output.TimedOut.ShouldBeTrue();
      output.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
      output.Stdout.ShouldContain("GRACEFUL");
      output.Stderr.ShouldContain("GRACEFUL-ERR");
    }

    [Timeout(20000)]
    public static async Task StreamToFile_Should_KeepPartialOutputAndRecordTimeout()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      string directory = Directory.CreateTempSubdirectory("amuru-timeout-file-").FullName;
      try
      {
        string outputPath = Path.Combine(directory, "out.txt");
        CommandResult command = Python(GracefulScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));

        await command.StreamToFileAsync(outputPath);

        command.LastOutput.ShouldNotBeNull();
        command.LastOutput.TimedOut.ShouldBeTrue();
        (await File.ReadAllTextAsync(outputPath)).ShouldContain("GRACEFUL");
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
