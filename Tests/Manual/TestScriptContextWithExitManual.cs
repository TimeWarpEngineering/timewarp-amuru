#!/usr/bin/dotnet --
#:project ../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:property LangVersion=preview
#:property EnablePreviewFeatures=true

using TimeWarp.Amuru;

Console.WriteLine("Testing ScriptContext with Environment.Exit:");

// Create a file to prove cleanup happened
string markerFile = Path.Combine(Path.GetTempPath(), "scriptcontext-test-marker.txt");
File.Delete(markerFile); // Clean up from any previous run

using var context = ScriptContext.FromEntryPoint(
  changeToScriptDirectory: true,
  onExit: () => 
  {
    // This should run even when Environment.Exit is called
    File.WriteAllText(markerFile, $"Cleanup ran at {DateTime.Now}");
    Console.WriteLine("✅ Cleanup callback executed!");
  });

Console.WriteLine($"Script directory: {context.ScriptDirectory}");
Console.WriteLine($"Working directory: {Directory.GetCurrentDirectory()}");
Console.WriteLine($"Marker file: {markerFile}");
Console.WriteLine("Calling Environment.Exit(0)...");

// This should trigger the cleanup
Environment.Exit(0);

// This line should never be reached
Console.WriteLine("❌ This should not print!");