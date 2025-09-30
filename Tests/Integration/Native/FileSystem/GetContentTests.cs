#!/usr/bin/dotnet --
#:project ../../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:project ../../../../Tests/TimeWarp.Amuru.Test.Helpers/TimeWarp.Amuru.Test.Helpers.csproj

using TimeWarp.Amuru;
using TimeWarp.Amuru.Native.FileSystem;
using TimeWarp.Amuru.Test.Helpers;
using static TimeWarp.Amuru.Native.Aliases.Bash;
using static TimeWarp.Amuru.Test.Helpers.Asserts;

await TestRunner.RunTests<GetContentTests>();

internal sealed class GetContentTests
{
  public static async Task TestGetContentWithExistingFile()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    string content = "Line 1\nLine 2\nLine 3";
    await File.WriteAllTextAsync(testFile, content);

    try
    {
      // Test Commands.GetContent
      CommandOutput result = Commands.GetContent(testFile);

      AssertTrue(
        result.Success,
        "GetContent should succeed for existing file"
      );

      AssertTrue(
        result.Stdout == content,
        $"Content should match. Expected: '{content}', Got: '{result.Stdout}'"
      );

      AssertTrue(
        result.ExitCode == 0,
        "Exit code should be 0 for success"
      );
    }
    finally
    {
      File.Delete(testFile);
    }
  }

  public static async Task TestGetContentWithMissingFile()
  {
    string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    CommandOutput result = Commands.GetContent(nonExistentFile);

    AssertFalse(
      result.Success,
      "GetContent should fail for non-existent file"
    );

    AssertTrue(
      result.Stderr.Contains("No such file", StringComparison.Ordinal),
      $"Error message should indicate file not found. Got: '{result.Stderr}'"
    );

    AssertTrue(
      result.ExitCode == 1,
      "Exit code should be 1 for failure"
    );

    await Task.CompletedTask;
  }

  public static async Task TestDirectApiStreaming()
  {
    // Create a test file with multiple lines
    string testFile = Path.GetTempFileName();
    IEnumerable<string> lines = Enumerable.Range(1, 100).Select(i => $"Line {i}");
    await File.WriteAllLinesAsync(testFile, lines);

    try
    {
      // Test Direct.GetContent streaming
      List<string> readLines = new();
      await foreach (string line in Direct.GetContent(testFile))
      {
        readLines.Add(line);

        // Verify we're actually streaming (could check memory usage in real scenario)
        if (readLines.Count == 10)
        {
          // We've read 10 lines, rest should still be pending
          AssertTrue(
            readLines[9] == "Line 10",
            "Should be reading lines in order"
          );
        }
      }

      AssertTrue(
        readLines.Count == 100,
        $"Should read all 100 lines. Got: {readLines.Count}"
      );
    }
    finally
    {
      File.Delete(testFile);
    }
  }

  public static async Task TestDirectApiThrowsExceptions()
  {
    string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    bool exceptionThrown = false;
    try
    {
      await foreach (string line in Direct.GetContent(nonExistentFile))
      {
        // Should not get here
      }
    }
    catch (FileNotFoundException)
    {
      exceptionThrown = true;
    }

    AssertTrue(
      exceptionThrown,
      "Direct API should throw FileNotFoundException for missing files"
    );
  }

  public static async Task TestBothApisCanCoexist()
  {
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "coexist test");

    try
    {
      // Commands API - returns CommandOutput
      CommandOutput commandResult = Commands.GetContent(testFile);
      AssertTrue(
        commandResult.Stdout == "coexist test",
        "Commands API should work"
      );

      // Direct API - returns IAsyncEnumerable
      List<string> lines = new();
      await foreach (string line in Direct.GetContent(testFile))
      {
        lines.Add(line);
      }

      AssertTrue(
        lines[0] == "coexist test",
        "Direct API should work"
      );

      // Both results should be the same content
      AssertTrue(
        commandResult.Stdout == string.Join("\n", lines),
        "Both APIs should return same content"
      );
    }
    finally
    {
      File.Delete(testFile);
    }
  }
}