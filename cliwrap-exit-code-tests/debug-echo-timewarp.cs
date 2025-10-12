#!/usr/bin/dotnet --
#:property RestoreNoCache true
#:property DisableImplicitNuGetFallbackFolder true

using TimeWarp.Cli;

Console.WriteLine("=== Echo Test via TimeWarp.Cli ===");

// Test 1: echo without path
Console.WriteLine("\nTest 1: echo without path");
try
{
  string result = await Run("echo", "test").GetStringAsync();
  Console.WriteLine($"Success - Output: {result.Trim()}");
}
catch (Exception ex)
{
  Console.WriteLine($"Failed - Exception: {ex.Message}");
}

// Test 2: /bin/echo directly
Console.WriteLine("\nTest 2: /bin/echo directly");
try
{
  string result = await Run("/bin/echo", "test").GetStringAsync();
  Console.WriteLine($"Success - Output: {result.Trim()}");
}
catch (Exception ex)
{
  Console.WriteLine($"Failed - Exception: {ex.Message}");
}

// Test 3: /usr/bin/echo directly
Console.WriteLine("\nTest 3: /usr/bin/echo directly");
try
{
  string result = await Run("/usr/bin/echo", "test").GetStringAsync();
  Console.WriteLine($"Success - Output: {result.Trim()}");
}
catch (Exception ex)
{
  Console.WriteLine($"Failed - Exception: {ex.Message}");
}

// Test 4: sh -c echo (shell builtin)
Console.WriteLine("\nTest 4: sh -c echo (shell builtin)");
try
{
  string result = await Run("sh", "-c", "echo test").GetStringAsync();
  Console.WriteLine($"Success - Output: {result.Trim()}");
}
catch (Exception ex)
{
  Console.WriteLine($"Failed - Exception: {ex.Message}");
}

// Test 5: Check with WithNoValidation
Console.WriteLine("\nTest 5: echo with WithNoValidation");
string[] echoArgs = { "test" };
string result5 = await Run("echo", echoArgs, new CommandOptions().WithNoValidation()).GetStringAsync();
Console.WriteLine($"Output: {result5.Trim()}");

return 0;