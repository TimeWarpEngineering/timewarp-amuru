#!/usr/bin/dotnet --
#:project ../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj

using TimeWarp.Amuru;

// MANUAL TEST: Run this to verify TUI editors work with TtyPassthroughAsync
// This demonstrates the difference between PassthroughAsync and TtyPassthroughAsync

Console.WriteLine("TUI Editor Test with TtyPassthroughAsync");
Console.WriteLine("========================================\n");

string testFile = Path.Combine(Path.GetTempPath(), $"tty-test-{Guid.NewGuid():N}.txt");
File.WriteAllText(testFile, "# Test file for TUI editor\n\nEdit this content and save to verify TTY passthrough works.\n");

Console.WriteLine($"Test file: {testFile}");

// Try to detect available editor
string editor = DetectEditor();
Console.WriteLine($"Using editor: {editor}\n");

Console.WriteLine("Opening editor with TtyPassthroughAsync...\n");

try
{
  ExecutionResult result = await Shell.Builder(editor)
    .WithArguments(testFile)
    .TtyPassthroughAsync();
  
  Console.WriteLine($"\nEditor exited with code: {result.ExitCode}");
  Console.WriteLine($"Runtime: {result.RunTime}");
  
  string content = await File.ReadAllTextAsync(testFile);
  Console.WriteLine($"\nFile contents after editing:\n---\n{content}\n---");
}
catch (Exception ex)
{
  Console.WriteLine($"Error: {ex.Message}");
}
finally
{
  File.Delete(testFile);
  Console.WriteLine("\nTest file cleaned up.");
}

// Compare with PassthroughAsync (which won't work for TUI)
Console.WriteLine("\n\nComparison: Testing PassthroughAsync (may fail or not render correctly)");
Console.WriteLine("======================================================================\n");

testFile = Path.Combine(Path.GetTempPath(), $"tty-test-{Guid.NewGuid():N}.txt");
File.WriteAllText(testFile, "# Test file\nThis is for PassthroughAsync comparison.\n");

Console.WriteLine($"Test file: {testFile}");
Console.WriteLine("Opening editor with PassthroughAsync (may fail for TUI editors)...\n");

try
{
  ExecutionResult result = await Shell.Builder(editor)
    .WithArguments(testFile)
    .PassthroughAsync();
  
  Console.WriteLine($"\nEditor exited with code: {result.ExitCode}");
  Console.WriteLine($"Runtime: {result.RunTime}");
}
catch (Exception ex)
{
  Console.WriteLine($"Error (expected for TUI editors): {ex.Message}");
}
finally
{
  File.Delete(testFile);
  Console.WriteLine("\nTest file cleaned up.");
}

static string DetectEditor()
{
  // Try to find an available editor
  string[] editors = ["nano", "vim", "vi", "edit", "notepad"];
  
  foreach (string editor in editors)
  {
    try
    {
      var process = new System.Diagnostics.Process
      {
        StartInfo = new System.Diagnostics.ProcessStartInfo
        {
          FileName = "which",
          Arguments = editor,
          RedirectStandardOutput = true,
          UseShellExecute = false
        }
      };
      
      process.Start();
      process.WaitForExit();
      
      if (process.ExitCode == 0)
      {
        return editor;
      }
    }
    catch
    {
      // Try next editor
    }
  }
  
  return "nano"; // Default to nano
}
