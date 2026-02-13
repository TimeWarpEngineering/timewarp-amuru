#!/usr/bin/dotnet --

#region Purpose
// Tests for ShellBuilder.TtyPassthroughAsync() - validates TTY passthrough execution
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// TtyPassthroughAsync connects stdin/stdout/stderr directly to console (no capture)
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class TtyPassthroughAsync_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<TtyPassthroughAsync_Given_>();

    public static async Task EchoCommand_Should_ReturnZeroExitCode()
    {
      ExecutionResult result = await Shell.Builder("echo")
        .WithArguments("Hello TTY")
        .TtyPassthroughAsync();

      result.ExitCode.ShouldBe(0);
      result.IsSuccess.ShouldBeTrue();
      result.StandardOutput.ShouldBeNullOrEmpty();
    }

    public static async Task EmptyCommand_Should_ReturnZeroExitCode()
    {
      CommandResult nullCommand = Shell.Builder("").Build();

      ExecutionResult result = await nullCommand.TtyPassthroughAsync();

      result.ExitCode.ShouldBe(0);
    }

    public static async Task WorkingDirectory_Should_Work()
    {
      string tempDir = Path.GetTempPath();

      ExecutionResult result = await Shell.Builder("pwd")
        .WithWorkingDirectory(tempDir)
        .TtyPassthroughAsync();

      result.ExitCode.ShouldBe(0);
    }

    public static async Task EnvironmentVariable_Should_SetExitCode()
    {
      ExecutionResult result = await Shell.Builder("sh")
        .WithArguments("-c", "exit ${MY_EXIT_CODE:-1}")
        .WithEnvironmentVariable("MY_EXIT_CODE", "42")
        .TtyPassthroughAsync();

      result.ExitCode.ShouldBe(42);
    }

    public static async Task Cancellation_Should_CancelLongRunning()
    {
      using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

      try
      {
        await Shell.Builder("sleep")
          .WithArguments("10")
          .TtyPassthroughAsync(cts.Token);

        throw new InvalidOperationException("Expected cancellation");
      }
      catch (OperationCanceledException)
      {
        // Expected - cancellation worked
      }
    }

    public static async Task FailingCommand_Should_ReturnNonZeroExitCode()
    {
      ExecutionResult result = await Shell.Builder("sh")
        .WithArguments("-c", "exit 5")
        .TtyPassthroughAsync();

      result.ExitCode.ShouldBe(5);
      result.IsSuccess.ShouldBeFalse();
    }

    public static async Task SleepCommand_Should_CaptureRunTime()
    {
      ExecutionResult result = await Shell.Builder("sleep")
        .WithArguments("0.1")
        .TtyPassthroughAsync();

      result.ExitCode.ShouldBe(0);
      result.RunTime.TotalMilliseconds.ShouldBeGreaterThan(50);
    }

    public static async Task DotNetBuild_Should_BeCallable()
    {
      ExecutionResult result = await DotNet.Build()
        .WithNoValidation()
        .TtyPassthroughAsync();

      // This may fail (no project) but we're testing the API is callable
      _ = result.ExitCode;
    }
  }
}
