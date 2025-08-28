namespace TimeWarp.Amuru.Native.Utilities;

/// <summary>
/// Utility for generating multi-style avatars from identifiers.
/// Provides deterministic avatar generation with various customization options.
/// </summary>
public static class Multiavatar
{
  /// <summary>
  /// Generates a multiavatar from an identifier string.
  /// </summary>
  /// <param name="identifier">The identifier to generate avatar from</param>
  /// <returns>An SVG string representing the multiavatar</returns>
  public static string Generate(string identifier)
  {
    return Generate(identifier, new MultiavatarOptions());
  }

  /// <summary>
  /// Generates a multiavatar from an identifier string with custom options.
  /// </summary>
  /// <param name="identifier">The identifier to generate avatar from</param>
  /// <param name="options">The options for avatar generation</param>
  /// <returns>An SVG string representing the multiavatar</returns>
  internal static string Generate(string identifier, MultiavatarOptions options)
  {
    if (string.IsNullOrWhiteSpace(identifier))
    {
      throw new ArgumentException("Identifier cannot be null or whitespace", nameof(identifier));
    }

    ArgumentNullException.ThrowIfNull(options);

    // Generate hash from identifier for deterministic output
#pragma warning disable CA5351 // MD5 is safe for non-cryptographic purposes
    byte[] hashBytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(identifier));
#pragma warning restore CA5351

    return GenerateSvgFromHash(hashBytes, options);
  }

  /// <summary>
  /// Creates a builder for configuring multiavatar generation options.
  /// </summary>
  /// <param name="identifier">The identifier to generate avatar from</param>
  /// <returns>A builder for configuring multiavatar generation options</returns>
  public static MultiavatarBuilder From(string identifier)
  {
    return new MultiavatarBuilder(identifier);
  }

  /// <summary>
  /// Generates a multiavatar hash for analysis or debugging.
  /// </summary>
  /// <param name="identifier">The identifier to generate hash from</param>
  /// <returns>A HashInfo object containing hash details</returns>
  internal static HashInfo GenerateHashInfo(string identifier)
  {
    if (string.IsNullOrWhiteSpace(identifier))
    {
      throw new ArgumentException("Identifier cannot be null or whitespace", nameof(identifier));
    }

#pragma warning disable CA5351 // MD5 is safe for non-cryptographic purposes
    byte[] hashBytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(identifier));
