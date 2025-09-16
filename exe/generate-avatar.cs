#!/usr/bin/dotnet --

#:project ../Source/TimeWarp.Multiavatar/TimeWarp.Multiavatar.csproj
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
  string? gitRoot = FindGitRoot();
  if (gitRoot == null)
  {
    WriteLine("Error: Not in a git repository");
    return 1;
  }

  WriteLine($"Git repository root: {gitRoot}");

  string repoName = await GetRepositoryNameAsync();
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

// Helper functions for git repository detection
static string? FindGitRoot(string? startPath = null)
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

static async Task<string> GetRepositoryNameAsync()
{
  // Try to get from git remote using Shell.Builder
  CommandOutput result =
    await Shell.Builder("git")
    .WithArguments("remote", "get-url", "origin")
    .WithNoValidation() // Don't throw on failure
    .CaptureAsync();

  if (result.Success && !string.IsNullOrWhiteSpace(result.Stdout))
  {
    string output = result.Stdout.Trim();

    // Extract repo name from URL
    if (output.Contains("github.com", StringComparison.OrdinalIgnoreCase))
    {
      string repoName = output.Split('/').Last();
      if (repoName.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
      {
        repoName = repoName[..^4]; // Remove ".git" suffix
      }

      return repoName;
    }
  }

  // Fallback to directory name
  string? gitRoot = FindGitRoot();
  return gitRoot != null ? new DirectoryInfo(gitRoot).Name : "avatar";
}