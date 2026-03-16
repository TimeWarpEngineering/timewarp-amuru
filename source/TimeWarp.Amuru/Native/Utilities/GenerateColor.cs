namespace TimeWarp.Amuru.Native.Utilities;

/// <summary>
/// Utility for generating color values and schemes from seeds or input strings.
/// Supports various color formats (HEX, RGB, HSL) and color generation algorithms.
/// </summary>
public static class GenerateColor
{
  /// <summary>
  /// Generates a color from a seed string using MD5 hashing.
  /// Returns the color in hexadecimal format (#RRGGBB).
  /// </summary>
  /// <param name="seed">The seed string to generate color from</param>
  /// <returns>A hexadecimal color string</returns>
  public static string FromSeed(string seed)
  {
    if (string.IsNullOrWhiteSpace(seed))
    {
      throw new ArgumentException("Seed cannot be null or whitespace", nameof(seed));
    }

    // Using MD5 for color generation is safe - not for security
#pragma warning disable CA5351
    byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(seed);
    byte[] hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
#pragma warning restore CA5351

    // Take first 3 bytes for RGB color
    string hex = Convert.ToHexString(hashBytes.Take(3).ToArray());
    return $"#{hex}";
  }

  /// <summary>
  /// Generates a color palette with the specified number of colors.
  /// </summary>
  /// <param name="count">The number of colors to generate</param>
  /// <returns>An array of hexadecimal color strings</returns>
  public static string[] GeneratePalette(int count)
  {
    if (count <= 0)
    {
      throw new ArgumentException("Count must be greater than 0", nameof(count));
    }

    string[] palette = new string[count];
    for (int i = 0; i < count; i++)
    {
      string seed = $"palette-{i}";
      palette[i] = FromSeed(seed);
    }

    return palette;
  }

  /// <summary>
  /// Generates a color from a repository name (convenience method).
  /// </summary>
  /// <param name="repoName">The repository name</param>
  /// <returns>A hexadecimal color string</returns>
  public static string FromRepositoryName(string repoName)
  {
    return FromSeed(repoName);
  }

  /// <summary>
  /// Converts a hexadecimal color to RGB values.
  /// </summary>
  /// <param name="hexColor">The hexadecimal color string (with or without #)</param>
  /// <returns>A tuple containing R, G, B values</returns>
  public static (int R, int G, int B) HexToRgb(string hexColor)
  {
    if (string.IsNullOrWhiteSpace(hexColor))
    {
      throw new ArgumentException("Hex color cannot be null or whitespace", nameof(hexColor));
    }

    string hex = hexColor.StartsWith('#') ? hexColor.Substring(1) : hexColor;

    if (hex.Length != 6)
    {
      throw new ArgumentException("Hex color must be 6 characters long", nameof(hexColor));
    }

    try
    {
      int r = Convert.ToInt32(hex.Substring(0, 2), 16);
      int g = Convert.ToInt32(hex.Substring(2, 2), 16);
      int b = Convert.ToInt32(hex.Substring(4, 2), 16);

      return (r, g, b);
    }
    catch (FormatException)
    {
      throw new ArgumentException("Invalid hexadecimal color format", nameof(hexColor));
    }
  }

  /// <summary>
  /// Converts RGB values to a hexadecimal color string.
  /// </summary>
  /// <param name="r">Red component (0-255)</param>
  /// <param name="g">Green component (0-255)</param>
  /// <param name="b">Blue component (0-255)</param>
  /// <returns>A hexadecimal color string</returns>
  public static string RgbToHex(int r, int g, int b)
  {
    if (r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255)
    {
      throw new ArgumentException("RGB values must be between 0 and 255");
    }

    return $"#{r:X2}{g:X2}{b:X2}";
  }

  /// <summary>
  /// Generates a complementary color for the given hex color.
  /// </summary>
  /// <param name="hexColor">The base hexadecimal color</param>
  /// <returns>The complementary color in hexadecimal format</returns>
  public static string GetComplementaryColor(string hexColor)
  {
    (int r, int g, int b) = HexToRgb(hexColor);
    int compR = 255 - r;
    int compG = 255 - g;
    int compB = 255 - b;

    return RgbToHex(compR, compG, compB);
  }

