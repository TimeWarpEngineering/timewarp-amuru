#!/usr/bin/dotnet --

// find-and-filter.cs
#:package CliWrap@3.9.0
using CliWrap;
using CliWrap.Buffered;

// Helper: Run a command with optional input and return output
async Task<string> Run(string cmd, string args, string? input = null, bool ignoreExitCode = false)
{
    var command = Cli.Wrap(cmd).WithArguments(args);
    if (input != null)
        command = command.WithStandardInputPipe(PipeSource.FromString(input));
    if (ignoreExitCode)
        command = command.WithValidation(CommandResultValidation.None);
    var result = await command.ExecuteBufferedAsync();
    return result.StandardOutput.Trim();
}

// Pipeline: find .cs files | grep class | fzf
var files = await Run("find", ". -name \"*.cs\"");
var filtered = await Run("grep", "-l class", files);
var selected = await Run("fzf", "--height 40%", filtered, true);

Console.WriteLine($"Selected: {(string.IsNullOrEmpty(selected) ? "None" : selected)}");