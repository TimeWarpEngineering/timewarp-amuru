#region Purpose
// Implementation of repository version checking operations
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Implementation of repository version checking operations.
/// </summary>
public sealed class RepoCheckVersionService : IRepoCheckVersionService
{
  private readonly INuGetPackageService NuGetPackageService;
  private readonly IRepoConfigService RepoConfigService;

  public RepoCheckVersionService
  (
    INuGetPackageService nuGetPackageService,
    IRepoConfigService repoConfigService
  )
  {
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
      return new CheckVersionResult(false, string.Empty, string.Empty, null, null, null, null);
    }

    string? version = await GetVersionFromDirectoryBuildPropsAsync(repoRoot, cancellationToken);
    if (version == null)
    {
      return new CheckVersionResult(false, string.Empty, string.Empty, null, null, null, null);
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

    return new CheckVersionResult(false, version, string.Empty, null, null, null, null);
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

#pragma warning disable IDE0007
    XDocument doc = XDocument.Parse(xml);
#pragma warning restore IDE0007
    XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

    XElement? versionElement = doc.Descendants(ns + "Version").FirstOrDefault();
    if (versionElement == null)
    {
      versionElement = doc.Descendants("Version").FirstOrDefault();
    }

    return versionElement?.Value;
  }

  private static async Task<CheckVersionResult> CheckGitTagVersionAsync(string version, string? tag, CancellationToken cancellationToken)
  {
    string? latestTag = tag;

    if (string.IsNullOrWhiteSpace(latestTag))
    {
      latestTag = Environment.GetEnvironmentVariable("GITHUB_REF_NAME");
    }

    if (string.IsNullOrWhiteSpace(latestTag))
    {
      latestTag = await GetLatestGitTagAsync(cancellationToken);
    }

    if (string.IsNullOrWhiteSpace(latestTag))
    {
      return new CheckVersionResult(true, version, "git-tag", null, null, null, null);
    }

    string tagVersion = latestTag.StartsWith('v') ? latestTag[1..] : latestTag;
    bool isNewVersion = !string.Equals(version, tagVersion, StringComparison.OrdinalIgnoreCase);

    return new CheckVersionResult(isNewVersion, version, "git-tag", latestTag, null, null, null);
  }

  private static async Task<string?> GetLatestGitTagAsync(CancellationToken cancellationToken)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("tag", "--sort=-v:refname")
      .CaptureAsync(cancellationToken);

    if (result.ExitCode != 0)
    {
      return null;
    }

    string tagOutput = result.Stdout.Trim();
    if (string.IsNullOrWhiteSpace(tagOutput))
    {
      return null;
    }

    string normalizedOutput = tagOutput.Replace("\r\n", "\n", StringComparison.Ordinal);
    string[] lines = normalizedOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (lines.Length == 0)
    {
      return null;
    }

    return lines[0];
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
      return new CheckVersionResult(false, version, "nuget-search", null, null, null, null);
    }

    string[] packages = packagesValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    List<string> checkedPackages = [];
    List<string> alreadyPublished = [];
    string? latestNuGetVersion = null;

    foreach (string pkg in packages)
    {
      checkedPackages.Add(pkg);

      NuGetSearchResult? result = await NuGetPackageService.SearchAsync(pkg, cancellationToken);
      if (result != null && result.Versions.Count > 0)
      {
        string latestVersion = result.Versions[0].Version;

        if
        (
          latestNuGetVersion == null ||
          string.Compare(latestVersion, latestNuGetVersion, StringComparison.OrdinalIgnoreCase) > 0
        )
        {
          latestNuGetVersion = latestVersion;
        }

        if (string.Equals(version, latestVersion, StringComparison.OrdinalIgnoreCase))
        {
          alreadyPublished.Add(pkg);
        }
      }
    }

    bool isNewVersion = alreadyPublished.Count == 0;
    IReadOnlyList<string>? alreadyPublishedPackages = alreadyPublished.Count > 0 ? alreadyPublished : null;

    return new CheckVersionResult
    (
      isNewVersion,
      version,
      "nuget-search",
      null,
      latestNuGetVersion,
      checkedPackages,
      alreadyPublishedPackages
    );
  }
}
