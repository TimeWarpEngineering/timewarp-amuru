#region Purpose
// Test command - runs the integration test suite
#endregion

using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static DevCli.ProcessHelpers;

namespace DevCli.Endpoints;

[NuruRoute("test", Description = "Run the integration test suite")]
public sealed class TestCommand : ICommand<Unit>
{
  public sealed class Handler : IRequestHandler<TestCommand, Unit>
  {
    public async ValueTask<Unit> Handle(TestCommand command, CancellationToken cancellationToken)
    {
      await TimeWarpTerminal.Default.WriteLineAsync("🧪 Running TimeWarp.Amuru Test Suite...");

      string? repoRoot = Git.FindRoot();
      if (repoRoot == null)
      {
        await TimeWarpTerminal.Default.WriteLineAsync("❌ Not in a git repository");
        Environment.Exit(1);
      }

      string testsDir = Path.Combine(repoRoot, "tests", "timewarp-amuru", "multi-file-runners");
      string runTestsPath = Path.Combine(testsDir, "run-tests.cs");

      if (!File.Exists(runTestsPath))
      {
        await TimeWarpTerminal.Default.WriteLineAsync($"❌ run-tests.cs not found: {runTestsPath}");
        Environment.Exit(1);
      }

      await TimeWarpTerminal.Default.WriteLineAsync($"Working from: {testsDir}");

      string originalDirectory = Directory.GetCurrentDirectory();
      try
      {
        Directory.SetCurrentDirectory(testsDir);
        // DotNet.Run carries the dotnet command span and TRACEPARENT into the suite (task 150).
        CommandOutput output = await DotNet.Run()
          .WithFile(runTestsPath)
          .WithNoValidation()
          .CaptureAsync(cancellationToken);
        int exitCode = await WriteOutputAsync(output);

        if (exitCode != 0)
        {
          await TimeWarpTerminal.Default.WriteLineAsync($"❌ Tests failed with exit code {exitCode}");
          Environment.Exit(1);
        }

        await TimeWarpTerminal.Default.WriteLineAsync("✅ All tests passed!");
      }
      finally
      {
        Directory.SetCurrentDirectory(originalDirectory);
      }

      return Unit.Value;
    }
  }
}
