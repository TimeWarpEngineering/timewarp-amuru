#!/usr/bin/dotnet --

#:project ../Source/TimeWarp.Multiavatar/TimeWarp.Multiavatar.csproj

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TimeWarp.Multiavatar;
using static System.Console;

// Main program entry point
return GenerateAvatar();

static int GenerateAvatar()
{
    string? gitRoot = FindGitRoot();
    if (gitRoot == null)
    {
        WriteLine("Error: Not in a git repository");
        return 1;
    }

    WriteLine($"Git repository root: {gitRoot}");

    string repoName = GetRepositoryName();
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
    File.WriteAllText(svgFilename, svgCode);
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

static string GetRepositoryName()
{
    // Try to get from git remote first
    var processInfo = new System.Diagnostics.ProcessStartInfo
    {
        FileName = "git",
        Arguments = "remote get-url origin",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    
    try
    {
        using var process = System.Diagnostics.Process.Start(processInfo);
        if (process != null)
        {
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            
            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                // Extract repo name from URL
                if (output.Contains("github.com", StringComparison.OrdinalIgnoreCase))
                {
                    string repoName = output.Split('/').Last();
                    if (repoName.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                    {
                        repoName = repoName.Substring(0, repoName.Length - 4);
                    }
                    
                    return repoName;
                }
            }
        }
    }
    catch { }
    
    // Fallback to directory name
    string? gitRoot = FindGitRoot();
    return gitRoot != null ? new DirectoryInfo(gitRoot).Name : "avatar";
}