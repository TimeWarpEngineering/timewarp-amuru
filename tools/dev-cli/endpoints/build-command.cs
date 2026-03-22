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

      string projectPath = Path.Combine(repoRoot, "source", "timewarp-amuru", "timewarp-amuru.csproj");

      if (!File.Exists(projectPath))
      {
        await TimeWarpTerminal.Default.WriteLineAsync($"❌ Project not found: {projectPath}");
        Environment.Exit(1);
      }

      int exitCode = await RunProcessAsync("dotnet", $"build \"{projectPath}\" -c Release --verbosity {verbosity}");

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
