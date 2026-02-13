namespace TimeWarp.Amuru.Testing;

/// <summary>
/// Shared test helper methods for Jaribu test files
/// </summary>
public static class TestHelpers
{
  /// <summary>
  /// Creates a temporary file and makes it executable on Unix systems
  /// </summary>
  public static async Task<string> CreateExecutableTempFile()
  {
    string tempFile = Path.GetTempFileName();

    if (!OperatingSystem.IsWindows())
    {
      await Shell.Builder("chmod").WithArguments("+x", tempFile).RunAsync();
    }

    return tempFile;
  }

  /// <summary>
  /// Creates multiple executable temp files
  /// </summary>
  public static async Task<List<string>> CreateExecutableTempFiles(int count)
  {
    List<string> files = [];

    for (int i = 0; i < count; i++)
    {
      files.Add(await CreateExecutableTempFile());
    }

    return files;
  }

  /// <summary>
  /// Cleans up temp files
  /// </summary>
  public static void CleanupTempFiles(params string[] files)
  {
    ArgumentNullException.ThrowIfNull(files);
    foreach (string file in files)
    {
      if (File.Exists(file))
      {
        File.Delete(file);
      }
    }
  }

  /// <summary>
  /// Runs a test action with temp files, ensuring cleanup
  /// </summary>
  public static async Task WithTempFiles(int count, Func<List<string>, Task> testAction)
  {
    ArgumentNullException.ThrowIfNull(testAction);
    List<string> tempFiles = await CreateExecutableTempFiles(count);

    try
    {
      await testAction(tempFiles);
    }
    finally
    {
      CleanupTempFiles([.. tempFiles]);
    }
  }
}
