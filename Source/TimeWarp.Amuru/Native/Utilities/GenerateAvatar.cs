namespace TimeWarp.Amuru.Native.Utilities;

/// <summary>
/// Utility for generating avatar images from email addresses, seeds, or repository names.
/// Supports SVG output format with customizable sizes and styles.
/// </summary>
public static class GenerateAvatar
{
  /// <summary>
  /// Generates an SVG avatar from an email address.
  /// </summary>
  /// <param name="email">The email address to generate avatar from</param>
  /// <returns>An SVG string representing the avatar</returns>
  public static string FromEmail(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
    {
      throw new ArgumentException("Email cannot be null or whitespace", nameof(email));
    }

    string? seed = GenerateSeedFromEmail(email);
    if (seed == null)
    {
      throw new ArgumentException("Unable to generate seed from email", nameof(email));
    }

    return GenerateSvgAvatar(seed);
  }

  /// <summary>
  /// Generates an SVG avatar from a seed string.
  /// </summary>
  /// <param name="seed">The seed string to generate avatar from</param>
  /// <returns>An SVG string representing the avatar</returns>
  public static string FromSeed(string seed)
  {
    if (string.IsNullOrWhiteSpace(seed))
    {
      throw new ArgumentException("Seed cannot be null or whitespace", nameof(seed));
    }

    return GenerateSvgAvatar(seed);
  }

  /// <summary>
  /// Creates a builder for configuring avatar generation options.
  /// </summary>
  /// <param name="input">The email address or seed to generate avatar from</param>
  /// <returns>A builder for configuring avatar generation options</returns>
  public static AvatarBuilder From(string input)
  {
    return new AvatarBuilder(input);
  }

  /// <summary>
  /// Generates an avatar for the current git repository.
  /// This is a convenience method that automatically detects the repository name.
  /// </summary>
  /// <returns>An SVG string representing the repository avatar</returns>
  public static string FromCurrentRepository()
  {
    string repoName = GetRepositoryName();
    return FromSeed(repoName);
  }

  /// <summary>
  /// Saves an avatar to a file.
  /// </summary>
  /// <param name="avatarSvg">The SVG avatar content</param>
  /// <param name="filePath">The file path to save the avatar to</param>
  public static void SaveToFile(string avatarSvg, string filePath)
  {
    if (string.IsNullOrWhiteSpace(avatarSvg))
    {
      throw new ArgumentException("Avatar SVG cannot be null or whitespace", nameof(avatarSvg));
    }

    if (string.IsNullOrWhiteSpace(filePath))
    {
      throw new ArgumentException("File path cannot be null or whitespace", nameof(filePath));
    }

    File.WriteAllText(filePath, avatarSvg);
  }

  /// <summary>
  /// Creates the assets directory in the git repository root if it doesn't exist.
  /// </summary>
  /// <returns>The path to the assets directory</returns>
  public static string EnsureAssetsDirectory()
  {
    string? gitRoot = FindGitRoot();
    if (gitRoot == null)
    {
      throw new InvalidOperationException("Not in a git repository");
    }

    string assetsDir = Path.Combine(gitRoot, "assets");
    if (!Directory.Exists(assetsDir))
    {
      Directory.CreateDirectory(assetsDir);
    }

    return assetsDir;
  }

  /// <summary>
  /// Generates and saves an avatar for the current repository to the assets directory.
  /// </summary>
  /// <param name="fileName">Optional filename (defaults to "{repoName}-avatar.svg")</param>
  /// <returns>The path to the saved avatar file</returns>
  public static string GenerateAndSaveRepositoryAvatar(string? fileName = null)
  {
    string repoName = GetRepositoryName();
    string avatarSvg = FromSeed(repoName);

    string assetsDir = EnsureAssetsDirectory();
    string actualFileName = fileName ?? $"{repoName}-avatar.svg";
    string filePath = Path.Combine(assetsDir, actualFileName);

    SaveToFile(avatarSvg, filePath);
    return filePath;
  }

  /// <summary>
  /// Builder class for configuring avatar generation options.
  /// </summary>
  public class AvatarBuilder
  {
    private readonly string Input;
    private int Size = 256;
    private AvatarStyle Style = AvatarStyle.Default;
    private string? CustomColors;

