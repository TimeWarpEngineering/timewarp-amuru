namespace TimeWarp.Amuru.Tool;

/// <summary>
/// CLI tool for TimeWarp Amuru utilities
/// </summary>
internal static class Program
{
    /// <summary>
    /// Main entry point with basic Nuru routing
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Exit code (0 for success, 1 for error)</returns>
    public static async Task<int> Main(string[] args)
    {
        NuruAppBuilder builder = new();

        // Enable auto-help
        builder.AddAutoHelp();

        // Default route - show available commands
        builder.AddDefaultRoute(() =>
        {
            WriteLine("TimeWarp Amuru CLI Tool");
            WriteLine();
            WriteLine("Available commands:");
            WriteLine("  multiavatar <input>                    Generate avatar from text");
            WriteLine("  generate-avatar                        Generate avatar for current repository");
            WriteLine("  convert-timestamp <timestamp>          Convert Unix timestamps");
            WriteLine("  generate-color <seed>                  Generate color from seed");
            WriteLine("  install [utility]                      Install standalone executables");
            WriteLine();
            WriteLine("Use 'timewarp <command> --help' for more information about a command.");
        });

        // Add multiavatar command routes
        AddMultiavatarRoutes(builder);
        
        // Add generate-avatar command routes
        AddGenerateAvatarRoutes(builder);
        
        // Add convert-timestamp command routes
        AddConvertTimestampRoutes(builder);
        
        // Add generate-color command routes
        AddGenerateColorRoutes(builder);
        
        // Add install command routes
        AddInstallRoutes(builder);

        NuruApp app = builder.Build();
        return await app.RunAsync(args);
    }

    private static void AddMultiavatarRoutes(NuruAppBuilder builder)
    {
        // Single route with optional parameters and flags
        builder.AddRoute("multiavatar {input|Text to generate avatar from} --output {file?|Save to file instead of stdout} --no-env|Generate without environment circle --output-hash|Display hash information", 
            (string input, string? file, bool noEnv, bool outputHash) =>
        {
            // If output-hash flag is set, show hash information instead
            if (outputHash)
            {
                string hash = HashService.GenerateHash(input);
                string sha256Hash = HashService.GetSha256Hash(input);
                string sha256Numbers = HashService.GetSha256Numbers(sha256Hash);
                
                WriteLine($"{input}:");
                WriteLine($"  SHA256: {sha256Hash}");
                WriteLine($"  Numbers: {sha256Numbers}");
                WriteLine($"  Hash-12: {hash}");
                WriteLine("  Parts:");
                WriteLine($"    env: {hash.Substring(0, 2)} -> {HashService.GetPartSelection(hash.Substring(0, 2))}");
                WriteLine($"    clo: {hash.Substring(2, 2)} -> {HashService.GetPartSelection(hash.Substring(2, 2))}");
                WriteLine($"    head: {hash.Substring(4, 2)} -> {HashService.GetPartSelection(hash.Substring(4, 2))}");
                WriteLine($"    mouth: {hash.Substring(6, 2)} -> {HashService.GetPartSelection(hash.Substring(6, 2))}");
                WriteLine($"    eyes: {hash.Substring(8, 2)} -> {HashService.GetPartSelection(hash.Substring(8, 2))}");
                WriteLine($"    top: {hash.Substring(10, 2)} -> {HashService.GetPartSelection(hash.Substring(10, 2))}");
                return;
            }
            
            // Generate the avatar
            string svg = MultiavatarGenerator.Generate(input, sansEnv: noEnv);
            
            // Output to file or stdout
            if (!string.IsNullOrEmpty(file))
            {
                File.WriteAllText(file, svg);
                WriteLine($"Avatar saved to: {file}");
            }
            else
            {
                Write(svg);
            }
        });
    }

