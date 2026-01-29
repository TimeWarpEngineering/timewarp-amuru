#!/usr/bin/dotnet --
#:package TimeWarp.Jaribu
#:package TimeWarp.Amuru

#if !JARIBU_MULTI
return await RunAllTests();
#endif

using TimeWarp.Amuru;
using Shouldly;

namespace TimeWarp.Amuru.Tests;

[TestTag("Core")]
public class RunBuilderTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<RunBuilderTests>();

  public static async Task Should_build_basic_command_string()
  {
    string command = Shell.Builder("echo")
      .WithArguments("Hello", "World")
      .Build()
      .ToCommandString();

    command.ShouldBe("echo Hello World");

    await Task.CompletedTask;
  }

  public static async Task Should_build_basic_command()
  {
    CommandOutput output = await Shell.Builder("echo")
      .WithArguments("Hello", "World")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("Hello World");
    await Task.CompletedTask;
  }

  public static async Task Should_build_with_multiple_WithArguments_command_string()
  {
    string command = Shell.Builder("echo")
      .WithArguments("arg1")
      .WithArguments("arg2", "arg3")
      .Build()
      .ToCommandString();

    command.ShouldBe("echo arg1 arg2 arg3");

    await Task.CompletedTask;
  }

  public static async Task Should_build_with_multiple_WithArguments()
  {
    CommandOutput output = await Shell.Builder("echo")
      .WithArguments("arg1")
      .WithArguments("arg2", "arg3")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("arg1 arg2 arg3");
    await Task.CompletedTask;
  }

  public static async Task Should_build_with_environment_variable_command_string()
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

  public static async Task Should_build_with_environment_variable()
  {
    CommandOutput output = await Shell.Builder("printenv")
      .WithEnvironmentVariable("TEST_VAR", "test_value")
      .WithArguments("TEST_VAR")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("test_value");
    await Task.CompletedTask;
  }

  public static async Task Should_build_with_no_validation()
  {
    // This would normally throw because 'false' exits with code 1
    int exitCode = await Shell.Builder("false")
      .WithNoValidation()
      .RunAsync();

    exitCode.ShouldBe(1);
    await Task.CompletedTask;
  }

  public static async Task Should_get_lines_async()
  {
    CommandOutput output = await Shell.Builder("printf")
      .WithArguments("line1\\nline2\\nline3")
      .CaptureAsync();
    string[] lines = output.GetLines();

    lines.Length.ShouldBe(3);
    lines[0].ShouldBe("line1");
    lines[1].ShouldBe("line2");
    lines[2].ShouldBe("line3");
    await Task.CompletedTask;
  }

  public static async Task Should_build_with_working_directory_command_string()
  {
    // Note: Working directory doesn't appear in ToCommandString()
    string command = Shell.Builder("pwd")
      .WithWorkingDirectory("/tmp")
      .Build()
      .ToCommandString();

    command.ShouldBe("pwd ");

    await Task.CompletedTask;
  }

  public static async Task Should_build_with_working_directory()
  {
    string tempDir = Path.GetTempPath();
    CommandOutput output = await Shell.Builder("pwd")
      .WithWorkingDirectory(tempDir)
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe(tempDir.TrimEnd('/'));
    await Task.CompletedTask;
  }

  public static async Task Should_build_pipeline_command_string()
  {
    string command = Shell.Builder("echo")
      .WithArguments("Hello\nWorld\nTest")
      .Build()
      .Pipe("grep", "World")
      .ToCommandString();

    command.ShouldBe("grep World");

    await Task.CompletedTask;
  }

  public static async Task Should_build_pipeline()
  {
    CommandOutput output = await Shell.Builder("echo")
      .WithArguments("Hello\nWorld\nTest")
      .Build()
      .Pipe("grep", "World")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("World");
    await Task.CompletedTask;
  }

  public static async Task Should_execute_async()
  {
    CommandOutput output = await Shell.Builder("echo")
      .WithArguments("test output")
      .CaptureAsync();

    output.Success.ShouldBeTrue();
    output.Stdout.Trim().ShouldBe("test output");
    await Task.CompletedTask;
  }

  public static async Task Should_support_chaining()
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
    await Task.CompletedTask;
  }

  public static async Task Should_build_with_standard_input()
  {
    CommandOutput output = await Shell.Builder("grep")
      .WithArguments("World")
      .WithStandardInput("Hello World\nGoodbye Moon\nHello Universe")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("Hello World");
    await Task.CompletedTask;
  }

  public static async Task Should_build_with_standard_input_lines()
  {
    CommandOutput output = await Shell.Builder("wc")
      .WithArguments("-l")
      .WithStandardInput("Line 1\nLine 2\nLine 3\nLine 4\nLine 5\n")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("5");
    await Task.CompletedTask;
  }

  public static async Task Should_build_with_standard_input_pipeline()
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
    await Task.CompletedTask;
  }

  public static async Task Should_build_with_empty_standard_input()
  {
    CommandOutput output = await Shell.Builder("cat")
      .WithStandardInput("")
      .CaptureAsync();
    string result = output.Stdout;

    result.Length.ShouldBe(0);
    await Task.CompletedTask;
  }

  public static async Task Should_build_with_complex_arguments_command_string()
  {
    string command = Shell.Builder("git")
      .WithArguments("log", "--oneline", "--author=\"John Doe\"", "--grep=fix")
      .Build()
      .ToCommandString();

    command.ShouldBe("git log --oneline \"--author=\\\"John Doe\\\"\" --grep=fix");

    await Task.CompletedTask;
  }

  public static async Task Should_build_with_no_validation_command_string()
  {
    string command = Shell.Builder("false")
      .WithNoValidation()
      .Build()
      .ToCommandString();

    command.ShouldBe("false ");

    await Task.CompletedTask;
  }
}
