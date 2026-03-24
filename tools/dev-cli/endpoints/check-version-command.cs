#region Purpose
// Check whether source version is safe to release
#endregion
#region Design
// Delegates version-check logic to RepoCheckVersionService and formats CLI output
#endregion

namespace DevCli.Endpoints;

[NuruRoute("check-version", Description = "Verify version matches git tag")]
internal sealed class CheckVersionCommand : ICommand<Unit>
{
  [Option("strategy", Description = "Version check strategy override (git-tag or nuget-search)")]
  public string? Strategy { get; set; }

  [Option("package", Description = "Specific package override for nuget-search strategy")]
  public string? Package { get; set; }

  [Option("tag", Description = "Specific tag override for git-tag strategy")]
  public string? Tag { get; set; }

  internal sealed class Handler : ICommandHandler<CheckVersionCommand, Unit>
  {
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(CheckVersionCommand command, CancellationToken ct)
    {
      ArgumentNullException.ThrowIfNull(command);

      string? repoRoot = Git.FindRoot();
      if (repoRoot == null)
      {
        Terminal.WriteErrorLine("❌ Not in a git repository");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      NuGetPackageService nuGetPackageService = new();
      RepoConfigService repoConfigService = new();
      RepoCheckVersionService repoCheckVersionService = new
      (
        nuGetPackageService,
        repoConfigService
      );

      CheckVersionResult result = await repoCheckVersionService.CheckAsync
      (
        strategy: command.Strategy,
        package: command.Package,
        tag: command.Tag,
        cancellationToken: ct
      );

      if (string.IsNullOrWhiteSpace(result.Strategy))
      {
        Terminal.WriteErrorLine("❌ Could not complete version check (missing repository data or invalid strategy)");
        Environment.ExitCode = 1;
        return Unit.Value;
      }

      if (string.Equals(result.Strategy, "git-tag", StringComparison.OrdinalIgnoreCase))
      {
        WriteGitTagOutput(result);
      }
      else if (string.Equals(result.Strategy, "nuget-search", StringComparison.OrdinalIgnoreCase))
      {
        WriteNuGetSearchOutput(result);
      }
      else
      {
        Terminal.WriteLine($"Strategy: {result.Strategy}");
        Terminal.WriteLine(string.Empty);
        Terminal.WriteLine($"Version in source: {result.Version}");
        Terminal.WriteLine(string.Empty);
      }

      if (!result.IsNewVersion)
      {
        Environment.ExitCode = 1;
      }

      return Unit.Value;
    }

    private void WriteGitTagOutput(CheckVersionResult result)
    {
      string latestReleaseTag = result.LatestReleaseTag ?? "(none found)";

      Terminal.WriteLine("Strategy: git-tag (GitHub releases)");
      Terminal.WriteLine(string.Empty);
      Terminal.WriteLine($"Version in source: {result.Version}");
      Terminal.WriteLine($"Latest release tag on GitHub: {latestReleaseTag}");
      Terminal.WriteLine(string.Empty);

      if (result.IsNewVersion)
      {
        Terminal.WriteLine("✓ Version in source is new — safe to release.");
      }
      else
      {
        Terminal.WriteErrorLine("✗ Version in source already matches latest release tag.");
      }
    }

    private void WriteNuGetSearchOutput(CheckVersionResult result)
    {
      IReadOnlyList<string> checkedPackages = result.CheckedPackages ?? [];
      string checkedPackagesText = checkedPackages.Count > 0
        ? string.Join(", ", checkedPackages)
        : "(none)";
      string latestNuGetVersion = result.LatestNuGetVersion ?? "(none found)";

      Terminal.WriteLine("Strategy: nuget-search (NuGet packages)");
      Terminal.WriteLine(string.Empty);
      Terminal.WriteLine($"Version in source: {result.Version}");
      Terminal.WriteLine($"Latest NuGet version: {latestNuGetVersion}");
      Terminal.WriteLine($"Packages checked: {checkedPackagesText}");
      Terminal.WriteLine(string.Empty);

      if (result.IsNewVersion)
      {
        Terminal.WriteLine("✓ Version in source is new — safe to release.");
      }
      else
      {
        IReadOnlyList<string> alreadyPublishedPackages = result.AlreadyPublishedPackages ?? [];
        string publishedPackagesText = alreadyPublishedPackages.Count > 0
          ? string.Join(", ", alreadyPublishedPackages)
          : "(unknown)";
        Terminal.WriteErrorLine($"✗ Version in source already published for: {publishedPackagesText}");
      }
    }
  }
}
