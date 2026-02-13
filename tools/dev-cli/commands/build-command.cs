using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static DevCli.ProcessHelpers;

namespace DevCli.Endpoints;

[NuruRoute("build", Description = "Build the TimeWarp.Amuru project")]
public sealed class BuildCommand : ICommand<Unit>
{
  public sealed class Handler : ICommandHandler<BuildCommand, Unit>
  {
    public async ValueTask<Unit> Handle(BuildCommand command, CancellationToken ct)
    {
      bool verbose = false;
      string verbosity = verbose ? "normal" : "minimal";

      Console.WriteLine("Building TimeWarp.Amuru...");

      string? repoRoot = Git.FindRoot();
      if (repoRoot == null)
      {
        Console.WriteLine("❌ Not in a git repository");
        Environment.Exit(1);
      }

      string projectPath = Path.Combine(repoRoot, "Source", "TimeWarp.Amuru", "TimeWarp.Amuru.csproj");

      if (!File.Exists(projectPath))
      {
        Console.WriteLine($"❌ Project not found: {projectPath}");
        Environment.Exit(1);
      }

      int exitCode = await RunProcessAsync("dotnet", $"build \"{projectPath}\" -c Release --verbosity {verbosity}");

      if (exitCode != 0)
      {
        Console.WriteLine($"❌ Build failed with exit code {exitCode}");
        Environment.Exit(1);
      }

      Console.WriteLine("✅ Build completed successfully!");

      return Unit.Value;
    }
  }
}
