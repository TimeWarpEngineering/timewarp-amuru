#!/usr/bin/dotnet --
#:package CliWrap@3.9.0

using System;
using System.Threading.Tasks;
using System.IO;
using CliWrap;
using CliWrap.Buffered;
using System.Diagnostics;

await FindLunaPs1FilesWithFzfAsync();

async Task FindLunaPs1FilesWithFzfAsync()
{
    // Get the home directory path
    string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    Console.WriteLine($"Searching for PowerShell (.ps1) files containing 'Luna' in {homeDirectory}...");

    try
    {
        // First, let's find files containing "Luna" and save them to a temporary file
        var tempFilePath = Path.GetTempFileName();
        
        // Execute find and grep to get all matching files
        var findAndGrepResult = await (
            Cli.Wrap("find")
                .WithArguments(new[] { homeDirectory, "-name", "*.ps1", "-type", "f" }) | 
            Cli.Wrap("xargs")
                .WithArguments(new[] { "grep", "-l", "Luna" })
        ).ExecuteBufferedAsync();
        
        // Check if we found any files
        var matchingFiles = findAndGrepResult.StandardOutput.Split(
            new[] { '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries
        );
        
        if (matchingFiles.Length == 0)
        {
            Console.WriteLine("No PowerShell files containing 'Luna' found in your home directory.");
            return;
        }
        
        // Write the matching files to a temporary file
        await File.WriteAllLinesAsync(tempFilePath, matchingFiles);
        
        Console.WriteLine($"Found {matchingFiles.Length} files containing 'Luna'.");
        Console.WriteLine("Launching fzf for selection. Press Enter to select, Esc to cancel.\n");
        
        // Now run fzf on this file to let the user select
        var fzfProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                // Use bash to interpret the redirection
                FileName = "/bin/bash",
                Arguments = $"-c \"cat {tempFilePath} | fzf --height 40% --layout=reverse --border --prompt='Select a file: '\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = homeDirectory
            }
        };
        
        fzfProcess.Start();
        string selectedFile = await fzfProcess.StandardOutput.ReadToEndAsync();
        await fzfProcess.WaitForExitAsync();
        
        // Delete the temporary file
        File.Delete(tempFilePath);
        
        // Process the selected file
        selectedFile = selectedFile.Trim();
        if (!string.IsNullOrEmpty(selectedFile))
        {
            Console.WriteLine($"\nYou selected: {selectedFile}");
            
            // Optionally, display the contents of the selected file
            if (File.Exists(selectedFile))
            {
                Console.WriteLine("\nFile contents:");
                Console.WriteLine("------------------------------------");
                Console.WriteLine(await File.ReadAllTextAsync(selectedFile));
                Console.WriteLine("------------------------------------");
            }
        }
        else
        {
            // This could happen if user pressed Esc
            Console.WriteLine("\nNo file was selected.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nAn error occurred: {ex.Message}");
    }
}