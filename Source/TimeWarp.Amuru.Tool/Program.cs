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

    // Enable auto-help (provides --help and command listing)
    builder.AddAutoHelp();

    // Register all commands with descriptions
    builder.AddRoute
    (
      "multiavatar {input|Text to generate avatar from (email, username, etc)} " +
      "--output,-o {file?|Save SVG to file instead of stdout} " +
      "--no-env|Generate without environment circle " +
      "--output-hash|Display hash calculation details instead of SVG",
      MultiavatarCommand,
      "Generate unique, deterministic SVG avatars from any text input"
    );

    builder.AddRoute
    (
      "generate-avatar",
      GenerateAvatarCommand,
      "Generate an SVG avatar for the current git repository and save to assets/"
    );

    builder.AddRoute
    (
      "convert-timestamp {timestamp|Unix timestamp (seconds since epoch)}",
      ConvertTimestampCommand,
      "Convert Unix timestamps to ISO 8601 format (yyyy-MM-ddTHH:mm:ssK)"
    );

    builder.AddRoute
    (
      "generate-color {seed|Text seed for deterministic color generation}",
      GenerateColorCommand,
      "Generate consistent colors from text seeds (outputs Hex, RGB, and HSL values)"
    );

    builder.AddRoute
    (
      "install {utility?|Utility name (multiavatar, generate-avatar, convert-timestamp, generate-color) or leave empty for all}",
      InstallCommand,
      "Download and install standalone utility executables to system PATH"
    );

    NuruApp app = builder.Build();
    return await app.RunAsync(args);
  }

  private static void MultiavatarCommand
  (
    string input,
    string? file,
    bool noEnv,
    bool outputHash
  )
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
      WriteLine($"    env: {hash[..2]} -> {HashService.GetPartSelection(hash[..2])}");
      WriteLine($"    clo: {hash[2..4]} -> {HashService.GetPartSelection(hash[2..4])}");
      WriteLine($"    head: {hash[4..6]} -> {HashService.GetPartSelection(hash[4..6])}");
      WriteLine($"    mouth: {hash[6..8]} -> {HashService.GetPartSelection(hash[6..8])}");
      WriteLine($"    eyes: {hash[8..10]} -> {HashService.GetPartSelection(hash[8..10])}");
      WriteLine($"    top: {hash[10..12]} -> {HashService.GetPartSelection(hash[10..12])}");
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
  }

  private static async Task<int> GenerateAvatarCommand()
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
          repoName = repoName[..^4];
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
  }

  private static int ConvertTimestampCommand(string timestamp)
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
  }

  private static void GenerateColorCommand(string seed)
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
  }

  private static async Task<int> InstallCommand(string? utility)
  {
    return await Installer.InstallUtilitiesAsync(utility == null ? null : [utility]);
  }
}