#!/usr/bin/dotnet --

#region Purpose
// Tests for ShellBuilder.RunAndCaptureAsync() - runtime, capture, and interior blank lines
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// RunAndCaptureAsync streams to the terminal and reconstructs CommandOutput from strings
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class RunAndCaptureAsync_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<RunAndCaptureAsync_Given_>();

    public static async Task SleepCommand_Should_CaptureRunTime()
    {
      CommandOutput output = await Shell.Builder("sleep")
        .WithArguments("0.1")
        .RunAndCaptureAsync();

      output.ExitCode.ShouldBe(0);
      output.RunTime.TotalMilliseconds.ShouldBeGreaterThan(50);
    }

    public static async Task InteriorEmptyLines_Should_BePreserved()
    {
      CommandOutput output = await Shell.Builder("printf")
        .WithArguments("line1\n\nline2\n\n")
        .RunAndCaptureAsync();
      string[] lines = output.GetLines();

      lines.Length.ShouldBe(3);
      lines[0].ShouldBe("line1");
      lines[1].ShouldBe("");
      lines[2].ShouldBe("line2");
    }

    public static async Task EchoCommand_Should_CaptureStdout()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("Hello World")
        .RunAndCaptureAsync();

      output.Stdout.Trim().ShouldBe("Hello World");
      output.Success.ShouldBeTrue();
    }
  }
}
