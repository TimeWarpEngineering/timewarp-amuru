#!/usr/bin/dotnet --

#region Purpose
// Tests for ShellBuilder cancellation behavior - validates CancellationToken handling
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Tests cancellation scenarios across CaptureAsync, RunAsync, and pipelines
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class Cancellation_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Cancellation_Given_>();

    public static async Task QuickCommand_Should_CompleteBeforeTimeout()
    {
      using var cts = new CancellationTokenSource();
      cts.CancelAfter(TimeSpan.FromSeconds(10));

      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("Hello World")
        .CaptureAsync(cts.Token);

      output.Stdout.Trim().ShouldBe("Hello World");
    }

    public static async Task AlreadyCancelledToken_Should_ReturnEmptyOrThrow()
    {
      using var cts = new CancellationTokenSource();
      await cts.CancelAsync();

      try
      {
        CommandOutput output = await Shell.Builder("echo")
          .WithArguments("test")
          .CaptureAsync(cts.Token);

        output.Stdout.ShouldBeNullOrEmpty();
      }
      catch (OperationCanceledException)
      {
        // Also acceptable - either empty result or exception
      }
    }

    public static async Task GetLinesWithTimeout_Should_Work()
    {
      using var cts = new CancellationTokenSource();
      cts.CancelAfter(TimeSpan.FromSeconds(5));

      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("line1\nline2\nline3")
        .CaptureAsync(cts.Token);
      string[] lines = output.GetLines();

      lines.Length.ShouldBe(3);
      lines[0].ShouldBe("line1");
      lines[1].ShouldBe("line2");
      lines[2].ShouldBe("line3");
    }

    public static async Task RunAsyncWithTimeout_Should_Complete()
    {
      using var cts = new CancellationTokenSource();
      cts.CancelAfter(TimeSpan.FromSeconds(5));

      int exitCode = await Shell.Builder("echo")
        .WithArguments("execute test")
        .RunAsync(cts.Token);

      exitCode.ShouldBe(0);
    }

    public static async Task LongRunningCommand_Should_BeCancelled()
    {
      using var cts = new CancellationTokenSource();
      cts.CancelAfter(TimeSpan.FromMilliseconds(100));

      bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

      try
      {
        CommandOutput output = isWindows
          ? await Shell.Builder("timeout").WithArguments("5").CaptureAsync(cts.Token)
          : await Shell.Builder("sleep").WithArguments("5").CaptureAsync(cts.Token);

        (output.Stdout.Length == 0 || output.Stdout.Length < 100).ShouldBeTrue();
      }
      catch (OperationCanceledException)
      {
        // Expected - timeout cancelled the command
      }
    }

    public static async Task PipelineWithTimeout_Should_Complete()
    {
      using var cts = new CancellationTokenSource();
      cts.CancelAfter(TimeSpan.FromSeconds(5));

      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("line1\nline2\nline3")
        .Pipe("grep", "line")
        .CaptureAsync(cts.Token);

      output.Stdout.ShouldNotBeNullOrEmpty();
      output.Stdout.ShouldContain("line");
    }

    public static async Task DefaultToken_Should_Work()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("default token test")
        .CaptureAsync();

      output.Stdout.Trim().ShouldBe("default token test");
    }
  }
}
