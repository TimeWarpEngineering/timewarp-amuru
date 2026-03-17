#region Purpose
// Implementation of NuGet package operations using dotnet package search
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Implementation of NuGet package operations using the dotnet CLI.
/// </summary>
public sealed class NuGetPackageService : INuGetPackageService
{
  public async Task<NuGetSearchResult?> SearchAsync(string packageId, CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

    CommandOutput result = await Shell.Builder("dotnet")
      .WithArguments("package", "search", packageId, "--exact-match", "--format", "json")
      .CaptureAsync(cancellationToken);

    if (result.ExitCode != 0)
    {
      return null;
    }

    return ParseSearchResult(packageId, result.Stdout);
  }

  private static NuGetSearchResult? ParseSearchResult(string packageId, string jsonOutput)
  {
    using var document = JsonDocument.Parse(jsonOutput);
    JsonElement root = document.RootElement;

    if (!root.TryGetProperty("searchResult", out JsonElement searchResults))
    {
      return null;
    }

    foreach (JsonElement searchResult in searchResults.EnumerateArray())
    {
      if (!searchResult.TryGetProperty("packages", out JsonElement packages))
      {
        continue;
      }

      foreach (JsonElement package in packages.EnumerateArray())
      {
        if (!package.TryGetProperty("id", out JsonElement idElement))
        {
          continue;
        }

        string? id = idElement.GetString();
        if (id == null || !string.Equals(id, packageId, StringComparison.OrdinalIgnoreCase))
        {
          continue;
        }

        if (!package.TryGetProperty("version", out JsonElement versionElement))
        {
          continue;
        }

        string? latestVersion = versionElement.GetString();
        if (latestVersion == null)
        {
          continue;
        }

        List<NuGetPackageVersion> versions =
        [
          new NuGetPackageVersion(latestVersion)
        ];

        return new NuGetSearchResult(id, versions);
      }
    }

    return null;
  }
}
