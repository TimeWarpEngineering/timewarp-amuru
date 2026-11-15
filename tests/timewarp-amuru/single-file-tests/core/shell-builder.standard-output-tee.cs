#!/usr/bin/env -S dotnet --

#region Purpose
// Tests for ShellBuilder.WithStandardOutputTee: a byte-level copy of stdout to a file.
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// The tests read the file while the child still runs, so they open it shared for read and write.
// Children are sh scripts that print with printf (no trailing newline) and then sleep.
// Each test owns a temp directory and deletes it in finally.
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class StandardOutputTee_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<StandardOutputTee_Given_>();

    public static async Task CaptureAsync_Should_CopyStdoutAndKeepResult()
    {
      string directory = CreateDirectory();
      try
      {
        string tee = Path.Combine(directory, "out.txt");
        CommandOutput output = await Shell.Builder("sh")
          .WithArguments("-c", "printf 'one\\ntwo'; printf 'err' >&2; exit 3")
          .WithStandardOutputTee(tee)
          .CaptureAsync();

        output.Stdout.ShouldBe("one\ntwo");
        output.Stderr.ShouldBe("err");
        output.ExitCode.ShouldBe(3);
        (await File.ReadAllTextAsync(tee)).ShouldBe("one\ntwo");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task BinaryOutput_Should_CopyBytesUnchanged()
    {
      string directory = CreateDirectory();
      try
      {
        string tee = Path.Combine(directory, "out.bin");
        await Shell.Builder("sh")
          .WithArguments("-c", "printf '\\000\\377\\r\\n\\200'")
          .WithStandardOutputTee(tee)
          .CaptureAsync();

        (await File.ReadAllBytesAsync(tee)).ShouldBe(new byte[] { 0x00, 0xFF, 0x0D, 0x0A, 0x80 });
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task UnterminatedLine_Should_BeInFileWhileChildRuns()
    {
      string directory = CreateDirectory();
      try
      {
        string tee = Path.Combine(directory, "out.txt");
        Task<CommandOutput> run = Shell.Builder("sh")
          .WithArguments("-c", "printf 'final answer'; sleep 3")
          .WithStandardOutputTee(tee)
          .CaptureAsync();

        string seen = await WaitForTextAsync(tee, "final answer", TimeSpan.FromSeconds(2.5));

        seen.ShouldBe("final answer");
        run.IsCompleted.ShouldBeFalse();

        CommandOutput output = await run;
        output.Stdout.ShouldBe("final answer");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task Cancellation_Should_KeepBytesWritten()
    {
      string directory = CreateDirectory();
      try
      {
        string tee = Path.Combine(directory, "out.txt");
        using var cancellation = new CancellationTokenSource();
        Task<CommandOutput> run = Shell.Builder("sh")
          .WithArguments("-c", "printf 'before cancel'; sleep 30")
          .WithStandardOutputTee(tee)
          .CaptureAsync(cancellation.Token);

        await WaitForTextAsync(tee, "before cancel", TimeSpan.FromSeconds(5));
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => run);
        (await File.ReadAllTextAsync(tee)).ShouldBe("before cancel");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task Timeout_Should_KeepBytesWritten()
    {
      string directory = CreateDirectory();
      try
      {
        string tee = Path.Combine(directory, "out.txt");
        CommandOutput output = await Shell.Builder("sh")
          .WithArguments("-c", "printf 'partial'; sleep 30")
          .WithTimeout(TimeSpan.FromMilliseconds(500))
          .WithTimeoutGracePeriod(TimeSpan.Zero)
          .WithStandardOutputTee(tee)
          .CaptureAsync();

        output.TimedOut.ShouldBeTrue();
        (await File.ReadAllTextAsync(tee)).ShouldBe("partial");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task StreamStdoutAsync_Should_TeeAndStreamLines()
    {
      string directory = CreateDirectory();
      try
      {
        string tee = Path.Combine(directory, "out.txt");
        List<string> lines = [];
        await foreach (string line in Shell.Builder("sh")
          .WithArguments("-c", "printf 'a\\nb'; printf 'err\\n' >&2")
          .WithStandardOutputTee(tee)
          .StreamStdoutAsync())
        {
          lines.Add(line);
        }

        lines.ShouldBe(["a", "b"]);
        (await File.ReadAllTextAsync(tee)).ShouldBe("a\nb");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task StreamStdoutAsyncWithTimeout_Should_Tee()
    {
      string directory = CreateDirectory();
      try
      {
        string tee = Path.Combine(directory, "out.txt");
        List<string> lines = [];
        await foreach (string line in Shell.Builder("sh")
          .WithArguments("-c", "printf 'x\\ny'")
          .WithTimeout(TimeSpan.FromSeconds(30))
          .WithStandardOutputTee(tee)
          .StreamStdoutAsync())
        {
          lines.Add(line);
        }

        lines.ShouldBe(["x", "y"]);
        (await File.ReadAllTextAsync(tee)).ShouldBe("x\ny");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task RunAsync_Should_Tee()
    {
      string directory = CreateDirectory();
      try
      {
        string tee = Path.Combine(directory, "out.txt");
        int exitCode = await Shell.Builder("sh")
          .WithArguments("-c", "printf 'run output'")
          .WithStandardOutputTee(tee)
          .RunAsync();

        exitCode.ShouldBe(0);
        (await File.ReadAllTextAsync(tee)).ShouldBe("run output");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task Pipe_Should_TeeLastStageOutput()
    {
      string directory = CreateDirectory();
      try
      {
        string tee = Path.Combine(directory, "out.txt");
        CommandOutput output = await Shell.Builder("echo")
          .WithArguments("hello")
          .WithStandardOutputTee(tee)
          .Pipe("tr", "a-z", "A-Z")
          .CaptureAsync();

        output.Stdout.Trim().ShouldBe("HELLO");
        (await File.ReadAllTextAsync(tee)).ShouldBe("HELLO\n");
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
        await Should.ThrowAsync<InvalidOperationException>(() => Shell.Builder("echo")
          .WithStandardOutputTee(Path.Combine(directory, "out.txt"))
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
        () => Shell.Builder("python3").WithStandardOutputTee("out.txt").AsJsonRpcClient()
      );
      return Task.CompletedTask;
    }

    public static async Task Mock_Should_WriteSetupStdout()
    {
      string directory = CreateDirectory();
      try
      {
        string tee = Path.Combine(directory, "out.txt");
        using (CommandMock.Enable())
        {
          CommandMock.Setup("agent", "run").Returns("mock answer", "mock error");

          CommandOutput output = await Shell.Builder("agent")
            .WithArguments("run")
            .WithStandardOutputTee(tee)
            .CaptureAsync();

          output.Stdout.ShouldBe("mock answer");
        }

        (await File.ReadAllTextAsync(tee)).ShouldBe("mock answer");
      }
      finally
      {
        Directory.Delete(directory, recursive: true);
      }
    }

    public static async Task MockCancelled_Should_LeaveEmptyFile()
    {
      string directory = CreateDirectory();
      try
      {
        string tee = Path.Combine(directory, "out.txt");
        using (CommandMock.Enable())
        {
          CommandMock.Setup("agent", "slow").Returns("late").Delays(TimeSpan.FromSeconds(30));
          using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

          await Should.ThrowAsync<OperationCanceledException>(() => Shell.Builder("agent")
            .WithArguments("slow")
            .WithStandardOutputTee(tee)
            .CaptureAsync(cancellation.Token));
        }

        (await File.ReadAllTextAsync(tee)).ShouldBe(string.Empty);
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

    // Polls the file opened shared, the way another reader would during the run.
    private static async Task<string> WaitForTextAsync(string path, string expected, TimeSpan limit)
    {
      var stopwatch = Stopwatch.StartNew();
      string text = string.Empty;
      while (stopwatch.Elapsed < limit)
      {
        if (File.Exists(path))
        {
          using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
          using var reader = new StreamReader(stream);
          text = await reader.ReadToEndAsync();
          if (text == expected)
          {
            return text;
          }
        }

        await Task.Delay(50);
      }

      return text;
    }
  }
}
