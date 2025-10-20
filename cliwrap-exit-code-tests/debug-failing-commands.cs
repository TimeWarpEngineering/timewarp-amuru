#!/usr/bin/dotnet --

#:property RestoreNoCache true
#:property DisableImplicitNuGetFallbackFolder true

using CliWrap;
using CliWrap.Buffered;

Console.WriteLine("=== Testing Commands That Should Fail ===");

// Commands that should fail
string[] failingCommands = { 
  "ls /nonexistent/directory",
  "cat /does/not/exist.txt",
  "false",  // always returns exit code 1
  "exit 42",  // should return 42
  "grep pattern /no/such/file"
};

foreach (string cmd in failingCommands)
{
  string[] parts = cmd.Split(' ', 2);
  string executable = parts[0];
  string arguments = parts.Length > 1 ? parts[1] : "";
  
  Console.WriteLine($"\n--- Testing: {cmd} ---");
  
  // Test 1: ExecuteAsync with validation None
  try
  {
    Command command = Cli.Wrap(executable)
      .WithValidation(CommandResultValidation.None);
    
    if (!string.IsNullOrEmpty(arguments))
      command = command.WithArguments(arguments);
      
    CliWrap.CommandResult result = await command.ExecuteAsync();
    Console.WriteLine($"ExecuteAsync (validation=None): Exit code = {result.ExitCode}");
  }
  catch (Exception ex)
  {
    Console.WriteLine($"ExecuteAsync (validation=None): Exception - {ex.Message}");
  }
  
  // Test 2: ExecuteBufferedAsync with validation None
  try
  {
    Command command = Cli.Wrap(executable)
      .WithValidation(CommandResultValidation.None);
      
    if (!string.IsNullOrEmpty(arguments))
      command = command.WithArguments(arguments);
      
    BufferedCommandResult result = await command.ExecuteBufferedAsync();
    Console.WriteLine($"ExecuteBufferedAsync (validation=None): Exit code = {result.ExitCode}");
    if (!string.IsNullOrEmpty(result.StandardError))
      Console.WriteLine($"  Stderr: {result.StandardError.Trim()}");
  }
  catch (Exception ex)
  {
    Console.WriteLine($"ExecuteBufferedAsync (validation=None): Exception - {ex.Message}");
  }
  
  // Test 3: ExecuteAsync without setting validation (default)
  try
  {
    Command command = Cli.Wrap(executable);
    
    if (!string.IsNullOrEmpty(arguments))
      command = command.WithArguments(arguments);
      
    CliWrap.CommandResult result = await command.ExecuteAsync();
    Console.WriteLine($"ExecuteAsync (default validation): Exit code = {result.ExitCode}");
  }
  catch (Exception ex)
  {
    Console.WriteLine($"ExecuteAsync (default validation): Exception - {ex.GetType().Name}: {ex.Message}");
  }
}

// Special test for 'exit' which needs to be run via shell
Console.WriteLine($"\n--- Testing: sh -c 'exit 42' ---");
string[] shellArgs = { "-c", "exit 42" };
try
{
  CliWrap.CommandResult result = await Cli.Wrap("sh")
    .WithArguments(shellArgs)
    .WithValidation(CommandResultValidation.None)
    .ExecuteAsync();
  Console.WriteLine($"ExecuteAsync (validation=None): Exit code = {result.ExitCode}");
}
catch (Exception ex)
{
  Console.WriteLine($"ExecuteAsync (validation=None): Exception - {ex.Message}");
}

try
{
  BufferedCommandResult result = await Cli.Wrap("sh")
    .WithArguments(shellArgs)
    .WithValidation(CommandResultValidation.None)
    .ExecuteBufferedAsync();
  Console.WriteLine($"ExecuteBufferedAsync (validation=None): Exit code = {result.ExitCode}");
}
catch (Exception ex)
{
  Console.WriteLine($"ExecuteBufferedAsync (validation=None): Exception - {ex.Message}");
}

return 0;