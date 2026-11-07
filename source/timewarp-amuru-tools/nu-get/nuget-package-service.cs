#region Purpose
// Reads nuget.org registration once per package id and derives search and latest versions from that read.
#endregion

#region Design
// Uses nuget.org's registration index to avoid the NuGet.Protocol dependency chain.
// Keeps NuGet.Versioning for NuGet-compatible parsing, normalization, and comparison.
// Registration metadata INCLUDES unlisted versions. Search keeps them (Listed=false)
// so existence checks see versions nuget.org still rejects on republish. Latest
// selection skips catalogEntry.listed == false. The service targets nuget.org
// package version checks, not authenticated/custom feeds.
// The parameterless constructor stores one walk per package id for the process.
// SearchAsync and GetLatestVersionsAsync share that walk, including a walk that
// is still in flight. The stored list keeps every version. A 404 and a failed
// walk are dropped when the attempt finishes so a later call can retry. Callers
// that already joined the attempt observe that one result. The caller's
// cancellation token cancels only that caller's wait. The HttpMessageHandler
// constructor keeps a separate cache so a stand-in handler cannot fill the
// process cache. The caller owns that handler.
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Reads package versions from the nuget.org v3 registration endpoint, including unlisted versions.
/// </summary>
public sealed class NuGetPackageService : INuGetPackageService
{
  private const string RegistrationBaseUrl = "https://api.nuget.org/v3/registration5-gz-semver2";
  private static readonly HttpClient SharedHttpClient = new
  (
    new HttpClientHandler
    {
      AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
      CheckCertificateRevocationList = true
    }
  );

  private static readonly ConcurrentDictionary<string, Task<RegistrationSnapshot>> ProcessRegistrations = new(StringComparer.Ordinal);

  private readonly ConcurrentDictionary<string, Task<RegistrationSnapshot>> Registrations;
  private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> SendAsync;

  /// <summary>
  /// Creates a service that calls the nuget.org registration endpoint
  /// and shares one registration walk per package id for the process.
  /// </summary>
  public NuGetPackageService()
    : this
    (
      ProcessRegistrations,
      static (request, cancellationToken) => SharedHttpClient.SendAsync(request, cancellationToken)
    )
  {
  }

  /// <summary>
  /// Creates a service that reads registration through <paramref name="handler"/>
  /// and stores walks on this instance only.
  /// The parameterless constructor stores walks in one process-wide cache.
  /// The caller owns <paramref name="handler"/> and this service does not dispose it.
  /// </summary>
  /// <param name="handler">Handler that receives each registration request.</param>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
  public NuGetPackageService(HttpMessageHandler handler)
    : this
    (
      new ConcurrentDictionary<string, Task<RegistrationSnapshot>>(StringComparer.Ordinal),
      CreateSend(handler)
    )
  {
  }

