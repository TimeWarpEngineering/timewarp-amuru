#!/usr/bin/dotnet --
#:package TimeWarp.Jaribu@1.0.0-beta.8
#:package TimeWarp.Amuru@1.0.0-beta.18

#if !JARIBU_MULTI
return await RunAllTests();
#endif

using TimeWarp.Amuru;
using TimeWarp.Amuru.Testing;
using Shouldly;
using System.Diagnostics;

namespace TimeWarp.Amuru.Tests;

[TestTag("Core")]
public class CommandResultInteractiveTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<CommandResultInteractiveTests>();

  public static async Task Should_get_string_interactive_async_with_mock_fzf()
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
    await Task.CompletedTask;
  }

  public static async Task Should_handle_pipeline_with_interactive_selection()
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
    await Task.CompletedTask;
  }

  public static async Task Should_execute_interactive_async()
  {
    // Test with a simple echo command (non-interactive but safe)
    ExecutionResult result = await Shell.Builder("echo")
      .WithArguments("Hello from interactive mode")
      .PassthroughAsync();

    result.ExitCode.ShouldBe(0);

    // Output strings should be empty since output went to console
    result.StandardOutput.ShouldBeNullOrEmpty();
    await Task.CompletedTask;
  }

  public static async Task Should_handle_interactive_methods_with_null_command()
  {
    // Test graceful degradation with empty command
    CommandResult nullCommand = Shell.Builder("").Build();

    string stringResult = await nullCommand.SelectAsync();
    stringResult.ShouldBeNullOrEmpty();

    ExecutionResult execResult = await nullCommand.PassthroughAsync();
    execResult.ExitCode.ShouldBe(0);
    await Task.CompletedTask;
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
