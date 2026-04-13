#region Purpose
// Cache for NuGet SourceRepository instances per source URL
#endregion

#region Design
// SourceRepository creation involves network calls and resource initialization.
// Caching avoids repeated initialization when querying the same feed multiple times.
// Uses ConcurrentDictionary for thread-safe access without explicit locking.
#endregion

namespace TimeWarp.Amuru;

public sealed class NuGetSourceCache
{
  private readonly ConcurrentDictionary<string, SourceRepository> Repositories = new(StringComparer.OrdinalIgnoreCase);

  #pragma warning disable CA1054
  public SourceRepository GetOrCreate(string sourceUrl)
#pragma warning restore CA1054
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(sourceUrl);

    return Repositories.GetOrAdd
    (
      sourceUrl,
      static url =>
      {
        PackageSource packageSource = new(url);
        return Repository.CreateSource(Repository.Provider.GetCoreV3(), packageSource);
      }
    );
  }

  public void Clear()
  {
    Repositories.Clear();
  }
}