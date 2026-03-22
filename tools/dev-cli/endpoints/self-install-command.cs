#region Purpose
// Self-install command - AOT compiles and installs dev CLI to ./bin
#endregion

// ═══════════════════════════════════════════════════════════════════════════════
// SELF-INSTALL COMMAND
// ═══════════════════════════════════════════════════════════════════════════════
// AOT compiles and installs the dev CLI to ./bin for fast execution.

namespace DevCli.Endpoints;

using System.Runtime.InteropServices;
using TimeWarp.Amuru;
using TimeWarp.Nuru;

/// <summary>
/// AOT compile and install dev CLI to ./bin directory.
/// </summary>
[NuruRoute("self-install", Description = "AOT compile and install dev CLI to ./bin")]
internal sealed class SelfInstallCommand : ICommand<Unit>
{
  internal sealed class Handler : ICommandHandler<SelfInstallCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(SelfInstallCommand command, CancellationToken ct)
    {
      // Get repo root using Git.FindRoot
      string? repoRoot = Git.FindRoot();

      if (repoRoot is null)
      {
        Terminal.WriteLine("ERROR: Could not find git repository root (.git not found)");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      string devCliSource = Path.Combine(repoRoot, "tools", "dev-cli", "dev.cs");
      string outputPath = Path.Combine(repoRoot, "bin");
      string rid = GetRuntimeIdentifier();

      Terminal.WriteLine("Installing dev CLI as AOT binary...");
      Terminal.WriteLine($"Source: {devCliSource}");
      Terminal.WriteLine($"Output: {outputPath}/dev");
      Terminal.WriteLine($"Runtime: {rid}");

      // Ensure bin directory exists
      Directory.CreateDirectory(outputPath);

      Terminal.WriteLine("Publishing dev CLI...");

      CommandOutput result = await DotNet.Publish(devCliSource)
        .WithOutput(outputPath)
        .WithNoValidation()
        .CaptureAsync();

      if (!result.Success)
      {
        Terminal.WriteLine("=== BUILD FAILED ===");
        Terminal.WriteLine($"Exit code: {result.ExitCode}");
        Terminal.WriteLine("=== STDOUT ===");
        Terminal.WriteLine(result.Stdout);
        Terminal.WriteLine("=== STDERR ===");
        Terminal.WriteLine(result.Stderr);
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      // Verify the binary was created
      string binaryName = rid.StartsWith("win", StringComparison.OrdinalIgnoreCase) ? "dev.exe" : "dev";
      string binaryPath = Path.Combine(outputPath, binaryName);

      if (File.Exists(binaryPath))
      {
        FileInfo info = new(binaryPath);
        Terminal.WriteLine($"Successfully installed dev CLI to {outputPath}");
        Terminal.WriteLine($"Size: {info.Length / 1024.0 / 1024.0:F1} MB");
      }
      else
      {
        Terminal.WriteLine($"ERROR: Binary not found at {binaryPath}");
        Environment.ExitCode = 1;
      }

      return Unit.Value;
    }

    private static string GetRuntimeIdentifier()
    {
      if (OperatingSystem.IsWindows())
      {
        return Environment.Is64BitOperatingSystem ? "win-x64" : "win-x86";
      }
      else if (OperatingSystem.IsMacOS())
      {
        return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";
      }
      else if (OperatingSystem.IsLinux())
      {
        return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "linux-arm64" : "linux-x64";
      }

      return "linux-x64";
    }
  }
}
