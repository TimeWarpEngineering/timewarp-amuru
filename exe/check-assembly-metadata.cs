#!/usr/bin/dotnet --
#:package TimeWarp.Nuru
#:project ../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj

using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Reflection;
using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static System.Console;

NuruApp app =
  new NuruAppBuilder()
  .AddAutoHelp()
  .AddRoute
  (
    "{dllPath|Path to the assembly DLL to check}",
    CheckMetadata,
    "Check assembly metadata for git commit information"
  )
  .AddRoute
  (
    "nuget {packageName|NuGet package name} --version? {version?|Package version (latest if not specified)}",
    CheckNuGetPackage,
    "Check assembly metadata in a NuGet package"
  )
  .Build();

return await app.RunAsync(args);

[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Assembly inspection tool requires reflection")]
static int CheckMetadata(string dllPath)
{
  if (!File.Exists(dllPath))
  {
    WriteLine($"❌ File not found: {dllPath}");
    return 1;
  }

  WriteLine($"Checking: {Path.GetFileName(dllPath)}\n");

  var assembly = Assembly.LoadFrom(Path.GetFullPath(dllPath));

  string? commitHash = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
    .FirstOrDefault(a => a.Key == "CommitHash")?.Value;
  string? commitDate = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
    .FirstOrDefault(a => a.Key == "CommitDate")?.Value;

  if (commitHash == null || commitDate == null)
  {
    WriteLine("❌ Git metadata not found!");
    return 1;
  }

  WriteLine("✅ Git metadata found:");
  WriteLine($"  CommitHash: {commitHash}");
  WriteLine($"  CommitDate: {commitDate}");

  return 0;
}

[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Assembly inspection tool requires reflection")]
static async Task<int> CheckNuGetPackage(string packageName, string? version = null)
{
  WriteLine($"Checking NuGet package: {packageName}" + (version != null ? $" (version {version})" : " (latest)"));
  WriteLine();

  // Create temporary directory for extraction
  string tempDir = Path.Combine(Path.GetTempPath(), $"nuget-check-{Guid.NewGuid()}");
  Directory.CreateDirectory(tempDir);

  try
  {
    WriteLine("Finding package cache location...");
    CommandOutput localsResult = await Shell.Builder("dotnet")
      .WithArguments("nuget", "locals", "global-packages", "--list")
      .CaptureAsync();

    if (!localsResult.Success)
    {
      WriteLine("❌ Failed to get NuGet cache location");
      return 1;
    }

    string cacheDir = localsResult.Stdout.Replace("global-packages: ", "", StringComparison.Ordinal).Trim();

    // Check if package is in cache, if not download it
    string packageDir = Path.Combine(cacheDir, packageName.ToLowerInvariant());
    string packagePath;

    if (!Directory.Exists(packageDir) || (version != null && !Directory.Exists(Path.Combine(packageDir, version))))
    {
      // Package not in cache, download it
      WriteLine("Package not in local cache, downloading from NuGet.org...");

      string downloadDir = Path.Combine(tempDir, "download");
      Directory.CreateDirectory(downloadDir);

      // Create a temporary project to download the package
      string projectDir = Path.Combine(downloadDir, "tempproj");
      Directory.CreateDirectory(projectDir);

      // Create a minimal project file
      string projectFile = Path.Combine(projectDir, "temp.csproj");
      await File.WriteAllTextAsync(projectFile, """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
          </PropertyGroup>
        </Project>
        """);

      // Add the package to download it
      RunBuilder builder = Shell.Builder("dotnet")
        .WithArguments("add", projectFile, "package", packageName);

      if (version != null)
      {
        builder = builder.WithArguments("--version", version);
      }
      else
      {
        builder = builder.WithArguments("--prerelease");
      }

      CommandOutput downloadResult = await builder.CaptureAsync();
      if (!downloadResult.Success)
      {
        WriteLine("❌ Failed to download package from NuGet.org");
        WriteLine(downloadResult.Combined);
        return 1;
      }

      WriteLine("Package downloaded to cache successfully");

      // Now the package should be in cache, look it up
      packageDir = Path.Combine(cacheDir, packageName.ToLowerInvariant());
      if (!Directory.Exists(packageDir))
      {
        WriteLine("❌ Package still not found in cache after download");
        return 1;
      }

      // Get the version from cache
      var versions = Directory.GetDirectories(packageDir)
        .Select(d => Path.GetFileName(d))
        .OrderByDescending(v => v)
        .ToList();

      if (versions.Count == 0)
      {
        WriteLine("❌ No versions found in cache after download");
        return 1;
      }

      version = versions[0];
      WriteLine($"Version: {version}");

      packagePath = Path.Combine(packageDir, version);

      // Find the .nupkg file in cache
      string[] cachedNupkgFiles = Directory.GetFiles(packagePath, "*.nupkg", SearchOption.AllDirectories);
      if (cachedNupkgFiles.Length == 0)
      {
        WriteLine($"❌ No .nupkg file found in {packagePath}");
        return 1;
      }

      string cachedNupkg = cachedNupkgFiles[0];
      WriteLine($"Found package: {Path.GetFileName(cachedNupkg)}");

      // Extract and analyze the package
      WriteLine("Extracting package...");
      await ZipFile.ExtractToDirectoryAsync(cachedNupkg, tempDir, CancellationToken.None);
    }
    else
    {
      // Package is in cache
      if (version == null)
      {
        var versions = Directory.GetDirectories(packageDir)
          .Select(d => Path.GetFileName(d))
          .OrderByDescending(v => v)
          .ToList();

        if (versions.Count == 0)
        {
          WriteLine($"❌ No versions found for {packageName}");
          return 1;
        }

        version = versions[0];
        WriteLine($"Found in cache, version: {version}");
      }
      else
      {
        WriteLine($"Found in cache, version: {version}");
      }

      packagePath = Path.Combine(packageDir, version);

      // Find the .nupkg file in cache
      string[] nupkgFiles = Directory.GetFiles(packagePath, "*.nupkg", SearchOption.AllDirectories);
      if (nupkgFiles.Length == 0)
      {
        WriteLine($"❌ No .nupkg file found in {packagePath}");
        return 1;
      }

      string nupkgFile = nupkgFiles[0];
      WriteLine($"Found package: {Path.GetFileName(nupkgFile)}");

      // Extract the package
      WriteLine("Extracting package...");
      await ZipFile.ExtractToDirectoryAsync(nupkgFile, tempDir, CancellationToken.None);
    }

    // Find DLL files in lib folders
    string[] dllFiles = [.. Directory.GetFiles(tempDir, "*.dll", SearchOption.AllDirectories)
      .Where(f => f.Contains("/lib/", StringComparison.Ordinal) || f.Contains("\\lib\\", StringComparison.Ordinal))];

    if (dllFiles.Length == 0)
    {
      WriteLine("❌ No DLL files found in package lib folder");
      return 1;
    }

    WriteLine($"Found {dllFiles.Length} DLL(s) in package\n");

    int result = 0;
    foreach (string dllFile in dllFiles)
    {
      string relativePath = Path.GetRelativePath(tempDir, dllFile);
      WriteLine($"=== {relativePath} ===");

      var assembly = Assembly.LoadFrom(dllFile);

      string? commitHash = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
        .FirstOrDefault(a => a.Key == "CommitHash")?.Value;
      string? commitDate = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
        .FirstOrDefault(a => a.Key == "CommitDate")?.Value;

      if (commitHash == null || commitDate == null)
      {
        WriteLine("❌ Git metadata not found!");
        result = 1;
      }
      else
      {
        WriteLine("✅ Git metadata found:");
        WriteLine($"  CommitHash: {commitHash}");
        WriteLine($"  CommitDate: {commitDate}");
      }

      WriteLine();
    }

    return result;
  }
  finally
  {
    // Cleanup
    if (Directory.Exists(tempDir))
    {
      Directory.Delete(tempDir, true);
    }
  }
}
