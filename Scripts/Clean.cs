#!/usr/bin/dotnet --
// Clean.cs - Clean build artifacts and local NuGet packages
#pragma warning disable IDE0005 // Using directive is unnecessary
#pragma warning restore IDE0005

// Get script directory using CallerFilePath (C# equivalent of PowerShell's $PSScriptRoot)
static string GetScriptDirectory([CallerFilePath] string scriptPath = "")
{
  return Path.GetDirectoryName(scriptPath) ?? "";
}

// Push current directory, change to script directory for relative paths
string originalDirectory = Directory.GetCurrentDirectory();
string scriptDir = GetScriptDirectory();
Directory.SetCurrentDirectory(scriptDir);

Console.WriteLine("Cleaning build artifacts and local NuGet packages...");
Console.WriteLine($"Script directory: {scriptDir}");
Console.WriteLine($"Working from: {Directory.GetCurrentDirectory()}");

try
{
    try
    {
        // Clean the project
    var cleanProcess = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "clean ../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    };

    cleanProcess.Start();
    string cleanOutput = await cleanProcess.StandardOutput.ReadToEndAsync();
    string cleanError = await cleanProcess.StandardError.ReadToEndAsync();
    await cleanProcess.WaitForExitAsync();

    Console.WriteLine(cleanOutput);
    if (!string.IsNullOrEmpty(cleanError))
    {
        Console.WriteLine($"Clean errors: {cleanError}");
    }

    if (cleanProcess.ExitCode == 0)
    {
        Console.WriteLine("✅ Project cleaned successfully!");
    }
    else
    {
        Console.WriteLine($"❌ Clean failed with exit code {cleanProcess.ExitCode}");
    }

    // Clean local NuGet feed
    string localFeedPath = "../LocalNuGetFeed";
    if (Directory.Exists(localFeedPath))
    {
        string[] timeWarpPackages = Directory.GetDirectories(localFeedPath, "timewarp.amuru", SearchOption.AllDirectories);
        foreach (string packageDir in timeWarpPackages)
        {
            Directory.Delete(packageDir, true);
            Console.WriteLine($"🗑️  Removed: {packageDir}");
        }

        string[] nupkgFiles = Directory.GetFiles(localFeedPath, "TimeWarp.Amuru.*.nupkg", SearchOption.AllDirectories);
        foreach (string file in nupkgFiles)
        {
            File.Delete(file);
            Console.WriteLine($"🗑️  Removed: {file}");
        }
    }

    // Clean TimeWarp.Amuru from user's global NuGet cache
    string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    string globalNuGetCache = Path.Combine(userProfile, ".nuget", "packages", "timewarp.amuru");
    if (Directory.Exists(globalNuGetCache))
    {
        Directory.Delete(globalNuGetCache, true);
        Console.WriteLine($"🗑️  Removed global NuGet cache: {globalNuGetCache}");
    }

    // Clean LocalNuGetCache directories
    string[] cacheDirectories = {
        "../LocalNuGetCache/timewarp.amuru",
        "../Tests/LocalNuGetCache/timewarp.amuru",
        "../Tests/Integration/Core/LocalNuGetCache/timewarp.amuru"
    };

    foreach (string cacheDir in cacheDirectories)
    {
        if (Directory.Exists(cacheDir))
        {
            Directory.Delete(cacheDir, true);
            Console.WriteLine($"🗑️  Removed cache directory: {cacheDir}");
        }
    }

    Console.WriteLine("✅ Cleanup completed!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ An error occurred: {ex.Message}");
        Environment.Exit(1);
    }
}
finally
{
    // Pop - restore original working directory
    Directory.SetCurrentDirectory(originalDirectory);
}