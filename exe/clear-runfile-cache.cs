#!/usr/bin/dotnet --
#:package TimeWarp.Nuru

using TimeWarp.Nuru;
using static System.Console;

NuruApp app =
  new NuruAppBuilder()
  .AddAutoHelp()
  .AddRoute
  (
    "{filePath|Full path to the file to clear cache for} " +
    "--single|Clear only the first matching cache entry",
    ClearCache,
    "Clear the runfile cache for a specific .NET script file"
  )
  .Build();

return await app.RunAsync(args);

static void ClearCache(string filePath, bool single)
{
  ClearRunfileCache(filePath, deleteAllPrefixed: !single);
}

/// <summary>
/// Clears the runfile cache entry(ies) for a specific file to ensure fresh compilation on the next run.
/// Deletes top-level cache dirs prefixed with the filename (e.g., "jaribu-05-cache-clearing-<hash>").
/// </summary>
/// <param name="filePath">Full path to the file (e.g., .cs script).</param>
/// <param name="deleteAllPrefixed">If true, deletes all matching prefixed dirs (default: true, for completeness).</param>
static void ClearRunfileCache(string filePath, bool deleteAllPrefixed = true)
{
  string runfileCacheRoot = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".local", "share", "dotnet", "runfile"
  );

  if (!Directory.Exists(runfileCacheRoot))
  {
    WriteLine($"⚠ Runfile cache directory does not exist: {runfileCacheRoot}");
    return;
  }

  if (!File.Exists(filePath))
  {
    WriteLine($"⚠ File does not exist: {filePath}");
    return;
  }

  string filePrefix = Path.GetFileNameWithoutExtension(filePath).ToUpperInvariant() + "-";
  bool clearedAny = false;

  foreach (string cacheDir in Directory.GetDirectories(runfileCacheRoot))
  {
    string cacheDirName = Path.GetFileName(cacheDir).ToUpperInvariant();
    if (cacheDirName.StartsWith(filePrefix, StringComparison.OrdinalIgnoreCase))
    {
      try
      {
        Directory.Delete(cacheDir, recursive: true);
        WriteLine($"✓ Cleared runfile cache for {Path.GetFileName(filePath)}: {Path.GetFileName(cacheDir)}");
        clearedAny = true;

        if (!deleteAllPrefixed)
        {
          return; // Stop after first match (if not deleting all)
        }
      }
      catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
      {
        WriteLine($"⚠ Skipped clearing {Path.GetFileName(cacheDir)} (locked/in use): {ex.Message}");
        // Continue to next; don't fail the whole op
      }
    }
  }

  if (!clearedAny)
  {
    WriteLine($"⚠ No runfile cache prefixed with '{filePrefix}' found for {Path.GetFileName(filePath)}; proceeding.");
  }
}
