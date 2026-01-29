#!/usr/bin/dotnet --

#region Purpose
// Tests for ShellBuilder.CaptureAsync() error handling - validates behavior with invalid commands and failures
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Tests error conditions: non-existent commands, non-zero exits, empty commands, special characters
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class CaptureAsync_Given_ErrorConditions_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<CaptureAsync_Given_ErrorConditions_>();

    public static async Task NonExistentCommand_Should_Throw()
    {
      await Should.ThrowAsync<Exception>(async () =>
        await Shell.Builder("nonexistentcommand12345").WithNoValidation().CaptureAsync()
      );
    }

    public static async Task NonZeroExitWithNoValidation_Should_ReturnEmptyStdout()
    {
      CommandOutput output = await Shell.Builder("ls")
        .WithArguments("/nonexistent/path/12345")
        .WithNoValidation()
        .CaptureAsync();

      output.Stdout.ShouldBeNullOrEmpty();
    }

    public static async Task NonZeroExit_Should_Throw()
    {
      await Should.ThrowAsync<Exception>(async () =>
        await Shell.Builder("ls").WithArguments("/nonexistent/path/12345").CaptureAsync()
      );
    }

    public static async Task FailedCommandWithNoValidation_Should_ReturnEmptyLines()
    {
      CommandOutput output = await Shell.Builder("ls")
        .WithArguments("/nonexistent/path/12345")
        .WithNoValidation()
        .CaptureAsync();
      string[] lines = output.GetStdoutLines();

      lines.Length.ShouldBe(0);
    }

    public static async Task SpecialCharacters_Should_PassThrough()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("Hello \"World\" with 'quotes' and $pecial chars!")
        .CaptureAsync();

      output.Stdout.ShouldNotBeNullOrEmpty();
    }

    public static async Task EmptyCommand_Should_ReturnEmpty()
    {
      CommandOutput output = await Shell.Builder("").CaptureAsync();

      output.Stdout.ShouldBeNullOrEmpty();
    }

    public static async Task WhitespaceCommand_Should_ReturnEmpty()
    {
      CommandOutput output = await Shell.Builder("   ").CaptureAsync();

      output.Stdout.ShouldBeNullOrEmpty();
    }

    public static async Task FailedCommandWithNoValidation_Should_SetSuccessFalse()
    {
      CommandOutput output = await Shell.Builder("ls")
        .WithArguments("/nonexistent/path/12345")
        .WithNoValidation()
        .CaptureAsync();

      output.Success.ShouldBeFalse();
    }
  }
}
