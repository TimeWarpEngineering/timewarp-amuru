#!/usr/bin/dotnet --
#:package TimeWarp.Nuru
#:property RestoreNoCache=true

// dev.cs - Development CLI for TimeWarp.Amuru
// Simple CLI without complex Nuru routing - uses direct execution

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

string command = args.Length > 0 ? args[0] : "help";

switch (command)
{
  case "build":
    await BuildCommand(args);
    break;
  case "test":
    await TestCommand(args);
    break;
  case "clean":
    await CleanCommand(args);
    break;
  case "self-install":
    await SelfInstallCommand(args);
    break;
  case "--capabilities":
    OutputCapabilities();
    break;
  case "help":
  case "--help":
  case "-h":
  default:
    ShowHelp();
    break;
}

void ShowHelp()
{
  Console.WriteLine("dev - Development CLI for TimeWarp.Amuru");
  Console.WriteLine("");
  Console.WriteLine("Usage: dev <command>");
  Console.WriteLine("");
  Console.WriteLine("Commands:");
  Console.WriteLine("  build        Build the TimeWarp.Amuru project");
  Console.WriteLine("  test         Run the integration test suite");
  Console.WriteLine("  clean        Clean build artifacts and caches");
  Console.WriteLine("  self-install AOT compile this CLI to ./bin/dev");
  Console.WriteLine("  --capabilities Output JSON for AI agent discovery");
  Console.WriteLine("");
}

void OutputCapabilities()
{
  // Output JSON directly without reflection-based serialization (AOT-compatible)
  Console.WriteLine("""
    {
      "name": "dev",
      "version": "0.1.0",
      "description": "Development CLI for TimeWarp.Amuru",
      "commands": [
        {
          "pattern": "build",
          "description": "Build the TimeWarp.Amuru project",
          "options": [
            { "name": "verbose", "shortName": "v", "description": "Show detailed output", "type": "bool" }
          ]
        },
        { "pattern": "test", "description": "Run the integration test suite", "options": [] },
        { "pattern": "clean", "description": "Clean build artifacts and caches", "options": [] },
        { "pattern": "self-install", "description": "AOT compile this CLI to ./bin/dev", "options": [] }
      ]
    }
    """);
}

async Task BuildCommand(string[] args)
{
  bool verbose = args.Contains("-v") || args.Contains("--verbose");
  string verbosity = verbose ? "normal" : "minimal";

  Console.WriteLine("Building TimeWarp.Amuru...");

  string scriptDir = GetScriptDirectory();
  string projectPath = Path.Combine(scriptDir, "..", "..", "Source", "TimeWarp.Amuru", "TimeWarp.Amuru.csproj");
  projectPath = Path.GetFullPath(projectPath);

  if (!File.Exists(projectPath))
  {
    Console.WriteLine($"❌ Project not found: {projectPath}");
    Environment.Exit(1);
  }

  int exitCode = await RunProcessAsync("dotnet", $"build \"{projectPath}\" -c Release --verbosity {verbosity}");

  if (exitCode != 0)
  {
    Console.WriteLine($"❌ Build failed with exit code {exitCode}");
    Environment.Exit(1);
  }

  Console.WriteLine("✅ Build completed successfully!");
}

async Task TestCommand(string[] args)
{
  Console.WriteLine("🧪 Running TimeWarp.Amuru Test Suite...");

  string scriptDir = GetScriptDirectory();
  string testsDir = Path.Combine(scriptDir, "..", "..", "Tests", "TimeWarp.Amuru");
  testsDir = Path.GetFullPath(testsDir);
  string runTestsPath = Path.Combine(testsDir, "run-tests.cs");

  if (!File.Exists(runTestsPath))
  {
    Console.WriteLine($"❌ run-tests.cs not found: {runTestsPath}");
    Environment.Exit(1);
  }

  Console.WriteLine($"Working from: {testsDir}");

  string originalDirectory = Directory.GetCurrentDirectory();
  try
  {
    Directory.SetCurrentDirectory(testsDir);
    int exitCode = await RunProcessAsync("dotnet", $"run {runTestsPath}");

    if (exitCode != 0)
    {
      Console.WriteLine($"❌ Tests failed with exit code {exitCode}");
      Environment.Exit(1);
    }

    Console.WriteLine("✅ All tests passed!");
  }
  finally
  {
    Directory.SetCurrentDirectory(originalDirectory);
  }
}

