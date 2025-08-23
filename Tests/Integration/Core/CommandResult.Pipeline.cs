#!/usr/bin/dotnet run

await RunTests<PipelineTests>();

internal sealed class PipelineTests
{

  public static async Task TestBasicPipeline()
  {
    string result = await Shell.Builder("echo").WithArguments("hello\nworld\ntest")
      .Pipe("grep", "world")
      .GetStringAsync();
    
    AssertTrue(
      result.Trim() == "world",
      "Basic pipeline should filter for 'world'"
    );
  }

  public static async Task TestMultiStagePipeline()
  {
    string result = await Shell.Builder("echo").WithArguments("line1\nline2\nline3\nline4")
      .Pipe("grep", "line")
      .Pipe("wc", "-l")
      .GetStringAsync();
    
    AssertTrue(
      result.Trim() == "4",
      "Multi-stage pipeline should count 4 lines"
    );
  }

  public static async Task TestPipelineWithGetLinesAsync()
  {
    string[] lines = await Shell.Builder("echo").WithArguments("apple\nbanana\ncherry")
      .Pipe("grep", "a")
      .GetLinesAsync();
    
    AssertTrue(
      lines.Length == 2 && lines[0] == "apple" && lines[1] == "banana",
      $"Pipeline with GetLinesAsync should return [apple, banana], got [{string.Join(", ", lines)}]"
    );
  }

  public static async Task TestPipelineWithExecuteAsync()
  {
    await Shell.Builder("echo").WithArguments("test")
      .Pipe("grep", "test")
      .ExecuteAsync();
    
    // Test passes if no exception is thrown
    AssertTrue(true, "Pipeline with ExecuteAsync should not throw");
  }

  public static async Task TestPipelineWithFailedFirstCommandThrows()
  {
    await AssertThrowsAsync<Exception>(
      async () => await Shell.Builder("nonexistentcommand12345").WithNoValidation()
        .Pipe("grep", "anything")
        .GetStringAsync(),
      "Pipeline with non-existent first command should throw even with no validation"
    );
  }

  public static async Task TestPipelineWithFailedSecondCommandThrows()
  {
    string[] echoArgs = { "test" };
    await AssertThrowsAsync<Exception>(
      async () => await Shell.Builder("echo").WithArguments(echoArgs).WithNoValidation()
        .Pipe("nonexistentcommand12345")
        .GetStringAsync(),
      "Pipeline with non-existent second command should throw even with no validation"
    );
  }

  public static async Task TestRealWorldPipelineFindAndFilter()
  {
    string[] files = await Shell.Builder("find").WithArguments(".", "-name", "*.cs", "-type", "f")
      .Pipe("head", "-5")
      .GetLinesAsync();
    
    AssertTrue(
      files.Length <= 5 && files.All(f => f.EndsWith(".cs", StringComparison.Ordinal)),
      $"Real-world pipeline should find up to 5 .cs files, got {files.Length} files"
    );
  }

  public static async Task TestComplexPipelineChaining()
  {
    string result = await Shell.Builder("echo").WithArguments("The quick brown fox jumps over the lazy dog")
      .Pipe("tr", " ", "\n")
      .Pipe("grep", "o")
      .Pipe("wc", "-l")
      .GetStringAsync();
    
    AssertTrue(
      result.Trim() == "4",
      "Complex pipeline should find 4 words containing 'o' (brown, fox, over, dog)"
    );
  }

  public static async Task TestPipeWithNoArguments()
  {
    // Test that Pipe works without arguments (using new optional parameter)
    string result = await Shell.Builder("echo").WithArguments("zebra\napple\nbanana")
      .Build()
      .Pipe("sort")  // No arguments!
      .GetStringAsync();
    
    string[] lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    
    AssertTrue(
      lines.Length == 3 && lines[0].Trim() == "apple",
      $"Pipe with no arguments should work for sort command, first line should be 'apple', got '{lines[0].Trim()}'"
    );
  }
}