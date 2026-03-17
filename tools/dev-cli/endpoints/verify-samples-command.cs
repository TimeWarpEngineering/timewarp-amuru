#region Purpose
// Verifies that all code samples in the repository compile
#endregion
#region Design
// Finds all .cs files in samples/ directory and compiles each one
// Uses dotnet build to verify compilation without running
#endregion

using static DevCli.ProcessHelpers;

namespace DevCli.Commands;

[NuruRoute("verify-samples", Description = "Verify code samples compile")]
internal sealed class VerifySamplesCommand : ICommand<Unit>
{
  internal sealed class Handler : ICommandHandler<VerifySamplesCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(VerifySamplesCommand command, CancellationToken ct)
    {
      Terminal.WriteLine("Verifying samples...");

      string? repoRoot = Git.FindRoot();
      if (repoRoot == null)
      {
        Terminal.WriteErrorLine("❌ Not in a git repository");
        Environment.ExitCode = 1;
        return Value;
      }

      string samplesDir = Path.Combine(repoRoot, "samples");
      if (!Directory.Exists(samplesDir))
      {
        Terminal.WriteErrorLine($"❌ Samples directory not found: {samplesDir}");
        Environment.ExitCode = 1;
        return Value;
      }

      string[] sampleFiles = Directory.GetFiles(samplesDir, "*.cs", SearchOption.AllDirectories);
      if (sampleFiles.Length == 0)
      {
        Terminal.WriteLine("No sample files found.");
        return Value;
      }

      Terminal.WriteLine($"Found {sampleFiles.Length} sample file(s)");

      int failedCount = 0;
      foreach (string sampleFile in sampleFiles)
      {
        string relativePath = Path.GetRelativePath(repoRoot, sampleFile);
        Terminal.WriteLine($"  Compiling: {relativePath}");

        int exitCode = await RunProcessAsync("dotnet", $"build \"{sampleFile}\" -c Release --verbosity minimal");

        if (exitCode != 0)
        {
          Terminal.WriteErrorLine($"    ❌ Failed: {relativePath}");
          failedCount++;
        }
        else
        {
          Terminal.WriteLine($"    ✅ Success: {relativePath}");
        }
      }

      if (failedCount > 0)
      {
        Terminal.WriteErrorLine($"\n❌ {failedCount} sample(s) failed to compile");
        Environment.ExitCode = 1;
      }
      else
      {
        Terminal.WriteLine($"\n✅ All {sampleFiles.Length} sample(s) verified successfully!");
      }

      return Value;
    }
  }
}
