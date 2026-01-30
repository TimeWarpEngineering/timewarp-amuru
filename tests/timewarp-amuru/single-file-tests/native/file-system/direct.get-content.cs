#!/usr/bin/dotnet --

#region Purpose
// Tests for Direct.GetContent - validates file content streaming via Direct API
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Direct (the static class providing direct file operations with IAsyncEnumerable)
// Action: GetContent (the method to stream file content)
// Tests verify streaming behavior and error handling
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Direct_
{
  [TestTag("Native")]
  public class GetContent_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<GetContent_Given_>();

    public static async Task MultipleLines_Should_StreamAsynchronously()
    {
      string testFile = Path.GetTempFileName();
      IEnumerable<string> lines = Enumerable.Range(1, 100).Select(i => $"Line {i}");
      await File.WriteAllLinesAsync(testFile, lines);

      try
      {
        List<string> readLines = new();
        await foreach (string line in Direct.GetContent(testFile))
        {
          readLines.Add(line);

          // Verify we're actually streaming
          if (readLines.Count == 10)
          {
            readLines[9].ShouldBe("Line 10");
          }
        }

        readLines.Count.ShouldBe(100);
      }
      finally
      {
        File.Delete(testFile);
      }
    }

    public static async Task MissingFile_Should_ThrowFileNotFoundException()
    {
      string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

      await Should.ThrowAsync<FileNotFoundException>(async () =>
      {
        await foreach (string line in Direct.GetContent(nonExistentFile))
        {
          // Should not get here
        }
      });
    }
  }
}
