#!/usr/bin/dotnet run
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using TimeWarp.Amuru.Testing;
using CliWrap;
using Shouldly;
using static TimeWarp.Amuru.Test.Helpers.TestRunner;

await RunTests<NewApiTests>();

internal sealed class NewApiTests
{
  public static async Task TestRunAsyncPrintsToConsole()
  {
    // RunAsync should stream output directly to console
    // We can't easily test console output, but we can verify it doesn't throw
    int exitCode = await Shell.Builder("echo").WithArguments("Hello World").RunAsync();

    exitCode.ShouldBe(0);
  }

  public static async Task TestCaptureAsyncReturnsCommandOutput()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("Hello World").CaptureAsync();

    output.Stdout.Trim().ShouldBe("Hello World");
    output.Success.ShouldBeTrue();
  }

  public static async Task TestCaptureAsyncSeparatesStdoutAndStderr()
  {
    // Use sh to write to both stdout and stderr
    CommandOutput output = await Shell.Builder("sh")
      .WithArguments("-c", "echo 'stdout text' && echo 'stderr text' >&2")
      .CaptureAsync();

    output.Stdout.ShouldContain("stdout text");
    output.Stderr.ShouldContain("stderr text");
  }

  public static async Task TestCaptureAsyncCombinedOutput()
  {
    CommandOutput output = await Shell.Builder("sh")
      .WithArguments("-c", "echo 'first' && echo 'error' >&2 && echo 'second'")
      .CaptureAsync();

    output.Combined.ShouldContain("first");
    output.Combined.ShouldContain("error");
    output.Combined.ShouldContain("second");
  }

  public static async Task TestStreamAsyncYieldsOutputLines()
  {
    List<OutputLine> lines = new();

    await foreach (OutputLine line in Shell.Builder("sh")
      .WithArguments("-c", "echo 'line1' && echo 'line2' && echo 'error' >&2")
      .StreamCombinedAsync())
    {
      lines.Add(line);
    }

    lines.Count.ShouldBeGreaterThanOrEqualTo(3);
    lines.Any(l => l.Text.Contains("line1", StringComparison.Ordinal) && !l.IsError).ShouldBeTrue();
    lines.Any(l => l.Text.Contains("error", StringComparison.Ordinal) && l.IsError).ShouldBeTrue();
  }

  public static async Task TestStreamStdoutAsyncOnlyYieldsStdout()
  {
    List<string> lines = new();

    await foreach (string line in Shell.Builder("sh")
      .WithArguments("-c", "echo 'stdout1' && echo 'stderr1' >&2 && echo 'stdout2'")
      .StreamStdoutAsync())
    {
      lines.Add(line);
    }

    lines.Count.ShouldBe(2);
    lines[0].ShouldContain("stdout1");
    lines[1].ShouldContain("stdout2");
  }

  public static async Task TestStreamStderrAsyncOnlyYieldsStderr()
  {
    List<string> lines = new();

    await foreach (string line in Shell.Builder("sh")
      .WithArguments("-c", "echo 'stdout' && echo 'stderr1' >&2 && echo 'stderr2' >&2")
      .StreamStderrAsync())
    {
      lines.Add(line);
    }

    lines.Count.ShouldBe(2);
    lines[0].ShouldContain("stderr1");
    lines[1].ShouldContain("stderr2");
  }

  public static async Task TestPassthroughAsyncExecutesInteractively()
  {
    // PassthroughAsync connects stdin/stdout/stderr to console
    // We can't test interactive behavior easily, just verify it doesn't throw
    ExecutionResult result = await Shell.Builder("echo")
      .WithArguments("test")
      .PassthroughAsync();

    result.ExitCode.ShouldBe(0);
  }

  public static async Task TestSelectAsyncReturnsSelection()
  {
    // SelectAsync would normally show interactive UI
    // For testing, we'll use echo as it doesn't require interaction
    string result = await Shell.Builder("echo")
      .WithArguments("selected item")
      .SelectAsync();

    result.Trim().ShouldBe("selected item");
  }

  public static async Task TestCaptureAsyncWithExitCode()
  {
    // Use false command which always returns exit code 1
    CommandOutput output = await Shell.Builder("sh")
      .WithArguments("-c", "exit 42")
      .WithNoValidation()
      .CaptureAsync();

    output.ExitCode.ShouldBe(42);
    output.Success.ShouldBeFalse();
  }

  public static async Task TestStreamAsyncWithCancellation()
  {
    using CancellationTokenSource cts = new();
    List<OutputLine> lines = new();

    // Start a long-running command
    IAsyncEnumerable<OutputLine> stream = Shell.Builder("sh")
      .WithArguments("-c", "for i in 1 2 3 4 5; do echo $i; sleep 0.1; done")
      .StreamCombinedAsync();

    try
    {
      await foreach (OutputLine line in stream.WithCancellation(cts.Token))
      {
        lines.Add(line);
        if (lines.Count >= 2)
        {
          await cts.CancelAsync(); // Cancel after receiving 2 lines
        }
      }
    }
    catch (OperationCanceledException)
    {
      // Expected
    }

    lines.Count.ShouldBeGreaterThanOrEqualTo(2);
    lines.Count.ShouldBeLessThan(5);
  }

  public static async Task TestCommandOutputLazyProperties()
  {
    CommandOutput output = await Shell.Builder("echo")
      .WithArguments("test")
      .CaptureAsync();

    // Access properties multiple times
    string stdout1 = output.Stdout;
    string stdout2 = output.Stdout;
    string combined1 = output.Combined;
    string combined2 = output.Combined;

    // Should be same instance (computed once)
    stdout1.ShouldBeSameAs(stdout2);
    combined1.ShouldBeSameAs(combined2);
  }

  public static async Task TestCommandMockBasicUsage()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "status")
        .Returns("On branch main\nnothing to commit", "");

      CommandOutput output = await Shell.Builder("git")
        .WithArguments("status")
        .CaptureAsync();

      output.Stdout.ShouldContain("On branch main");
      CommandMock.VerifyCalled("git", "status");
    }
  }

  public static async Task TestCommandMockWithError()
  {
    using (CommandMock.Enable())
    {
      CommandMock.Setup("git", "push")
        .ReturnsError("remote: Permission denied", 128);

      CommandOutput output = await Shell.Builder("git")
        .WithArguments("push")
        .WithNoValidation()
        .CaptureAsync();

      output.Stderr.ShouldContain("Permission denied");
      output.ExitCode.ShouldBe(128);
    }
  }

  public static async Task TestCommandMockIsolation()
  {
    // First mock scope
    using (CommandMock.Enable())
    {
      CommandMock.Setup("echo", "test1")
        .Returns("mocked1");

      CommandOutput result1 = await Shell.Builder("echo")
        .WithArguments("test1")
        .CaptureAsync();

      result1.Stdout.Trim().ShouldBe("mocked1");
    }

    // Second mock scope - should be isolated
    using (CommandMock.Enable())
    {
      CommandMock.Setup("echo", "test2")
        .Returns("mocked2");

      CommandOutput result2 = await Shell.Builder("echo")
        .WithArguments("test2")
        .CaptureAsync();

      result2.Stdout.Trim().ShouldBe("mocked2");

      // test1 should not be mocked in this scope
      CommandMock.CallCount("echo", "test1").ShouldBe(0);
    }
  }
}