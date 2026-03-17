#!/usr/bin/dotnet --
#:project ../source/timewarp-amuru/timewarp-amuru.csproj
#:property LangVersion=preview
#:property EnablePreviewFeatures=true

// Example demonstrating ScriptContext usage

using TimeWarp.Amuru;

Console.WriteLine("=== ScriptContext Example ===\n");

// Example 1: Basic usage - work in script's directory
Console.WriteLine("Example 1: Working in script directory");
using (var context = ScriptContext.FromEntryPoint())
{
  Console.WriteLine($"Script: {context.ScriptFilePath}");
  Console.WriteLine($"Script Dir: {context.ScriptDirectory}");
  Console.WriteLine($"Working Dir: {Directory.GetCurrentDirectory()}");
  
  // Do work in script directory
  CommandOutput lsResult = await Shell.Builder("ls").CaptureAsync();
  string[] localFiles = lsResult.GetStdoutLines();
  Console.WriteLine($"Files in script dir: {localFiles.Length}");
}

Console.WriteLine($"After disposal: {Directory.GetCurrentDirectory()}\n");

// Example 2: Navigate to parent directory
Console.WriteLine("Example 2: Working from parent directory");
using (var context = ScriptContext.FromRelativePath("..", changeToTargetDirectory: true))
{
  Console.WriteLine($"Working Dir: {Directory.GetCurrentDirectory()}");
  
  // List parent directory items
  CommandOutput parentResult = await Shell.Builder("ls").CaptureAsync();
  string[] parentItems = parentResult.GetStdoutLines();
  Console.WriteLine("Parent directory items:");
  foreach (string item in parentItems.Take(5))
  {
    Console.WriteLine($"  - {item}");
  }
}

Console.WriteLine($"After disposal: {Directory.GetCurrentDirectory()}\n");

// Example 3: With cleanup callback
Console.WriteLine("Example 3: With cleanup callback");
bool cleanupExecuted = false;
using (var context = ScriptContext.FromEntryPoint(
  changeToScriptDirectory: true,
  onExit: () => 
  {
    cleanupExecuted = true;
    Console.WriteLine("  Cleanup callback executed!");
  }))
{
  Console.WriteLine($"Working in: {Directory.GetCurrentDirectory()}");
  Console.WriteLine("  Doing some work...");
}

Console.WriteLine($"Cleanup executed: {cleanupExecuted}");

Console.WriteLine("\n✅ All examples completed!");