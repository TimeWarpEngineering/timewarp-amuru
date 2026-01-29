#!/usr/bin/dotnet --

#region Purpose
// Tests for ShellBuilder fluent API - validates command execution and output capture
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// - Namespace = SUT (ShellBuilder_)
// - Class = Action + Given (CaptureAsync_Given_)
// - Method = Given condition + Should + Result (EchoCommand_Should_CaptureStdout)
// Full test name: ShellBuilder_.CaptureAsync_Given_.EchoCommand_Should_CaptureStdout
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class CaptureAsync_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<CaptureAsync_Given_>();

    public static async Task EchoCommand_Should_CaptureStdout()
    {
      CommandOutput output = await Shell.Builder("echo").WithArguments("Hello World").CaptureAsync();

      output.Stdout.Trim().ShouldBe("Hello World");
    }

    public static async Task MultipleArgs_Should_PassAllArgs()
    {
      CommandOutput output = await Shell.Builder("echo").WithArguments("arg1", "arg2", "arg3").CaptureAsync();

      output.Stdout.Trim().ShouldBe("arg1 arg2 arg3");
    }

    public static async Task SuccessfulCommand_Should_SetSuccessTrue()
    {
      CommandOutput output = await Shell.Builder("echo").WithArguments("test").CaptureAsync();

      output.Success.ShouldBeTrue();
    }

    public static async Task DateCommand_Should_ReturnOutput()
    {
      CommandOutput output = await Shell.Builder("date").CaptureAsync();

      output.Stdout.Trim().ShouldNotBeNullOrEmpty();
    }

    public static async Task EnvironmentVariable_Should_SetEnvVar()
    {
      CommandOutput output = await Shell.Builder("printenv")
        .WithEnvironmentVariable("TEST_VAR", "test_value")
        .WithArguments("TEST_VAR")
        .CaptureAsync();

      output.Stdout.Trim().ShouldBe("test_value");
    }

    public static async Task WorkingDirectory_Should_ChangeDirectory()
    {
      string tempDir = Path.GetTempPath();
      CommandOutput output = await Shell.Builder("pwd")
        .WithWorkingDirectory(tempDir)
        .CaptureAsync();

      output.Stdout.Trim().ShouldBe(tempDir.TrimEnd('/'));
    }

    public static async Task FluentChaining_Should_CombineOptions()
    {
      CommandOutput output = await Shell.Builder("bash")
        .WithArguments("-c", "echo $TEST1 $TEST2")
        .WithEnvironmentVariable("TEST1", "Hello")
        .WithEnvironmentVariable("TEST2", "World")
        .WithNoValidation()
        .CaptureAsync();

      output.Stdout.Trim().ShouldBe("Hello World");
    }

    public static async Task StandardInput_Should_PassToCommand()
    {
      CommandOutput output = await Shell.Builder("grep")
        .WithArguments("World")
        .WithStandardInput("Hello World\nGoodbye Moon\nHello Universe")
        .CaptureAsync();

      output.Stdout.Trim().ShouldBe("Hello World");
    }

    public static async Task StandardInputLines_Should_CountCorrectly()
    {
      CommandOutput output = await Shell.Builder("wc")
        .WithArguments("-l")
        .WithStandardInput("Line 1\nLine 2\nLine 3\nLine 4\nLine 5\n")
        .CaptureAsync();

      output.Stdout.Trim().ShouldBe("5");
    }

    public static async Task EmptyStandardInput_Should_ReturnEmpty()
    {
      CommandOutput output = await Shell.Builder("cat")
        .WithStandardInput("")
        .CaptureAsync();

      output.Stdout.Length.ShouldBe(0);
    }

    public static async Task PrintfOutput_Should_SplitIntoLines()
    {
      CommandOutput output = await Shell.Builder("printf")
        .WithArguments("line1\\nline2\\nline3")
        .CaptureAsync();
      string[] lines = output.GetLines();

      lines.Length.ShouldBe(3);
      lines[0].ShouldBe("line1");
      lines[1].ShouldBe("line2");
      lines[2].ShouldBe("line3");
    }
  }
}
