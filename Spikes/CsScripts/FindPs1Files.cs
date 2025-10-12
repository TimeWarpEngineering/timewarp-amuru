#!/usr/bin/dotnet --
#:package CliWrap@3.9.0

using System;
using System.Threading.Tasks;
using System.IO;
using CliWrap;
using CliWrap.Buffered;

// This attribute tells the compiler to generate a top-level main method
// which is needed for async operations in a single file program
await FindPs1FilesAsync();

async Task FindPs1FilesAsync()
{
    // Get the home directory path
    string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    Console.WriteLine($"Searching for PowerShell (.ps1) files in your home directory ({homeDirectory})...\n");

    try
    {
        // Use CliWrap to execute the find command with the expanded home directory path
        var result = await Cli.Wrap("find")
            .WithArguments(new[] { homeDirectory, "-name", "*.ps1", "-type", "f" })
            .WithValidation(CommandResultValidation.None) // Don't throw on non-zero exit codes
            .ExecuteBufferedAsync();

        // Check the output
        var files = result.StandardOutput.Split(
            new[] { '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries
        );

        // Display results
        if (files.Length > 0)
        {
            Console.WriteLine($"Found {files.Length} PowerShell files:");
            foreach (var file in files)
            {
                Console.WriteLine($"- {file}");
            }
            Console.WriteLine("\nSearch completed successfully.");
        }
        else
        {
            Console.WriteLine("No .ps1 files found in your home directory.");
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
    }
}