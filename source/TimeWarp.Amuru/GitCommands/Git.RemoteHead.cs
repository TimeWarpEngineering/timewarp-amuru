namespace TimeWarp.Amuru;

/// <summary>
/// Git operations - RemoteHead configuration implementation.
/// </summary>
public static partial class Git
{
  /// <summary>
  /// Sets the remote HEAD to automatically determine the default branch.
  /// This configures the repository to automatically detect which branch
  /// is the default on the remote.
  /// </summary>
  /// <param name="repositoryPath">The path to the repository.</param>
  /// <param name="cancellationToken">Cancellation token for the operation.</param>
  /// <returns>True if configuration succeeded, false otherwise.</returns>
  /// <example>
  /// bool configured = await Git.SetRemoteHeadAutoAsync("/path/to/repo.git");
  /// if (configured)
  /// {
  ///   Console.WriteLine("Remote HEAD configured to auto-detect default branch");
  /// }
  /// </example>
  public static async Task<bool> SetRemoteHeadAutoAsync(string repositoryPath, CancellationToken cancellationToken = default)
  {
    CommandOutput result = await Shell.Builder("git")
      .WithArguments("remote", "set-head", "origin", "--auto")
      .WithWorkingDirectory(repositoryPath)
      .WithNoValidation()
      .CaptureAsync(cancellationToken);

    return result.Success;
  }
}
