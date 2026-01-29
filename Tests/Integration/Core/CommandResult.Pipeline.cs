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
public class CommandResultPipelineTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<CommandResultPipelineTests>();

  public static async Task Should_execute_basic_pipeline()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("hello\nworld\ntest")
      .Pipe("grep", "world")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("world");
    await Task.CompletedTask;
  }

  public static async Task Should_execute_multi_stage_pipeline()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("line1\nline2\nline3\nline4")
      .Pipe("grep", "line")
      .Pipe("wc", "-l")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("4");
    await Task.CompletedTask;
  }

  public static async Task Should_execute_pipeline_with_get_lines_async()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("apple\nbanana\ncherry")
      .Pipe("grep", "a")
      .CaptureAsync();
    string[] lines = output.GetLines();

    lines.Length.ShouldBe(2);
    lines[0].ShouldBe("apple");
    lines[1].ShouldBe("banana");
    await Task.CompletedTask;
  }

  public static async Task Should_execute_pipeline_with_execute_async()
  {
    await Shell.Builder("echo").WithArguments("test")
      .Pipe("grep", "test")
      .RunAsync();

    // Test passes if no exception is thrown
    true.ShouldBeTrue();
    await Task.CompletedTask;
  }

  public static async Task Should_throw_when_first_pipeline_command_fails()
  {
    await Should.ThrowAsync<Exception>(async () =>
      await Shell.Builder("nonexistentcommand12345").WithNoValidation()
        .Pipe("grep", "anything")
        .CaptureAsync()
    );
    await Task.CompletedTask;
  }

  public static async Task Should_throw_when_second_pipeline_command_fails()
  {
    string[] echoArgs = { "test" };
    await Should.ThrowAsync<Exception>(async () =>
      await Shell.Builder("echo").WithArguments(echoArgs).WithNoValidation()
        .Pipe("nonexistentcommand12345")
        .CaptureAsync()
    );
    await Task.CompletedTask;
  }

  public static async Task Should_execute_real_world_find_and_filter_pipeline()
  {
    CommandOutput output = await Shell.Builder("find").WithArguments(".", "-name", "*.cs", "-type", "f")
      .Pipe("head", "-5")
      .CaptureAsync();
    string[] files = output.GetLines();

    files.Length.ShouldBeLessThanOrEqualTo(5);
    files.All(f => f.EndsWith(".cs", StringComparison.Ordinal)).ShouldBeTrue();
    await Task.CompletedTask;
  }

  public static async Task Should_execute_complex_pipeline_chaining()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("The quick brown fox jumps over the lazy dog")
      .Pipe("tr", " ", "\n")
      .Pipe("grep", "o")
      .Pipe("wc", "-l")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("4");
    await Task.CompletedTask;
  }

  public static async Task Should_pipe_with_no_arguments()
  {
    // Test that Pipe works without arguments (using new optional parameter)
    CommandOutput output = await Shell.Builder("echo").WithArguments("zebra\napple\nbanana")
      .Build()
      .Pipe("sort")  // No arguments!
      .CaptureAsync();
    string result = output.Stdout;

    string[] lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);

    lines.Length.ShouldBe(3);
    lines[0].Trim().ShouldBe("apple");
    await Task.CompletedTask;
  }
}
