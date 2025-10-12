#!/usr/bin/dotnet --
#:property RestoreNoCache true
#:property DisableImplicitNuGetFallbackFolder true

using CliWrap;
using CliWrap.Buffered;

Console.WriteLine("=== Testing WITHOUT setting validation ===");

// Test echo without setting validation
Console.WriteLine("\nTesting echo:");
try
{
  CliWrap.CommandResult result1 = await Cli.Wrap("echo").WithArguments("test").ExecuteAsync();
  Console.WriteLine($"ExecuteAsync: Exit code = {result1.ExitCode}");
}
catch (Exception ex)
{
  Console.WriteLine($"ExecuteAsync: Failed - {ex.Message}");
}

try
{
  BufferedCommandResult result2 = await Cli.Wrap("echo").WithArguments("test").ExecuteBufferedAsync();
  Console.WriteLine($"ExecuteBufferedAsync: Exit code = {result2.ExitCode}");
}
catch (Exception ex)
{
  Console.WriteLine($"ExecuteBufferedAsync: Failed - {ex.Message}");
}

return 0;