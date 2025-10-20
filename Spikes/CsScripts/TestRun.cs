#!/usr/bin/dotnet --
#:package CliWrap@3.9.0

using System;
using System.Linq;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

// Test the Run() method
Console.WriteLine("Testing Run() method...\n");

// Test 1: Simple string output
Console.WriteLine("1. Testing GetStringAsync():");
var dateOutput = await CommandExtensions.Run("date").GetStringAsync();
Console.WriteLine($"Date: {dateOutput.Trim()}");

// Test 2: Lines output
Console.WriteLine("\n2. Testing GetLinesAsync():");
var files = await CommandExtensions.Run("find", ".", "-name", "*.cs", "-type", "f").GetLinesAsync();
Console.WriteLine($"Found {files.Length} .cs files:");
foreach (var file in files.Take(5))
{
    Console.WriteLine($"  - {file}");
}

// Test 3: Find PowerShell files (like our original script)
Console.WriteLine("\n3. Testing PowerShell file search:");
var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var ps1Files = await CommandExtensions.Run("find", homeDirectory, "-name", "*.ps1", "-type", "f").GetLinesAsync();
Console.WriteLine($"Found {ps1Files.Length} PowerShell files in home directory");

Console.WriteLine("\nRun() method test completed!");

public static class CommandExtensions
{
    public static CommandResult Run(string command, params string[] args)
    {
        var cliCommand = Cli.Wrap(command)
            .WithArguments(args)
            .WithValidation(CommandResultValidation.None);
            
        return new CommandResult(cliCommand);
    }
}

public class CommandResult
{
    private readonly Command _command;
    
    internal CommandResult(Command command)
    {
        _command = command;
    }
    
    public async Task<string> GetStringAsync()
    {
        var result = await _command.ExecuteBufferedAsync();
        return result.StandardOutput;
    }
    
    public async Task<string[]> GetLinesAsync()
    {
        var result = await _command.ExecuteBufferedAsync();
        return result.StandardOutput.Split(
            new[] { '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries
        );
    }
    
    public async Task ExecuteAsync()
    {
        await _command.ExecuteAsync();
    }
}