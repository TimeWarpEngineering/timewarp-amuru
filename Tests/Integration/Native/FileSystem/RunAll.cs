#!/usr/bin/dotnet --
#:project ../../../../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj

using TimeWarp.Amuru;

// Run all FileSystem tests
Console.WriteLine("🧪 Running all FileSystem tests...\n");

var tests = new[]
{
  "./GetContentTests.cs",
  "./GetChildItemTests.cs",
  "./RemoveItemTests.cs",
  "./GetLocationTests.cs",
  "./AliasTests.cs"
};

int totalPassed = 0;
int totalFailed = 0;

foreach (string test in tests)
{
  Console.WriteLine($"Running {Path.GetFileNameWithoutExtension(test)}...");

  CommandOutput result = await Shell.Builder("dotnet", test)
    .WithWorkingDirectory(AppContext.BaseDirectory)
    .CaptureAsync();

  if (result.Success)
  {
    // Parse the output to extract pass/fail counts
    string output = result.Stdout;
    if (output.Contains("Results:"))
    {
      // Extract test counts from output like "Results: 5/5 tests passed"
      var match = System.Text.RegularExpressions.Regex.Match(
        output,
        @"Results: (\d+)/(\d+) tests passed"
      );

      if (match.Success)
      {
        int passed = int.Parse(match.Groups[1].Value);
        int total = int.Parse(match.Groups[2].Value);
        int failed = total - passed;

        totalPassed += passed;
        totalFailed += failed;
      }
    }

    Console.WriteLine(result.Stdout);
  }
  else
  {
    Console.WriteLine($"❌ Failed to run {test}");
    Console.WriteLine(result.Stderr);
    totalFailed++;
  }

  Console.WriteLine();
}

Console.WriteLine($"📊 Overall FileSystem Test Results: {totalPassed} passed, {totalFailed} failed");

if (totalFailed > 0)
{
  Environment.Exit(1);
}