  private NuGetPackageService
  (
    ConcurrentDictionary<string, Task<RegistrationSnapshot>> registrations,
    Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync
  )
  {
    Registrations = registrations;
    SendAsync = sendAsync;
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Reliability",
    "CA2000",
    Justification = "disposeHandler is false. The invoker only forwards to the caller-owned handler and holds no resources of its own. The send delegate keeps it alive for the life of this service."
  )]
  private static Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> CreateSend(HttpMessageHandler handler)
  {
    ArgumentNullException.ThrowIfNull(handler);
    HttpMessageInvoker invoker = new(handler, disposeHandler: false);
    return (request, cancellationToken) => invoker.SendAsync(request, cancellationToken);
  }

  /// <summary>
  /// Returns every registration version for the package, including unlisted versions.
  /// </summary>
  /// <param name="packageId">Package id to look up on nuget.org.</param>
  /// <param name="cancellationToken">Token that cancels this call's wait. A shared registration read keeps running for other callers.</param>
  /// <returns>The package id and its versions, or null when nuget.org has no registration for that id.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="packageId"/> is null or whitespace.</exception>
  public async Task<NuGetSearchResult?> SearchAsync(string packageId, CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

    RegistrationSnapshot snapshot = await GetRegistrationAsync(packageId, cancellationToken).ConfigureAwait(false);
    if (!snapshot.Found || snapshot.Versions.Count == 0)
    {
      return null;
    }

    #pragma warning disable IDE0007
    List<NuGetPackageVersion> packageVersions = snapshot.Versions
#pragma warning restore IDE0007
      .OrderByDescending(static v => v.Version, VersionComparer.Default)
      .Select(static v => new NuGetPackageVersion(v.Version.ToNormalizedString(), Listed: v.Listed))
      .ToList();

    return new NuGetSearchResult(packageId, packageVersions);
  }

  /// <summary>
  /// Returns the highest listed stable version and the highest listed prerelease version.
  /// </summary>
  /// <param name="packageId">Package id to look up on nuget.org.</param>
  /// <param name="cancellationToken">Token that cancels this call's wait. A shared registration read keeps running for other callers.</param>
  /// <returns>The latest listed stable and prerelease versions, or null when none are listed.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="packageId"/> is null or whitespace.</exception>
  public async Task<PackageVersionInfo?> GetLatestVersionsAsync(string packageId, CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

    RegistrationSnapshot snapshot = await GetRegistrationAsync(packageId, cancellationToken).ConfigureAwait(false);
    if (!snapshot.Found)
    {
      return null;
    }

    NuGetVersion? stableVersion = null;
    NuGetVersion? prereleaseVersion = null;

    foreach (RegisteredPackageVersion registered in snapshot.Versions)
    {
      if (!registered.Listed)
      {
        continue;
      }

      NuGetVersion version = registered.Version;
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

  /// <summary>
  /// Parses a NuGet version, dropping a leading v, and returns the normalized version string.
  /// </summary>
  /// <param name="version">Version text to parse.</param>
  /// <returns>The normalized version, or null when the text is not a NuGet version.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="version"/> is null or whitespace.</exception>
  public string? ParseVersion(string version)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(version);

    string normalized = version.StartsWith('v', StringComparison.Ordinal) ? version[1..] : version;

    if (NuGetVersion.TryParse(normalized, out NuGetVersion? parsed))
    {
      return parsed.ToNormalizedString();
    }

    return null;
  }

  /// <summary>
  /// Compares two NuGet versions with NuGet version ordering.
  /// </summary>
  /// <param name="version1">The first version.</param>
  /// <param name="version2">The second version.</param>
  /// <returns>A negative value when <paramref name="version1"/> is older, zero when the versions are equal, and a positive value when <paramref name="version1"/> is newer.</returns>
  /// <exception cref="ArgumentException">Thrown when either version is null, whitespace, or not a NuGet version.</exception>
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

  /// <summary>
  /// Classifies the difference from the current version to the latest version as major, minor, patch, stable, or none.
  /// </summary>
  /// <param name="currentVersion">The version already in use.</param>
  /// <param name="latestVersion">The version being considered as an update.</param>
  /// <returns><c>none</c> when the latest version is not newer, <c>stable</c> when a prerelease moves to the same stable version, and otherwise <c>major</c>, <c>minor</c>, or <c>patch</c>.</returns>
  /// <exception cref="ArgumentException">Thrown when either version is null, whitespace, or not a NuGet version.</exception>
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

  private readonly record struct RegisteredPackageVersion(NuGetVersion Version, bool Listed);

  private readonly record struct RegistrationSnapshot
  (
    bool Found,
    IReadOnlyList<RegisteredPackageVersion> Versions
  );

  private async Task<RegistrationSnapshot> GetRegistrationAsync(string packageId, CancellationToken cancellationToken)
  {
    string key = packageId.ToLowerInvariant();
    Task<RegistrationSnapshot> registration = GetOrStartRegistration(key);
    return await registration.WaitAsync(cancellationToken).ConfigureAwait(false);
  }

  private Task<RegistrationSnapshot> GetOrStartRegistration(string key)
  {
    while (true)
    {
      if (Registrations.TryGetValue(key, out Task<RegistrationSnapshot>? existing))
      {
        if (IsReusable(existing))
        {
          return existing;
        }

        Registrations.TryRemove(new KeyValuePair<string, Task<RegistrationSnapshot>>(key, existing));
        continue;
      }

      TaskCompletionSource<RegistrationSnapshot> created = new(TaskCreationOptions.RunContinuationsAsynchronously);
      if (!Registrations.TryAdd(key, created.Task))
      {
        continue;
      }

      StartRegistrationRead(key, created);
      return created.Task;
    }
  }

  private static bool IsReusable(Task<RegistrationSnapshot> registration)
  {
    if (!registration.IsCompleted)
    {
      return true;
    }

    return registration.Status == TaskStatus.RanToCompletion && registration.Result.Found;
  }

  private void StartRegistrationRead(string key, TaskCompletionSource<RegistrationSnapshot> created)
  {
    // Queue the walk so GetOrStartRegistration publishes the in-flight task
    // before the walk can finish and drop a 404 or a failure.
    _ = Task.Run(() => ReadAndPublishAsync(key, created));
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "Every failed walk must leave the cache and complete the shared task. JSON, IO, and HTTP failures are all retryable and none of them may stick."
  )]
  private async Task ReadAndPublishAsync(string key, TaskCompletionSource<RegistrationSnapshot> created)
  {
    try
    {
      RegistrationSnapshot snapshot = await ReadRegistrationAsync(key).ConfigureAwait(false);
      if (!snapshot.Found)
      {
        created.TrySetResult(snapshot);
        Registrations.TryRemove(new KeyValuePair<string, Task<RegistrationSnapshot>>(key, created.Task));
        return;
      }

      created.TrySetResult(snapshot);
    }
    catch (Exception exception)
    {
      created.TrySetException(exception);
      Registrations.TryRemove(new KeyValuePair<string, Task<RegistrationSnapshot>>(key, created.Task));
      ObserveFault(created.Task);
    }
  }

  private static void ObserveFault(Task registration)
  {
    _ = registration.ContinueWith
    (
      static completed =>
      {
        _ = completed.Exception;
      },
      CancellationToken.None,
      TaskContinuationOptions.OnlyOnFaulted,
      TaskScheduler.Default
    );
  }

  private async Task<RegistrationSnapshot> ReadRegistrationAsync(string packageId)
  {
    string escapedPackageId = Uri.EscapeDataString(packageId);
    string url = $"{RegistrationBaseUrl}/{escapedPackageId}/index.json";

    using HttpRequestMessage request = new(HttpMethod.Get, url);
    using HttpResponseMessage response = await SendAsync(request, CancellationToken.None).ConfigureAwait(false);

    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
      return new RegistrationSnapshot(false, []);
    }

    response.EnsureSuccessStatusCode();

    using JsonDocument document = await ReadJsonDocumentAsync(response.Content, CancellationToken.None).ConfigureAwait(false);

    if (!document.RootElement.TryGetProperty("items", out JsonElement pagesElement) ||
        pagesElement.ValueKind != JsonValueKind.Array)
    {
      return new RegistrationSnapshot(true, []);
    }

    List<RegisteredPackageVersion> versions = [];
    foreach (JsonElement pageElement in pagesElement.EnumerateArray())
    {
      await AddPageVersionsAsync(pageElement, versions).ConfigureAwait(false);
    }

    return new RegistrationSnapshot(true, versions);
  }

  private async Task AddPageVersionsAsync
  (
    JsonElement pageElement,
    List<RegisteredPackageVersion> versions
  )
  {
    if (pageElement.TryGetProperty("items", out JsonElement itemsElement))
    {
      AddLeafVersions(itemsElement, versions);
      return;
    }

    await AddExternalPageVersionsAsync(pageElement, versions).ConfigureAwait(false);
  }

  private async Task AddExternalPageVersionsAsync
  (
    JsonElement pageElement,
    List<RegisteredPackageVersion> versions
  )
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
    using HttpResponseMessage response = await SendAsync(request, CancellationToken.None).ConfigureAwait(false);

    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
      return;
    }

    response.EnsureSuccessStatusCode();

    using JsonDocument pageDocument = await ReadJsonDocumentAsync(response.Content, CancellationToken.None).ConfigureAwait(false);

    if (!pageDocument.RootElement.TryGetProperty("items", out JsonElement itemsElement))
    {
      return;
    }

    AddLeafVersions(itemsElement, versions);
  }

  private static void AddLeafVersions
  (
    JsonElement itemsElement,
    List<RegisteredPackageVersion> versions
  )
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

      // Absent "listed" means listed. False is an unlisted version that still
      // occupies the id/version on the feed.
      bool listed = !catalogEntryElement.TryGetProperty("listed", out JsonElement listedElement)
        || listedElement.ValueKind != JsonValueKind.False;

      string? version = versionElement.GetString();
      if (version != null && NuGetVersion.TryParse(version, out NuGetVersion? parsedVersion))
      {
        versions.Add(new RegisteredPackageVersion(parsedVersion, listed));
      }
    }
  }

  private static async Task<JsonDocument> ReadJsonDocumentAsync(HttpContent content, CancellationToken cancellationToken)
  {
    byte[] bytes = await content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
    if (bytes.Length >= 2 && bytes[0] == 0x1F && bytes[1] == 0x8B)
    {
      using MemoryStream compressedStream = new(bytes);
      using System.IO.Compression.GZipStream gzipStream = new
      (
        compressedStream,
        System.IO.Compression.CompressionMode.Decompress
      );
      using MemoryStream decompressedStream = new();
      await gzipStream.CopyToAsync(decompressedStream, cancellationToken).ConfigureAwait(false);
      return JsonDocument.Parse(decompressedStream.ToArray());
    }

    return JsonDocument.Parse(bytes);
  }
}
