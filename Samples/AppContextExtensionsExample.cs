#!/usr/bin/dotnet --
#:project ../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:property LangVersion=preview
#:property EnablePreviewFeatures=true

// Example demonstrating AppContext extensions for .NET 10 file-based apps

using TimeWarp.Amuru;

Console.WriteLine("=== AppContext Extensions Example ===\n");

// These extension methods provide access to .NET 10's file-based app context data
// without using magic strings

Console.WriteLine("File-based app context information:");
Console.WriteLine($"Entry Point File Path: {AppContext.EntryPointFilePath()}");
Console.WriteLine($"Entry Point Directory: {AppContext.EntryPointFileDirectoryPath()}");

// These are equivalent to the magic string approach:
// AppContext.GetData("EntryPointFilePath")
// AppContext.GetData("EntryPointFileDirectoryPath")

Console.WriteLine("\nUsing the information:");

string? scriptPath = AppContext.EntryPointFilePath();
if (scriptPath != null)
{
  Console.WriteLine($"Script name: {Path.GetFileName(scriptPath)}");
  Console.WriteLine($"Script extension: {Path.GetExtension(scriptPath)}");
}

string? scriptDir = AppContext.EntryPointFileDirectoryPath();
if (scriptDir != null)
{
  Console.WriteLine($"Parent directory: {Path.GetFileName(scriptDir)}");
  
  // List other scripts in the same directory
  string[] otherScripts = await Shell.Builder("find")
    .WithArguments(scriptDir, "-maxdepth", "1", "-name", "*.cs", "-type", "f")
    .GetLinesAsync();
  
  Console.WriteLine($"\nOther scripts in {Path.GetFileName(scriptDir)}:");
  foreach (string script in otherScripts)
  {
    Console.WriteLine($"  - {Path.GetFileName(script)}");
  }
}

Console.WriteLine("\n✅ AppContext extensions demonstration complete!");
Console.WriteLine("Testing CA1303 suppression with a literal string!");

// Test CA2007 suppression - await without ConfigureAwait
string testOutput = await Shell.Builder("echo").WithArguments("CA2007 test").GetStringAsync();
Console.WriteLine($"CA2007 test result: {testOutput.Trim()}");