#!/usr/bin/dotnet --
#:package CliWrap@3.9.0

using System;
using System.Threading.Tasks;
using System.IO;
using CliWrap;
using CliWrap.Buffered;
using System.Text;

await FindLunaSyncPs1FilesAsync();

async Task FindLunaSyncPs1FilesAsync()
{
    // Get the home directory path
    string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    Console.WriteLine($"Searching for PowerShell (.ps1) files containing 'LunaSync' in {homeDirectory}...\n");

    try
    {
        // Create a pipeline using the CliWrap Pipes API
        // First, find all .ps1 files and pipe to grep
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

        // Execute the commands in pipeline
        var findCommand = Cli.Wrap("find")
            .WithArguments(new[] { homeDirectory, "-name", "*.ps1", "-type", "f" });
            
        var grepCommand = Cli.Wrap("xargs")
            .WithArguments(new[] { "grep", "-l", "LunaSync" });

        // Build and execute the pipeline properly
        var result = await (
            findCommand | 
            grepCommand
        ).ExecuteBufferedAsync();

        // Process the results
        var files = result.StandardOutput.Split(
            new[] { '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries
        );

        // Display results
        if (files.Length > 0)
        {
            Console.WriteLine($"Found {files.Length} PowerShell files containing 'LunaSync':");
            foreach (var file in files)
            {
                Console.WriteLine($"- {file}");
            }
            Console.WriteLine("\nSearch completed successfully.");
        }
        else
        {
            Console.WriteLine("No PowerShell files containing 'LunaSync' found in your home directory.");
        }

        // Display any errors
        if (!string.IsNullOrWhiteSpace(result.StandardError))
        {
            Console.WriteLine($"\nErrors encountered:\n{result.StandardError}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
        Console.WriteLine($"Error details: {ex}");
    }
}