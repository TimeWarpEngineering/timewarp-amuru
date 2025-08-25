#!/usr/bin/dotnet run
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using TimeWarp.Amuru.Testing;
using CliWrap;
using static TimeWarp.Amuru.Test.Helpers.Asserts;
using static TimeWarp.Amuru.Test.Helpers.TestRunner;

await RunTests<NewApiTests>();

internal sealed class NewApiTests
{
  public static async Task TestRunAsyncPrintsToConsole()
  {
    // RunAsync should stream output directly to console
    // We can't easily test console output, but we can verify it doesn't throw
    int exitCode = await Shell.Builder("echo").WithArguments("Hello World").RunAsync();
    
    AssertTrue(
      exitCode == 0,
      $"RunAsync should return 0 for successful command, got {exitCode}"
    );
  }

  public static async Task TestCaptureAsyncReturnsCommandOutput()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("Hello World").CaptureAsync();
    
    AssertTrue(
      output.Stdout.Trim() == "Hello World",
      $"CaptureAsync should capture stdout, got: '{output.Stdout.Trim()}'"
    );
    
    AssertTrue(
      output.Success,
      "Echo command should succeed"
    );
  }

  public static async Task TestCaptureAsyncSeparatesStdoutAndStderr()
  {
    // Use sh to write to both stdout and stderr
    CommandOutput output = await Shell.Builder("sh")
      .WithArguments("-c", "echo 'stdout text' && echo 'stderr text' >&2")
      .CaptureAsync();
    
    AssertTrue(
      output.Stdout.Contains("stdout text", StringComparison.Ordinal),
      $"Stdout should contain 'stdout text', got: '{output.Stdout}'"
    );
    
    AssertTrue(
      output.Stderr.Contains("stderr text", StringComparison.Ordinal),
      $"Stderr should contain 'stderr text', got: '{output.Stderr}'"
    );
  }

  public static async Task TestCaptureAsyncCombinedOutput()
  {
    CommandOutput output = await Shell.Builder("sh")
      .WithArguments("-c", "echo 'first' && echo 'error' >&2 && echo 'second'")
      .CaptureAsync();
    
    AssertTrue(
      output.Combined.Contains("first", StringComparison.Ordinal) && 
      output.Combined.Contains("error", StringComparison.Ordinal) && 
      output.Combined.Contains("second", StringComparison.Ordinal),
      $"Combined should contain all output in order, got: '{output.Combined}'"
    );
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
    
    AssertTrue(
      lines.Count >= 3,
      $"Should have at least 3 output lines, got {lines.Count}"
    );
    
    AssertTrue(
      lines.Any(l => l.Text.Contains("line1", StringComparison.Ordinal) && !l.IsError),
      "Should have stdout line1"
    );
    
    AssertTrue(
      lines.Any(l => l.Text.Contains("error", StringComparison.Ordinal) && l.IsError),
      "Should have stderr error line"
    );
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
    
    AssertTrue(
      lines.Count == 2,
      $"Should have 2 stdout lines, got {lines.Count}"
    );
    
    AssertTrue(
      lines[0].Contains("stdout1", StringComparison.Ordinal) && lines[1].Contains("stdout2", StringComparison.Ordinal),
      $"Should only have stdout lines, got: [{string.Join(", ", lines)}]"
    );
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
    
    AssertTrue(
      lines.Count == 2,
      $"Should have 2 stderr lines, got {lines.Count}"
    );
    
    AssertTrue(
      lines[0].Contains("stderr1", StringComparison.Ordinal) && lines[1].Contains("stderr2", StringComparison.Ordinal),
      $"Should only have stderr lines, got: [{string.Join(", ", lines)}]"
    );
  }

  public static async Task TestPassthroughAsyncExecutesInteractively()
  {
    // PassthroughAsync connects stdin/stdout/stderr to console
    // We can't test interactive behavior easily, just verify it doesn't throw
    ExecutionResult result = await Shell.Builder("echo")
      .WithArguments("test")
      .PassthroughAsync();
    
    AssertTrue(
      result.ExitCode == 0,
      $"PassthroughAsync should succeed, got exit code {result.ExitCode}"
    );
  }

  public static async Task TestSelectAsyncReturnsSelection()
  {
    // SelectAsync would normally show interactive UI
    // For testing, we'll use echo as it doesn't require interaction
    string result = await Shell.Builder("echo")
      .WithArguments("selected item")
      .SelectAsync();
    
    AssertTrue(
      result.Trim() == "selected item",
      $"SelectAsync should return output, got: '{result.Trim()}'"
    );
  }

  public static async Task TestCaptureAsyncWithExitCode()
  {
    // Use false command which always returns exit code 1
    CommandOutput output = await Shell.Builder("sh")
      .WithArguments("-c", "exit 42")
      .WithNoValidation()
      .CaptureAsync();
    
    AssertTrue(
      output.ExitCode == 42,
      $"Should capture exit code 42, got {output.ExitCode}"
    );
    
    AssertFalse(
      output.Success,
      "Command with non-zero exit code should not be successful"
    );
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
    
    AssertTrue(
      lines.Count >= 2 && lines.Count < 5,
      $"Should have cancelled after ~2 lines, got {lines.Count}"
    );
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
    AssertTrue(
      ReferenceEquals(stdout1, stdout2),
      "Stdout should be computed once and cached"
    );
    
    AssertTrue(
      ReferenceEquals(combined1, combined2),
      "Combined should be computed once and cached"
    );
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
      
      AssertTrue(
        output.Stdout.Contains("On branch main", StringComparison.Ordinal),
        $"Mock should return configured output, got: '{output.Stdout}'"
      );
      
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
      
      AssertTrue(
        output.Stderr.Contains("Permission denied", StringComparison.Ordinal),
        $"Mock should return error output, got: '{output.Stderr}'"
      );
      
      AssertTrue(
        output.ExitCode == 128,
        $"Mock should return exit code 128, got {output.ExitCode}"
      );
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
        
      AssertTrue(
        result1.Stdout.Trim() == "mocked1",
        $"First mock should work, got: '{result1.Stdout.Trim()}'"
      );
    }
    
    // Second mock scope - should be isolated
    using (CommandMock.Enable())
    {
      CommandMock.Setup("echo", "test2")
        .Returns("mocked2");
        
      CommandOutput result2 = await Shell.Builder("echo")
        .WithArguments("test2")
        .CaptureAsync();
        
      AssertTrue(
        result2.Stdout.Trim() == "mocked2",
        $"Second mock should work independently, got: '{result2.Stdout.Trim()}'"
      );
      
      // test1 should not be mocked in this scope
      AssertTrue(
        CommandMock.CallCount("echo", "test1") == 0,
        "First mock should not affect second scope"
      );
    }
  }
}