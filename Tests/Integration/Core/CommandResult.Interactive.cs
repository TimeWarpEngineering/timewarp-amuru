#!/usr/bin/dotnet run

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
      
      AssertTrue(
        result == "option1",
        "Mock FZF should return first option"
      );
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
      
      AssertTrue(
        result == "red",
        "Pipeline with mock FZF should return first line"
      );
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
    
    AssertTrue(
      result.ExitCode == 0,
      "Echo command should succeed"
    );
    
    // Output strings should be empty since output went to console
    AssertTrue(
      string.IsNullOrEmpty(result.StandardOutput),
      "PassthroughAsync should not capture stdout"
    );
  }
  
  public static async Task TestInteractiveMethodsWithNullCommand()
  {
    // Test graceful degradation with empty command
    CommandResult nullCommand = Shell.Builder("").Build();
    
    string stringResult = await nullCommand.SelectAsync();
    AssertTrue(
      string.IsNullOrEmpty(stringResult),
      "SelectAsync with null command should return empty string"
    );
    
    ExecutionResult execResult = await nullCommand.PassthroughAsync();
    AssertTrue(
      execResult.ExitCode == 0,
      "PassthroughAsync with null command should return exit code 0"
    );
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