#!/usr/bin/dotnet --

#region Purpose
// Tests for CommandOutput.GetLines() - validates line splitting and empty line handling
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// CommandOutput is the result object from CaptureAsync(), GetLines() parses stdout into array
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace CommandOutput_
{
  [TestTag("Core")]
  public class GetLines_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<GetLines_Given_>();

    public static async Task MultilineOutput_Should_ContainAllLines()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("line1\nline2\nline3")
        .CaptureAsync();

      output.Stdout.ShouldContain("line1");
      output.Stdout.ShouldContain("line2");
      output.Stdout.ShouldContain("line3");
    }

    public static async Task MultilineOutput_Should_SplitCorrectly()
    {
      CommandOutput output = await Shell.Builder("printf")
        .WithArguments("line1\nline2\nline3")
        .CaptureAsync();
      string[] lines = output.GetLines();

      lines.Length.ShouldBe(3);
      lines[0].ShouldBe("line1");
      lines[1].ShouldBe("line2");
      lines[2].ShouldBe("line3");
    }

    public static async Task EmptyLines_Should_BeRemoved()
    {
      CommandOutput output = await Shell.Builder("printf")
        .WithArguments("line1\n\nline2\n\n")
        .CaptureAsync();
      string[] lines = output.GetLines();

      lines.Length.ShouldBe(2);
      lines[0].ShouldBe("line1");
      lines[1].ShouldBe("line2");
    }

    public static async Task EmptyOutput_Should_ReturnEmptyArray()
    {
      CommandOutput output = await Shell.Builder("echo")
        .WithArguments("")
        .CaptureAsync();
      string[] lines = output.GetLines();

      output.Stdout.Length.ShouldBeLessThanOrEqualTo(2);
      lines.Length.ShouldBe(0);
    }

    public static async Task LsCommand_Should_ReturnFiles()
    {
      CommandOutput output = await Shell.Builder("ls")
        .WithArguments("-1")
        .CaptureAsync();
      string[] files = output.GetLines();

      files.Length.ShouldBeGreaterThan(0);
    }
  }
}
