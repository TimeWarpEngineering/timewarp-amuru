#!/usr/bin/dotnet --

#region Purpose
// Tests for CommandOutput properties - validates Stdout, Stderr, Combined, ExitCode, Success
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// CommandOutput is the result from CaptureAsync, these test its property behavior
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace CommandOutput_
{
  [TestTag("Core")]
  public class Properties_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Properties_Given_>();

    public static async Task StdoutAndStderr_Should_BeSeparate()
    {
      CommandOutput output = await Shell.Builder("sh")
        .WithArguments("-c", "echo 'stdout text' && echo 'stderr text' >&2")
        .CaptureAsync();

      output.Stdout.ShouldContain("stdout text");
      output.Stderr.ShouldContain("stderr text");
    }

    public static async Task Combined_Should_ContainBoth()
    {
      CommandOutput output = await Shell.Builder("sh")
        .WithArguments("-c", "echo 'first' && echo 'error' >&2 && echo 'second'")
        .CaptureAsync();

      output.Combined.ShouldContain("first");
      output.Combined.ShouldContain("error");
      output.Combined.ShouldContain("second");
    }

    public static async Task ExitCode_Should_ReflectCommandResult()
    {
      CommandOutput output = await Shell.Builder("sh")
        .WithArguments("-c", "exit 42")
        .WithNoValidation()
        .CaptureAsync();

      output.ExitCode.ShouldBe(42);
      output.Success.ShouldBeFalse();
    }

    public static async Task LazyProperties_Should_ReturnSameInstance()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("test")
        .CaptureAsync();

      string stdout1 = output.Stdout;
      string stdout2 = output.Stdout;
      string combined1 = output.Combined;
      string combined2 = output.Combined;

      stdout1.ShouldBeSameAs(stdout2);
      combined1.ShouldBeSameAs(combined2);
    }
  }
}
