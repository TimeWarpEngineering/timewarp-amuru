#!/usr/bin/dotnet --
#:property RestoreNoCache true
#:property DisableImplicitNuGetFallbackFolder true

using CliWrap;
using CliWrap.Buffered;

Console.WriteLine("=== Echo Command Execution Comparison ===");

// Test 1: Direct CliWrap ExecuteAsync (no buffering)
Console.WriteLine("\nTest 1: CliWrap ExecuteAsync (no buffering)");
try
{
  Command cmd1 = Cli.Wrap("echo").WithArguments("execute test");
  CliWrap.CommandResult result1 = await cmd1.ExecuteAsync();
  Console.WriteLine($"Exit code: {result1.ExitCode}");
  Console.WriteLine($"Success!");
}
catch (Exception ex)
{
  Console.WriteLine($"Failed: {ex.Message}");
}

// Test 2: CliWrap ExecuteBufferedAsync
Console.WriteLine("\nTest 2: CliWrap ExecuteBufferedAsync");
try
{
  Command cmd2 = Cli.Wrap("echo").WithArguments("execute test");
  BufferedCommandResult result2 = await cmd2.ExecuteBufferedAsync();
  Console.WriteLine($"Exit code: {result2.ExitCode}");
  Console.WriteLine($"Stdout: '{result2.StandardOutput}'");
  Console.WriteLine($"Stderr: '{result2.StandardError}'");
}
catch (Exception ex)
{
  Console.WriteLine($"Failed: {ex.Message}");
}

// Test 3: With validation disabled on ExecuteAsync
Console.WriteLine("\nTest 3: ExecuteAsync with validation disabled");
try
{
  Command cmd3 = Cli.Wrap("echo")
    .WithArguments("execute test")
    .WithValidation(CommandResultValidation.None);
  CliWrap.CommandResult result3 = await cmd3.ExecuteAsync();
  Console.WriteLine($"Exit code: {result3.ExitCode}");
}
catch (Exception ex)
{
  Console.WriteLine($"Failed: {ex.Message}");
}

// Test 4: With validation disabled on ExecuteBufferedAsync
Console.WriteLine("\nTest 4: ExecuteBufferedAsync with validation disabled");
try
{
  Command cmd4 = Cli.Wrap("echo")
    .WithArguments("execute test")
    .WithValidation(CommandResultValidation.None);
  BufferedCommandResult result4 = await cmd4.ExecuteBufferedAsync();
  Console.WriteLine($"Exit code: {result4.ExitCode}");
  Console.WriteLine($"Stdout: '{result4.StandardOutput}'");
  Console.WriteLine($"Stderr: '{result4.StandardError}'");
}
catch (Exception ex)
{
  Console.WriteLine($"Failed: {ex.Message}");
}

return 0;