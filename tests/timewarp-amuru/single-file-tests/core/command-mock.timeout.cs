#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for CommandMock timeout simulation and its parity with a timed-out CommandOutput.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// TimesOut and a delay that meets the command timeout both produce exit 124 and TimedOut.
// Caller cancellation stays OperationCanceledException. TtyPassthroughAsync does not throw.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace CommandMock_
{
  [TestTag("Core")]
  public class TimesOut_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<TimesOut_Given_>();

    public static async Task TimesOut_Should_MatchTheRealResultShape()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "status")
          .Returns("partial output")
          .TimesOut();
        CommandResult command = Shell.Builder("git")
          .WithArguments("status")
          .WithTimeout(TimeSpan.FromSeconds(2))
          .Build();

        CommandOutput output = await command.CaptureAsync();

        output.TimedOut.ShouldBeTrue();
        output.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
        output.Success.ShouldBeFalse();
        output.Stdout.ShouldContain("partial output");
        output.RunTime.ShouldBe(TimeSpan.Zero);
        command.LastOutput.ShouldBeSameAs(output);
      }
    }

    public static async Task NoCommandTimeout_Should_DescribeTimeoutWithoutADuration()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "push").TimesOut();

        TimeoutException exception = await Should.ThrowAsync<TimeoutException>(async () =>
          await Shell.Builder("git")
            .WithArguments("push")
            .WithZeroExitCodeValidation()
            .CaptureAsync());

        exception.Message.ShouldBe("Command timed out and was terminated.");
      }
    }

    public static async Task ConfiguredTimeout_Should_NameTheDuration()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "fetch").TimesOut();

        TimeoutException exception = await Should.ThrowAsync<TimeoutException>(async () =>
          await Shell.Builder("git")
            .WithArguments("fetch")
            .WithTimeout(TimeSpan.FromSeconds(1))
            .WithZeroExitCodeValidation()
            .CaptureAsync());

        exception.Message.ShouldBe("Command timed out after 1 second and was terminated.");
      }
    }

    public static async Task DelayBeyondTimeout_Should_TimeOut()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "fetch")
          .Delays(TimeSpan.FromSeconds(5))
          .Returns("nope");

        var stopwatch = Stopwatch.StartNew();
        CommandOutput output = await Shell.Builder("git")
          .WithArguments("fetch")
          .WithTimeout(TimeSpan.FromMilliseconds(200))
          .CaptureAsync();
        stopwatch.Stop();

        output.TimedOut.ShouldBeTrue();
        output.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(2));
      }
    }

    public static async Task DelayInsideTimeout_Should_ReturnTheMockResult()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "diff")
          .Delays(TimeSpan.FromMilliseconds(50))
          .Returns("diff-text");

        CommandOutput output = await Shell.Builder("git")
          .WithArguments("diff")
          .WithTimeout(TimeSpan.FromSeconds(2))
          .CaptureAsync();

        output.TimedOut.ShouldBeFalse();
        output.ExitCode.ShouldBe(0);
        output.Stdout.ShouldContain("diff-text");
      }
    }

    public static async Task CallerCancelDuringDelay_Should_NotReportTimeout()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "log")
          .Delays(TimeSpan.FromSeconds(5))
          .Returns("log");
        CommandResult command = Shell.Builder("git")
          .WithArguments("log")
          .WithTimeout(TimeSpan.FromSeconds(30))
          .Build();
        using CancellationTokenSource source = new(TimeSpan.FromMilliseconds(100));

        await Should.ThrowAsync<OperationCanceledException>(async () => await command.CaptureAsync(source.Token));

        command.LastOutput.ShouldBeNull();
      }
    }

    public static async Task TimesOut_Should_WinOverThrows()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "rebase")
          .Throws(new InvalidOperationException("boom"))
          .TimesOut();

        CommandOutput output = await Shell.Builder("git").WithArguments("rebase").CaptureAsync();

        output.TimedOut.ShouldBeTrue();
        output.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
      }
    }

    public static async Task Tty_Should_NotThrowWhenStrict()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("vim", "file").TimesOut();

        CommandOutput output = await Shell.Builder("vim")
          .WithArguments("file")
          .WithZeroExitCodeValidation()
          .Build()
          .TtyPassthroughAsync();

        output.TimedOut.ShouldBeTrue();
        output.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
      }
    }

    public static async Task Stream_Should_YieldLinesThenRecordTimeout()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "show")
          .Returns("line-a\nline-b")
          .TimesOut();
        CommandResult command = Shell.Builder("git").WithArguments("show").Build();
        List<string> lines = [];

        await foreach (string line in command.StreamStdoutAsync())
        {
          lines.Add(line);
        }

        lines.ShouldBe(["line-a", "line-b"]);
        command.LastOutput.ShouldNotBeNull();
        command.LastOutput.TimedOut.ShouldBeTrue();
        command.LastOutput.ExitCode.ShouldBe(CommandResult.TimeoutExitCode);
      }
    }

    public static async Task StrictStream_Should_ThrowAfterTheLines()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "show")
          .Returns("kept")
          .TimesOut();
        CommandResult command = Shell.Builder("git")
          .WithArguments("show")
          .WithZeroExitCodeValidation()
          .Build();
        List<string> lines = [];

        TimeoutException exception = await Should.ThrowAsync<TimeoutException>(async () =>
        {
          await foreach (string line in command.StreamStdoutAsync())
          {
            lines.Add(line);
          }
        });

        lines.ShouldBe(["kept"]);
        exception.Message.ShouldBe("Command timed out and was terminated.");
        command.LastOutput.ShouldNotBeNull();
        command.LastOutput.TimedOut.ShouldBeTrue();
      }
    }

    public static async Task Run_Should_Return124()
    {
      using (CommandMock.Enable())
      {
        CommandMock.Setup("git", "gc").Returns("cleaning").TimesOut();

        int exitCode = await Shell.Builder("git").WithArguments("gc").RunAsync();

        exitCode.ShouldBe(CommandResult.TimeoutExitCode);
      }
    }
  }
}
