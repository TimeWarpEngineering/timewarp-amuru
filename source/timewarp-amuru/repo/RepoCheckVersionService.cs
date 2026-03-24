#region Purpose
// Implementation of repository version checking operations
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Implementation of repository version checking operations.
/// </summary>
public sealed class RepoCheckVersionService : IRepoCheckVersionService
{
  private readonly ITerminal Terminal;
  private readonly INuGetPackageService NuGetPackageService;
  private readonly IRepoConfigService RepoConfigService;

  public RepoCheckVersionService
  (
    ITerminal terminal,
    INuGetPackageService nuGetPackageService,
    IRepoConfigService repoConfigService
  )
  {
    Terminal = terminal;
    NuGetPackageService = nuGetPackageService;
    RepoConfigService = repoConfigService;
  }

  public async Task<CheckVersionResult> CheckAsync
  (
    string? strategy = null,
    string? package = null,
    string? tag = null,
    CancellationToken cancellationToken = default
  )
  {
    string? repoRoot = Git.FindRoot();
    if (repoRoot == null)
    {
      await Terminal.WriteErrorLineAsync("Not in a git repository");
      return new CheckVersionResult(false, string.Empty, null, null);
    }

    string? version = await GetVersionFromDirectoryBuildPropsAsync(repoRoot, cancellationToken);
    if (version == null)
    {
      await Terminal.WriteErrorLineAsync("Could not find version in Directory.Build.props");
      return new CheckVersionResult(false, string.Empty, null, null);
    }

    RepoConfig config = await RepoConfigService.GetConfigAsync(cancellationToken);
    string resolvedStrategy = strategy ?? config.CheckVersion?.Strategy ?? "git-tag";

    if (string.Equals(resolvedStrategy, "git-tag", StringComparison.OrdinalIgnoreCase))
    {
      return await CheckGitTagVersionAsync(version, tag, cancellationToken);
    }

    if (string.Equals(resolvedStrategy, "nuget-search", StringComparison.OrdinalIgnoreCase))
    {
      return await CheckNuGetVersionAsync(version, package, config, cancellationToken);
    }

    await Terminal.WriteErrorLineAsync($"Unknown strategy: {resolvedStrategy}");
    return new CheckVersionResult(false, version, null, null);
  }

  private static async Task<string?> GetVersionFromDirectoryBuildPropsAsync(string repoRoot, CancellationToken cancellationToken)
  {
    string sourceDir = Path.Combine(repoRoot, "source");
    if (!Directory.Exists(sourceDir))
    {
      return null;
    }

    string[] buildPropsFiles = Directory.GetFiles(sourceDir, "Directory.Build.props", SearchOption.TopDirectoryOnly);
    if (buildPropsFiles.Length == 0)
    {
      return null;
    }

    string buildPropsPath = buildPropsFiles[0];
    string xml = await File.ReadAllTextAsync(buildPropsPath, cancellationToken);

    var doc = XDocument.Parse(xml);
    XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

    XElement? versionElement = doc.Descendants(ns + "Version").FirstOrDefault();
    if (versionElement == null)
    {
      versionElement = doc.Descendants("Version").FirstOrDefault();
    }

    return versionElement?.Value;
  }

  private async Task<CheckVersionResult> CheckGitTagVersionAsync(string version, string? tag, CancellationToken cancellationToken)
  {
    string? resolvedTag = tag;

    if (string.IsNullOrEmpty(resolvedTag))
    {
      resolvedTag = Environment.GetEnvironmentVariable("GITHUB_REF_NAME");
    }

    if (string.IsNullOrEmpty(resolvedTag))
    {
      // Check if the exact tag v{version} exists rather than finding the nearest ancestor tag.
      // git tag -l always returns exit code 0; an empty stdout means the tag does not exist.
      string expectedTag = $"v{version}";
      CommandOutput result = await Shell.Builder("git")
        .WithArguments("tag", "-l", expectedTag)
        .CaptureAsync(cancellationToken);

      string tagOutput = result.Stdout.Trim();
      if (!string.IsNullOrEmpty(tagOutput))
      {
        resolvedTag = tagOutput;
      }
    }

    if (string.IsNullOrEmpty(resolvedTag))
    {
      await Terminal.WriteLineAsync("No git tag found, assuming new version");
      return new CheckVersionResult(true, version, null, null);
    }

    string tagVersion = resolvedTag.StartsWith('v') ? resolvedTag[1..] : resolvedTag;
    bool isNewVersion = !string.Equals(version, tagVersion, StringComparison.OrdinalIgnoreCase);

    await Terminal.WriteLineAsync($"Version: {version}, Tag: {tagVersion}, IsNew: {isNewVersion}");
    return new CheckVersionResult(isNewVersion, version, resolvedTag, null);
  }

  private async Task<CheckVersionResult> CheckNuGetVersionAsync
  (
    string version,
    string? package,
    RepoConfig config,
    CancellationToken cancellationToken
  )
  {
    string packagesValue = package ?? config.CheckVersion?.Packages ?? string.Empty;
    if (string.IsNullOrWhiteSpace(packagesValue))
    {
      await Terminal.WriteErrorLineAsync("No packages specified for nuget-search strategy");
      return new CheckVersionResult(false, version, null, null);
    }

    string[] packages = packagesValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    List<string> alreadyPublished = [];

    foreach (string pkg in packages)
    {
      NuGetSearchResult? result = await NuGetPackageService.SearchAsync(pkg, cancellationToken);
      if (result != null && result.Versions.Count > 0)
      {
        string latestVersion = result.Versions[0].Version;
        if (string.Equals(version, latestVersion, StringComparison.OrdinalIgnoreCase))
        {
          alreadyPublished.Add(pkg);
        }
      }
    }

    bool isNewVersion = alreadyPublished.Count == 0;
    await Terminal.WriteLineAsync($"Version: {version}, Already published: {alreadyPublished.Count}/{packages.Length}");

    return new CheckVersionResult(isNewVersion, version, null, alreadyPublished.Count > 0 ? alreadyPublished : null);
  }
}
