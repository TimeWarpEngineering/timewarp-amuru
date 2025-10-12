#!/usr/bin/dotnet --
#:package CliWrap@3.9.0

using System;
using System.Threading.Tasks;
using System.IO;
using CliWrap;
using CliWrap.Buffered;

var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
Console.WriteLine($"Searching for PowerShell (.ps1) files containing 'Luna' in {homeDirectory}...");

try
{
    // Find files containing "Luna" 
    var findResult = await (
        Cli.Wrap("find")
            .WithArguments(new[] { homeDirectory, "-name", "*.ps1", "-type", "f" }) | 
        Cli.Wrap("xargs")
            .WithArguments(new[] { "grep", "-l", "Luna" })
    ).ExecuteBufferedAsync();
    
    var matchingFiles = findResult.StandardOutput.Split(
        new[] { '\n', '\r' }, 
        StringSplitOptions.RemoveEmptyEntries
    );
    
    if (matchingFiles.Length == 0)
    {
        Console.WriteLine("No PowerShell files containing 'Luna' found in your home directory.");
        return;
    }
    
    Console.WriteLine($"Found {matchingFiles.Length} files containing 'Luna'.");
    Console.WriteLine("Launching fzf for selection. Press Enter to select, Esc to cancel.\n");
    
    // Pipe files directly to fzf without temp file
    var fzfResult = await (
        Cli.Wrap("echo")
            .WithArguments(string.Join('\n', matchingFiles)) |
        Cli.Wrap("fzf")
            .WithArguments("--height", "40%", "--layout=reverse", "--border", "--prompt=Select a file: ")
    ).WithValidation(CommandResultValidation.None)
     .ExecuteBufferedAsync();
    
    var selectedFile = fzfResult.StandardOutput.Trim();
    
    if (!string.IsNullOrEmpty(selectedFile) && File.Exists(selectedFile))
    {
        Console.WriteLine($"\nYou selected: {selectedFile}");
        Console.WriteLine("\nFile contents:");
        Console.WriteLine("------------------------------------");
        Console.WriteLine(await File.ReadAllTextAsync(selectedFile));
        Console.WriteLine("------------------------------------");
    }
    else
    {
        Console.WriteLine("\nNo file was selected.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\nAn error occurred: {ex.Message}");
}