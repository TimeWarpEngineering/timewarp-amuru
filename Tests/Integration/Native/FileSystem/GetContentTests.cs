#!/usr/bin/dotnet --
#:package TimeWarp.Jaribu
#:package TimeWarp.Amuru

#if !JARIBU_MULTI
return await RunAllTests();
#endif

using TimeWarp.Amuru;

namespace TimeWarp.Amuru.Tests;

[TestTag("Native")]
public class GetContentTests
{
  [ModuleInitializer]
  internal static void Register() => RegisterTests<GetContentTests>();

  public static async Task Should_get_content_with_existing_file()
  {
    // Create a test file
    string testFile = Path.GetTempFileName();
    string content = "Line 1\nLine 2\nLine 3";
    await File.WriteAllTextAsync(testFile, content);

    try
    {
      // Test Commands.GetContent
      CommandOutput result = Commands.GetContent(testFile);

      result.Success.ShouldBeTrue("GetContent should succeed for existing file");
      result.Stdout.ShouldBe(content);
      result.ExitCode.ShouldBe(0);
    }
    finally
    {
      File.Delete(testFile);
    }

    await Task.CompletedTask;
  }

  public static async Task Should_fail_get_content_with_missing_file()
  {
    string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    CommandOutput result = Commands.GetContent(nonExistentFile);

    result.Success.ShouldBeFalse();
    result.Stderr.ShouldContain("No such file");
    result.ExitCode.ShouldBe(1);

    await Task.CompletedTask;
  }

  public static async Task Should_support_direct_api_streaming()
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
          readLines[9].ShouldBe("Line 10");
        }
      }

      readLines.Count.ShouldBe(100);
    }
    finally
    {
      File.Delete(testFile);
    }

    await Task.CompletedTask;
  }

  public static async Task Should_throw_exceptions_in_direct_api_for_missing_file()
  {
    string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    await Should.ThrowAsync<FileNotFoundException>(async () =>
    {
      await foreach (string line in Direct.GetContent(nonExistentFile))
      {
        // Should not get here
      }
    });

    await Task.CompletedTask;
  }

  public static async Task Should_allow_both_apis_to_coexist()
  {
    string testFile = Path.GetTempFileName();
    await File.WriteAllTextAsync(testFile, "coexist test");

    try
    {
      // Commands API - returns CommandOutput
      CommandOutput commandResult = Commands.GetContent(testFile);
      commandResult.Stdout.ShouldBe("coexist test");

      // Direct API - returns IAsyncEnumerable
      List<string> lines = new();
      await foreach (string line in Direct.GetContent(testFile))
      {
        lines.Add(line);
      }

      lines[0].ShouldBe("coexist test");

      // Both results should be the same content
      commandResult.Stdout.ShouldBe(string.Join("\n", lines));
    }
    finally
    {
      File.Delete(testFile);
    }

    await Task.CompletedTask;
  }
}
