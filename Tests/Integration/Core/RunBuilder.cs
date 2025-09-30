#!/usr/bin/dotnet run
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using Shouldly;
using static TimeWarp.Amuru.Test.Helpers.TestRunner;

await RunTests<RunBuilderTests>();

internal sealed class RunBuilderTests
{
  public static async Task TestBasicRunBuilderCommandString()
  {
    string command = Shell.Builder("echo")
      .WithArguments("Hello", "World")
      .Build()
      .ToCommandString();

    command.ShouldBe("echo Hello World");

    await Task.CompletedTask;
  }

  public static async Task TestBasicRunBuilder()
  {
    CommandOutput output = await Shell.Builder("echo")
      .WithArguments("Hello", "World")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("Hello World");
  }

  public static async Task TestRunBuilderWithMultipleWithArgumentsCommandString()
  {
    string command = Shell.Builder("echo")
      .WithArguments("arg1")
      .WithArguments("arg2", "arg3")
      .Build()
      .ToCommandString();

    command.ShouldBe("echo arg1 arg2 arg3");

    await Task.CompletedTask;
  }

  public static async Task TestRunBuilderWithMultipleWithArguments()
  {
    CommandOutput output = await Shell.Builder("echo")
      .WithArguments("arg1")
      .WithArguments("arg2", "arg3")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("arg1 arg2 arg3");
  }

  public static async Task TestRunBuilderWithEnvironmentVariableCommandString()
  {
    // Note: Environment variables don't appear in ToCommandString()
    string command = Shell.Builder("printenv")
      .WithEnvironmentVariable("TEST_VAR", "test_value")
      .WithArguments("TEST_VAR")
      .Build()
      .ToCommandString();

    command.ShouldBe("printenv TEST_VAR");

    await Task.CompletedTask;
  }

  public static async Task TestRunBuilderWithEnvironmentVariable()
  {
    CommandOutput output = await Shell.Builder("printenv")
      .WithEnvironmentVariable("TEST_VAR", "test_value")
      .WithArguments("TEST_VAR")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("test_value");
  }

  public static async Task TestRunBuilderWithNoValidation()
  {
    // This would normally throw because 'false' exits with code 1
    int exitCode = await Shell.Builder("false")
      .WithNoValidation()
      .RunAsync();

    exitCode.ShouldBe(1);
  }

  public static async Task TestRunBuilderGetLinesAsync()
  {
    CommandOutput output = await Shell.Builder("printf")
      .WithArguments("line1\\nline2\\nline3")
      .CaptureAsync();
    string[] lines = output.GetLines();

    lines.Length.ShouldBe(3);
    lines[0].ShouldBe("line1");
    lines[1].ShouldBe("line2");
    lines[2].ShouldBe("line3");
  }

  public static async Task TestRunBuilderWithWorkingDirectoryCommandString()
  {
    // Note: Working directory doesn't appear in ToCommandString()
    string command = Shell.Builder("pwd")
      .WithWorkingDirectory("/tmp")
      .Build()
      .ToCommandString();

    command.ShouldBe("pwd ");

    await Task.CompletedTask;
  }

  public static async Task TestRunBuilderWithWorkingDirectory()
  {
    string tempDir = Path.GetTempPath();
    CommandOutput output = await Shell.Builder("pwd")
      .WithWorkingDirectory(tempDir)
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe(tempDir.TrimEnd('/'));
  }

  public static async Task TestRunBuilderPipelineCommandString()
  {
    string command = Shell.Builder("echo")
      .WithArguments("Hello\nWorld\nTest")
      .Build()
      .Pipe("grep", "World")
      .ToCommandString();

    command.ShouldBe("grep World");

    await Task.CompletedTask;
  }

  public static async Task TestRunBuilderPipeline()
  {
    CommandOutput output = await Shell.Builder("echo")
      .WithArguments("Hello\nWorld\nTest")
      .Build()
      .Pipe("grep", "World")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("World");
  }

  public static async Task TestRunBuilderExecuteAsync()
  {
    CommandOutput output = await Shell.Builder("echo")
      .WithArguments("test output")
      .CaptureAsync();

    output.Success.ShouldBeTrue();
    output.Stdout.Trim().ShouldBe("test output");
  }

  public static async Task TestRunBuilderChaining()
  {
    // Test that all methods can be chained fluently
    CommandOutput output = await Shell.Builder("bash")
      .WithArguments("-c", "echo $TEST1 $TEST2")
      .WithEnvironmentVariable("TEST1", "Hello")
      .WithEnvironmentVariable("TEST2", "World")
      .WithNoValidation()
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("Hello World");
  }

  public static async Task TestRunBuilderWithStandardInput()
  {
    CommandOutput output = await Shell.Builder("grep")
      .WithArguments("World")
      .WithStandardInput("Hello World\nGoodbye Moon\nHello Universe")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("Hello World");
  }

  public static async Task TestRunBuilderWithStandardInputLines()
  {
    CommandOutput output = await Shell.Builder("wc")
      .WithArguments("-l")
      .WithStandardInput("Line 1\nLine 2\nLine 3\nLine 4\nLine 5\n")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("5");
  }

  public static async Task TestRunBuilderWithStandardInputPipeline()
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

  public static async Task TestRunBuilderWithEmptyStandardInput()
  {
    CommandOutput output = await Shell.Builder("cat")
      .WithStandardInput("")
      .CaptureAsync();
    string result = output.Stdout;

    result.Length.ShouldBe(0);
  }

  public static async Task TestRunBuilderWithComplexArgumentsCommandString()
  {
    string command = Shell.Builder("git")
      .WithArguments("log", "--oneline", "--author=\"John Doe\"", "--grep=fix")
      .Build()
      .ToCommandString();

    command.ShouldBe("git log --oneline \"--author=\\\"John Doe\\\"\" --grep=fix");

    await Task.CompletedTask;
  }

  public static async Task TestRunBuilderWithNoValidationCommandString()
  {
    string command = Shell.Builder("false")
      .WithNoValidation()
      .Build()
      .ToCommandString();

    command.ShouldBe("false ");

    await Task.CompletedTask;
  }
}