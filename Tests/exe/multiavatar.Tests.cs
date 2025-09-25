#!/usr/bin/dotnet --

#:package TimeWarp.Multiavatar
#:project ../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:property TrimMode=partial

using TimeWarp.Amuru;
using TimeWarp.Multiavatar;
using static System.Console;

// Use ScriptContext to work from the correct directory
using var context = ScriptContext.FromRelativePath("../..");

// Simple test runner
async Task<CommandOutput> RunMultiAvatarAsync(params string[] args)
{
    string scriptPath = Path.Combine("exe", "multiavatar.cs");
    return await Shell.Builder(scriptPath)
        .WithArguments(args)
        .WithNoValidation() // Don't throw on non-zero exit
        .CaptureAsync();
}

void Assert(bool condition, string message)
{
    if (!condition)
    {
        WriteLine($"❌ FAILED: {message}");
        Environment.Exit(1);
    }
}

void AssertEqual(string actual, string expected, string message)
{
    if (actual != expected)
    {
        WriteLine($"❌ FAILED: {message}");
        WriteLine($"  Expected: {expected[..Math.Min(expected.Length, 100)]}...");
        WriteLine($"  Actual: {actual[..Math.Min(actual.Length, 100)]}...");
        Environment.Exit(1);
    }
}

// Run tests
WriteLine("Running multiavatar tests...\n");

// Test 1: Basic SVG generation
WriteLine("Test 1: Should generate valid SVG for input");
CommandOutput result1 = await RunMultiAvatarAsync("test@example.com");
if (!result1.Success)
{
    WriteLine($"Command failed with exit code: {result1.ExitCode}");
    WriteLine($"Stdout: {result1.Stdout}");
    WriteLine($"Stderr: {result1.Stderr}");
}

Assert(result1.Success, "Command should succeed");
Assert(result1.Stdout.StartsWith("<svg", StringComparison.Ordinal), "Output should start with <svg");
Assert(result1.Stdout.TrimEnd().EndsWith("</svg>", StringComparison.Ordinal), "Output should end with </svg>");
Assert(result1.Stdout.Contains("viewBox", StringComparison.Ordinal), "Output should contain viewBox");
WriteLine("✅ PASSED\n");

// Test 2: Consistent output
WriteLine("Test 2: Should generate consistent output for same input");
CommandOutput consistent1 = await RunMultiAvatarAsync("consistent@test.com");
CommandOutput consistent2 = await RunMultiAvatarAsync("consistent@test.com");
AssertEqual(consistent1.Stdout, consistent2.Stdout, "Same input should generate same output");
WriteLine("✅ PASSED\n");

// Test 3: Different outputs for different inputs
WriteLine("Test 3: Should generate different avatars for different inputs");
CommandOutput different1 = await RunMultiAvatarAsync("user1@example.com");
CommandOutput different2 = await RunMultiAvatarAsync("user2@example.com");
Assert(different1.Stdout != different2.Stdout, "Different inputs should generate different outputs");
WriteLine("✅ PASSED\n");

// Test 4: Save to file
WriteLine("Test 4: Should save to file with -o option");
string tempFile = Path.GetTempFileName() + ".svg";
try
{
    CommandOutput fileResult = await RunMultiAvatarAsync("file@test.com", "-o", tempFile);
    Assert(fileResult.Success, "Command should succeed");
    Assert(fileResult.Stdout.Contains($"Avatar saved to: {tempFile}", StringComparison.Ordinal), "Should confirm file was saved");
    Assert(File.Exists(tempFile), "File should exist");

    string content = await File.ReadAllTextAsync(tempFile);
    Assert(content.StartsWith("<svg", StringComparison.Ordinal), "File content should start with <svg");
    Assert(content.TrimEnd().EndsWith("</svg>", StringComparison.Ordinal), "File content should end with </svg>");
    WriteLine("✅ PASSED\n");
}
finally
{
    if (File.Exists(tempFile))
        File.Delete(tempFile);
}

// Test 5: No environment flag
WriteLine("Test 5: Should generate without environment with --no-env flag");
CommandOutput withEnv = await RunMultiAvatarAsync("noenv@test.com");
CommandOutput withoutEnv = await RunMultiAvatarAsync("noenv@test.com", "--no-env");
Assert(withEnv.Success && withoutEnv.Success, "Both commands should succeed");
Assert(withoutEnv.Stdout.Length < withEnv.Stdout.Length, "Without env should be shorter");
WriteLine("✅ PASSED\n");

// Test 6: Hash output
WriteLine("Test 6: Should display hash details with --output-hash flag");
CommandOutput hashResult = await RunMultiAvatarAsync("hash@test.com", "--output-hash");
Assert(hashResult.Success, "Command should succeed");
Assert(hashResult.Stdout.Contains("SHA256:", StringComparison.Ordinal), "Should contain SHA256");
Assert(hashResult.Stdout.Contains("Numbers:", StringComparison.Ordinal), "Should contain Numbers");
Assert(hashResult.Stdout.Contains("Hash-12:", StringComparison.Ordinal), "Should contain Hash-12");
Assert(hashResult.Stdout.Contains("Parts:", StringComparison.Ordinal), "Should contain Parts");
Assert(!hashResult.Stdout.Contains("<svg", StringComparison.Ordinal), "Should NOT contain SVG");
WriteLine("✅ PASSED\n");

// Test 7: Help flag
WriteLine("Test 7: Should show help with --help flag");
CommandOutput helpResult = await RunMultiAvatarAsync("--help");
Assert(helpResult.Success, "Command should succeed");
Assert(helpResult.Stdout.Contains("Generate unique, deterministic SVG avatars", StringComparison.Ordinal), "Should contain description");
Assert(helpResult.Stdout.Contains("--output", StringComparison.Ordinal), "Should mention --output option");
WriteLine("✅ PASSED\n");

// Test 8: Compare with library
WriteLine("Test 8: Should generate same avatar as direct library call");
string input = "library@test.com";
CommandOutput scriptResult = await RunMultiAvatarAsync(input);
string libraryResult = MultiavatarGenerator.Generate(input);
AssertEqual(scriptResult.Stdout.Trim(), libraryResult, "Script output should match library output");
WriteLine("✅ PASSED\n");

WriteLine("🎉 All tests passed!");
return 0;