namespace TimeWarp.Amuru;

/// <summary>
/// Git operations - Fetch implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Fetches updates from a remote repository.
  /// </summary>
  /// <param name="repositoryPath">The path to the repository.</param>
  /// <param name="remote">The remote name to fetch from (defaults to "origin").</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>True if fetch succeeded, false otherwise.</returns>
  /// <example>
  /// bool fetched = await Git.FetchAsync("/path/to/repo.git");
  /// if (fetched)
  /// {
  ///   Console.WriteLine("Fetched updates from origin");
  /// }
  /// </example>
  public static async Task<bool> FetchAsync(string repositoryPath, string remote = "origin", CancellationToken cancellationToken = default)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("fetch", remote)
      .WithWorkingDirectory(repositoryPath)
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    return result.Success;
  }
}
