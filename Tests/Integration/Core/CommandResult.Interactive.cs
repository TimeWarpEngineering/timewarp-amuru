#!/usr/bin/dotnet run
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using TimeWarp.Amuru.Testing;
using Shouldly;
using static TimeWarp.Amuru.Test.Helpers.TestRunner;

// MANUAL TEST: This test requires interactive terminal and cannot run in CI
// Run manually to verify interactive functionality works correctly

await RunTests<CommandResultInteractiveTests>();

internal sealed class CommandResultInteractiveTests
{
  public static async Task TestGetStringInteractiveAsyncWithMockFzf()
  {
    // Since we can't run interactive commands in CI/automated tests,
    // we'll use a mock that simulates FZF behavior
    string mockFzfPath = CreateMockFzf();

    try
    {
      // Configure to use our mock
      CliConfiguration.SetCommandPath("fzf", mockFzfPath);

      // Test simple selection
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

  public static async Task TestPipelineWithInteractiveSelection()
  {
    string mockFzfPath = CreateMockFzf();

    try
    {
      CliConfiguration.SetCommandPath("fzf", mockFzfPath);

      // Test pipeline: echo | fzf
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

  public static async Task TestExecuteInteractiveAsync()
  {
    // Test with a simple echo command (non-interactive but safe)
    ExecutionResult result = await Shell.Builder("echo")
      .WithArguments("Hello from interactive mode")
      .PassthroughAsync();

    result.ExitCode.ShouldBe(0);

    // Output strings should be empty since output went to console
    result.StandardOutput.ShouldBeNullOrEmpty();
  }

  public static async Task TestInteractiveMethodsWithNullCommand()
  {
    // Test graceful degradation with empty command
    CommandResult nullCommand = Shell.Builder("").Build();

    string stringResult = await nullCommand.SelectAsync();
    stringResult.ShouldBeNullOrEmpty();

    ExecutionResult execResult = await nullCommand.PassthroughAsync();
    execResult.ExitCode.ShouldBe(0);
  }

  private static string CreateMockFzf()
  {
    // Create a simple script that acts like FZF but just returns the first line
    string mockPath = Path.GetTempFileName();
    File.Delete(mockPath); // Delete the file so we can recreate it with .sh extension
    mockPath = mockPath + ".sh";

    string mockScript = @"#!/bin/bash
# Mock FZF - just output the first line of input
head -n 1
";

    File.WriteAllText(mockPath, mockScript);

    // Make it executable
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