#region Purpose
// Dev CLI command for TimeWarp.Amuru CI/CD pipeline
#endregion

// ═══════════════════════════════════════════════════════════════════════════════
// CI COMMAND
// ═══════════════════════════════════════════════════════════════════════════════
// Orchestrates the full CI/CD pipeline with mode detection.
// Auto-detects mode from GITHUB_EVENT_NAME or accepts explicit --mode flag.
//
// Modes:
//   pr/merge:  clean -> build -> verify-samples -> test -> check-version
//   release:   clean -> build -> push

using DevCli.Endpoints;

namespace DevCli.Commands;

[NuruRoute("workflow", Description = "Run full CI/CD pipeline")]
internal sealed class WorkflowCommand : ICommand<Unit>
{
  [Option("mode", "m", Description = "CI mode: pr, merge, or release (auto-detected from GITHUB_EVENT_NAME if not specified)")]
  public string? Mode { get; set; }

  [Option("api-key", Description = "NuGet API key for publishing (from OIDC Trusted Publishing)")]
  public string? ApiKey { get; set; }

  internal sealed class Handler : ICommandHandler<WorkflowCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(WorkflowCommand command, CancellationToken ct)
    {
      CiMode mode = DetermineMode(command.Mode);

      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine($"  CI/CD Pipeline - Mode: {mode}");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("");

      if (mode == CiMode.Release)
      {
        await RunReleaseWorkflowAsync(command.ApiKey);
      }
      else
      {
        await RunPrWorkflowAsync();
      }

      return Unit.Value;
    }

    private CiMode DetermineMode(string? explicitMode)
    {
      if (!string.IsNullOrEmpty(explicitMode))
      {
        return explicitMode.ToLowerInvariant() switch
        {
          "pr" => CiMode.Pr,
          "merge" => CiMode.Merge,
          "release" => CiMode.Release,
          _ => CiMode.Pr
        };
      }

      string? eventName = Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME");

      CiMode mode = eventName switch
      {
        "pull_request" => CiMode.Pr,
        "push" => CiMode.Merge,
        "release" => CiMode.Release,
        "workflow_dispatch" => CiMode.Release,
        _ => CiMode.Pr
      };

      string displayEventName = eventName ?? "(not set)";
      Terminal.WriteLine($"Detected GITHUB_EVENT_NAME: {displayEventName} -> Mode: {mode}");
      return mode;
    }

    private async Task RunPrWorkflowAsync()
    {
      Terminal.WriteLine("Pipeline: clean -> build -> verify-samples -> test -> check-version");
      Terminal.WriteLine("");

      Environment.ExitCode = 0;

      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 1/5: Clean");
      Terminal.WriteLine("===============================================================================");
      CleanCommand.Handler cleanHandler = new();
      await cleanHandler.Handle(new CleanCommand(), CancellationToken.None);

      if (StopOnFailure("Clean"))
      {
        return;
      }

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 2/5: Build");
      Terminal.WriteLine("===============================================================================");
      BuildCommand.Handler buildHandler = new();
      await buildHandler.Handle(new BuildCommand(), CancellationToken.None);

      if (StopOnFailure("Build"))
      {
        return;
      }

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 3/5: Verify Samples");
      Terminal.WriteLine("===============================================================================");
      VerifySamplesCommand.Handler verifySamplesHandler = new(Terminal);
      await verifySamplesHandler.Handle(new VerifySamplesCommand(), CancellationToken.None);

      if (StopOnFailure("Verify Samples"))
      {
        return;
      }

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 4/5: Test");
      Terminal.WriteLine("===============================================================================");
      TestCommand.Handler testHandler = new();
      await testHandler.Handle(new TestCommand(), CancellationToken.None);

      if (StopOnFailure("Test"))
      {
        return;
      }

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 5/5: Check Version");
      Terminal.WriteLine("===============================================================================");
      CheckVersionCommand.Handler checkVersionHandler = new(Terminal);
      await checkVersionHandler.Handle(new CheckVersionCommand(), CancellationToken.None);

      if (StopOnFailure("Check Version"))
      {
        return;
      }

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Pipeline SUCCEEDED");
      Terminal.WriteLine("===============================================================================");
    }

