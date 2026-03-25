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

  public RepoCheckVersionService(INuGetPackageService nuGetPackageService)
  {
    NuGetPackageService = nuGetPackageService;
  }

  public async Task<GitTagCheckResult> CheckGitTagVersionAsync
  (
    string? tag = null,
    CancellationToken cancellationToken = default
  )
  {
    string? repoRoot = Git.FindRoot();
    if (repoRoot == null)
    {
      return new GitTagCheckResult(false, string.Empty, null);
    }

    string? version = await GetVersionFromDirectoryBuildPropsAsync(repoRoot, cancellationToken);
    if (version == null)
    {
      return new GitTagCheckResult(false, string.Empty, null);
    }

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
      return new GitTagCheckResult(true, version, null);
    }

    string tagVersion = latestTag.StartsWith('v') ? latestTag[1..] : latestTag;
    bool isNewVersion = !string.Equals(version, tagVersion, StringComparison.OrdinalIgnoreCase);

    return new GitTagCheckResult(isNewVersion, version, latestTag);
  }

  public async Task<NuGetCheckResult> CheckNuGetVersionAsync
  (
    IReadOnlyList<string> packages,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(packages);

    if (packages.Count == 0)
    {
      return new NuGetCheckResult(false, string.Empty, null, [], null);
    }

    string? repoRoot = Git.FindRoot();
    if (repoRoot == null)
    {
      return new NuGetCheckResult(false, string.Empty, null, [], null);
    }

    string? version = await GetVersionFromDirectoryBuildPropsAsync(repoRoot, cancellationToken);
    if (version == null)
    {
      return new NuGetCheckResult(false, string.Empty, null, [], null);
    }

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
          NuGetPackageService.CompareVersions(latestVersion, latestNuGetVersion) > 0
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

    return new NuGetCheckResult
    (
      isNewVersion,
      version,
      latestNuGetVersion,
      checkedPackages,
      alreadyPublishedPackages
    );
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
}
