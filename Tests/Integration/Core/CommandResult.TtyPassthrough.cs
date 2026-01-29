#!/usr/bin/dotnet --
#:package TimeWarp.Jaribu
#:package TimeWarp.Amuru

#if !JARIBU_MULTI
return await RunAllTests();
#endif

using TimeWarp.Amuru;
using Shouldly;

namespace TimeWarp.Amuru.Tests;

[TestTag("Core")]
public class CommandResultTtyPassthroughTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<CommandResultTtyPassthroughTests>();

  public static async Task Should_execute_tty_passthrough_with_echo_and_return_exit_code()
  {
    // Basic test - echo command should work with TTY passthrough
    ExecutionResult result = await Shell.Builder("echo")
      .WithArguments("Hello TTY")
      .TtyPassthroughAsync();

    result.ExitCode.ShouldBe(0);
    result.IsSuccess.ShouldBeTrue();
    // Output is empty because streams are not captured
    result.StandardOutput.ShouldBeNullOrEmpty();
    await Task.CompletedTask;
  }

  public static async Task Should_handle_tty_passthrough_with_null_command()
  {
    // Test graceful degradation with empty command
    CommandResult nullCommand = Shell.Builder("").Build();

    ExecutionResult result = await nullCommand.TtyPassthroughAsync();
    result.ExitCode.ShouldBe(0);
    await Task.CompletedTask;
  }

  public static async Task Should_execute_tty_passthrough_with_working_directory()
  {
    string tempDir = Path.GetTempPath();

    ExecutionResult result = await Shell.Builder("pwd")
      .WithWorkingDirectory(tempDir)
      .TtyPassthroughAsync();

    result.ExitCode.ShouldBe(0);
    await Task.CompletedTask;
  }

  public static async Task Should_execute_tty_passthrough_with_environment_variable()
  {
    ExecutionResult result = await Shell.Builder("sh")
      .WithArguments("-c", "exit ${MY_EXIT_CODE:-1}")
      .WithEnvironmentVariable("MY_EXIT_CODE", "42")
      .TtyPassthroughAsync();

    result.ExitCode.ShouldBe(42);
    await Task.CompletedTask;
  }

  public static async Task Should_handle_tty_passthrough_with_cancellation()
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
    await Task.CompletedTask;
  }

  public static async Task Should_return_non_zero_exit_code_for_failing_command()
  {
    ExecutionResult result = await Shell.Builder("sh")
      .WithArguments("-c", "exit 5")
      .TtyPassthroughAsync();

    result.ExitCode.ShouldBe(5);
    result.IsSuccess.ShouldBeFalse();
    await Task.CompletedTask;
  }

  public static async Task Should_capture_run_time()
  {
    ExecutionResult result = await Shell.Builder("sleep")
      .WithArguments("0.1")
      .TtyPassthroughAsync();

    result.ExitCode.ShouldBe(0);
    result.RunTime.TotalMilliseconds.ShouldBeGreaterThan(50);
    await Task.CompletedTask;
  }

  public static async Task Should_execute_tty_passthrough_with_run_builder()
  {
    // Test using RunBuilder directly
    ExecutionResult result = await Shell.Builder("echo")
      .WithArguments("Hello")
      .TtyPassthroughAsync();

    result.ExitCode.ShouldBe(0);
    await Task.CompletedTask;
  }

  public static async Task Should_execute_tty_passthrough_with_dotnet_build()
  {
    // Test using DotNet.Build() builder to verify the method exists and can be called
    // Use a non-existent project which will fail but still tests the API
    ExecutionResult result = await DotNet.Build()
      .WithNoValidation()
      .TtyPassthroughAsync();

    // This may fail (no project) but we're just testing the method exists
    // Exit code doesn't matter - just that the method is callable
    _ = result.ExitCode;
    await Task.CompletedTask;
  }
}