  /// <summary>
  /// Generates an analogous color scheme (colors adjacent on the color wheel).
  /// </summary>
  /// <param name="hexColor">The base hexadecimal color</param>
  /// <param name="count">The number of colors in the scheme (including the base color)</param>
  /// <returns>An array of hexadecimal color strings</returns>
  public static string[] GetAnalogousScheme(string hexColor, int count = 3)
  {
    if (count < 1)
    {
      throw new ArgumentException("Count must be at least 1", nameof(count));
    }

    (int r, int g, int b) = HexToRgb(hexColor);
    string[] scheme = new string[count];

    for (int i = 0; i < count; i++)
    {
      // Rotate hue by 30 degrees for each adjacent color
      double hueRotation = (i - (count - 1) / 2.0) * 30.0;
      (int newR, int newG, int newB) = RotateHue(r, g, b, hueRotation);
      scheme[i] = RgbToHex(newR, newG, newB);
    }

    return scheme;
  }

  /// <summary>
  /// Generates a triadic color scheme (three colors evenly spaced on the color wheel).
  /// </summary>
  /// <param name="hexColor">The base hexadecimal color</param>
  /// <returns>An array of three hexadecimal color strings</returns>
  public static string[] GetTriadicScheme(string hexColor)
  {
    (int r, int g, int b) = HexToRgb(hexColor);
    string[] scheme = new string[3];

    scheme[0] = hexColor;
    (int r1, int g1, int b1) = RotateHue(r, g, b, 120);
    scheme[1] = RgbToHex(r1, g1, b1);
    (int r2, int g2, int b2) = RotateHue(r, g, b, 240);
    scheme[2] = RgbToHex(r2, g2, b2);

    return scheme;
  }

  /// <summary>
  /// Rotates the hue of an RGB color by the specified degrees.
  /// </summary>
  /// <param name="r">Red component</param>
  /// <param name="g">Green component</param>
  /// <param name="b">Blue component</param>
  /// <param name="degrees">Degrees to rotate the hue</param>
  /// <returns>A tuple containing the new R, G, B values</returns>
  private static (int, int, int) RotateHue(int r, int g, int b, double degrees)
  {
    // Convert RGB to HSL
    double rNorm = r / 255.0;
    double gNorm = g / 255.0;
    double bNorm = b / 255.0;

    double max = Math.Max(Math.Max(rNorm, gNorm), bNorm);
    double min = Math.Min(Math.Min(rNorm, gNorm), bNorm);
    double lightness = (max + min) / 2.0;
    double saturation = 0;

    if (max != min)
    {
      saturation = lightness > 0.5
        ? (max - min) / (2.0 - max - min)
        : (max - min) / (max + min);
    }

    double hue = 0;
    if (max == rNorm)
    {
      hue = ((gNorm - bNorm) / (max - min) + (gNorm < bNorm ? 6 : 0)) / 6.0;
    }
    else if (max == gNorm)
    {
      hue = ((bNorm - rNorm) / (max - min) + 2.0) / 6.0;
    }
    else
    {
      hue = ((rNorm - gNorm) / (max - min) + 4.0) / 6.0;
    }

    // Rotate hue
    hue = (hue + degrees / 360.0) % 1.0;
    if (hue < 0) hue += 1.0;

    // Convert back to RGB
    if (saturation == 0)
    {
      int gray = (int)(lightness * 255);
      return (gray, gray, gray);
    }

    double c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
    double x = c * (1 - Math.Abs((hue * 6) % 2 - 1));
    double m = lightness - c / 2;

    double rNew, gNew, bNew;
    if (hue < 1.0 / 6.0)
    {
      rNew = c; gNew = x; bNew = 0;
    }
    else if (hue < 2.0 / 6.0)
    {
      rNew = x; gNew = c; bNew = 0;
    }
    else if (hue < 3.0 / 6.0)
    {
      rNew = 0; gNew = c; bNew = x;
    }
    else if (hue < 4.0 / 6.0)
    {
      rNew = 0; gNew = x; bNew = c;
    }
    else if (hue < 5.0 / 6.0)
    {
      rNew = x; gNew = 0; bNew = c;
    }
    else
    {
      rNew = c; gNew = 0; bNew = x;
    }

    int rResult = (int)((rNew + m) * 255);
    int gResult = (int)((gNew + m) * 255);
    int bResult = (int)((bNew + m) * 255);

    return (rResult, gResult, bResult);
  }
}