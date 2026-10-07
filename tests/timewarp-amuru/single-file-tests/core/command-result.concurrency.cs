#!/usr/bin/env -S dotnet --

#region Purpose
// Proves one CommandResult can execute concurrently, and that CommandMock scopes stay on their async flow.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Shared-instance tests use a deterministic command. Per-call environment variables cannot differ on one instance.
// LastOutput is the last writer after the calls finish. Each returned CommandOutput is the caller's result.
// Pipe tests build distinct downstreams from one source and run them together with a source capture.
// Mock isolation enables CommandMock only after the first await so each flow has its own AsyncLocal copy.
// Linux commands are skipped on other operating systems. Jaribu has no runtime skip signal; the method returns.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace CommandResult_
{
  [TestTag("Core")]
  public class Concurrency_Given_
  {
    private const int CaptureCount = 32;
    private const int PipeCopies = 4;
    private const int TimeoutCopies = 8;

    [ModuleInitializer]
    internal static void Register() => RegisterTests<Concurrency_Given_>();

    public static async Task SharedCapture_Should_ReturnIdenticalCompleteOutput()
    {
      if (!RequireLinux())
      {
        return;
      }

      CommandResult command = Shell.Builder("echo").WithArguments("concurrent-line").Build();
      var tasks = new Task<CommandOutput>[CaptureCount];
      for (int index = 0; index < CaptureCount; index++)
      {
        tasks[index] = command.CaptureAsync();
      }

      CommandOutput[] outputs = await Task.WhenAll(tasks);

      foreach (CommandOutput output in outputs)
      {
        output.ExitCode.ShouldBe(0);
        output.Success.ShouldBeTrue();
        output.Stdout.Trim().ShouldBe("concurrent-line");
        output.Stderr.ShouldBeEmpty();
        output.GetLines().ShouldBe(["concurrent-line"]);
      }

      command.LastOutput.ShouldNotBeNull();
      outputs.ShouldContain(command.LastOutput);
    }

    public static async Task ConcurrentPipes_Should_IsolateDownstreamResults()
    {
      if (!RequireLinux())
      {
        return;
      }

      CommandResult source = Shell.Builder("printf")
        .WithArguments("%s\\n%s\\n%s\\n", "alpha", "beta", "gamma")
        .Build();

      (string Filter, string Expected, int ExitCode)[] cases =
      [
        ("alpha", "alpha", 0),
        ("beta", "beta", 0),
        ("gamma", "gamma", 0),
        ("nomatch", "", 1)
      ];

      List<Task> pipes = [];
      foreach ((string filter, string expected, int exitCode) in cases)
      {
        for (int copy = 0; copy < PipeCopies; copy++)
        {
          pipes.Add(AssertPipeAsync(source, filter, expected, exitCode));
        }
      }

      Task<CommandOutput> sourceCapture = source.CaptureAsync();
      await Task.WhenAll(pipes.Append(sourceCapture));

      CommandOutput sourceOutput = await sourceCapture;
      sourceOutput.ExitCode.ShouldBe(0);
      sourceOutput.GetLines().ShouldBe(["alpha", "beta", "gamma"]);
    }

    public static async Task MixedModes_Should_CompleteWithEachModesOutput()
    {
      if (!RequireLinux())
      {
        return;
      }

      CommandResult command = Shell.Builder("echo").WithArguments("mixed-mode").Build();
      Task<CommandOutput> capture = command.CaptureAsync();
      Task<int> run = command.RunAsync();
      Task<List<string>> stream = ReadStdoutAsync(command);

      await Task.WhenAll(capture, run, stream);

      CommandOutput captured = await capture;
      int exitCode = await run;
      List<string> lines = await stream;

      captured.ExitCode.ShouldBe(0);
      captured.Success.ShouldBeTrue();
      captured.Stdout.Trim().ShouldBe("mixed-mode");
      exitCode.ShouldBe(0);
      lines.ShouldBe(["mixed-mode"]);
    }

    [Timeout(20000)]
    public static async Task ConcurrentTimeouts_Should_ReturnEachCallersOutput()
    {
      if (!RequireLinux())
      {
        return;
      }

      CommandResult slow = Shell.Builder("sleep")
        .WithArguments("30")
        .WithTimeout(TimeSpan.FromMilliseconds(200))
        .WithTimeoutGracePeriod(TimeSpan.Zero)
        .Build();
      CommandResult fast = Shell.Builder("echo")
        .WithArguments("ready")
        .WithTimeout(TimeSpan.FromSeconds(5))
        .Build();

      var slowTasks = new Task<CommandOutput>[TimeoutCopies];
      var fastTasks = new Task<CommandOutput>[TimeoutCopies];
      for (int index = 0; index < TimeoutCopies; index++)
      {
        slowTasks[index] = slow.CaptureAsync();
        fastTasks[index] = fast.CaptureAsync();
      }

      CommandOutput[] slowOutputs = await Task.WhenAll(slowTasks);
      CommandOutput[] fastOutputs = await Task.WhenAll(fastTasks);

      foreach (CommandOutput output in slowOutputs)
      {
        output.TimedOut.ShouldBeTrue();
        output.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
        output.Success.ShouldBeFalse();
      }

      foreach (CommandOutput output in fastOutputs)
      {
        output.TimedOut.ShouldBeFalse();
        output.ExitCode.ShouldBe(0);
        output.Success.ShouldBeTrue();
        output.Stdout.Trim().ShouldBe("ready");
      }

      slow.LastOutput.ShouldNotBeNull();
      slowOutputs.ShouldContain(slow.LastOutput);
      fast.LastOutput.ShouldNotBeNull();
      fastOutputs.ShouldContain(fast.LastOutput);
    }

    public static async Task CommandMockParallelFlows_Should_IsolateAsyncLocalSetups()
    {
      string[] markers = await Task.WhenAll(MockFlowAsync("alpha"), MockFlowAsync("beta"));

      markers.ShouldContain("alpha");
      markers.ShouldContain("beta");
      markers[0].ShouldNotBe(markers[1]);
    }

    private static async Task AssertPipeAsync(
      CommandResult source,
      string filter,
      string expected,
      int expectedExit)
    {
      CommandOutput output = await source.Pipe("grep", "-x", filter).CaptureAsync();
      output.Stdout.Trim().ShouldBe(expected);
      output.ExitCode.ShouldBe(expectedExit);
    }

    private static async Task<List<string>> ReadStdoutAsync(CommandResult command)
    {
      List<string> lines = [];
      await foreach (string line in command.StreamStdoutAsync())
      {
        lines.Add(line);
      }

      return lines;
    }

    private static async Task<string> MockFlowAsync(string marker)
    {
      await Task.Yield();
      using (CommandMock.Enable())
      {
        CommandMock.Setup("tool", "run").Returns(marker);
        await Task.Delay(TimeSpan.FromMilliseconds(50));
        CommandOutput output = await Shell.Builder("tool").WithArguments("run").CaptureAsync();
        CommandMock.CallCount("tool", "run").ShouldBe(1);
        return output.Stdout.Trim();
      }
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
