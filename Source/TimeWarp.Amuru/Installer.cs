namespace TimeWarp.Amuru;

using System.Reflection;
using static System.Console;

/// <summary>
/// Provides utility installation functionality for TimeWarp command-line tools
/// </summary>
public static class Installer
{
    private const string GitHubOwner = "TimeWarpEngineering";
    private const string GitHubRepo = "timewarp-amuru";

    private static readonly string[] Utilities = ["multiavatar", "generate-avatar", "convert-timestamp", "generate-color"];

    /// <summary>
    /// Installs TimeWarp utilities by downloading pre-built executables from GitHub releases
    /// </summary>
    /// <param name="specificUtilities">Optional array of specific utilities to install. If null, installs all.</param>
    /// <returns>Exit code (0 for success, non-zero for error)</returns>
    public static async Task<int> InstallUtilitiesAsync(string[]? specificUtilities = null)
    {
        string[] utilitiesToInstall = specificUtilities ?? Utilities;

        // Get the version of the current assembly
        string version = GetCurrentVersion();

        WriteLine($"Installing TimeWarp utilities v{version}...");
        WriteLine();

        // Determine platform
        string platform = GetPlatform();
        string archiveExt = platform.Contains("win", StringComparison.OrdinalIgnoreCase) ? ".zip" : ".tar.gz";

        // Determine installation directory
        string installDir = GetInstallDirectory();

        // Create directory if it doesn't exist
        if (!Directory.Exists(installDir))
        {
            Directory.CreateDirectory(installDir);
            WriteLine($"Created installation directory: {installDir}");
        }

        // Check for gh CLI availability
        bool hasGhCli = await CheckGhCliAsync();
        if (!hasGhCli)
        {
            WriteLine("⚠️  GitHub CLI (gh) not found. Attestation verification will be skipped.");
            WriteLine("   For enhanced security, install gh: https://cli.github.com");
            WriteLine();
        }

        try
        {
            // Download the utilities archive for the specific version
            string archiveUrl = $"https://github.com/{GitHubOwner}/{GitHubRepo}/releases/download/v{version}/timewarp-utilities-{platform}{archiveExt}";
            string archivePath = Path.Combine(Path.GetTempPath(), $"timewarp-utilities-{platform}{archiveExt}");

            WriteLine($"Downloading utilities from GitHub releases...");
            WriteLine($"  URL: {archiveUrl}");

            await DownloadFileAsync(archiveUrl, archivePath);
            WriteLine($"✓ Downloaded to {archivePath}");

            // Verify with gh attestation if available
            if (hasGhCli)
            {
                WriteLine("Verifying attestation with GitHub CLI...");
                bool verified = await VerifyAttestationAsync(archivePath);
                if (!verified)
                {
                    WriteLine("⚠️  Attestation verification failed.");
                    Write("Continue without verification? [y/N]: ");
                    string? response = ReadLine();
                    if (response?.ToLowerInvariant() != "y")
                    {
                        WriteLine("Installation cancelled.");
                        return 1;
                    }
                }
                else
                {
                    WriteLine("✓ Attestation verified successfully");
                }
            }

            // Extract archive
            WriteLine("Extracting utilities...");
            await ExtractArchiveAsync(archivePath, installDir);

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
                    WriteLine($"✓ Installed {utility} to {destPath}");
                }
                else
                {
                    WriteLine($"⚠️  {utility} not found in archive");
                }
            }

            // Clean up temporary files
            File.Delete(archivePath);
            Directory.Delete(Path.Combine(installDir, platform), true);

            WriteLine();
            WriteLine("Installation complete!");
            WriteLine($"Make sure {installDir} is in your PATH.");

            return 0;
        }
        catch (Exception ex)
        {
            WriteLine($"❌ Installation failed: {ex.Message}");
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
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".tools"
            );
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local",
            "bin"
        );
    }

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