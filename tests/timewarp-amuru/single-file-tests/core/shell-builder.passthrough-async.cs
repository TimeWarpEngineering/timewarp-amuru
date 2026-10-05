#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for ShellBuilder.PassthroughAsync() - validates interactive passthrough execution
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// PassthroughAsync sends stdout and stderr to the console.
// Configured standard input is kept; console stdin is used only when none was configured.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class PassthroughAsync_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<PassthroughAsync_Given_>();

    public static async Task EchoCommand_Should_ReturnZeroExitCode()
    {
      CommandOutput result = await Shell.Builder("echo")
        .WithArguments("Hello from interactive mode")
        .PassthroughAsync();

      result.ExitCode.ShouldBe(0);
      result.Stdout.ShouldBeNullOrEmpty();
    }

    public static async Task EmptyCommand_Should_ReportNeverRanFailure()
    {
      CommandResult nullCommand = Shell.Builder("").Build();

      CommandOutput execResult = await nullCommand.PassthroughAsync();

      execResult.ExitCode.ShouldBe(CommandResult.NeverRanExitCode);
      execResult.Success.ShouldBeFalse();
    }

    public static async Task ConfiguredStandardInput_Should_ReachTheChild()
    {
      string directory = Directory.CreateTempSubdirectory("amuru-passthrough-").FullName;
      string outputPath = Path.Combine(directory, "sorted.txt");
      try
      {
        CommandOutput result = await Shell.Builder("sort")
          .WithArguments("-o", outputPath)
          .WithStandardInput("b\na\nc\n")
          .PassthroughAsync();

        result.ExitCode.ShouldBe(0);
        string[] lines = (await File.ReadAllTextAsync(outputPath))
          .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.ShouldBe(["a", "b", "c"]);
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task Pipeline_Should_FeedTheUpstreamStage()
    {
      string directory = Directory.CreateTempSubdirectory("amuru-passthrough-pipe-").FullName;
      string outputPath = Path.Combine(directory, "sorted.txt");
      try
      {
        CommandOutput result = await Shell.Builder("printf")
          .WithArguments("b\na\nc\n")
          .Pipe("sort", "-o", outputPath)
          .PassthroughAsync();

        result.ExitCode.ShouldBe(0);
        string[] lines = (await File.ReadAllTextAsync(outputPath))
          .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.ShouldBe(["a", "b", "c"]);
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task EmptyStandardInput_Should_SendImmediateEof()
    {
      string directory = Directory.CreateTempSubdirectory("amuru-passthrough-empty-").FullName;
      string outputPath = Path.Combine(directory, "sorted.txt");
      try
      {
        CommandOutput result = await Shell.Builder("sort")
          .WithArguments("-o", outputPath)
          .WithStandardInput("")
          .PassthroughAsync();

        result.ExitCode.ShouldBe(0);
        string written = await File.ReadAllTextAsync(outputPath);
        written.ShouldBeEmpty();
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }
  }
}
