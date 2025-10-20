#!/usr/bin/dotnet --

using System.Diagnostics;

Console.WriteLine("=== Echo Test Debug ===");

// Test 1: Direct shell builtin echo
Console.WriteLine("\nTest 1: Shell builtin echo via sh -c");
var p1 = Process.Start(new ProcessStartInfo
{
  FileName = "sh",
  Arguments = "-c \"echo 'test'\"",
  RedirectStandardOutput = true,
  RedirectStandardError = true
});
if (p1 != null)
{
  p1.WaitForExit();
  Console.WriteLine($"Exit code: {p1.ExitCode}");
  Console.WriteLine($"Output: {p1.StandardOutput.ReadToEnd().Trim()}");
  Console.WriteLine($"Error: {p1.StandardError.ReadToEnd().Trim()}");
}

// Test 2: /bin/echo directly
Console.WriteLine("\nTest 2: /bin/echo directly");
var p2 = Process.Start(new ProcessStartInfo
{
  FileName = "/bin/echo",
  Arguments = "test",
  RedirectStandardOutput = true,
  RedirectStandardError = true
});
if (p2 != null)
{
  p2.WaitForExit();
  Console.WriteLine($"Exit code: {p2.ExitCode}");
  Console.WriteLine($"Output: {p2.StandardOutput.ReadToEnd().Trim()}");
  Console.WriteLine($"Error: {p2.StandardError.ReadToEnd().Trim()}");
}

// Test 3: /usr/bin/echo directly
Console.WriteLine("\nTest 3: /usr/bin/echo directly");
var p3 = Process.Start(new ProcessStartInfo
{
  FileName = "/usr/bin/echo",
  Arguments = "test",
  RedirectStandardOutput = true,
  RedirectStandardError = true
});
if (p3 != null)
{
  p3.WaitForExit();
  Console.WriteLine($"Exit code: {p3.ExitCode}");
  Console.WriteLine($"Output: {p3.StandardOutput.ReadToEnd().Trim()}");
  Console.WriteLine($"Error: {p3.StandardError.ReadToEnd().Trim()}");
}

// Test 4: echo without path (let system find it)
Console.WriteLine("\nTest 4: echo without path");
var p4 = Process.Start(new ProcessStartInfo
{
  FileName = "echo",
  Arguments = "test",
  RedirectStandardOutput = true,
  RedirectStandardError = true
});
if (p4 != null)
{
  p4.WaitForExit();
  Console.WriteLine($"Exit code: {p4.ExitCode}");
  Console.WriteLine($"Output: {p4.StandardOutput.ReadToEnd().Trim()}");
  Console.WriteLine($"Error: {p4.StandardError.ReadToEnd().Trim()}");
}

// Test 5: Check PATH environment variable
Console.WriteLine($"\nPATH: {Environment.GetEnvironmentVariable("PATH")}");

return 0;