    internal AvatarBuilder(string input)
    {
      Input = input ?? throw new ArgumentNullException(nameof(input));
    }

    /// <summary>
    /// Sets the size of the avatar.
    /// </summary>
    /// <param name="size">The size in pixels (default: 256)</param>
    /// <returns>The builder instance for method chaining</returns>
    public AvatarBuilder WithSize(int size)
    {
      if (size <= 0)
      {
        throw new ArgumentException("Size must be greater than 0", nameof(size));
      }

      Size = size;
      return this;
    }

    /// <summary>
    /// Sets the style of the avatar.
    /// </summary>
    /// <param name="style">The avatar style</param>
    /// <returns>The builder instance for method chaining</returns>
    public AvatarBuilder WithStyle(AvatarStyle style)
    {
      Style = style;
      return this;
    }

    /// <summary>
    /// Sets custom colors for the avatar (hex color string).
    /// </summary>
    /// <param name="colors">Custom colors in hex format</param>
    /// <returns>The builder instance for method chaining</returns>
    public AvatarBuilder WithColors(string colors)
    {
      CustomColors = colors ?? throw new ArgumentNullException(nameof(colors));
      return this;
    }

    /// <summary>
    /// Generates the avatar as an SVG string.
    /// </summary>
    /// <returns>The SVG avatar content</returns>
    public string AsSvg()
    {
      string seed = GenerateSeedFromEmail(Input) ?? Input;
      return GenerateSvgAvatar(seed, Size, Style, CustomColors);
    }

    /// <summary>
    /// Saves the avatar to a file.
    /// </summary>
    /// <param name="filePath">The file path to save the avatar to</param>
    /// <returns>The builder instance for method chaining</returns>
    public AvatarBuilder SaveTo(string filePath)
    {
      string svg = AsSvg();
      SaveToFile(svg, filePath);
      return this;
    }
  }

  /// <summary>
  /// Avatar style enumeration.
  /// </summary>
  public enum AvatarStyle
  {
    /// <summary>
    /// Default geometric style
    /// </summary>
    Default,

    /// <summary>
    /// Circular design
    /// </summary>
    Circle,

    /// <summary>
    /// Rounded corners
    /// </summary>
    Rounded,

    /// <summary>
    /// Minimalist design
    /// </summary>
    Minimal
  }

  /// <summary>
  /// Generates a seed from an email address.
  /// </summary>
  /// <param name="email">The email address</param>
  /// <returns>A seed string derived from the email</returns>
  private static string? GenerateSeedFromEmail(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
    {
      return null;
    }

    // Use the local part of the email as seed
    int atIndex = email.IndexOf('@', StringComparison.Ordinal);
    if (atIndex > 0)
    {
      return email.Substring(0, atIndex);
    }

    return email;
  }

  /// <summary>
  /// Generates an SVG avatar from a seed.
  /// </summary>
  /// <param name="seed">The seed string</param>
  /// <param name="size">The size of the avatar</param>
  /// <param name="style">The avatar style</param>
  /// <param name="customColors">Custom colors (optional)</param>
  /// <returns>An SVG string</returns>
  private static string GenerateSvgAvatar(string seed, int size = 256, AvatarStyle style = AvatarStyle.Default, string? customColors = null)
  {
    // Generate hash from seed for deterministic output
#pragma warning disable CA5351 // MD5 is safe for non-cryptographic purposes
    byte[] hashBytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(seed));
#pragma warning restore CA5351

    // Extract values from hash
    int hue = (hashBytes[0] * 256 + hashBytes[1]) % 360;
    int saturation = 60 + (hashBytes[2] % 40); // 60-99%
    int lightness = 40 + (hashBytes[3] % 40);  // 40-79%

    string backgroundColor = customColors ?? $"hsl({hue}, {saturation}%, {lightness}%)";

    // Generate geometric pattern based on hash
    string pattern = GenerateGeometricPattern(hashBytes, size);

    string svg = $@"
<svg width=""{size}"" height=""{size}"" viewBox=""0 0 {size} {size}"" xmlns=""http://www.w3.org/2000/svg"">
  <rect width=""{size}"" height=""{size}"" fill=""{backgroundColor}""/>
  {pattern}
</svg>";

