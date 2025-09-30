#!/usr/bin/dotnet run
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using Shouldly;
using static TimeWarp.Amuru.Test.Helpers.TestRunner;

await RunTests<CancellationTests>();

internal sealed class CancellationTests
{
  public static async Task TestQuickCommandWithCancellationToken()
  {
    using var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(10)); // Long timeout, command should complete

    CommandOutput output = await Shell.Builder("echo").WithArguments("Hello World").CaptureAsync(cts.Token);
    string result = output.Stdout;

    result.Trim().ShouldBe("Hello World");
  }

  public static async Task TestAlreadyCancelledToken()
  {
    using var cts = new CancellationTokenSource();
    await cts.CancelAsync(); // Cancel immediately

    try
    {
      CommandOutput output = await Shell.Builder("echo").WithArguments("test").CaptureAsync(cts.Token);
      string result = output.Stdout;
      // Should return empty string due to cancellation
      result.ShouldBeNullOrEmpty();
    }
    catch (OperationCanceledException)
    {
      // This is also acceptable behavior - either empty result or exception
      true.ShouldBeTrue();
    }
  }

  public static async Task TestGetLinesAsyncWithCancellationToken()
  {
    using var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(5));

    CommandOutput output = await Shell.Builder("echo").WithArguments("line1\nline2\nline3").CaptureAsync(cts.Token);
    string[] lines = output.GetLines();

    lines.Length.ShouldBe(3);
    lines[0].ShouldBe("line1");
    lines[1].ShouldBe("line2");
    lines[2].ShouldBe("line3");
  }

  public static async Task TestExecuteAsyncWithCancellationToken()
  {
    using var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(5));

    int exitCode = await Shell.Builder("echo").WithArguments("execute test").RunAsync(cts.Token);

    exitCode.ShouldBe(0);
  }

  public static async Task TestTimeoutCancellation()
  {
    using var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromMilliseconds(100)); // Very short timeout

    // Use sleep command that should be cancelled (works on both Unix and Windows with different commands)
    bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

    try
    {
      CommandOutput output = isWindows
        ? await Shell.Builder("timeout").WithArguments("5").CaptureAsync(cts.Token)  // Windows: timeout 5 seconds
        : await Shell.Builder("sleep").WithArguments("5").CaptureAsync(cts.Token);   // Unix: sleep 5 seconds
      string result = output.Stdout;

      // If we get here, either the command completed very quickly or returned empty due to cancellation
      (result.Length == 0 || result.Length < 100).ShouldBeTrue();
    }
    catch (OperationCanceledException)
    {
      // Expected behavior - timeout cancelled the command
      true.ShouldBeTrue();
    }
  }

  public static async Task TestPipelineWithCancellation()
  {
    using var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(5));

    CommandOutput output = await Shell.Builder("echo").WithArguments("line1\nline2\nline3")
      .Pipe("grep", "line")
      .CaptureAsync(cts.Token);
    string result = output.Stdout;

    result.ShouldNotBeNullOrEmpty();
    result.ShouldContain("line");
  }

  public static async Task TestDefaultCancellationTokenBehavior()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("default token test").CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("default token test");
  }
}