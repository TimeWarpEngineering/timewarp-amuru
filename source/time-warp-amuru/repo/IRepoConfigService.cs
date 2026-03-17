#region Purpose
// Interface for repository configuration operations
#endregion

namespace TimeWarp.Amuru;

/// <summary>
/// Provides operations for reading and writing repository configuration.
/// </summary>
public interface IRepoConfigService
{
  /// <summary>
  /// Gets the path to the configuration file.
  /// </summary>
  string ConfigPath { get; }

  /// <summary>
  /// Gets the repository configuration.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The repository configuration</returns>
  Task<RepoConfig> GetConfigAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Sets the repository configuration.
  /// </summary>
  /// <param name="config">The configuration to save</param>
  /// <param name="cancellationToken">Cancellation token</param>
  Task SetConfigAsync(RepoConfig config, CancellationToken cancellationToken = default);
}
