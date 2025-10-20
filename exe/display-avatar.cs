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
  string? gitRoot = Git.FindRoot();
  if (gitRoot == null)
  {
    WriteLine("❌ Not in a git repository");
    return 1;
  }

  string? repoName = await Git.GetRepositoryNameAsync(gitRoot);
  if (repoName == null)
  {
    WriteLine("❌ Could not determine repository name");
    return 1;
  }

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
