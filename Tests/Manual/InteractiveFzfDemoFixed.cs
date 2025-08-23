#!/usr/bin/dotnet run

// MANUAL TEST: Run this to test interactive FZF functionality
// This demonstrates the new GetStringInteractiveAsync() and ExecuteInteractiveAsync() methods

Console.WriteLine("TimeWarp.Cli Interactive Features Demo (Fixed)");
Console.WriteLine("==============================================\n");

// Test 1: Simple FZF selection
Console.WriteLine("Test 1: Select a fruit using GetStringInteractiveAsync()");
Console.WriteLine("---------------------------------------------------------");

string selectedFruit = await Fzf.Builder()
  .FromInput("Apple", "Banana", "Cherry", "Date", "Elderberry")
  .WithPrompt("Select a fruit: ")
  .WithHeightPercent(40)
  .WithBorder("rounded")
  .GetStringInteractiveAsync();

Console.WriteLine($"\nYou selected: '{selectedFruit}'");

// Test 2: Pipeline with interactive selection - LIMITED results
Console.WriteLine("\n\nTest 2: Select a .cs file from current directory (limited to 10)");
Console.WriteLine("-----------------------------------------------------------------");

string selectedFile = await Shell.Run("find")
  .WithArguments(".", "-name", "*.cs", "-type", "f")
  .Pipe("head", "-10")  // Limit to first 10 files
  .Pipe("fzf", "--prompt", "Select a C# file: ", "--height", "50%")
  .GetStringInteractiveAsync();

Console.WriteLine($"\nYou selected: '{selectedFile}'");

// Test 3: Multi-select
Console.WriteLine("\n\nTest 3: Multi-select colors (use Tab to select multiple)");
Console.WriteLine("--------------------------------------------------------");

string selectedColors = await Fzf.Builder()
  .FromInput("Red", "Green", "Blue", "Yellow", "Purple", "Orange", "Pink", "Brown")
  .WithMulti()
  .WithPrompt("Select colors (Tab to multi-select): ")
  .WithHeightPercent(50)
  .GetStringInteractiveAsync();

if (!string.IsNullOrEmpty(selectedColors))
{
  Console.WriteLine("\nYou selected:");
  foreach (string color in selectedColors.Split('\n', StringSplitOptions.RemoveEmptyEntries))
  {
    Console.WriteLine($"  - {color}");
  }
}

// Test 4: ExecuteInteractiveAsync (output goes to console)
Console.WriteLine("\n\nTest 4: ExecuteInteractiveAsync (full console output)");
Console.WriteLine("----------------------------------------------------");
Console.WriteLine("The following will show FZF but won't capture the selection:\n");

await Shell.Run("echo")
  .WithArguments("Option A\nOption B\nOption C\nOption D")
  .Pipe("fzf", "--prompt", "This selection won't be captured: ")
  .ExecuteInteractiveAsync();

// Test 5: Find test files - with better filtering
Console.WriteLine("\n\nTest 5: Find C# files in Tests directory");
Console.WriteLine("----------------------------------------");

// First check if we're in the right location
string currentDir = Environment.CurrentDirectory;
Console.WriteLine($"Current directory: {currentDir}");

// Look specifically in the Tests directory if it exists
string searchPath = Directory.Exists("Tests") ? "Tests" : ".";

string testFile = await Shell.Run("find")
  .WithArguments(searchPath, "-name", "*.cs", "-type", "f")
  .Pipe("head", "-15")  // Limit results
  .Pipe("fzf", "--prompt", "Select a test file: ", "--height", "60%")
  .GetStringInteractiveAsync();

if (!string.IsNullOrEmpty(testFile))
{
  Console.WriteLine($"\nYou selected: '{testFile}'");
}
else
{
  Console.WriteLine("\nNo file selected or cancelled");
}

// Test 6: Simple list selection to verify FZF is working
Console.WriteLine("\n\nTest 6: Simple list selection (debugging test)");
Console.WriteLine("----------------------------------------------");

string simpleChoice = await Shell.Run("printf")
  .WithArguments("Option1\\nOption2\\nOption3")
  .Pipe("fzf", "--prompt", "Choose: ")
  .GetStringInteractiveAsync();

Console.WriteLine($"You selected: '{simpleChoice}'");

Console.WriteLine("\n\nAll interactive tests completed!");
Console.WriteLine("================================");