#region Purpose
// TODO: Add purpose description
#endregion

namespace TimeWarp.Amuru;

using System.Reflection;

/// <summary>
/// Provides utility installation functionality for TimeWarp command-line tools
/// </summary>
public static class Installer
{
  private const string GitHubOwner = "TimeWarpEngineering";
  private const string GitHubRepo = "timewarp-amuru";

  /// <summary>
  /// Installs TimeWarp utilities by downloading pre-built executables from GitHub releases
  /// </summary>
  /// <param name="specificUtilities">Optional array of specific utilities to install. If null, installs all from archive.</param>
  /// <returns>Exit code (0 for success, non-zero for error)</returns>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI installer: top-level installation method should report errors and exit gracefully, not throw."
  )]
  public static async Task<int> InstallUtilitiesAsync(string[]? specificUtilities = null)
  {

    // Get the version of the current assembly
    string version = GetCurrentVersion();

    await TimeWarpTerminal.Default.WriteLineAsync($"Installing TimeWarp utilities v{version}...");
    await TimeWarpTerminal.Default.WriteLineAsync();

    // Determine platform
    string platform = GetPlatform();
    string archiveExt = platform.Contains("win", StringComparison.OrdinalIgnoreCase) ? ".zip" : ".tar.gz";

    // Determine installation directory
    string installDir = GetInstallDirectory();

    // Create directory if it doesn't exist
    if (!Directory.Exists(installDir))
    {
      Directory.CreateDirectory(installDir);
      await TimeWarpTerminal.Default.WriteLineAsync($"Created installation directory: {installDir}");
    }

    // Check for gh CLI availability
    bool hasGhCli = await CheckGhCliAsync();
    if (!hasGhCli)
    {
      await TimeWarpTerminal.Default.WriteLineAsync("⚠️  GitHub CLI (gh) not found. Attestation verification will be skipped.");
      await TimeWarpTerminal.Default.WriteLineAsync("   For enhanced security, install gh: https://cli.github.com");
      await TimeWarpTerminal.Default.WriteLineAsync();
    }

    try
    {
      // Download the utilities archive for the specific version
      string archiveUrl = $"https://github.com/{GitHubOwner}/{GitHubRepo}/releases/download/v{version}/timewarp-utilities-{platform}{archiveExt}";
      string archivePath = Path.Combine(Path.GetTempPath(), $"timewarp-utilities-{platform}{archiveExt}");

      await TimeWarpTerminal.Default.WriteLineAsync("Downloading utilities from GitHub releases...");
      await TimeWarpTerminal.Default.WriteLineAsync($"  URL: {archiveUrl}");

      await DownloadFileAsync(archiveUrl, archivePath);
      await TimeWarpTerminal.Default.WriteLineAsync($"✓ Downloaded to {archivePath}");

      // Verify with gh attestation if available
      if (hasGhCli)
      {
        await TimeWarpTerminal.Default.WriteLineAsync("Verifying attestation with GitHub CLI...");
        bool verified = await VerifyAttestationAsync(archivePath);
        if (!verified)
        {
          await TimeWarpTerminal.Default.WriteLineAsync("⚠️  Attestation verification failed.");
          TimeWarpTerminal.Default.Write("Continue without verification? [y/N]: ");
          string? response = TimeWarpTerminal.Default.ReadLine();
          if (response?.ToLowerInvariant() != "y")
          {
            await TimeWarpTerminal.Default.WriteLineAsync("Installation cancelled.");
            return 1;
          }
        }
        else
        {
          await TimeWarpTerminal.Default.WriteLineAsync("✓ Attestation verified successfully");
        }
      }

      // Extract archive
      await TimeWarpTerminal.Default.WriteLineAsync("Extracting utilities...");
      await ExtractArchiveAsync(archivePath, installDir);

      // Discover all executables in the extracted archive
      string extractedDir = Path.Combine(installDir, platform);
      string[] allUtilities = Directory.GetFiles(extractedDir)
        .Select(Path.GetFileNameWithoutExtension)
        .Where(name => !string.IsNullOrEmpty(name))
        .ToArray()!;

      // Determine which utilities to install
      string[] utilitiesToInstall = specificUtilities ?? allUtilities;

      // If user specified utilities, validate they exist
      if (specificUtilities != null)
      {
        string[] missingUtilities = specificUtilities
          .Except(allUtilities, StringComparer.OrdinalIgnoreCase)
          .ToArray();

        if (missingUtilities.Length > 0)
        {
          await TimeWarpTerminal.Default.WriteLineAsync($"⚠️  Warning: The following utilities were not found in the archive:");
          foreach (string missing in missingUtilities)
          {
            await TimeWarpTerminal.Default.WriteLineAsync($"   - {missing}");
          }
        }

        // Filter to only utilities that exist
        utilitiesToInstall = specificUtilities
          .Intersect(allUtilities, StringComparer.OrdinalIgnoreCase)
          .ToArray();
      }

      // Make executables on Unix-like systems
      if (!OperatingSystem.IsWindows())
      {
        foreach (string utility in utilitiesToInstall)
        {
          string utilityPath = Path.Combine(installDir, platform, utility);
          if (File.Exists(utilityPath))
          {
            await Shell.Builder("chmod")
                .WithArguments("+x", utilityPath)
                .RunAsync();
          }
        }
      }

      // Move utilities to install directory (from platform subdirectory)
      foreach (string utility in utilitiesToInstall)
      {
        string sourcePath = Path.Combine(installDir, platform, utility);
        if (OperatingSystem.IsWindows())
        {
          sourcePath += ".exe";
        }

        string destPath = Path.Combine(installDir, Path.GetFileName(sourcePath));

        if (File.Exists(sourcePath))
        {
          // Remove existing file if present
          if (File.Exists(destPath))
          {
            File.Delete(destPath);
          }

          File.Move(sourcePath, destPath);
          await TimeWarpTerminal.Default.WriteLineAsync($"✓ Installed {utility} to {destPath}");
        }
        else
        {
          await TimeWarpTerminal.Default.WriteLineAsync($"⚠️  {utility} not found in archive");
        }
      }

      // Clean up temporary files
      File.Delete(archivePath);
      Directory.Delete(Path.Combine(installDir, platform), true);

      // Create symlinks on Unix-like systems
      if (!OperatingSystem.IsWindows())
      {
        await CreateSymlinksAsync(installDir, utilitiesToInstall);
      }

      await TimeWarpTerminal.Default.WriteLineAsync();
      await TimeWarpTerminal.Default.WriteLineAsync("Installation complete!");

      if (OperatingSystem.IsWindows())
      {
        await TimeWarpTerminal.Default.WriteLineAsync($"Utilities installed to: {installDir}");
        await TimeWarpTerminal.Default.WriteLineAsync();
        await TimeWarpTerminal.Default.WriteLineAsync("To use these utilities, add the following directory to your PATH:");
        await TimeWarpTerminal.Default.WriteLineAsync($"  {installDir}");
      }
      else
      {
        await TimeWarpTerminal.Default.WriteLineAsync($"Utilities installed to: {installDir}");
        await TimeWarpTerminal.Default.WriteLineAsync($"Symlinks created in: {GetSymlinkDirectory()}");
      }

      return 0;
    }
    catch (Exception ex)
    {
      await TimeWarpTerminal.Default.WriteLineAsync($"❌ Installation failed: {ex.Message}");
      return 1;
    }
  }

  private static string GetPlatform()
  {
    if (OperatingSystem.IsWindows())
      return "win-x64";
    if (OperatingSystem.IsMacOS())
      return "osx-x64";
    return "linux-x64";
  }

  private static string GetInstallDirectory()
  {
    // All platforms use ~/.timewarp/bin/
    return Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".timewarp",
        "bin"
    );
  }

  private static string GetSymlinkDirectory()
  {
    // Directory where we'll create symlinks (Unix) or note in instructions (Windows)
    if (OperatingSystem.IsWindows())
    {
      // Windows doesn't use symlinks, we'll update PATH instead
      return GetInstallDirectory();
    }

    return Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".local",
        "bin"
    );
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI installer: gh CLI availability check should return false on any failure, not throw."
  )]
  private static async Task<bool> CheckGhCliAsync()
  {
    try
    {
      CommandOutput result = await Shell.Builder("gh")
          .WithArguments("--version")
          .CaptureAsync();

      return result.Success;
    }
    catch
    {
      return false;
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI installer: attestation verification should return false on any failure, not throw."
  )]
  private static async Task<bool> VerifyAttestationAsync(string filePath)
  {
    try
    {
      CommandOutput result = await Shell.Builder("gh")
          .WithArguments(
              "attestation", "verify", filePath,
              "--repo", $"{GitHubOwner}/{GitHubRepo}"
          )
          .CaptureAsync();

      return result.Success;
    }
    catch
    {
      return false;
    }
  }

  private static async Task DownloadFileAsync(string url, string destinationPath)
  {
    using HttpClient client = new();
    client.DefaultRequestHeaders.UserAgent.ParseAdd("TimeWarp.Amuru.Installer/1.0");

    using HttpResponseMessage response = await client.GetAsync(new Uri(url));
    response.EnsureSuccessStatusCode();

    await using FileStream fileStream = new(destinationPath, FileMode.Create);
    await response.Content.CopyToAsync(fileStream);
  }

  private static async Task ExtractArchiveAsync(string archivePath, string destinationDir)
  {
    if (archivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
    {
      // Windows - use PowerShell
      await Shell.Builder("powershell")
          .WithArguments(
              "-Command",
              $"Expand-Archive -Path '{archivePath}' -DestinationPath '{destinationDir}' -Force"
          )
          .RunAsync();
    }
    else if (archivePath.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
    {
      // Unix-like - use tar
      await Shell.Builder("tar")
          .WithArguments(
              "-xzf", archivePath,
              "-C", destinationDir
          )
          .RunAsync();
    }
    else
    {
      throw new NotSupportedException($"Archive format not supported: {archivePath}");
    }
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1031",
    Justification = "CLI installer: best-effort symlink creation should ignore errors and continue, not throw."
  )]
  private static async Task CreateSymlinksAsync(string installDir, string[] utilities)
  {
    string symlinkDir = GetSymlinkDirectory();

    // Ensure symlink directory exists
    if (!Directory.Exists(symlinkDir))
    {
      Directory.CreateDirectory(symlinkDir);
      await TimeWarpTerminal.Default.WriteLineAsync($"Created symlink directory: {symlinkDir}");
    }

    await TimeWarpTerminal.Default.WriteLineAsync();
    await TimeWarpTerminal.Default.WriteLineAsync("Creating symlinks...");

    foreach (string utility in utilities)
    {
      string targetPath = Path.Combine(installDir, utility);
      string symlinkPath = Path.Combine(symlinkDir, utility);

      // Remove existing symlink if present
      if (File.Exists(symlinkPath) || Directory.Exists(symlinkPath))
      {
        try
        {
          File.Delete(symlinkPath);
        }
        catch
        {
          // Ignore errors - might be a directory or permission issue
        }
      }

      // Create symlink using ln -sf
      CommandOutput result = await Shell.Builder("ln")
        .WithArguments("-sf", targetPath, symlinkPath)
        .CaptureAsync();

      if (result.Success)
      {
        await TimeWarpTerminal.Default.WriteLineAsync($"✓ Created symlink: {utility}");
      }
      else
      {
        await TimeWarpTerminal.Default.WriteLineAsync($"⚠️  Failed to create symlink for {utility}");
      }
    }
  }

  private static string GetCurrentVersion()
  {
    // Get version from the assembly
    Assembly assembly = typeof(Installer).Assembly;
    Version? version = assembly.GetName().Version;

    // Check for informational version (includes pre-release suffix like -beta.10)
    string? informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    if (!string.IsNullOrEmpty(informationalVersion))
    {
      // Remove commit hash if present (e.g., "1.0.0-beta.10+abc123" -> "1.0.0-beta.10")
      int plusIndex = informationalVersion.IndexOf('+', StringComparison.Ordinal);
      return plusIndex > 0 ? informationalVersion[..plusIndex] : informationalVersion;
    }

    // Fallback to assembly version
    return version?.ToString() ?? "1.0.0";
  }
}