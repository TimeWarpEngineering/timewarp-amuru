#!/usr/bin/dotnet --
#:project $(SourceDirectory)timewarp-amuru/timewarp-amuru.csproj
#:property LangVersion=preview
#:property EnablePreviewFeatures=true

// Example demonstrating ScriptContext usage

using TimeWarp.Amuru;
using TimeWarp.Terminal;

await TimeWarpTerminal.Default.WriteLineAsync("=== ScriptContext Example ===\n");

// Example 1: Basic usage - work in script's directory
await TimeWarpTerminal.Default.WriteLineAsync("Example 1: Working in script directory");
using (var context = ScriptContext.FromEntryPoint())
{
  await TimeWarpTerminal.Default.WriteLineAsync($"Script: {context.ScriptFilePath}");
  await TimeWarpTerminal.Default.WriteLineAsync($"Script Dir: {context.ScriptDirectory}");
  await TimeWarpTerminal.Default.WriteLineAsync($"Working Dir: {Directory.GetCurrentDirectory()}");
  
  // Do work in script directory
  CommandOutput lsResult = await Shell.Builder("ls").CaptureAsync();
  string[] localFiles = lsResult.GetStdoutLines();
  await TimeWarpTerminal.Default.WriteLineAsync($"Files in script dir: {localFiles.Length}");
}

await TimeWarpTerminal.Default.WriteLineAsync($"After disposal: {Directory.GetCurrentDirectory()}\n");

// Example 2: Navigate to parent directory
await TimeWarpTerminal.Default.WriteLineAsync("Example 2: Working from parent directory");
using (var context = ScriptContext.FromRelativePath("..", changeToTargetDirectory: true))
{
  await TimeWarpTerminal.Default.WriteLineAsync($"Working Dir: {Directory.GetCurrentDirectory()}");
  
  // List parent directory items
  CommandOutput parentResult = await Shell.Builder("ls").CaptureAsync();
  string[] parentItems = parentResult.GetStdoutLines();
  await TimeWarpTerminal.Default.WriteLineAsync("Parent directory items:");
  foreach (string item in parentItems.Take(5))
  {
    await TimeWarpTerminal.Default.WriteLineAsync($"  - {item}");
  }
}

await TimeWarpTerminal.Default.WriteLineAsync($"After disposal: {Directory.GetCurrentDirectory()}\n");

// Example 3: With cleanup callback
await TimeWarpTerminal.Default.WriteLineAsync("Example 3: With cleanup callback");
bool cleanupExecuted = false;
using (var context = ScriptContext.FromEntryPoint(
  changeToScriptDirectory: true,
  onExit: () => 
  {
    cleanupExecuted = true;
    TimeWarpTerminal.Default.WriteLine("  Cleanup callback executed!");
  }))
{
  await TimeWarpTerminal.Default.WriteLineAsync($"Working in: {Directory.GetCurrentDirectory()}");
  await TimeWarpTerminal.Default.WriteLineAsync("  Doing some work...");
}

await TimeWarpTerminal.Default.WriteLineAsync($"Cleanup executed: {cleanupExecuted}");

await TimeWarpTerminal.Default.WriteLineAsync("\n✅ All examples completed!");