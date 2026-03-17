#region Purpose
// Clean command - removes build artifacts and local NuGet caches
#endregion

using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static DevCli.ProcessHelpers;

namespace DevCli.Endpoints;

[NuruRoute("clean", Description = "Clean build artifacts and caches")]
public sealed class CleanCommand : ICommand<Unit>
{
  public sealed class Handler : ICommandHandler<CleanCommand, Unit>
  {
    public async ValueTask<Unit> Handle(CleanCommand command, CancellationToken cancellationToken)
    {
      Console.WriteLine("🧹 Cleaning build artifacts...");

      string? repoRoot = Git.FindRoot();
      if (repoRoot == null)
      {
        Console.WriteLine("❌ Not in a git repository");
        Environment.Exit(1);
      }

      // Clean the project
      string projectPath = Path.Combine(repoRoot, "source", "TimeWarp.Amuru", "TimeWarp.Amuru.csproj");

      if (File.Exists(projectPath))
      {
        Console.WriteLine("Cleaning project...");
        await RunProcessAsync("dotnet", $"clean \"{projectPath}\"");
      }

      // Clean local NuGet feed
      string localFeedPath = Path.Combine(repoRoot, "artifacts", "packages");
      if (Directory.Exists(localFeedPath))
      {
        foreach (string dir in Directory.GetDirectories(localFeedPath, "timewarp.amuru", SearchOption.AllDirectories))
        {
          Directory.Delete(dir, true);
          Console.WriteLine($"🗑️  Removed: {dir}");
        }

        foreach (string file in Directory.GetFiles(localFeedPath, "TimeWarp.Amuru.*.nupkg", SearchOption.AllDirectories))
        {
          File.Delete(file);
          Console.WriteLine($"🗑️  Removed: {file}");
        }
      }

      // Clean bin directory
      string binDir = Path.Combine(repoRoot, "bin");
      if (Directory.Exists(binDir))
      {
        Directory.Delete(binDir, true);
        Console.WriteLine($"🗑️  Removed bin directory: {binDir}");
      }

      Console.WriteLine("✅ Cleanup completed!");

      return Unit.Value;
    }
  }
}
