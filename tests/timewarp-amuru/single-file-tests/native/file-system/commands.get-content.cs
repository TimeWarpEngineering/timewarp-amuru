#!/usr/bin/dotnet --

#region Purpose
// Tests for Commands.GetContent - validates file content reading via Commands API
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// SUT: Commands (the static class providing command-style file operations)
// Action: GetContent (the method to read file content)
// Tests verify file reading with success and error cases
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace Commands_
{
  [TestTag("Native")]
  public class GetContent_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<GetContent_Given_>();

    public static async Task ExistingFile_Should_ReturnContent()
    {
      string testFile = Path.GetTempFileName();
      string content = "Line 1\nLine 2\nLine 3";
      await File.WriteAllTextAsync(testFile, content);

      try
      {
        CommandOutput result = Commands.GetContent(testFile);

        result.Success.ShouldBeTrue("GetContent should succeed for existing file");
        result.Stdout.ShouldBe(content);
        result.ExitCode.ShouldBe(0);
      }
      finally
      {
        File.Delete(testFile);
      }
    }

    public static async Task MissingFile_Should_Fail()
    {
      string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

      CommandOutput result = Commands.GetContent(nonExistentFile);

      result.Success.ShouldBeFalse();
      result.Stderr.ShouldContain("No such file");
      result.ExitCode.ShouldBe(1);

      await Task.CompletedTask;
    }

    public static async Task BothApis_Should_ReturnSameContent()
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
    }
  }
}
