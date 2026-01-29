#!/usr/bin/dotnet --

#region Purpose
// Tests for ShellBuilder.SelectAsync() - validates interactive selection via pipeline
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SelectAsync captures stdout while rendering UI to stderr (for tools like fzf)
#endregion

using System.Diagnostics;

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
      string mockFzfPath = CreateMockFzf();

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

    private static string CreateMockFzf()
    {
      string mockPath = Path.GetTempFileName();
      File.Delete(mockPath);
      mockPath = mockPath + ".sh";

      string mockScript = @"#!/bin/bash
# Mock FZF - just output the first line of input
head -n 1
";

      File.WriteAllText(mockPath, mockScript);

      if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
      {
        using Process chmod = Process.Start(new ProcessStartInfo
        {
          FileName = "chmod",
          Arguments = $"+x \"{mockPath}\"",
          RedirectStandardOutput = true,
          RedirectStandardError = true
        })!;
        chmod.WaitForExit();
      }

      return mockPath;
    }
  }
}