    private static void AddGenerateAvatarRoutes(NuruAppBuilder builder)
    {
        builder.AddRoute("generate-avatar", async () =>
        {
            CommandOutput result = await Shell.Builder("git")
                .WithArguments("rev-parse", "--show-toplevel")
                .CaptureAsync();
            
            if (!result.Success)
            {
                WriteLine("Error: Not in a git repository");
                return 1;
            }

            string gitRoot = result.Stdout.Trim();
            
            CommandOutput repoNameResult = await Shell.Builder("git")
                .WithArguments("remote", "get-url", "origin")
                .CaptureAsync();
            
            string repoName = "avatar";
            if (repoNameResult.Success && !string.IsNullOrWhiteSpace(repoNameResult.Stdout))
            {
                string output = repoNameResult.Stdout.Trim();
                if (output.Contains("github.com", StringComparison.OrdinalIgnoreCase))
                {
                    repoName = output.Split('/').Last();
                    if (repoName.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                    {
                        repoName = repoName.Substring(0, repoName.Length - 4);
                    }
                }
            }
            else
            {
                repoName = new DirectoryInfo(gitRoot).Name;
            }

            WriteLine($"Git repository root: {gitRoot}");
            WriteLine($"Repository name: {repoName}");

            string assetsDir = Path.Combine(gitRoot, "assets");
            if (!Directory.Exists(assetsDir))
            {
                Directory.CreateDirectory(assetsDir);
                WriteLine($"Created assets directory: {assetsDir}");
            }

            string svgCode = MultiavatarGenerator.Generate(repoName);
            string svgFilename = Path.Combine(assetsDir, $"{repoName}-avatar.svg");
            await File.WriteAllTextAsync(svgFilename, svgCode);
            WriteLine($"Generated {svgFilename}");
            WriteLine($"Successfully generated avatar for '{repoName}'");
            
            return 0;
        });
    }

    private static void AddConvertTimestampRoutes(NuruAppBuilder builder)
    {
        builder.AddRoute("convert-timestamp {timestamp|Unix timestamp to convert}", (string timestamp) =>
        {
            if (long.TryParse(timestamp, out long unixTimestamp))
            {
                var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                WriteLine(dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ssK", System.Globalization.CultureInfo.InvariantCulture));
            }
            else
            {
                WriteLine("Invalid timestamp");
                return 1;
            }
            
            return 0;
        });
    }

    private static void AddGenerateColorRoutes(NuruAppBuilder builder)
    {
        builder.AddRoute("generate-color {seed|Text to generate color from}", (string seed) =>
        {
            byte[] hashBytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(seed));
            
            // Use first 3 bytes for RGB
            int r = hashBytes[0];
            int g = hashBytes[1];
            int b = hashBytes[2];
            
            string hex = $"#{r:X2}{g:X2}{b:X2}";
            
            // Calculate HSL
            double rNorm = r / 255.0;
            double gNorm = g / 255.0;
            double bNorm = b / 255.0;
            
            double max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
            double min = Math.Min(rNorm, Math.Min(gNorm, bNorm));
            double lightness = (max + min) / 2;
            
            double saturation = 0;
            double hue = 0;
            
            if (max != min)
            {
                double delta = max - min;
                saturation = lightness > 0.5 ? delta / (2 - max - min) : delta / (max + min);
                
                if (max == rNorm)
                    hue = ((gNorm - bNorm) / delta + (gNorm < bNorm ? 6 : 0)) / 6;
                else if (max == gNorm)
                    hue = ((bNorm - rNorm) / delta + 2) / 6;
                else
                    hue = ((rNorm - gNorm) / delta + 4) / 6;
            }
            
            WriteLine($"Seed: {seed}");
            WriteLine($"Hex: {hex}");
            WriteLine($"RGB: rgb({r}, {g}, {b})");
            WriteLine($"HSL: hsl({(int)(hue * 360)}, {(int)(saturation * 100)}%, {(int)(lightness * 100)}%)");
        });
    }

    private static void AddInstallRoutes(NuruAppBuilder builder)
    {
        builder.AddRoute("install", async () => await InstallUtilities());
        builder.AddRoute("install {utility|Specific utility to install}", async (string utility) => await InstallUtility(utility));
    }

    private static async Task<int> InstallUtilities()
    {
        WriteLine("Installing all TimeWarp utilities...");
        
        string[] utilities = { "multiavatar", "generate-avatar", "convert-timestamp", "generate-color" };
        
        foreach (string utility in utilities)
        {
            int result = await InstallUtility(utility);
            if (result != 0) return result;
        }
        
        WriteLine();
        WriteLine("All utilities installed successfully!");
        WriteLine("Make sure your PATH includes the installation directory.");
        
        return 0;
    }

    private static async Task<int> InstallUtility(string utility)
    {
        // Determine installation directory based on OS
        string installDir = OperatingSystem.IsWindows() 
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".tools")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin");
        
        // Create directory if it doesn't exist
        if (!Directory.Exists(installDir))
        {
            Directory.CreateDirectory(installDir);
            WriteLine($"Created installation directory: {installDir}");
        }

        // Find the exe directory (relative to the tool's location)
        string exeDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "exe"));
        string scriptPath = Path.Combine(exeDir, $"{utility}.cs");
        
        if (!File.Exists(scriptPath))
        {
            WriteLine($"Error: Utility script not found: {scriptPath}");
            return 1;
        }

        WriteLine($"Publishing {utility}...");
        
        // Determine runtime identifier
        string rid = OperatingSystem.IsWindows() ? "win-x64" 
            : OperatingSystem.IsMacOS() ? "osx-x64" 
            : "linux-x64";
        
        string outputPath = Path.Combine(installDir, utility);
        if (OperatingSystem.IsWindows())
        {
            outputPath += ".exe";
        }

        // Publish the executable
        CommandOutput publishResult = await Shell.Builder("dotnet")
            .WithArguments("publish", scriptPath,
                "-c", "Release",
                "-r", rid,
                "--self-contained",
                "-p:PublishSingleFile=true",
                "-p:PublishTrimmed=true",
                "-p:TrimMode=partial",
                "-o", installDir)
            .CaptureAsync();
        
        if (!publishResult.Success)
        {
            WriteLine($"Error: Failed to publish {utility}");
            return 1;
        }

        // Make executable on Unix-like systems
        if (!OperatingSystem.IsWindows())
        {
            await Shell.Builder("chmod")
                .WithArguments("+x", outputPath)
                .RunAsync();
        }

        WriteLine($"Installed {utility} to {outputPath}");
        return 0;
    }
}
