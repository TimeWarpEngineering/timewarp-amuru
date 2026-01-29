#!/usr/bin/dotnet --

#region Purpose
// Tests for ShellBuilder.Pipe() - validates command pipeline chaining
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Pipe chains commands, passing stdout of one to stdin of next
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class Pipe_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<Pipe_Given_>();

    public static async Task SinglePipe_Should_PassOutputToNextCommand()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("Hello\nWorld\nTest")
        .Build()
        .Pipe("grep", "World")
        .CaptureAsync();

      output.Stdout.Trim().ShouldBe("World");
    }

    public static async Task PipelineToCommandString_Should_ReturnLastCommand()
    {
      string command = Shell.Builder("echo")
        .WithArguments("Hello\nWorld\nTest")
        .Build()
        .Pipe("grep", "World")
        .ToCommandString();

      command.ShouldBe("grep World");
    }

    public static async Task MultiplePipes_Should_ChainCorrectly()
    {
      CommandOutput output = await Shell.Builder("cat")
        .WithStandardInput("apple\nbanana\ncherry\ndate")
        .Pipe("sort")
        .Pipe("head", "-2")
        .CaptureAsync();
      string[] result = output.GetLines();

      result.Length.ShouldBe(2);
      result[0].ShouldBe("apple");
      result[1].ShouldBe("banana");
    }

    public static async Task MultiStagePipe_Should_CountLines()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("line1\nline2\nline3\nline4")
        .Pipe("grep", "line")
        .Pipe("wc", "-l")
        .CaptureAsync();

      output.Stdout.Trim().ShouldBe("4");
    }

    public static async Task PipeWithGetLines_Should_FilterCorrectly()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("apple\nbanana\ncherry")
        .Pipe("grep", "a")
        .CaptureAsync();
      string[] lines = output.GetLines();

      lines.Length.ShouldBe(2);
      lines[0].ShouldBe("apple");
      lines[1].ShouldBe("banana");
    }

    public static async Task PipeWithRunAsync_Should_NotThrow()
    {
      await Shell.Builder("echo")
        .WithArguments("test")
        .Pipe("grep", "test")
        .RunAsync();

      true.ShouldBeTrue();
    }

    public static async Task FirstCommandFails_Should_Throw()
    {
      await Should.ThrowAsync<Exception>(async () =>
        await Shell.Builder("nonexistentcommand12345")
          .WithNoValidation()
          .Pipe("grep", "anything")
          .CaptureAsync()
      );
    }

    public static async Task SecondCommandFails_Should_Throw()
    {
      await Should.ThrowAsync<Exception>(async () =>
        await Shell.Builder("echo")
          .WithArguments("test")
          .WithNoValidation()
          .Pipe("nonexistentcommand12345")
          .CaptureAsync()
      );
    }

    public static async Task FindAndFilter_Should_ReturnCsFiles()
    {
      CommandOutput output = await Shell.Builder("find")
        .WithArguments(".", "-name", "*.cs", "-type", "f")
        .Pipe("head", "-5")
        .CaptureAsync();
      string[] files = output.GetLines();

      files.Length.ShouldBeLessThanOrEqualTo(5);
      files.All(f => f.EndsWith(".cs", StringComparison.Ordinal)).ShouldBeTrue();
    }

    public static async Task ComplexFourStagePipe_Should_CountCorrectly()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("The quick brown fox jumps over the lazy dog")
        .Pipe("tr", " ", "\n")
        .Pipe("grep", "o")
        .Pipe("wc", "-l")
        .CaptureAsync();

      output.Stdout.Trim().ShouldBe("4");
    }

    public static async Task PipeWithNoArguments_Should_Work()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("zebra\napple\nbanana")
        .Build()
        .Pipe("sort")
        .CaptureAsync();
      string[] lines = output.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries);

      lines.Length.ShouldBe(3);
      lines[0].Trim().ShouldBe("apple");
    }
  }
}
