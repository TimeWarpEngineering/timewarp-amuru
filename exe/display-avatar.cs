#!/usr/bin/dotnet --

#:project ../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:package TimeWarp.Nuru

using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static System.Console;

NuruApp app = new NuruAppBuilder()
  .AddAutoHelp()
  .AddDefaultRoute(DisplayAvatar, "Display the repository's avatar using chafa")
  .Build();

return await app.RunAsync(args);

static async Task<int> DisplayAvatar()
{
  string? gitRoot = FindGitRoot();
  if (gitRoot == null)
  {
    WriteLine("❌ Not in a git repository");
    return 1;
  }

  string repoName = await GetRepositoryNameAsync(gitRoot);
  string avatarPath = Path.Combine(gitRoot, "assets", $"{repoName}-avatar.svg");

  if (!File.Exists(avatarPath))
  {
    WriteLine($"❌ Avatar not found: {avatarPath}");
    WriteLine("   Run ./exe/generate-avatar.cs first");
    return 1;
  }

  WriteLine($"Displaying avatar for '{repoName}':\n");

  // Display the avatar using chafa
  int exitCode = await Shell.Builder("chafa")
    .WithArguments("--size", "50", avatarPath)
    .RunAsync();

  return exitCode;
}

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

static async Task<string> GetRepositoryNameAsync(string gitRoot)
{
  // Try to get from git remote
  CommandOutput result = await Shell.Builder("git")
    .WithArguments("remote", "get-url", "origin")
    .WithNoValidation()
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
  return new DirectoryInfo(gitRoot).Name;
}
