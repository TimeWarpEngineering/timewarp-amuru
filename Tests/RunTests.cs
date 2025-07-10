#!/usr/bin/dotnet run
#:package TimeWarp.Cli
#:property RestoreNoCache true

#pragma warning disable IDE0005 // Using directive is unnecessary
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TimeWarp.Cli;
using static TimeWarp.Cli.CommandExtensions;
#pragma warning restore IDE0005

// Get script directory using CallerFilePath (C# equivalent of PowerShell's $PSScriptRoot)
static string GetScriptDirectory([CallerFilePath] string scriptPath = "")
{
  return Path.GetDirectoryName(scriptPath) ?? "";
}

// Push current directory, change to script directory for relative paths
string originalDirectory = Directory.GetCurrentDirectory();
string scriptDir = GetScriptDirectory();
Directory.SetCurrentDirectory(scriptDir);

Console.WriteLine("🧪 Running TimeWarp.Cli Test Suite...");
Console.WriteLine($"Script directory: {scriptDir}");
Console.WriteLine($"Working from: {Directory.GetCurrentDirectory()}\n");

try
{
    var testResults = new List<(string TestName, bool Passed, string Output)>();

    // Discover all test files
var testFiles = await Run("find", "Integration", "-name", "*.cs", "-type", "f").GetLinesAsync();

if (testFiles.Length == 0)
{
    Console.WriteLine("❌ No test files found in Integration/");
    Environment.Exit(1);
}

Console.WriteLine($"Found {testFiles.Length} test files:");
foreach (var file in testFiles)
{
    Console.WriteLine($"  - {file}");
}
Console.WriteLine();

// Run each test
foreach (var testFile in testFiles)
{
    var testName = Path.GetFileNameWithoutExtension(testFile);
    Console.WriteLine($"🏃 Running {testName}...");
    
    try
    {
        // Run the test script
        var process = new Process
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
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        var passed = process.ExitCode == 0;
        var fullOutput = output + (!string.IsNullOrEmpty(error) ? $"\nSTDERR:\n{error}" : "");
        
        testResults.Add((testName, passed, fullOutput));
        
        if (passed)
        {
            Console.WriteLine($"✅ {testName} PASSED");
        }
        else
        {
            Console.WriteLine($"❌ {testName} FAILED (exit code: {process.ExitCode})");
        }
        
        // Show test output with indentation
        var lines = fullOutput.Split('\n');
        foreach (var line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                Console.WriteLine($"   {line}");
            }
        }
        
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ {testName} FAILED: Exception - {ex.Message}");
        testResults.Add((testName, false, $"Exception: {ex.Message}"));
        Console.WriteLine();
    }
}

// Summary
var totalTests = testResults.Count;
var passedTests = testResults.Count(r => r.Passed);
var failedTests = totalTests - passedTests;

Console.WriteLine(new string('=', 60));
Console.WriteLine($"🎯 TEST SUMMARY");
Console.WriteLine($"Total Test Suites: {totalTests}");
Console.WriteLine($"Passed: {passedTests}");
Console.WriteLine($"Failed: {failedTests}");
Console.WriteLine($"Success Rate: {(double)passedTests / totalTests * 100:F1}%");

if (failedTests > 0)
{
    Console.WriteLine("\n❌ FAILED TESTS:");
    foreach (var result in testResults.Where(r => !r.Passed))
    {
        Console.WriteLine($"  - {result.TestName}");
    }
}

Console.WriteLine(new string('=', 60));

    // Exit with appropriate code
    Environment.Exit(failedTests == 0 ? 0 : 1);
}
finally
{
    // Pop - restore original working directory
    Directory.SetCurrentDirectory(originalDirectory);
}