#pragma warning restore CA5351

    string sha256Hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(identifier))).ToLowerInvariant();
    string sha256Numbers = string.Concat(sha256Hash.Select(c => c >= 'a' ? (c - 'a' + 10).ToString(System.Globalization.CultureInfo.InvariantCulture) : c.ToString()));

    return new HashInfo
    {
      Identifier = identifier,
      Sha256Hash = sha256Hash,
      Sha256Numbers = sha256Numbers,
      Hash12 = GetHash12(hashBytes),
      Parts = GetParts(hashBytes)
    };
  }

  /// <summary>
  /// Builder class for configuring multiavatar generation options.
  /// </summary>
  public class MultiavatarBuilder
  {
    private readonly string Identifier;
    private MultiavatarOptions Options = new();

    internal MultiavatarBuilder(string identifier)
    {
      Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
    }

    /// <summary>
    /// Sets whether to include the environment circle.
    /// </summary>
    /// <param name="includeEnv">true to include environment, false to exclude</param>
    /// <returns>The builder instance for method chaining</returns>
    public MultiavatarBuilder WithEnvironment(bool includeEnv)
    {
      Options.IncludeEnvironment = includeEnv;
      return this;
    }

    /// <summary>
    /// Excludes the environment circle from the avatar.
    /// </summary>
    /// <returns>The builder instance for method chaining</returns>
    public MultiavatarBuilder WithoutEnvironment()
    {
      Options.IncludeEnvironment = false;
      return this;
    }

    /// <summary>
    /// Sets the avatar style.
    /// </summary>
    /// <param name="style">The avatar style</param>
    /// <returns>The builder instance for method chaining</returns>
    public MultiavatarBuilder WithStyle(AvatarStyle style)
    {
      Options.Style = style;
      return this;
    }

    /// <summary>
    /// Sets custom colors for the avatar.
    /// </summary>
    /// <param name="colors">Array of custom colors</param>
    /// <returns>The builder instance for method chaining</returns>
    public MultiavatarBuilder WithColors(params string[] colors)
    {
      Options.CustomColors = colors ?? throw new ArgumentNullException(nameof(colors));
      return this;
    }

    /// <summary>
    /// Generates the multiavatar as an SVG string.
    /// </summary>
    /// <returns>The SVG multiavatar content</returns>
    public string AsSvg()
    {
      return Generate(Identifier, Options);
    }

    /// <summary>
    /// Saves the multiavatar to a file.
    /// </summary>
    /// <param name="filePath">The file path to save the avatar to</param>
    /// <returns>The builder instance for method chaining</returns>
    public MultiavatarBuilder SaveTo(string filePath)
    {
      string svg = AsSvg();
      File.WriteAllText(filePath, svg);
      return this;
    }
  }

  /// <summary>
  /// Options for multiavatar generation.
  /// </summary>
  internal class MultiavatarOptions
  {
    /// <summary>
    /// Whether to include the environment circle (default: true)
    /// </summary>
    public bool IncludeEnvironment { get; set; } = true;

    /// <summary>
    /// The avatar style (default: Default)
    /// </summary>
    public AvatarStyle Style { get; set; } = AvatarStyle.Default;

    /// <summary>
    /// Custom colors for the avatar (optional)
    /// </summary>
    public IReadOnlyList<string>? CustomColors { get; set; }
  }

  /// <summary>
  /// Avatar style enumeration.
  /// </summary>
  public enum AvatarStyle
  {
    /// <summary>
    /// Default multiavatar style
    /// </summary>
    Default,

    /// <summary>
    /// Minimal style
    /// </summary>
    Minimal,

    /// <summary>
    /// Retro style
    /// </summary>
    Retro,

    /// <summary>
    /// Bot style
    /// </summary>
    Bot
  }

  /// <summary>
  /// Hash information for debugging and analysis.
  /// </summary>
  internal class HashInfo
  {
    /// <summary>
    /// The original identifier
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// The SHA256 hash
    /// </summary>
    public string Sha256Hash { get; set; } = string.Empty;

    /// <summary>
    /// The SHA256 hash converted to numbers
    /// </summary>
    public string Sha256Numbers { get; set; } = string.Empty;

    /// <summary>
    /// The 12-character hash used for avatar generation
    /// </summary>
    public string Hash12 { get; set; } = string.Empty;

    /// <summary>
    /// The parts breakdown for avatar components
    /// </summary>
    public IReadOnlyDictionary<string, string> Parts { get; set; } = new Dictionary<string, string>();
  }

  /// <summary>
  /// Generates an SVG avatar from hash bytes.
  /// </summary>
  /// <param name="hashBytes">The hash bytes</param>
  /// <param name="options">The generation options</param>
  /// <returns>An SVG string</returns>
  private static string GenerateSvgFromHash(byte[] hashBytes, MultiavatarOptions options)
  {
    string hash12 = GetHash12(hashBytes);
    var parts = GetParts(hashBytes);

    int size = 256;
    string svg = $@"
<svg width=""{size}"" height=""{size}"" viewBox=""0 0 {size} {size}"" xmlns=""http://www.w3.org/2000/svg"">
  <defs>
    <linearGradient id=""grad1"" x1=""0%"" y1=""0%"" x2=""100%"" y2=""100%"">
      <stop offset=""0%"" style=""stop-color:#{(hashBytes[0] * 256 + hashBytes[1]) % 16777216:X6};stop-opacity:1"" />
      <stop offset=""100%"" style=""stop-color:#{(hashBytes[2] * 256 + hashBytes[3]) % 16777216:X6};stop-opacity:1"" />
    </linearGradient>
  </defs>
  {GenerateAvatarBody(hash12, parts, size, options)}
  {(options.IncludeEnvironment ? GenerateEnvironment(hashBytes, size) : "")}
</svg>";

    return svg.Trim();
  }

  /// <summary>
  /// Generates the main avatar body.
  /// </summary>
  /// <param name="hash12">The 12-character hash</param>
  /// <param name="parts">The avatar parts</param>
  /// <param name="size">The avatar size</param>
  /// <param name="options">The generation options</param>
  /// <returns>SVG elements for the avatar body</returns>
  private static string GenerateAvatarBody(string hash12, Dictionary<string, string> parts, int size, MultiavatarOptions options)
  {
    var elements = new List<string>();

    // Generate based on style
    switch (options.Style)
    {
      case AvatarStyle.Minimal:
        elements.Add(GenerateMinimalBody(hash12, size));
        break;
      case AvatarStyle.Retro:
        elements.Add(GenerateRetroBody(hash12, size));
        break;
      case AvatarStyle.Bot:
        elements.Add(GenerateBotBody(hash12, size));
        break;
      default:
        elements.Add(GenerateDefaultBody(hash12, parts, size));
        break;
    }

    return string.Join("\n  ", elements);
  }

  /// <summary>
  /// Generates the default avatar body.
  /// </summary>
  /// <param name="hash12">The 12-character hash</param>
  /// <param name="parts">The avatar parts</param>
  /// <param name="size">The avatar size</param>
  /// <returns>SVG elements for the default body</returns>
  private static string GenerateDefaultBody(string hash12, Dictionary<string, string> parts, int size)
  {
    int centerX = size / 2;
    int centerY = size / 2;
    int radius = size / 3;

    return $@"
  <circle cx=""{centerX}"" cy=""{centerY}"" r=""{radius}"" fill=""url(#grad1)"" />
  <text x=""{centerX}"" y=""{centerY + 8}"" font-family=""Arial"" font-size=""{size / 8}"" text-anchor=""middle"" fill=""white"">
    {parts["head"].Substring(0, 1).ToUpper(System.Globalization.CultureInfo.InvariantCulture)}
  </text>";
  }

  /// <summary>
  /// Generates the minimal avatar body.
  /// </summary>
  /// <param name="hash12">The 12-character hash</param>
  /// <param name="size">The avatar size</param>
  /// <returns>SVG elements for the minimal body</returns>
  private static string GenerateMinimalBody(string hash12, int size)
  {
    int centerX = size / 2;
    int centerY = size / 2;
    int radius = size / 4;

    return $@"
  <circle cx=""{centerX}"" cy=""{centerY}"" r=""{radius}"" fill=""#{(hash12[0] * 256 + hash12[1]) % 16777216:X6}"" />
  <circle cx=""{centerX}"" cy=""{centerY}"" r=""{radius / 2}"" fill=""#{(hash12[2] * 256 + hash12[3]) % 16777216:X6}"" />";
  }

  /// <summary>
  /// Generates the retro avatar body.
  /// </summary>
  /// <param name="hash12">The 12-character hash</param>
  /// <param name="size">The avatar size</param>
  /// <returns>SVG elements for the retro body</returns>
  private static string GenerateRetroBody(string hash12, int size)
  {
    int step = size / 8;
    var elements = new List<string>();

    for (int i = 0; i < 8; i++)
    {
      for (int j = 0; j < 8; j++)
      {
        if ((hash12[i] + hash12[j]) % 2 == 0)
        {
          elements.Add($@"<rect x=""{i * step}"" y=""{j * step}"" width=""{step}"" height=""{step}"" fill=""#{(hash12[i] * 256 + hash12[j]) % 16777216:X6}"" />");
        }
      }
    }

    return string.Join("\n  ", elements);
  }

  /// <summary>
  /// Generates the bot avatar body.
  /// </summary>
  /// <param name="hash12">The 12-character hash</param>
  /// <param name="size">The avatar size</param>
  /// <returns>SVG elements for the bot body</returns>
  private static string GenerateBotBody(string hash12, int size)
  {
    int centerX = size / 2;
    int centerY = size / 2;
    int bodyWidth = size / 2;
    int bodyHeight = size / 3;

    return $@"
  <rect x=""{centerX - bodyWidth / 2}"" y=""{centerY - bodyHeight / 2}"" width=""{bodyWidth}"" height=""{bodyHeight}"" rx=""{size / 16}"" fill=""#{(hash12[0] * 256 + hash12[1]) % 16777216:X6}"" />
  <circle cx=""{centerX - bodyWidth / 4}"" cy=""{centerY - bodyHeight / 4}"" r=""{size / 16}"" fill=""#{(hash12[2] * 256 + hash12[3]) % 16777216:X6}"" />
  <circle cx=""{centerX + bodyWidth / 4}"" cy=""{centerY - bodyHeight / 4}"" r=""{size / 16}"" fill=""#{(hash12[2] * 256 + hash12[3]) % 16777216:X6}"" />";
  }

  /// <summary>
  /// Generates the environment circle.
  /// </summary>
  /// <param name="hashBytes">The hash bytes</param>
  /// <param name="size">The avatar size</param>
  /// <returns>SVG elements for the environment</returns>
  private static string GenerateEnvironment(byte[] hashBytes, int size)
  {
    int centerX = size / 2;
    int centerY = size / 2;
    int envRadius = size / 2 - 4;

    return $@"
  <circle cx=""{centerX}"" cy=""{centerY}"" r=""{envRadius}"" fill=""none"" stroke=""#{(hashBytes[4] * 256 + hashBytes[5]) % 16777216:X6}"" stroke-width=""2"" opacity=""0.3"" />";
  }

  /// <summary>
  /// Gets the 12-character hash from hash bytes.
  /// </summary>
  /// <param name="hashBytes">The hash bytes</param>
  /// <returns>The 12-character hash</returns>
  private static string GetHash12(byte[] hashBytes)
  {
    const string chars = "0123456789abcdef";
    char[] result = new char[12];

    for (int i = 0; i < 12; i++)
    {
      result[i] = chars[hashBytes[i] % 16];
    }

    return new string(result);
  }

  /// <summary>
  /// Gets the parts breakdown from hash bytes.
  /// </summary>
  /// <param name="hashBytes">The hash bytes</param>
  /// <returns>A dictionary of avatar parts</returns>
  private static Dictionary<string, string> GetParts(byte[] hashBytes)
  {
    string hash12 = GetHash12(hashBytes);

    return new Dictionary<string, string>
    {
      ["env"] = hash12.Substring(0, 2),
      ["top"] = hash12.Substring(10, 2),
      ["bot"] = hash12.Substring(8, 2),
      ["head"] = hash12.Substring(4, 2),
      ["body"] = hash12.Substring(2, 2),
      ["prop"] = hash12.Substring(6, 2)
    };
  }
}