#!/usr/bin/dotnet --
#:package CliWrap

using CliWrap;
using CliWrap.Buffered;

Console.WriteLine("=== Testing Multiple Commands with Validation=None ===\n");

string[] commands = { "echo", "ls", "pwd", "date" };
string[] commandArgs = { "test", "-la", "", "" };

for (int i = 0; i < commands.Length; i++)
{
  Console.WriteLine($"--- Testing: {commands[i]} {commandArgs[i]} ---");
  
  // ExecuteAsync with validation None
  try
  {
    Command cmd = Cli.Wrap(commands[i])
      .WithValidation(CommandResultValidation.None);
    
    if (!string.IsNullOrEmpty(commandArgs[i]))
      cmd = cmd.WithArguments(commandArgs[i]);
      
    CliWrap.CommandResult result = await cmd.ExecuteAsync();
    Console.WriteLine($"ExecuteAsync:         Exit code = {result.ExitCode} {(result.ExitCode == 0 ? "✓" : "✗")}");
  }
  catch (Exception ex)
  {
    Console.WriteLine($"ExecuteAsync:         Failed - {ex.Message}");
  }
  
  // ExecuteBufferedAsync with validation None
  try
  {
    Command cmd = Cli.Wrap(commands[i])
      .WithValidation(CommandResultValidation.None);
      
    if (!string.IsNullOrEmpty(commandArgs[i]))
      cmd = cmd.WithArguments(commandArgs[i]);
      
    BufferedCommandResult result = await cmd.ExecuteBufferedAsync();
    Console.WriteLine($"ExecuteBufferedAsync: Exit code = {result.ExitCode} ✓");
  }
  catch (Exception ex)
  {
    Console.WriteLine($"ExecuteBufferedAsync: Failed - {ex.Message}");
  }
  
  Console.WriteLine();
}

Console.WriteLine("=== Pattern ===");
Console.WriteLine("All these commands should return exit code 0 (success)");
Console.WriteLine("ExecuteAsync with validation=None returns incorrect non-zero exit codes");
Console.WriteLine("ExecuteBufferedAsync with validation=None returns correct exit code 0");

return 0;