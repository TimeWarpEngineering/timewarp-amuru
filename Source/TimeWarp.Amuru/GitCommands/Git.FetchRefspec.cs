namespace TimeWarp.Amuru;

/// <summary>
/// Git operations - FetchRefspec configuration implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Configures the fetch refspec to fetch all branches from the remote.
  /// This is typically needed after cloning a bare repository to ensure
  /// all remote branches are available for worktree operations.
  /// </summary>
  /// <param name="repositoryPath">The path to the bare repository.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>True if configuration succeeded, false otherwise.</returns>
  /// <example>
  /// bool configured = await Git.ConfigureFetchRefspecAsync("/path/to/repo.git");
  /// if (configured)
  /// {
  ///   Console.WriteLine("Fetch refspec configured successfully");
  /// }
  /// </example>
  public static async Task<bool> ConfigureFetchRefspecAsync(string repositoryPath, CancellationToken cancellationToken = default)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("config", "remote.origin.fetch", "+refs/heads/*:refs/remotes/origin/*")
      .WithWorkingDirectory(repositoryPath)
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    return result.Success;
  }
}
