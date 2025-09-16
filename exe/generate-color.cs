#!/usr/bin/dotnet --

#:package TimeWarp.Nuru
#:property TrimMode=partial
#:property NoWarn=IL2104;IL3053

using System.Security.Cryptography;
using System.Text;
using TimeWarp.Nuru;
using static System.Console;

NuruAppBuilder builder = new();

builder.AddAutoHelp();

builder.AddRoute
(
  "{seed|Text seed for deterministic color generation}",
  GenerateColor,
  "Generate consistent colors from text seeds (outputs Hex, RGB, and HSL values)"
);

NuruApp app = builder.Build();
return await app.RunAsync(args);

static void GenerateColor(string seed)
{
  byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(seed));

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