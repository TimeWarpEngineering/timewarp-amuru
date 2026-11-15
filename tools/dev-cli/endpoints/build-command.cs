#region Purpose
// Build command - builds the TimeWarp.Amuru project
#endregion

using TimeWarp.Amuru;
using TimeWarp.Nuru;

namespace DevCli.Endpoints;

[NuruRoute("build", Description = "Build the TimeWarp.Amuru project")]
public sealed class BuildCommand : ICommand<Unit>
{
  public sealed class Handler : IRequestHandler<BuildCommand, Unit>
  {
    public async ValueTask<Unit> Handle(BuildCommand command, CancellationToken cancellationToken)
    {
      bool verbose = false;
      string verbosity = verbose ? "normal" : "minimal";

      await TimeWarpTerminal.Default.WriteLineAsync("Building TimeWarp.Amuru...");

      string? repoRoot = Git.FindRoot();
      if (repoRoot == null)
      {
        await TimeWarpTerminal.Default.WriteLineAsync("❌ Not in a git repository");
        Environment.Exit(1);
      }

      string solutionPath = Path.Combine(repoRoot, "timewarp-amuru.slnx");

      if (!File.Exists(solutionPath))
      {
        await TimeWarpTerminal.Default.WriteLineAsync($"❌ Solution not found: {solutionPath}");
        Environment.Exit(1);
      }

      // DotNet.Build tags this command span with build.command and build.configuration (task 144).
      CommandOutput output = await DotNet.Build(solutionPath)
        .WithConfiguration("Release")
        .WithVerbosity(verbosity)
        .WithNoValidation()
        .CaptureAsync(cancellationToken);

      if (!string.IsNullOrEmpty(output.Stdout))
        await TimeWarpTerminal.Default.WriteLineAsync(output.Stdout);

      if (!string.IsNullOrEmpty(output.Stderr))
        await TimeWarpTerminal.Default.WriteErrorLineAsync(output.Stderr);

      int exitCode = output.ExitCode;

      if (exitCode != 0)
      {
        await TimeWarpTerminal.Default.WriteLineAsync($"❌ Build failed with exit code {exitCode}");
        Environment.Exit(1);
      }

      await TimeWarpTerminal.Default.WriteLineAsync("✅ Build completed successfully!");

      return Unit.Value;
    }
  }
}
