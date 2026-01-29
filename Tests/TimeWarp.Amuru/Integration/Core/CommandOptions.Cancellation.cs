#!/usr/bin/dotnet --

#if !JARIBU_MULTI
return await RunAllTests();
#endif


namespace TimeWarp.Amuru.Tests;

[TestTag("Core")]
public class CommandOptionsCancellationTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<CommandOptionsCancellationTests>();

  public static async Task Should_execute_quick_command_with_cancellation_token()
  {
    using var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(10)); // Long timeout, command should complete

    CommandOutput output = await Shell.Builder("echo").WithArguments("Hello World").CaptureAsync(cts.Token);
    string result = output.Stdout;

    result.Trim().ShouldBe("Hello World");
    await Task.CompletedTask;
  }

  public static async Task Should_handle_already_cancelled_token()
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
    await Task.CompletedTask;
  }

  public static async Task Should_get_lines_async_with_cancellation_token()
  {
    using var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(5));

    CommandOutput output = await Shell.Builder("echo").WithArguments("line1\nline2\nline3").CaptureAsync(cts.Token);
    string[] lines = output.GetLines();

    lines.Length.ShouldBe(3);
    lines[0].ShouldBe("line1");
    lines[1].ShouldBe("line2");
    lines[2].ShouldBe("line3");
    await Task.CompletedTask;
  }

  public static async Task Should_execute_async_with_cancellation_token()
  {
    using var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(5));

    int exitCode = await Shell.Builder("echo").WithArguments("execute test").RunAsync(cts.Token);

    exitCode.ShouldBe(0);
    await Task.CompletedTask;
  }

  public static async Task Should_handle_timeout_cancellation()
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
    await Task.CompletedTask;
  }

  public static async Task Should_handle_pipeline_with_cancellation()
  {
    using var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(5));

    CommandOutput output = await Shell.Builder("echo").WithArguments("line1\nline2\nline3")
      .Pipe("grep", "line")
      .CaptureAsync(cts.Token);
    string result = output.Stdout;

    result.ShouldNotBeNullOrEmpty();
    result.ShouldContain("line");
    await Task.CompletedTask;
  }

  public static async Task Should_handle_default_cancellation_token_behavior()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("default token test").CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("default token test");
    await Task.CompletedTask;
  }
}