    private async Task RunReleaseWorkflowAsync(string? apiKey)
    {
      Terminal.WriteLine("Pipeline: clean -> build -> push");
      Terminal.WriteLine("");

      Environment.ExitCode = 0;

      string? repoRoot = Git.FindRoot();
      if (repoRoot == null)
      {
        Terminal.WriteErrorLine("❌ Not in a git repository");
        Environment.ExitCode = 1;
        return;
      }

      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 1/3: Clean");
      Terminal.WriteLine("===============================================================================");
      CleanCommand.Handler cleanHandler = new();
      await cleanHandler.Handle(new CleanCommand(), CancellationToken.None);

      if (StopOnFailure("Clean"))
      {
        return;
      }

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 2/3: Build");
      Terminal.WriteLine("===============================================================================");
      BuildCommand.Handler buildHandler = new();
      await buildHandler.Handle(new BuildCommand(), CancellationToken.None);

      if (StopOnFailure("Build"))
      {
        return;
      }

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Step 3/3: Push to NuGet");
      Terminal.WriteLine("===============================================================================");
      await PushPackageAsync(repoRoot, apiKey);

      if (StopOnFailure("Push to NuGet"))
      {
        return;
      }

      Terminal.WriteLine("");
      Terminal.WriteLine("===============================================================================");
      Terminal.WriteLine("  Pipeline SUCCEEDED - Package published to NuGet.org");
      Terminal.WriteLine("===============================================================================");
    }

    private async Task PushPackageAsync(string repoRoot, string? apiKey)
    {
      string artifactsDir = Path.Combine(repoRoot, "artifacts", "packages");

      // Versions are per package: the repo version in source/Directory.Build.props is the
      // core (TimeWarp.Amuru) version; TimeWarp.Amuru.Tools overrides <Version> in its csproj.
      string coreVersion = ReadVersion(Path.Combine(repoRoot, "source", "Directory.Build.props"));
      string toolsVersion = ReadVersion(Path.Combine(repoRoot, "source", "timewarp-amuru-tools", "timewarp-amuru-tools.csproj"));

      (string PackageId, string Version)[] packages =
      [
        ("TimeWarp.Amuru", coreVersion),
        ("TimeWarp.Amuru.Tools", toolsVersion),
      ];

      foreach ((string packageId, string version) in packages)
      {
        string nupkgPath = Path.Combine(artifactsDir, $"{packageId}.{version}.nupkg");

        if (!File.Exists(nupkgPath))
        {
          throw new FileNotFoundException($"Package not found: {nupkgPath}");
        }

        Terminal.WriteLine($"Pushing {packageId}.{version}.nupkg...");

        List<string> args = ["nuget", "push", nupkgPath, "--source", "https://api.nuget.org/v3/index.json", "--no-symbols"];

        if (!string.IsNullOrEmpty(apiKey))
        {
          args.AddRange(["--api-key", apiKey]);
        }

        int exitCode = await Shell.Builder("dotnet")
          .WithArguments([.. args])
          .WithWorkingDirectory(repoRoot)
          .WithNoValidation()
          .RunAsync();

        if (exitCode != 0)
        {
          Terminal.WriteErrorLine($"\n❌ NuGet push failed for {packageId} with exit code {exitCode}");
          Environment.ExitCode = 1;
          return;
        }
      }

      Terminal.WriteLine("\n✅ Packages pushed successfully!");
    }

    private static string ReadVersion(string msbuildFilePath)
    {
      XDocument doc = XDocument.Load(msbuildFilePath);
      string? version = doc.Descendants("Version").FirstOrDefault()?.Value;

      if (string.IsNullOrEmpty(version))
      {
        throw new InvalidOperationException($"Could not determine version from {msbuildFilePath}");
      }

      return version;
    }

    private bool StopOnFailure(string stepName)
    {
      if (Environment.ExitCode == 0)
      {
        return false;
      }

      Terminal.WriteErrorLine("");
      Terminal.WriteErrorLine("===============================================================================");
      Terminal.WriteErrorLine($"  Pipeline FAILED - {stepName} failed");
      Terminal.WriteErrorLine("===============================================================================");
      return true;
    }
  }
}

internal enum CiMode
{
  Pr,
  Merge,
  Release
}
