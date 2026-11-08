#!/usr/bin/env -S dotnet --

#region Purpose
// Timeout grace-period coverage for stream, passthrough, TTY, pipe, select, and run.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// These modes share the classification in shell-builder.with-timeout.cs.
// The sibling file holds capture and strict validation. This file stays under the 500-line cap.
// Signal-specific cases run on Linux and macOS. Temp directories come from CreateTempSubdirectory.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class WithTimeoutModes_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<WithTimeoutModes_Given_>();

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

    [Timeout(20000)]
    public static async Task Passthrough_GracefulExit_Should_ReportTheChildCode()
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

        output.TimedOut.ShouldBeFalse();
        output.ExitCode.ShouldBe(0);
        output.Stdout.ShouldBeEmpty();
        (await File.ReadAllTextAsync(marker)).ShouldContain("GRACEFUL");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Timeout(20000)]
    public static async Task TtyPassthrough_GracefulExit_Should_ReportTheChildCodeWithoutThrowing()
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

        output.TimedOut.ShouldBeFalse();
        output.ExitCode.ShouldBe(0);
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
    public static async Task TtyPassthrough_GracefulExitSeven_Should_ReportSevenWithoutThrowing()
    {
      if (!CanDeliverSigInt())
      {
        TimeWarp.Jaribu.TestHelpers.TestSkipped("SIGINT is not delivered on this platform");
        return;
      }

      CommandOutput output = await Python(
        ExitSevenScript,
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(3),
        strict: true).TtyPassthroughAsync();

      output.TimedOut.ShouldBeFalse();
      output.ExitCode.ShouldBe(7);
    }

    [Timeout(20000)]
    public static async Task TtyPassthrough_UnhandledSigInt_Should_ReportExit130WithoutThrowing()
    {
      if (!CanDeliverSigInt())
      {
        TimeWarp.Jaribu.TestHelpers.TestSkipped("SIGINT is not delivered on this platform");
        return;
      }

      CommandOutput output = await Python(
        DefaultDispositionScript,
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(3),
        strict: true).TtyPassthroughAsync();

      output.TimedOut.ShouldBeFalse();
      output.ExitCode.ShouldBe(130);
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
    public static async Task Pipe_LastStageExitsDuringGrace_Should_ReportThatExitCode()
    {
      if (!CanDeliverSigInt())
      {
        TimeWarp.Jaribu.TestHelpers.TestSkipped("SIGINT is not delivered on this platform");
        return;
      }

      CommandOptions stage = new CommandOptions()
        .WithTimeout(TimeSpan.FromSeconds(1))
        .WithTimeoutGracePeriod(TimeSpan.FromSeconds(3));
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("stage")
        .Pipe("python3", stage, "-c", ExitSevenScript)
        .CaptureAsync();

      output.TimedOut.ShouldBeFalse();
      output.ExitCode.ShouldBe(7);
    }

    [Timeout(20000)]
    public static async Task Pipe_LastStageExitsDuringGrace_Strict_Should_ThrowCommandExecutionException()
    {
      if (!CanDeliverSigInt())
      {
        TimeWarp.Jaribu.TestHelpers.TestSkipped("SIGINT is not delivered on this platform");
        return;
      }

      CommandOptions stage = new CommandOptions()
        .WithTimeout(TimeSpan.FromSeconds(1))
        .WithTimeoutGracePeriod(TimeSpan.FromSeconds(3))
        .WithZeroExitCodeValidation();

      CliWrap.Exceptions.CommandExecutionException exception = await Should.ThrowAsync<CliWrap.Exceptions.CommandExecutionException>(
        async () => await Shell.Builder("echo")
          .WithArguments("stage")
          .Pipe("python3", stage, "-c", ExitSevenScript)
          .CaptureAsync());

      exception.ExitCode.ShouldBe(7);
    }

    [Timeout(20000)]
    public static async Task Select_GracefulExit_Should_ReturnTheChildText()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      CommandResult command = Python(GracefulScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));

      string selected = await command.SelectAsync();

      selected.ShouldContain("GRACEFUL");
      command.LastOutput.ShouldNotBeNull();
      command.LastOutput.TimedOut.ShouldBeFalse();
      command.LastOutput.ExitCode.ShouldBe(0);
    }

    [Timeout(20000)]
    public static async Task Run_GracefulExit_Should_ReturnTheChildCode()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      int exitCode = await Python(GracefulScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)).RunAsync();

      exitCode.ShouldBe(0);
    }

    [Timeout(20000)]
    public static async Task RunAndCapture_GracefulExit_Should_KeepStdout()
    {
      if (!CanDeliverSigInt())
      {
        return;
      }

      CommandOutput output = await Python(GracefulScript, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3))
        .RunAndCaptureAsync();

      output.TimedOut.ShouldBeFalse();
      output.ExitCode.ShouldBe(0);
      output.Stdout.ShouldContain("GRACEFUL");
      output.Stderr.ShouldContain("GRACEFUL-ERR");
    }

    [Timeout(20000)]
    public static async Task StreamToFile_GracefulExit_Should_KeepTheChildOutput()
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
        command.LastOutput.TimedOut.ShouldBeFalse();
        command.LastOutput.ExitCode.ShouldBe(0);
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

  }
}
