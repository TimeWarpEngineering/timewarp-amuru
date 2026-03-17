#region Purpose
// Implementation of repository configuration operations
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Implementation of repository configuration operations.
/// </summary>
public sealed class RepoConfigService : IRepoConfigService
{
  private static readonly JsonSerializerOptions ReadOptions = new()
  {
    ReadCommentHandling = JsonCommentHandling.Skip,
    PropertyNameCaseInsensitive = true
  };

  private static readonly JsonSerializerOptions WriteOptions = new()
  {
    WriteIndented = true
  };

  private readonly string ConfigDirectory;
  private RepoConfig? CachedConfig;

  public RepoConfigService()
  {
    string? repoRoot = Git.FindRoot();
    if (repoRoot == null)
    {
      throw new InvalidOperationException("Not in a git repository");
    }

    ConfigDirectory = Path.Combine(repoRoot, ".timewarp");
    ConfigPath = Path.Combine(ConfigDirectory, "ganda.jsonc");
  }

  public string ConfigPath { get; }

  public async Task<RepoConfig> GetConfigAsync(CancellationToken cancellationToken = default)
  {
    if (CachedConfig != null)
    {
      return CachedConfig;
    }

    if (!File.Exists(ConfigPath))
    {
      CachedConfig = new RepoConfig();
      return CachedConfig;
    }

    string json = await File.ReadAllTextAsync(ConfigPath, cancellationToken);

    RepoConfig? config = JsonSerializer.Deserialize<RepoConfig>(json, ReadOptions);
    CachedConfig = config ?? new RepoConfig();
    return CachedConfig;
  }

  public async Task SetConfigAsync(RepoConfig config, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(config);

    if (!Directory.Exists(ConfigDirectory))
    {
      Directory.CreateDirectory(ConfigDirectory);
    }

    string json = JsonSerializer.Serialize(config, WriteOptions);
    await File.WriteAllTextAsync(ConfigPath, json, cancellationToken);
    CachedConfig = config;
  }
}
