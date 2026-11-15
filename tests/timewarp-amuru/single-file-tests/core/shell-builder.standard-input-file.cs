#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for ShellBuilder.WithStandardInputFile: a file as the child's standard input.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Each test owns a temp directory and deletes it in finally.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class StandardInputFile_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<StandardInputFile_Given_>();

    public static async Task File_Should_BeChildStdin()
    {
      string directory = CreateDirectory();
      try
      {
        string input = Path.Combine(directory, "prompt.txt");
        await File.WriteAllTextAsync(input, "Hello World\nGoodbye Moon\n");

        CommandOutput output = await Shell.Builder("grep")
          .WithArguments("World")
          .WithStandardInputFile(input)
          .CaptureAsync();

        output.Stdout.ShouldBe("Hello World");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task LargeFile_Should_StreamEveryByte()
    {
      string directory = CreateDirectory();
      try
      {
        string input = Path.Combine(directory, "prompt.txt");
        await File.WriteAllTextAsync(input, new string('x', 300 * 1024));

        CommandOutput output = await Shell.Builder("wc")
          .WithArguments("-c")
          .WithStandardInputFile(input)
          .CaptureAsync();

        output.Stdout.Trim().ShouldBe((300 * 1024).ToString(System.Globalization.CultureInfo.InvariantCulture));
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task TextAfterFile_Should_ReplaceFile()
    {
      string directory = CreateDirectory();
      try
      {
        string input = Path.Combine(directory, "prompt.txt");
        await File.WriteAllTextAsync(input, "from file");

        CommandOutput output = await Shell.Builder("cat")
          .WithStandardInputFile(input)
          .WithStandardInput("from text")
          .CaptureAsync();

        output.Stdout.ShouldBe("from text");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task FileAfterText_Should_ReplaceText()
    {
      string directory = CreateDirectory();
      try
      {
        string input = Path.Combine(directory, "prompt.txt");
        await File.WriteAllTextAsync(input, "from file");

        CommandOutput output = await Shell.Builder("cat")
          .WithStandardInput("from text")
          .WithStandardInputFile(input)
          .CaptureAsync();

        output.Stdout.ShouldBe("from file");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task MissingFile_Should_ThrowFileNotFound()
    {
      string directory = CreateDirectory();
      try
      {
        await Should.ThrowAsync<FileNotFoundException>(() => Shell.Builder("cat")
          .WithStandardInputFile(Path.Combine(directory, "missing.txt"))
          .CaptureAsync());
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task WithTee_Should_RoundTrip()
    {
      string directory = CreateDirectory();
      try
      {
        string input = Path.Combine(directory, "prompt.txt");
        string tee = Path.Combine(directory, "out.txt");
        await File.WriteAllTextAsync(input, "echoed without newline");

        CommandOutput output = await Shell.Builder("cat")
          .WithStandardInputFile(input)
          .WithStandardOutputTee(tee)
          .CaptureAsync();

        output.Stdout.ShouldBe("echoed without newline");
        (await File.ReadAllTextAsync(tee)).ShouldBe("echoed without newline");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task TtyPassthroughAsync_Should_Refuse()
    {
      string directory = CreateDirectory();
      try
      {
        string input = Path.Combine(directory, "prompt.txt");
        await File.WriteAllTextAsync(input, "text");

        await Should.ThrowAsync<InvalidOperationException>(() => Shell.Builder("cat")
          .WithStandardInputFile(input)
          .TtyPassthroughAsync());
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static Task AsJsonRpcClient_Should_Refuse()
    {
      Should.Throw<InvalidOperationException>(
        () => Shell.Builder("python3").WithStandardInputFile("prompt.txt").AsJsonRpcClient()
      );
      return Task.CompletedTask;
    }

    public static async Task Mock_Should_RecordFileText()
    {
      string directory = CreateDirectory();
      try
      {
        string input = Path.Combine(directory, "prompt.txt");
        await File.WriteAllTextAsync(input, "prompt body");

        using (CommandMock.Enable())
        {
          CommandMock.Setup("agent", "run").Returns("ok");

          CommandOutput output = await Shell.Builder("agent")
            .WithArguments("run")
            .WithStandardInputFile(input)
            .CaptureAsync();

          output.Stdout.ShouldBe("ok");
          CommandMock.GetConfiguredStandardInput("agent", "run").ShouldBe("prompt body");
        }
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    private static string CreateDirectory()
    {
      string directory = Path.Combine(Path.GetTempPath(), "amuru-standard-streams-" + Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(directory);
      return directory;
    }
  }
}
