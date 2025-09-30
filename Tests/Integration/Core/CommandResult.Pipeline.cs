#!/usr/bin/dotnet run
#:project ../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using Shouldly;
using static TimeWarp.Amuru.Test.Helpers.TestRunner;

await RunTests<PipelineTests>();

internal sealed class PipelineTests
{

  public static async Task TestBasicPipeline()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("hello\nworld\ntest")
      .Pipe("grep", "world")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("world");
  }

  public static async Task TestMultiStagePipeline()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("line1\nline2\nline3\nline4")
      .Pipe("grep", "line")
      .Pipe("wc", "-l")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("4");
  }

  public static async Task TestPipelineWithGetLinesAsync()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("apple\nbanana\ncherry")
      .Pipe("grep", "a")
      .CaptureAsync();
    string[] lines = output.GetLines();

    lines.Length.ShouldBe(2);
    lines[0].ShouldBe("apple");
    lines[1].ShouldBe("banana");
  }

  public static async Task TestPipelineWithExecuteAsync()
  {
    await Shell.Builder("echo").WithArguments("test")
      .Pipe("grep", "test")
      .RunAsync();

    // Test passes if no exception is thrown
    true.ShouldBeTrue();
  }

  public static async Task TestPipelineWithFailedFirstCommandThrows()
  {
    await Should.ThrowAsync<Exception>(async () =>
      await Shell.Builder("nonexistentcommand12345").WithNoValidation()
        .Pipe("grep", "anything")
        .CaptureAsync()
    );
  }

  public static async Task TestPipelineWithFailedSecondCommandThrows()
  {
    string[] echoArgs = { "test" };
    await Should.ThrowAsync<Exception>(async () =>
      await Shell.Builder("echo").WithArguments(echoArgs).WithNoValidation()
        .Pipe("nonexistentcommand12345")
        .CaptureAsync()
    );
  }

  public static async Task TestRealWorldPipelineFindAndFilter()
  {
    CommandOutput output = await Shell.Builder("find").WithArguments(".", "-name", "*.cs", "-type", "f")
      .Pipe("head", "-5")
      .CaptureAsync();
    string[] files = output.GetLines();

    files.Length.ShouldBeLessThanOrEqualTo(5);
    files.All(f => f.EndsWith(".cs", StringComparison.Ordinal)).ShouldBeTrue();
  }

  public static async Task TestComplexPipelineChaining()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("The quick brown fox jumps over the lazy dog")
      .Pipe("tr", " ", "\n")
      .Pipe("grep", "o")
      .Pipe("wc", "-l")
      .CaptureAsync();
    string result = output.Stdout;

    result.Trim().ShouldBe("4");
  }

  public static async Task TestPipeWithNoArguments()
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
  }
}