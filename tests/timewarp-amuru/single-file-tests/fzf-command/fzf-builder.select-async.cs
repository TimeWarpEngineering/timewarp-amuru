#!/usr/bin/dotnet --

#region Purpose
// Tests for FzfBuilder.SelectAsync() - validates fzf interactive selection
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// FzfBuilder provides fluent API for fzf command, SelectAsync runs interactive selection
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace FzfBuilder_
{
  [TestTag("FzfCommand")]
  public class SelectAsync_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<SelectAsync_Given_>();

    public static async Task MockFzfWithOptions_Should_ReturnFirstOption()
    {
      string mockFzfPath = await CreateMockFzf();

      try
      {
        CliConfiguration.SetCommandPath("fzf", mockFzfPath);

        string result = await Fzf.Builder()
          .FromInput("option1", "option2", "option3")
          .WithPrompt("Select: ")
          .SelectAsync();

        result.ShouldBe("option1");
      }
      finally
      {
        CliConfiguration.Reset();
        File.Delete(mockFzfPath);
      }
    }

    private static async Task<string> CreateMockFzf()
    {
      string mockPath = Path.GetTempFileName();
      File.Delete(mockPath);
      mockPath += ".sh";

      const string mockScript = @"#!/bin/bash
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
