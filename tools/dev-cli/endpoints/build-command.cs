#region Purpose
// Build command - builds the TimeWarp.Amuru project
#endregion

using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static DevCli.ProcessHelpers;

namespace DevCli.Endpoints;

[NuruRoute("build", Description = "Build the TimeWarp.Amuru project")]
public sealed class BuildCommand : ICommand<Unit>
{
  public sealed class Handler : ICommandHandler<BuildCommand, Unit>
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

      int exitCode = await RunProcessAsync("dotnet", $"build \"{solutionPath}\" -c Release --verbosity {verbosity}");

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
