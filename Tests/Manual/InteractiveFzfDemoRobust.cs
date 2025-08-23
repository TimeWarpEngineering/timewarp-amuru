#!/usr/bin/dotnet run

// MANUAL TEST: Robust interactive FZF tests with edge case handling

Console.WriteLine("TimeWarp.Cli Interactive Features - Robust Tests");
Console.WriteLine("===============================================\n");

// Test 1: Basic selection (always works)
Console.WriteLine("Test 1: Basic FZF selection");
Console.WriteLine("---------------------------");

string selectedFruit = await Fzf.Builder()
  .FromInput("Apple", "Banana", "Cherry", "Date", "Elderberry")
  .WithPrompt("Select a fruit: ")
  .WithHeightPercent(40)
  .GetStringInteractiveAsync();

Console.WriteLine($"Result: '{selectedFruit}'\n");

// Test 2: Find actual files in current directory
Console.WriteLine("Test 2: List files in current directory");
Console.WriteLine("---------------------------------------");

// Use ls instead of find for simpler output
string selectedLocalFile = await Shell.Run("ls")
  .WithArguments("-1")  // One file per line
  .Pipe("grep", "\\.cs$")  // Only .cs files
  .Pipe("head", "-10")
  .Pipe("fzf", "--prompt", "Select a .cs file: ")
  .GetStringInteractiveAsync();

Console.WriteLine($"Result: '{selectedLocalFile}'\n");

// Test 3: Test with guaranteed content
Console.WriteLine("Test 3: Selection from echo command");
Console.WriteLine("-----------------------------------");

string echoChoice = await Shell.Run("echo")
  .WithArguments("-e", "First Option\\nSecond Option\\nThird Option")
  .Pipe("fzf", "--prompt", "Pick one: ")
  .GetStringInteractiveAsync();

Console.WriteLine($"Result: '{echoChoice}'\n");

// Test 4: Handle empty pipeline gracefully
Console.WriteLine("Test 4: Empty pipeline test (with --exit-0)");
Console.WriteLine("-------------------------------------------");
Console.WriteLine("This should exit immediately if no input...");

// Use --exit-0 to make FZF exit immediately if there's no input
string emptyResult = await Shell.Run("find")
  .WithArguments(".", "-name", "*.doesnotexist")
  .Pipe("fzf", "--exit-0", "--prompt", "Should be empty: ")
  .GetStringInteractiveAsync();

Console.WriteLine($"Result: '{emptyResult}' (should be empty)\n");

// Test 5: Use FZF's --print-query to always get output
Console.WriteLine("Test 5: FZF with --print-query (always returns something)");
Console.WriteLine("---------------------------------------------------------");

string queryResult = await Fzf.Builder()
  .FromInput("TypeScript", "JavaScript", "CoffeeScript")
  .WithPrompt("Type to search: ")
  .WithPrintQuery()  // This makes FZF print what you typed even if nothing selected
  .GetStringInteractiveAsync();

Console.WriteLine($"Result (may include your query): '{queryResult}'\n");

// Test 6: Create test files and search them
Console.WriteLine("Test 6: Search in known test files");
Console.WriteLine("----------------------------------");

// Create some temporary test files
string tempDir = Path.Combine(Path.GetTempPath(), $"fzf-test-{Guid.NewGuid()}");
Directory.CreateDirectory(tempDir);

try
{
  // Create test files
  await File.WriteAllTextAsync(Path.Combine(tempDir, "TestFile1.cs"), "public class Test1 { }");
  await File.WriteAllTextAsync(Path.Combine(tempDir, "TestFile2.cs"), "public class Test2 { }");
  await File.WriteAllTextAsync(Path.Combine(tempDir, "RegularFile.cs"), "public class Regular { }");
  
  Console.WriteLine($"Created test files in: {tempDir}");
  
  string foundTestFile = await Shell.Run("find")
    .WithArguments(tempDir, "-name", "*.cs")
    .Pipe("xargs", "grep", "-l", "Test")
    .Pipe("fzf", "--prompt", "Select a file containing 'Test': ")
    .GetStringInteractiveAsync();
  
  Console.WriteLine($"Result: '{foundTestFile}'");
}
finally
{
  // Cleanup
  if (Directory.Exists(tempDir))
  {
    Directory.Delete(tempDir, true);
  }
}

Console.WriteLine("\n\nAll tests completed!");
Console.WriteLine("====================");
Console.WriteLine("\nKey findings:");
Console.WriteLine("- Use --exit-0 flag to handle empty input gracefully");
Console.WriteLine("- Use --print-query to always get some output");
Console.WriteLine("- Simple commands like 'ls' work better than complex 'find' pipelines");
Console.WriteLine("- FZF waits for input when pipeline is empty (0/0 items)");