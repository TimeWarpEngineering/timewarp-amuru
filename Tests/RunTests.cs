#!/usr/bin/dotnet run

using static System.Console;

// Get script directory using CallerFilePath (C# equivalent of PowerShell's $PSScriptRoot)
static string GetScriptDirectory([CallerFilePath] string scriptPath = "")
{
  return Path.GetDirectoryName(scriptPath) ?? "";
}

// Push current directory, change to script directory for relative paths
string originalDirectory = Directory.GetCurrentDirectory();
string scriptDir = GetScriptDirectory();
Directory.SetCurrentDirectory(scriptDir);

WriteLine("🧪 Running TimeWarp.Amuru Test Suite...");
WriteLine($"Script directory: {scriptDir}");
WriteLine($"Working from: {Directory.GetCurrentDirectory()}\n");

try
{
  var testResults = new List<(string TestName, bool Passed, string Output)>();

  // Discover all test files
  CommandOutput findOutput = await Shell.Builder("find").WithArguments("Integration", "-name", "*.cs", "-type", "f").CaptureAsync();
  string[] testFiles = findOutput.GetLines();

  if (testFiles.Length == 0)
  {
    WriteLine("❌ No test files found in Integration/");
    Environment.Exit(1);
  }

  WriteLine($"Found {testFiles.Length} test files:");
  foreach (string file in testFiles)
  {
    WriteLine($"  - {file}");
  }

  WriteLine();

  // Run each test
  foreach (string testFile in testFiles)
  {
    string testName = Path.GetFileNameWithoutExtension(testFile);
    WriteLine($"🏃 Running {testName}...");

    try
    {
      // Run the test script
      using var process = new Process()
      {
        StartInfo = new ProcessStartInfo
        {
          FileName = testFile,
          UseShellExecute = false,
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          CreateNoWindow = true
        }
      };

      process.Start();
      string output = await process.StandardOutput.ReadToEndAsync();
      string error = await process.StandardError.ReadToEndAsync();
      await process.WaitForExitAsync();

      bool passed = process.ExitCode == 0;
      string fullOutput = output + (!string.IsNullOrEmpty(error) ? $"\nSTDERR:\n{error}" : "");

      testResults.Add((testName, passed, fullOutput));

      if (passed)
      {
        WriteLine($"✅ {testName} PASSED");
      }
      else
      {
        WriteLine($"❌ {testName} FAILED (exit code: {process.ExitCode})");
      }

      // Show test output with indentation
      string[] lines = fullOutput.Split('\n');
      foreach (string line in lines)
      {
        if (!string.IsNullOrEmpty(line))
        {
          WriteLine($"   {line}");
        }
      }

      WriteLine();
    }
    catch (Exception ex)
    {
      WriteLine($"❌ {testName} FAILED: Exception - {ex.Message}");
      testResults.Add((testName, false, $"Exception: {ex.Message}"));
      WriteLine();
    }
  }

  // Summary
  int totalTests = testResults.Count;
  int passedTests = testResults.Count(r => r.Passed);
  int failedTests = totalTests - passedTests;

  WriteLine(new string('=', 60));
  WriteLine($"🎯 TEST SUMMARY");
  WriteLine($"Total Test Suites: {totalTests}");
  WriteLine($"Passed: {passedTests}");
  WriteLine($"Failed: {failedTests}");
  WriteLine($"Success Rate: {(double)passedTests / totalTests * 100:F1}%");

  if (failedTests > 0)
  {
    WriteLine("\n❌ FAILED TESTS:");
    foreach ((string TestName, bool Passed, string Output) result in testResults.Where(r => !r.Passed))
    {
      WriteLine($"  - {result.TestName}");
    }
  }

  WriteLine(new string('=', 60));

  // Exit with appropriate code
  Environment.Exit(failedTests == 0 ? 0 : 1);
}
finally
{
  // Pop - restore original working directory
  Directory.SetCurrentDirectory(originalDirectory);
}