async Task CleanCommand(string[] args)
{
  Console.WriteLine("🧹 Cleaning build artifacts...");

  string scriptDir = GetScriptDirectory();
  string repoRoot = Path.GetFullPath(Path.Combine(scriptDir, "..", ".."));

  // Clean the project
  string projectPath = Path.Combine(repoRoot, "Source", "TimeWarp.Amuru", "TimeWarp.Amuru.csproj");

  if (File.Exists(projectPath))
  {
    Console.WriteLine("Cleaning project...");
    await RunProcessAsync("dotnet", $"clean \"{projectPath}\"");
  }

  // Clean local NuGet feed
  string localFeedPath = Path.Combine(repoRoot, "artifacts", "packages");
  if (Directory.Exists(localFeedPath))
  {
    foreach (string dir in Directory.GetDirectories(localFeedPath, "timewarp.amuru", SearchOption.AllDirectories))
    {
      Directory.Delete(dir, true);
      Console.WriteLine($"🗑️  Removed: {dir}");
    }

    foreach (string file in Directory.GetFiles(localFeedPath, "TimeWarp.Amuru.*.nupkg", SearchOption.AllDirectories))
    {
      File.Delete(file);
      Console.WriteLine($"🗑️  Removed: {file}");
    }
  }

  // Clean bin directory
  string binDir = Path.Combine(repoRoot, "bin");
  if (Directory.Exists(binDir))
  {
    Directory.Delete(binDir, true);
    Console.WriteLine($"🗑️  Removed bin directory: {binDir}");
  }

  // Clean caches
  string[] cacheDirs = new[]
  {
    Path.Combine(repoRoot, "LocalNuGetCache", "timewarp.amuru"),
    Path.Combine(repoRoot, "Tests", "LocalNuGetCache", "timewarp.amuru")
  };

  foreach (string cacheDir in cacheDirs)
  {
    if (Directory.Exists(cacheDir))
    {
      Directory.Delete(cacheDir, true);
      Console.WriteLine($"🗑️  Removed cache: {cacheDir}");
    }
  }

  Console.WriteLine("✅ Cleanup completed!");
}

async Task SelfInstallCommand(string[] args)
{
  Console.WriteLine("🔨 Self-installing dev CLI...");

  string scriptDir = GetScriptDirectory();
  string repoRoot = Path.GetFullPath(Path.Combine(scriptDir, "..", ".."));
  string binDir = Path.Combine(repoRoot, "bin");
  string devPath = Path.Combine(binDir, "dev");
  string thisScriptPath = Path.Combine(scriptDir, "dev.cs");

  Directory.CreateDirectory(binDir);

  if (File.Exists(devPath))
  {
    File.Delete(devPath);
  }

  // AOT publish
  Console.WriteLine("AOT compiling dev.cs...");
  int exitCode = await RunProcessAsync("dotnet", $"publish \"{thisScriptPath}\" -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -p:PublishAot=true -o \"{binDir}\"");

  if (exitCode != 0)
  {
    Console.WriteLine($"❌ AOT publish failed with exit code {exitCode}");
    Environment.Exit(1);
  }

  await RunProcessAsync("chmod", $"+x \"{devPath}\"");

  Console.WriteLine($"✅ dev CLI installed to: {devPath}");
  Console.WriteLine("\nYou can now use: ./bin/dev <command>");
}

async Task<int> RunProcessAsync(string fileName, string arguments)
{
  using Process process = new();
  process.StartInfo = new ProcessStartInfo
  {
    FileName = fileName,
    Arguments = arguments,
    UseShellExecute = false,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    CreateNoWindow = true
  };

  process.Start();
  string output = await process.StandardOutput.ReadToEndAsync();
  string error = await process.StandardError.ReadToEndAsync();
  await process.WaitForExitAsync();

  if (!string.IsNullOrEmpty(output)) Console.WriteLine(output);
  if (!string.IsNullOrEmpty(error)) Console.Error.WriteLine(error);

  return process.ExitCode;
}

string GetScriptDirectory([CallerFilePath] string scriptPath = "")
{
  return Path.GetDirectoryName(scriptPath) ?? Directory.GetCurrentDirectory();
}
