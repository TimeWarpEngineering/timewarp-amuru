#!/usr/bin/dotnet --
#:project $(SourceDirectory)timewarp-amuru/timewarp-amuru.csproj
#:property LangVersion=preview
#:property EnablePreviewFeatures=true

// Example demonstrating AppContext extensions for .NET 10 file-based apps

using TimeWarp.Amuru;

await TimeWarpTerminal.Default.WriteLineAsync("=== AppContext Extensions Example ===\n");

// These extension methods provide access to .NET 10's file-based app context data
// without using magic strings

await TimeWarpTerminal.Default.WriteLineAsync("File-based app context information:");
await TimeWarpTerminal.Default.WriteLineAsync($"Entry Point File Path: {AppContext.EntryPointFilePath()}");
await TimeWarpTerminal.Default.WriteLineAsync($"Entry Point Directory: {AppContext.EntryPointFileDirectoryPath()}");

// These are equivalent to the magic string approach:
// AppContext.GetData("EntryPointFilePath")
// AppContext.GetData("EntryPointFileDirectoryPath")

await TimeWarpTerminal.Default.WriteLineAsync("\nUsing the information:");

string? scriptPath = AppContext.EntryPointFilePath();
if (scriptPath != null)
{
  await TimeWarpTerminal.Default.WriteLineAsync($"Script name: {Path.GetFileName(scriptPath)}");
  await TimeWarpTerminal.Default.WriteLineAsync($"Script extension: {Path.GetExtension(scriptPath)}");
}

string? scriptDir = AppContext.EntryPointFileDirectoryPath();
if (scriptDir != null)
{
  await TimeWarpTerminal.Default.WriteLineAsync($"Parent directory: {Path.GetFileName(scriptDir)}");
  
  // List other scripts in the same directory
  CommandOutput findResult = await Shell.Builder("find")
    .WithArguments(scriptDir, "-maxdepth", "1", "-name", "*.cs", "-type", "f")
    .CaptureAsync();
  string[] otherScripts = findResult.GetStdoutLines();
  
  await TimeWarpTerminal.Default.WriteLineAsync($"\nOther scripts in {Path.GetFileName(scriptDir)}:");
  foreach (string script in otherScripts)
  {
    await TimeWarpTerminal.Default.WriteLineAsync($"  - {Path.GetFileName(script)}");
  }
}

await TimeWarpTerminal.Default.WriteLineAsync("\n✅ AppContext extensions demonstration complete!");
await TimeWarpTerminal.Default.WriteLineAsync("Testing CA1303 suppression with a literal string!");

// Test CA2007 suppression - await without ConfigureAwait
CommandOutput echoResult = await Shell.Builder("echo").WithArguments("CA2007 test").CaptureAsync();
await TimeWarpTerminal.Default.WriteLineAsync($"CA2007 test result: {echoResult.Stdout.Trim()}");