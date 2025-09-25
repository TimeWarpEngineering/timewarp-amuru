#!/usr/bin/dotnet run
// Build.cs - Build all TimeWarp.Amuru projects
#pragma warning disable IDE0005 // Using directive is unnecessary
#pragma warning restore IDE0005

// Get script directory using CallerFilePath (C# equivalent of PowerShell's $PSScriptRoot)
static string GetScriptDirectory([CallerFilePath] string scriptPath = "")
{
  return Path.GetDirectoryName(scriptPath) ?? "";
}

// Helper to build a project
async Task<bool> BuildProjectAsync(string projectPath, string projectName)
{
    Console.WriteLine($"\n🔨 Building {projectName}...");
    
    var buildProcess = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build {projectPath} --configuration Release",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    };

    buildProcess.Start();
    string output = await buildProcess.StandardOutput.ReadToEndAsync();
    string error = await buildProcess.StandardError.ReadToEndAsync();
    await buildProcess.WaitForExitAsync();

    Console.WriteLine(output);
    if (!string.IsNullOrEmpty(error))
    {
        Console.WriteLine($"Errors: {error}");
    }

    if (buildProcess.ExitCode == 0)
    {
        Console.WriteLine($"✅ {projectName} build completed successfully!");
        return true;
    }
    else
    {
        Console.WriteLine($"❌ {projectName} build failed with exit code {buildProcess.ExitCode}");
        return false;
    }
}

// Push current directory, change to script directory for relative paths
string originalDirectory = Directory.GetCurrentDirectory();
string scriptDir = GetScriptDirectory();
Directory.SetCurrentDirectory(scriptDir);

Console.WriteLine("Building TimeWarp.Amuru projects...");
Console.WriteLine($"Script directory: {scriptDir}");
Console.WriteLine($"Working from: {Directory.GetCurrentDirectory()}");

try
{
    try
    {
        bool allSuccess = true;
        
        // Build TimeWarp.Amuru library
        allSuccess &= await BuildProjectAsync(
            "../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj", 
            "TimeWarp.Amuru");
        
        // Build TimeWarp.Multiavatar library
        allSuccess &= await BuildProjectAsync(
            "../Source/TimeWarp.Multiavatar/TimeWarp.Multiavatar.csproj", 
            "TimeWarp.Multiavatar");
        
        // Build TimeWarp.Ganda (which references both libraries)
        allSuccess &= await BuildProjectAsync(
            "../Source/TimeWarp.Ganda/TimeWarp.Ganda.csproj",
            "TimeWarp.Ganda");
        
        if (allSuccess)
        {
            Console.WriteLine("\n✅ All builds completed successfully!");
        }
        else
        {
            Console.WriteLine("\n❌ One or more builds failed");
            Environment.Exit(1);
        }
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