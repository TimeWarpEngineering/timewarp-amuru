#!/usr/bin/dotnet run

await RunTests<OutputFormatsTests>();

internal sealed class OutputFormatsTests
{
  public static async Task TestGetStringAsyncReturnsRawOutput()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("line1\nline2\nline3").CaptureAsync();
    
    AssertTrue(
      output.Stdout.Contains("line1", StringComparison.Ordinal) && 
      output.Stdout.Contains("line2", StringComparison.Ordinal) && 
      output.Stdout.Contains("line3", StringComparison.Ordinal),
      "CaptureAsync should return raw output with all lines"
    );
  }

  public static async Task TestGetLinesAsyncSplitsLinesCorrectly()
  {
    // Use printf to ensure consistent cross-platform newlines
    CommandOutput output = await Shell.Builder("printf").WithArguments("line1\nline2\nline3").CaptureAsync();
    string[] lines = output.GetLines();
    
    AssertTrue(
      lines.Length == 3 && lines[0] == "line1" && lines[1] == "line2" && lines[2] == "line3",
      $"GetLines() should return 3 lines [line1, line2, line3], got {lines.Length} lines: [{string.Join(", ", lines)}]"
    );
  }

  public static async Task TestGetLinesAsyncRemovesEmptyLines()
  {
    CommandOutput output = await Shell.Builder("printf").WithArguments("line1\n\nline2\n\n").CaptureAsync();
    string[] lines = output.GetLines();
    
    AssertTrue(
      lines.Length == 2 && lines[0] == "line1" && lines[1] == "line2",
      $"GetLines() should remove empty lines, expected [line1, line2], got [{string.Join(", ", lines)}]"
    );
  }

  public static async Task TestEmptyOutputHandling()
  {
    CommandOutput output = await Shell.Builder("echo").WithArguments("").CaptureAsync();
    string[] lines = output.GetLines();
    
    AssertTrue(
      output.Stdout.Length <= 2 && lines.Length == 0,
      $"Empty output should be handled correctly - string length: {output.Stdout.Length}, lines count: {lines.Length}"
    );
  }

  public static async Task TestRealWorldLsCommand()
  {
    CommandOutput output = await Shell.Builder("ls").WithArguments("-1").CaptureAsync();
    string[] files = output.GetLines();
    
    AssertTrue(
      files.Length > 0,
      $"ls command should return files/directories, found {files.Length} items"
    );
  }
}