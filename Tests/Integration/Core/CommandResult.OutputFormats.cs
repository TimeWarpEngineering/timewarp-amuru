#!/usr/bin/dotnet run

await RunTests<OutputFormatsTests>();

internal sealed class OutputFormatsTests
{
  public static async Task TestGetStringAsyncReturnsRawOutput()
  {
    string result = await Shell.Builder("echo").WithArguments("line1\nline2\nline3").GetStringAsync();
    
    AssertTrue(
      result.Contains("line1", StringComparison.Ordinal) && 
      result.Contains("line2", StringComparison.Ordinal) && 
      result.Contains("line3", StringComparison.Ordinal),
      "GetStringAsync should return raw output with all lines"
    );
  }

  public static async Task TestGetLinesAsyncSplitsLinesCorrectly()
  {
    // Use printf to ensure consistent cross-platform newlines
    string[] lines = await Shell.Builder("printf").WithArguments("line1\nline2\nline3").GetLinesAsync();
    
    AssertTrue(
      lines.Length == 3 && lines[0] == "line1" && lines[1] == "line2" && lines[2] == "line3",
      $"GetLinesAsync should return 3 lines [line1, line2, line3], got {lines.Length} lines: [{string.Join(", ", lines)}]"
    );
  }

  public static async Task TestGetLinesAsyncRemovesEmptyLines()
  {
    string[] lines = await Shell.Builder("printf").WithArguments("line1\n\nline2\n\n").GetLinesAsync();
    
    AssertTrue(
      lines.Length == 2 && lines[0] == "line1" && lines[1] == "line2",
      $"GetLinesAsync should remove empty lines, expected [line1, line2], got [{string.Join(", ", lines)}]"
    );
  }

  public static async Task TestEmptyOutputHandling()
  {
    string result = await Shell.Builder("echo").WithArguments("").GetStringAsync();
    string[] lines = await Shell.Builder("echo").WithArguments("").GetLinesAsync();
    
    AssertTrue(
      result.Length <= 2 && lines.Length == 0,
      $"Empty output should be handled correctly - string length: {result.Length}, lines count: {lines.Length}"
    );
  }

  public static async Task TestRealWorldLsCommand()
  {
    string[] files = await Shell.Builder("ls").WithArguments("-1").GetLinesAsync();
    
    AssertTrue(
      files.Length > 0,
      $"ls command should return files/directories, found {files.Length} items"
    );
  }
}