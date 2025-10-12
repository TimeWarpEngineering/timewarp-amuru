#!/usr/bin/dotnet --
#:property RestoreNoCache true
#:property DisableImplicitNuGetFallbackFolder true

using CliWrap;
using CliWrap.Buffered;

Console.WriteLine("=== Command Comparison: ExecuteAsync vs ExecuteBufferedAsync ===");

string[] commands = { "echo", "ls", "pwd", "date" };
string[] commandArgs = { "test", "-la", "", "" };

for (int i = 0; i < commands.Length; i++)
{
  Console.WriteLine($"\n--- Testing {commands[i]} ---");
  
  // ExecuteAsync with validation None
  try
  {
    Command cmd = Cli.Wrap(commands[i])
      .WithValidation(CommandResultValidation.None);
    
    if (!string.IsNullOrEmpty(commandArgs[i]))
      cmd = cmd.WithArguments(commandArgs[i]);
      
    CliWrap.CommandResult result = await cmd.ExecuteAsync();
    Console.WriteLine($"ExecuteAsync: Exit code = {result.ExitCode}");
  }
  catch (Exception ex)
  {
    Console.WriteLine($"ExecuteAsync: Failed - {ex.Message}");
  }
  
  // ExecuteBufferedAsync with validation None
  try
  {
    Command cmd = Cli.Wrap(commands[i])
      .WithValidation(CommandResultValidation.None);
      
    if (!string.IsNullOrEmpty(commandArgs[i]))
      cmd = cmd.WithArguments(commandArgs[i]);
      
    BufferedCommandResult result = await cmd.ExecuteBufferedAsync();
    Console.WriteLine($"ExecuteBufferedAsync: Exit code = {result.ExitCode}");
  }
  catch (Exception ex)
  {
    Console.WriteLine($"ExecuteBufferedAsync: Failed - {ex.Message}");
  }
}

return 0;