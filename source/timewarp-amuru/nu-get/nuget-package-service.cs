#region Purpose
// Implementation of NuGet package operations using NuGet registration API
#endregion

#region Design
// Uses nuget.org's registration index to avoid the NuGet.Protocol dependency chain.
// Keeps NuGet.Versioning for NuGet-compatible parsing, normalization, and comparison.
// Registration metadata excludes unlisted packages and matches package update semantics.
// The service targets nuget.org package version checks, not authenticated/custom feeds.
#endregion

namespace TimeWarp.Amuru;

public sealed class NuGetPackageService : INuGetPackageService
{
  private const string RegistrationBaseUrl = "https://api.nuget.org/v3/registration5-gz-semver2";
  private static readonly HttpClient HttpClient = new
  (
    new HttpClientHandler
    {
      AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
      CheckCertificateRevocationList = true
    }
  );

  public NuGetPackageService()
  {
  }

  public async Task<NuGetSearchResult?> SearchAsync(string packageId, CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

    List<NuGetVersion>? versionList = await GetVersionsAsync(packageId, cancellationToken);
    if (versionList.Count == 0)
    {
      return null;
    }

    #pragma warning disable IDE0007
    List<NuGetPackageVersion> packageVersions = versionList
#pragma warning restore IDE0007
      .OrderByDescending(static v => v, VersionComparer.Default)
      .Select(static v => new NuGetPackageVersion(v.ToNormalizedString()))
      .ToList();

    return new NuGetSearchResult(packageId, packageVersions);
  }

  public async Task<PackageVersionInfo?> GetLatestVersionsAsync(string packageId, CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

    List<NuGetVersion>? versions = await GetVersionsAsync(packageId, cancellationToken);

    NuGetVersion? stableVersion = null;
    NuGetVersion? prereleaseVersion = null;

    foreach (NuGetVersion version in versions)
    {
      if (!version.IsPrerelease)
      {
        if (stableVersion == null || version > stableVersion)
        {
          stableVersion = version;
        }
      }
      else
      {
        if (prereleaseVersion == null || version > prereleaseVersion)
        {
          prereleaseVersion = version;
        }
      }
    }

    if (stableVersion == null && prereleaseVersion == null)
    {
      return null;
    }

    return new PackageVersionInfo
    (
      stableVersion?.ToNormalizedString(),
      prereleaseVersion?.ToNormalizedString()
    );
  }

  public string? ParseVersion(string version)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(version);

    string normalized = version.StartsWith('v') ? version[1..] : version;

    if (NuGetVersion.TryParse(normalized, out NuGetVersion? parsed))
    {
      return parsed.ToNormalizedString();
    }

    return null;
  }

  public int CompareVersions(string version1, string version2)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(version1);
    ArgumentException.ThrowIfNullOrWhiteSpace(version2);

    if (!NuGetVersion.TryParse(version1, out NuGetVersion? v1))
    {
      throw new ArgumentException($"Invalid version format: {version1}", nameof(version1));
    }

    if (!NuGetVersion.TryParse(version2, out NuGetVersion? v2))
    {
      throw new ArgumentException($"Invalid version format: {version2}", nameof(version2));
    }

    return v1.CompareTo(v2);
  }

  public string GetUpdateType(string currentVersion, string latestVersion)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(currentVersion);
    ArgumentException.ThrowIfNullOrWhiteSpace(latestVersion);

    if (!NuGetVersion.TryParse(currentVersion, out NuGetVersion? current))
    {
      throw new ArgumentException($"Invalid version format: {currentVersion}", nameof(currentVersion));
    }

    if (!NuGetVersion.TryParse(latestVersion, out NuGetVersion? latest))
    {
      throw new ArgumentException($"Invalid version format: {latestVersion}", nameof(latestVersion));
    }

    if (current.CompareTo(latest) >= 0)
    {
      return "none";
    }

    if (current.IsPrerelease && !latest.IsPrerelease &&
        current.Major == latest.Major &&
        current.Minor == latest.Minor &&
        current.Patch == latest.Patch)
    {
      return "stable";
    }

    if (latest.Major > current.Major) return "major";
    if (latest.Minor > current.Minor) return "minor";
    return "patch";
  }

  private static async Task<List<NuGetVersion>> GetVersionsAsync(string packageId, CancellationToken cancellationToken)
  {
    string lowerPackageId = packageId.ToLowerInvariant();
    string escapedPackageId = Uri.EscapeDataString(lowerPackageId);
    string url = $"{RegistrationBaseUrl}/{escapedPackageId}/index.json";

    using HttpRequestMessage request = new(HttpMethod.Get, url);
    using HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);

    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
      return [];
    }

    response.EnsureSuccessStatusCode();

    using JsonDocument document = await ReadJsonDocumentAsync(response.Content, cancellationToken);

    if (!document.RootElement.TryGetProperty("items", out JsonElement pagesElement) ||
        pagesElement.ValueKind != JsonValueKind.Array)
    {
      return [];
    }

    List<NuGetVersion> versions = [];
    foreach (JsonElement pageElement in pagesElement.EnumerateArray())
    {
      await AddPageVersionsAsync(pageElement, versions, cancellationToken);
    }

    return versions;
  }

  private static async Task AddPageVersionsAsync
  (
    JsonElement pageElement,
    List<NuGetVersion> versions,
    CancellationToken cancellationToken
  )
  {
    if (!pageElement.TryGetProperty("items", out JsonElement itemsElement))
    {
      if (!pageElement.TryGetProperty("@id", out JsonElement pageUrlElement))
      {
        return;
      }

      string? pageUrl = pageUrlElement.GetString();
      if (string.IsNullOrWhiteSpace(pageUrl))
      {
        return;
      }

      using HttpRequestMessage request = new(HttpMethod.Get, pageUrl);
      using HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
      response.EnsureSuccessStatusCode();

      using JsonDocument pageDocument = await ReadJsonDocumentAsync(response.Content, cancellationToken);

      if (!pageDocument.RootElement.TryGetProperty("items", out itemsElement))
      {
        return;
      }
    }

    AddLeafVersions(itemsElement, versions);
  }

  private static void AddLeafVersions(JsonElement itemsElement, List<NuGetVersion> versions)
  {
    if (itemsElement.ValueKind != JsonValueKind.Array)
    {
      return;
    }

    foreach (JsonElement itemElement in itemsElement.EnumerateArray())
    {
      if (!itemElement.TryGetProperty("catalogEntry", out JsonElement catalogEntryElement) ||
          !catalogEntryElement.TryGetProperty("version", out JsonElement versionElement))
      {
        continue;
      }

      string? version = versionElement.GetString();
      if (version != null && NuGetVersion.TryParse(version, out NuGetVersion? parsedVersion))
      {
        versions.Add(parsedVersion);
      }
    }
  }

  private static async Task<JsonDocument> ReadJsonDocumentAsync(HttpContent content, CancellationToken cancellationToken)
  {
    byte[] bytes = await content.ReadAsByteArrayAsync(cancellationToken);
    if (bytes.Length >= 2 && bytes[0] == 0x1F && bytes[1] == 0x8B)
    {
      using MemoryStream compressedStream = new(bytes);
      using System.IO.Compression.GZipStream gzipStream = new
      (
        compressedStream,
        System.IO.Compression.CompressionMode.Decompress
      );
      using MemoryStream decompressedStream = new();
      await gzipStream.CopyToAsync(decompressedStream, cancellationToken);
      return JsonDocument.Parse(decompressedStream.ToArray());
    }

    return JsonDocument.Parse(bytes);
  }
}
