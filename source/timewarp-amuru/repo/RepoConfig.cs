#region Purpose
// Configuration models for repository settings
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Root configuration for repository settings.
/// </summary>
public sealed class RepoConfig
{
  public RepoCheckVersionConfig? CheckVersion { get; set; }
}

/// <summary>
/// Configuration for version checking operations.
/// </summary>
public sealed class RepoCheckVersionConfig
{
  public string? Strategy { get; set; }
  public string? Packages { get; set; }
}
