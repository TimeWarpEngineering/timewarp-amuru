#!/usr/bin/dotnet --
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using Shouldly;
using static TimeWarp.Amuru.Test.Helpers.TestRunner;

// Tests for TtyPassthroughAsync method which provides true TTY passthrough
// for TUI applications like vim, nano, edit, etc.

await RunTests<TtyPassthroughTests>();

internal sealed class TtyPassthroughTests
{
  public static async Task TestTtyPassthroughAsync_WithEcho_ReturnsExitCode()
  {
    // Basic test - echo command should work with TTY passthrough
    ExecutionResult result = await Shell.Builder("echo")
      .WithArguments("Hello TTY")
      .TtyPassthroughAsync();
    
    result.ExitCode.ShouldBe(0);
    result.IsSuccess.ShouldBeTrue();
    // Output is empty because streams are not captured
    result.StandardOutput.ShouldBeNullOrEmpty();
  }
  
  public static async Task TestTtyPassthroughAsync_WithNullCommand_ReturnsSuccess()
  {
    // Test graceful degradation with empty command
    CommandResult nullCommand = Shell.Builder("").Build();
    
    ExecutionResult result = await nullCommand.TtyPassthroughAsync();
    result.ExitCode.ShouldBe(0);
  }
  
  public static async Task TestTtyPassthroughAsync_WithWorkingDirectory()
  {
    string tempDir = Path.GetTempPath();
    
    ExecutionResult result = await Shell.Builder("pwd")
      .WithWorkingDirectory(tempDir)
      .TtyPassthroughAsync();
    
    result.ExitCode.ShouldBe(0);
  }
  
  public static async Task TestTtyPassthroughAsync_WithEnvironmentVariable()
  {
    ExecutionResult result = await Shell.Builder("sh")
      .WithArguments("-c", "exit ${MY_EXIT_CODE:-1}")
      .WithEnvironmentVariable("MY_EXIT_CODE", "42")
      .TtyPassthroughAsync();
    
    result.ExitCode.ShouldBe(42);
  }
  
  public static async Task TestTtyPassthroughAsync_WithCancellation()
  {
    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
    
    try
    {
      await Shell.Builder("sleep")
        .WithArguments("10")
        .TtyPassthroughAsync(cts.Token);
      
      // Should not reach here
      throw new InvalidOperationException("Expected cancellation");
    }
    catch (OperationCanceledException)
    {
      // Expected - cancellation worked
    }
  }
  
  public static async Task TestTtyPassthroughAsync_WithFailingCommand_ReturnsNonZeroExitCode()
  {
    ExecutionResult result = await Shell.Builder("sh")
      .WithArguments("-c", "exit 5")
      .TtyPassthroughAsync();
    
    result.ExitCode.ShouldBe(5);
    result.IsSuccess.ShouldBeFalse();
  }
  
  public static async Task TestTtyPassthroughAsync_CapturesRunTime()
  {
    ExecutionResult result = await Shell.Builder("sleep")
      .WithArguments("0.1")
      .TtyPassthroughAsync();
    
    result.ExitCode.ShouldBe(0);
    result.RunTime.TotalMilliseconds.ShouldBeGreaterThan(50);
  }
  
  public static async Task TestTtyPassthroughAsync_RunBuilder_Works()
  {
    // Test using RunBuilder directly
    ExecutionResult result = await Shell.Builder("echo")
      .WithArguments("Hello")
      .TtyPassthroughAsync();
    
    result.ExitCode.ShouldBe(0);
  }
  
  public static async Task TestTtyPassthroughAsync_DotNetBuild_Works()
  {
    // Test using DotNet.Build() builder to verify the method exists and can be called
    // Use a non-existent project which will fail but still tests the API
    ExecutionResult result = await DotNet.Build()
      .WithNoValidation()
      .TtyPassthroughAsync();
    
    // This may fail (no project) but we're just testing the method exists
    // Exit code doesn't matter - just that the method is callable
    _ = result.ExitCode;
  }
}
