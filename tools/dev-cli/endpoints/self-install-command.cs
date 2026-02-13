using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static DevCli.ProcessHelpers;

namespace DevCli.Endpoints;

[NuruRoute("self-install", Description = "AOT compile this CLI to ./bin/dev")]
public sealed class SelfInstallCommand : ICommand<Unit>
{
  public sealed class Handler : ICommandHandler<SelfInstallCommand, Unit>
  {
    public async ValueTask<Unit> Handle(SelfInstallCommand command, CancellationToken ct)
    {
      Console.WriteLine("🔨 Self-installing dev CLI...");

      string? repoRoot = Git.FindRoot();
      if (repoRoot == null)
      {
        Console.WriteLine("❌ Not in a git repository");
        Environment.Exit(1);
      }

      string binDir = Path.Combine(repoRoot, "bin");
      string devPath = Path.Combine(binDir, "dev");
      string projectPath = Path.Combine(repoRoot, "tools", "dev-cli", "dev-cli.csproj");

      Directory.CreateDirectory(binDir);

      if (File.Exists(devPath))
      {
        File.Delete(devPath);
      }

      // Publish as single-file (AOT disabled due to trim warnings in dependencies)
      Console.WriteLine("Publishing dev-cli...");
      int exitCode = await RunProcessAsync("dotnet", $"publish \"{projectPath}\" -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o \"{binDir}\"");

      if (exitCode != 0)
      {
        Console.WriteLine($"❌ AOT publish failed with exit code {exitCode}");
        Environment.Exit(1);
      }

      await RunProcessAsync("chmod", $"+x \"{devPath}\"");

      Console.WriteLine($"✅ dev CLI installed to: {devPath}");
      Console.WriteLine("\nYou can now use: ./bin/dev <command>");

      return Unit.Value;
    }
  }
}
