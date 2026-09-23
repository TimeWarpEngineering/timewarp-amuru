#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for ShellBuilder.SelectAsync() - validates interactive selection via pipeline
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SelectAsync captures stdout while rendering UI to stderr (for tools like fzf)
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class SelectAsync_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<SelectAsync_Given_>();

    public static async Task PipelineWithMockFzf_Should_ReturnFirstLine()
    {
      string mockFzfPath = await CreateMockFzf();

      try
      {
        CliConfiguration.SetCommandPath("fzf", mockFzfPath);

        string result = await Shell.Builder("echo")
          .WithArguments("red\ngreen\nblue")
          .Pipe("fzf", "--prompt", "Select color: ")
          .SelectAsync();

        result.ShouldBe("red");
      }
      finally
      {
        CliConfiguration.Reset();
        File.Delete(mockFzfPath);
      }
    }

    public static async Task EmptyCommand_Should_ReturnEmpty()
    {
      CommandResult nullCommand = Shell.Builder("").Build();

      string stringResult = await nullCommand.SelectAsync();

      stringResult.ShouldBeNullOrEmpty();
    }

    public static async Task NonZeroExitWithZeroExitCodeValidation_Should_Throw()
    {
      await Should.ThrowAsync<CliWrap.Exceptions.CommandExecutionException>(async () =>
        await Shell.Builder("ls")
          .WithArguments("/nonexistent/path/12345")
          .WithZeroExitCodeValidation()
          .SelectAsync()
      );
    }

    public static async Task NonZeroExitWithoutValidation_Should_ReturnStdout()
    {
      string result = await Shell.Builder("sh")
        .WithArguments("-c", "echo selected; exit 2")
        .SelectAsync();

      result.ShouldBe("selected");
    }

    private static async Task<string> CreateMockFzf()
    {
      string mockPath = Path.GetTempFileName();
      File.Delete(mockPath);
      mockPath = mockPath + ".sh";

      string mockScript = @"#!/bin/bash
# Mock FZF - just output the first line of input
head -n 1
";

      await File.WriteAllTextAsync(mockPath, mockScript);

      if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
      {
        await Shell.Builder("chmod")
          .WithArguments("+x", mockPath)
          .RunAsync();
      }

      return mockPath;
    }
  }
}
