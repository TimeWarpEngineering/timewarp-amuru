#!/usr/bin/dotnet --

#region Purpose
// Tests for ShellBuilder streaming methods - StreamCombinedAsync, StreamStdoutAsync, StreamStderrAsync
#endregion

#region Design
// Naming convention: SUT_Action_Given_Should_Result
// Streaming methods return IAsyncEnumerable for processing output line-by-line
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace ShellBuilder_
{
  [TestTag("Core")]
  public class StreamAsync_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<StreamAsync_Given_>();

    public static async Task StreamCombined_Should_YieldAllLines()
    {
      List<OutputLine> lines = [];

      await foreach (OutputLine line in Shell.Builder("sh")
        .WithArguments("-c", "echo 'line1' && echo 'line2' && echo 'error' >&2")
        .StreamCombinedAsync())
      {
        lines.Add(line);
      }

      lines.Count.ShouldBeGreaterThanOrEqualTo(3);
      lines.Any(l => l.Text.Contains("line1", StringComparison.Ordinal) && !l.IsError).ShouldBeTrue();
      lines.Any(l => l.Text.Contains("error", StringComparison.Ordinal) && l.IsError).ShouldBeTrue();
    }

    public static async Task StreamStdout_Should_YieldOnlyStdout()
    {
      List<string> lines = [];

      await foreach (string line in Shell.Builder("sh")
        .WithArguments("-c", "echo 'stdout1' && echo 'stderr1' >&2 && echo 'stdout2'")
        .StreamStdoutAsync())
      {
        lines.Add(line);
      }

      lines.Count.ShouldBe(2);
      lines[0].ShouldContain("stdout1");
      lines[1].ShouldContain("stdout2");
    }

    public static async Task StreamStderr_Should_YieldOnlyStderr()
    {
      List<string> lines = [];

      await foreach (string line in Shell.Builder("sh")
        .WithArguments("-c", "echo 'stdout' && echo 'stderr1' >&2 && echo 'stderr2' >&2")
        .StreamStderrAsync())
      {
        lines.Add(line);
      }

      lines.Count.ShouldBe(2);
      lines[0].ShouldContain("stderr1");
      lines[1].ShouldContain("stderr2");
    }

    public static async Task StreamCombined_Should_SupportCancellation()
    {
      using CancellationTokenSource cts = new();
      List<OutputLine> lines = [];

      IAsyncEnumerable<OutputLine> stream = Shell.Builder("sh")
        .WithArguments("-c", "for i in 1 2 3 4 5; do echo $i; sleep 0.1; done")
        .StreamCombinedAsync();

      try
      {
        await foreach (OutputLine line in stream.WithCancellation(cts.Token))
        {
          lines.Add(line);
          if (lines.Count >= 2)
          {
            await cts.CancelAsync();
          }
        }
      }
      catch (OperationCanceledException)
      {
        // Expected
      }

      lines.Count.ShouldBeGreaterThanOrEqualTo(2);
      lines.Count.ShouldBeLessThan(5);
    }
  }
}
