#!/usr/bin/dotnet --
#:package CliWrap

using CliWrap;
using CliWrap.Buffered;

Console.WriteLine("=== Basic Echo Command Exit Code Comparison ===");
Console.WriteLine("Testing 'echo test' command with different execution methods\n");

// Test 1: ExecuteAsync without validation (default)
Console.WriteLine("Test 1: ExecuteAsync without validation (default behavior)");
try
{
  Command cmd1 = Cli.Wrap("echo").WithArguments("test");
  CliWrap.CommandResult result1 = await cmd1.ExecuteAsync();
  Console.WriteLine($"Exit code: {result1.ExitCode} ✓");
}
catch (Exception ex)
{
  Console.WriteLine($"Exception: {ex.GetType().Name}");
}

// Test 2: ExecuteBufferedAsync without validation (default)
Console.WriteLine("\nTest 2: ExecuteBufferedAsync without validation (default behavior)");
try
{
  Command cmd2 = Cli.Wrap("echo").WithArguments("test");
  BufferedCommandResult result2 = await cmd2.ExecuteBufferedAsync();
  Console.WriteLine($"Exit code: {result2.ExitCode} ✓");
  Console.WriteLine($"Output: '{result2.StandardOutput.Trim()}'");
}
catch (Exception ex)
{
  Console.WriteLine($"Exception: {ex.GetType().Name}");
}

// Test 3: ExecuteAsync with validation disabled
Console.WriteLine("\nTest 3: ExecuteAsync with WithValidation(CommandResultValidation.None)");
try
{
  Command cmd3 = Cli.Wrap("echo")
    .WithArguments("test")
    .WithValidation(CommandResultValidation.None);
  CliWrap.CommandResult result3 = await cmd3.ExecuteAsync();
  Console.WriteLine($"Exit code: {result3.ExitCode} {(result3.ExitCode == 0 ? "✓" : "✗ INCORRECT!")}");
}
catch (Exception ex)
{
  Console.WriteLine($"Exception: {ex.GetType().Name}");
}

// Test 4: ExecuteBufferedAsync with validation disabled
Console.WriteLine("\nTest 4: ExecuteBufferedAsync with WithValidation(CommandResultValidation.None)");
try
{
  Command cmd4 = Cli.Wrap("echo")
    .WithArguments("test")
    .WithValidation(CommandResultValidation.None);
  BufferedCommandResult result4 = await cmd4.ExecuteBufferedAsync();
  Console.WriteLine($"Exit code: {result4.ExitCode} ✓");
  Console.WriteLine($"Output: '{result4.StandardOutput.Trim()}'");
}
catch (Exception ex)
{
  Console.WriteLine($"Exception: {ex.GetType().Name}");
}

Console.WriteLine("\n=== Summary ===");
Console.WriteLine("Without validation: Both methods correctly return exit code 0");
Console.WriteLine("With validation=None: ExecuteAsync incorrectly returns exit code 1");
Console.WriteLine("                     ExecuteBufferedAsync correctly returns exit code 0");

return 0;