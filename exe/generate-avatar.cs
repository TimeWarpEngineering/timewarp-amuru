#!/usr/bin/dotnet --

#:package TimeWarp.Multiavatar
#:project ../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:package TimeWarp.Nuru
#:property TrimMode=partial
#:property NoWarn=IL2104;IL3053;IL2087

using TimeWarp.Amuru;
using TimeWarp.Multiavatar;
using TimeWarp.Nuru;
using static System.Console;

NuruAppBuilder builder = new();

builder.AddAutoHelp();

builder.AddDefaultRoute
(
  GenerateAvatar,
  "Generate an SVG avatar for the current git repository and save to assets/"
);

NuruApp app = builder.Build();
return await app.RunAsync(args);

static async Task<int> GenerateAvatar()
{
  string? gitRoot = Git.FindRoot();
  if (gitRoot == null)
  {
    WriteLine("Error: Not in a git repository");
    return 1;
  }

  WriteLine($"Git repository root: {gitRoot}");

  string? repoName = await Git.GetRepositoryNameAsync(gitRoot);
  if (repoName == null)
  {
    WriteLine("Error: Could not determine repository name");
    return 1;
  }

  WriteLine($"Repository name: {repoName}");

  // Create assets directory if it doesn't exist
  string assetsDir = Path.Combine(gitRoot, "assets");
  if (!Directory.Exists(assetsDir))
  {
    Directory.CreateDirectory(assetsDir);
    WriteLine($"Created assets directory: {assetsDir}");
  }

  // Generate avatar
  string svgCode = MultiavatarGenerator.Generate(repoName);

  // Save SVG file
  string svgFilename = Path.Combine(assetsDir, $"{repoName}-avatar.svg");
  await File.WriteAllTextAsync(svgFilename, svgCode);
  WriteLine($"Generated {svgFilename}");

  WriteLine($"Successfully generated avatar for '{repoName}'");

  return 0;
}