    return svg.Trim();
  }

  /// <summary>
  /// Generates a geometric pattern based on hash bytes.
  /// </summary>
  /// <param name="hashBytes">The hash bytes</param>
  /// <param name="size">The size of the pattern</param>
  /// <returns>SVG pattern elements</returns>
  private static string GenerateGeometricPattern(byte[] hashBytes, int size)
  {
    var elements = new List<string>();
    int shapeCount = 3 + (hashBytes[4] % 4); // 3-6 shapes

    for (int i = 0; i < shapeCount; i++)
    {
      int shapeType = hashBytes[5 + i] % 4;
      int x = (hashBytes[10 + i * 3] * size) / 256;
      int y = (hashBytes[11 + i * 3] * size) / 256;
      int shapeSize = 20 + (hashBytes[12 + i * 3] % 60);

      int shapeHue = (hashBytes[15 + i] % 360);
      string shapeColor = $"hsl({shapeHue}, 70%, 60%)";

      switch (shapeType)
      {
        case 0: // Circle
          elements.Add($@"<circle cx=""{x}"" cy=""{y}"" r=""{shapeSize / 2}"" fill=""{shapeColor}"" opacity=""0.8""/>");
          break;
        case 1: // Rectangle
          elements.Add($@"<rect x=""{x - shapeSize / 2}"" y=""{y - shapeSize / 2}"" width=""{shapeSize}"" height=""{shapeSize}"" fill=""{shapeColor}"" opacity=""0.8""/>");
          break;
        case 2: // Triangle
          int halfSize = shapeSize / 2;
          string trianglePoints = $"{x},{y - halfSize} {x - halfSize},{y + halfSize} {x + halfSize},{y + halfSize}";
          elements.Add($@"<polygon points=""{trianglePoints}"" fill=""{shapeColor}"" opacity=""0.8""/>");
          break;
        case 3: // Diamond
          halfSize = shapeSize / 2;
          string diamondPoints = $"{x},{y - halfSize} {x + halfSize},{y} {x},{y + halfSize} {x - halfSize},{y}";
          elements.Add($@"<polygon points=""{diamondPoints}"" fill=""{shapeColor}"" opacity=""0.8""/>");
          break;
      }
    }

    return string.Join("\n  ", elements);
  }

  /// <summary>
  /// Finds the git repository root directory.
  /// </summary>
  /// <param name="startPath">Optional starting path (defaults to current directory)</param>
  /// <returns>The path to the git root directory, or null if not found</returns>
  private static string? FindGitRoot(string? startPath = null)
  {
    string currentPath = startPath ?? Directory.GetCurrentDirectory();

    while (!string.IsNullOrEmpty(currentPath))
    {
      string gitPath = Path.Combine(currentPath, ".git");

      // Check if .git exists (either as directory or file for worktrees)
      if (Directory.Exists(gitPath) || File.Exists(gitPath))
      {
        return currentPath;
      }

      DirectoryInfo? parent = Directory.GetParent(currentPath);
      if (parent == null)
      {
        break;
      }

      currentPath = parent.FullName;
    }

    return null;
  }

  /// <summary>
  /// Gets the repository name from git remote or directory name.
  /// </summary>
  /// <returns>The repository name</returns>
  private static string GetRepositoryName()
  {
    // Try to get from git remote first
    try
    {
#pragma warning disable CA1416 // Validate platform compatibility
      var processInfo = new System.Diagnostics.ProcessStartInfo
      {
        FileName = "git",
        Arguments = "remote get-url origin",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };
#pragma warning restore CA1416

      using var process = System.Diagnostics.Process.Start(processInfo);
      if (process != null)
      {
        string output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();

        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
        {
          // Extract repo name from URL
          if (output.Contains("github.com", StringComparison.OrdinalIgnoreCase))
          {
            string repoName = output.Split('/').Last();
            if (repoName.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
              repoName = repoName.Substring(0, repoName.Length - 4);
            }

            return repoName;
          }
        }
      }
    }
    catch
    {
      // Ignore errors and fall back to directory name
    }

    // Fallback to directory name
    string? gitRoot = FindGitRoot();
    return gitRoot != null ? new DirectoryInfo(gitRoot).Name : "avatar";
  }
}