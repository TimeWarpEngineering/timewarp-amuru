#!/usr/bin/dotnet --

// MANUAL TEST: Run this to test interactive FZF functionality
// This demonstrates the new GetStringInteractiveAsync() and ExecuteInteractiveAsync() methods

Console.WriteLine("TimeWarp.Cli Interactive Features Demo");
Console.WriteLine("=====================================\n");

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

// Test 2: Pipeline with interactive selection
Console.WriteLine("\n\nTest 2: Select a .cs file from current directory");
Console.WriteLine("-------------------------------------------------");

string selectedFile = await Shell.Builder("find")
  .WithArguments(".", "-name", "*.cs", "-type", "f")
  .Pipe("head", "-30")
  .Pipe("fzf", "--prompt", "Select a C# file: ", "--height", "50%", "--preview", "head -20 {}")
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

await Shell.Builder("echo")
  .WithArguments("Option A\nOption B\nOption C\nOption D")
  .Pipe("fzf", "--prompt", "This selection won't be captured: ")
  .ExecuteInteractiveAsync();

// Test 5: Complex pipeline
Console.WriteLine("\n\nTest 5: Complex pipeline - find files containing 'Test'");
Console.WriteLine("-------------------------------------------------------");

string testFile = await Shell.Builder("find")
  .WithArguments(".", "-name", "*.cs", "-type", "f")
  .Pipe("xargs", "grep", "-l", "Test")
  .Pipe("head", "-20")
  .Pipe("fzf", "--prompt", "Select a test file: ")
  .GetStringInteractiveAsync();

if (!string.IsNullOrEmpty(testFile))
{
  Console.WriteLine($"\nYou selected: '{testFile}'");
}
else
{
  Console.WriteLine("\nNo file selected or no files found containing 'Test'");
}

Console.WriteLine("\n\nAll interactive tests completed!");
Console.WriteLine("================================");