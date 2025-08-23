#!/usr/bin/dotnet run

await RunTests<RunBuilderTests>();

internal sealed class RunBuilderTests
{
  public static async Task TestBasicRunBuilderCommandString()
  {
    string command = Shell.Builder("echo")
      .WithArguments("Hello", "World")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "echo Hello World",
      $"Expected 'echo Hello World', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestBasicRunBuilder()
  {
    string result = await Shell.Builder("echo")
      .WithArguments("Hello", "World")
      .GetStringAsync();
    
    AssertTrue(
      result.Trim() == "Hello World",
      "RunBuilder should work with basic arguments"
    );
  }

  public static async Task TestRunBuilderWithMultipleWithArgumentsCommandString()
  {
    string command = Shell.Builder("echo")
      .WithArguments("arg1")
      .WithArguments("arg2", "arg3")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "echo arg1 arg2 arg3",
      $"Expected 'echo arg1 arg2 arg3', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestRunBuilderWithMultipleWithArguments()
  {
    string result = await Shell.Builder("echo")
      .WithArguments("arg1")
      .WithArguments("arg2", "arg3")
      .GetStringAsync();
    
    AssertTrue(
      result.Trim() == "arg1 arg2 arg3",
      $"Multiple WithArguments calls should accumulate, got '{result.Trim()}'"
    );
  }

  public static async Task TestRunBuilderWithEnvironmentVariableCommandString()
  {
    // Note: Environment variables don't appear in ToCommandString()
    string command = Shell.Builder("printenv")
      .WithEnvironmentVariable("TEST_VAR", "test_value")
      .WithArguments("TEST_VAR")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "printenv TEST_VAR",
      $"Expected 'printenv TEST_VAR', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestRunBuilderWithEnvironmentVariable()
  {
    string result = await Shell.Builder("printenv")
      .WithEnvironmentVariable("TEST_VAR", "test_value")
      .WithArguments("TEST_VAR")
      .GetStringAsync();
    
    AssertTrue(
      result.Trim() == "test_value",
      $"Environment variable should be set correctly, got '{result.Trim()}'"
    );
  }

  public static async Task TestRunBuilderWithNoValidation()
  {
    // This would normally throw because 'false' exits with code 1
    ExecutionResult result = await Shell.Builder("false")
      .WithNoValidation()
      .ExecuteAsync();
    
    AssertTrue(
      result.ExitCode == 1,
      "WithNoValidation should allow non-zero exit codes"
    );
  }

  public static async Task TestRunBuilderGetLinesAsync()
  {
    string[] lines = await Shell.Builder("printf")
      .WithArguments("line1\\nline2\\nline3")
      .GetLinesAsync();
    
    AssertTrue(
      lines.Length == 3,
      $"Should return 3 lines, got {lines.Length}"
    );
    
    AssertTrue(
      lines[0] == "line1" && lines[1] == "line2" && lines[2] == "line3",
      "Lines should match expected values"
    );
  }

  public static async Task TestRunBuilderWithWorkingDirectoryCommandString()
  {
    // Note: Working directory doesn't appear in ToCommandString()
    string command = Shell.Builder("pwd")
      .WithWorkingDirectory("/tmp")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "pwd ",
      $"Expected 'pwd ', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestRunBuilderWithWorkingDirectory()
  {
    string tempDir = Path.GetTempPath();
    string result = await Shell.Builder("pwd")
      .WithWorkingDirectory(tempDir)
      .GetStringAsync();
    
    AssertTrue(
      result.Trim() == tempDir.TrimEnd('/'),
      $"Working directory should be set to {tempDir}, got {result.Trim()}"
    );
  }

  public static async Task TestRunBuilderPipelineCommandString()
  {
    string command = Shell.Builder("echo")
      .WithArguments("Hello\nWorld\nTest")
      .Build()
      .Pipe("grep", "World")
      .ToCommandString();
    
    AssertTrue(
      command == "grep World",
      $"Expected 'grep World', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestRunBuilderPipeline()
  {
    string result = await Shell.Builder("echo")
      .WithArguments("Hello\nWorld\nTest")
      .Build()
      .Pipe("grep", "World")
      .GetStringAsync();
    
    AssertTrue(
      result.Trim() == "World",
      $"Pipeline should filter for 'World', got '{result.Trim()}'"
    );
  }

  public static async Task TestRunBuilderExecuteAsync()
  {
    ExecutionResult result = await Shell.Builder("echo")
      .WithArguments("test output")
      .ExecuteAsync();
    
    AssertTrue(
      result.IsSuccess,
      "Command should execute successfully"
    );
    
    AssertTrue(
      result.StandardOutput.Trim() == "test output",
      $"Output should match, got '{result.StandardOutput.Trim()}'"
    );
  }

  public static async Task TestRunBuilderChaining()
  {
    // Test that all methods can be chained fluently
    string result = await Shell.Builder("bash")
      .WithArguments("-c", "echo $TEST1 $TEST2")
      .WithEnvironmentVariable("TEST1", "Hello")
      .WithEnvironmentVariable("TEST2", "World")
      .WithNoValidation()
      .GetStringAsync();
    
    AssertTrue(
      result.Trim() == "Hello World",
      $"Chained configuration should work correctly, got '{result.Trim()}'"
    );
  }

  public static async Task TestRunBuilderWithStandardInput()
  {
    string result = await Shell.Builder("grep")
      .WithArguments("World")
      .WithStandardInput("Hello World\nGoodbye Moon\nHello Universe")
      .GetStringAsync();
    
    AssertTrue(
      result.Trim() == "Hello World",
      $"StandardInput with grep should find 'Hello World', got '{result.Trim()}'"
    );
  }

  public static async Task TestRunBuilderWithStandardInputLines()
  {
    string result = await Shell.Builder("wc")
      .WithArguments("-l")
      .WithStandardInput("Line 1\nLine 2\nLine 3\nLine 4\nLine 5\n")
      .GetStringAsync();
    
    AssertTrue(
      result.Trim() == "5",
      $"StandardInput line count should be 5, got '{result.Trim()}'"
    );
  }

  public static async Task TestRunBuilderWithStandardInputPipeline()
  {
    string[] result = await Shell.Builder("cat")
      .WithStandardInput("apple\nbanana\ncherry\ndate")
      .Pipe("sort")
      .Pipe("head", "-2")
      .GetLinesAsync();
    
    AssertTrue(
      result.Length == 2 && result[0] == "apple" && result[1] == "banana",
      $"StandardInput with pipeline should return sorted first 2 items"
    );
  }

  public static async Task TestRunBuilderWithEmptyStandardInput()
  {
    string result = await Shell.Builder("cat")
      .WithStandardInput("")
      .GetStringAsync();
    
    AssertTrue(
      result.Length == 0,
      $"Empty StandardInput should return empty string"
    );
  }

  public static async Task TestRunBuilderWithComplexArgumentsCommandString()
  {
    string command = Shell.Builder("git")
      .WithArguments("log", "--oneline", "--author=\"John Doe\"", "--grep=fix")
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "git log --oneline \"--author=\\\"John Doe\\\"\" --grep=fix",
      $"Expected 'git log --oneline \"--author=\\\"John Doe\\\"\" --grep=fix', got '{command}'"
    );
    
    await Task.CompletedTask;
  }

  public static async Task TestRunBuilderWithNoValidationCommandString()
  {
    string command = Shell.Builder("false")
      .WithNoValidation()
      .Build()
      .ToCommandString();
    
    AssertTrue(
      command == "false ",
      $"Expected 'false ', got '{command}'"
    );
    
    await Task.CompletedTask;
  }
}