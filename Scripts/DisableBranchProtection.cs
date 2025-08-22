#!/usr/bin/dotnet --
#:project ../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:property LangVersion=preview
#:property EnablePreviewFeatures=true

// DisableBranchProtection.cs - Disable branch protection on the default branch

using TimeWarp.Amuru;

// Use ScriptContext to manage directory changes
using var context = ScriptContext.FromEntryPoint();

Console.WriteLine("🔓 Disabling branch protection on master branch...");
Console.WriteLine($"Script directory: {context.ScriptDirectory}");
Console.WriteLine($"Working from: {Directory.GetCurrentDirectory()}");

try
{
  // Delete protection using gh CLI
  var process = new Process
  {
    StartInfo = new ProcessStartInfo
    {
      FileName = "gh",
      Arguments = "api /repos/TimeWarpEngineering/timewarp-cli/branches/master/protection " +
                  "--method DELETE " +
                  "--header \"Accept: application/vnd.github+json\"",
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

  if (process.ExitCode == 0 || (process.ExitCode == 1 && error.Contains("404", StringComparison.Ordinal)))
  {
    Console.WriteLine("✅ Branch protection disabled successfully!");
    Console.WriteLine("\n⚠️  Warning: The master branch is now unprotected!");
    Console.WriteLine("Anyone with write access can now:");
    Console.WriteLine("- Push directly to master");
    Console.WriteLine("- Force push to master");
    Console.WriteLine("- Delete the branch");
    Console.WriteLine("\nRemember to re-enable protection when you're done!");
  }
  else
  {
    Console.WriteLine($"❌ Failed to disable branch protection. Exit code: {process.ExitCode}");
    if (!string.IsNullOrEmpty(error))
    {
      Console.WriteLine($"Error: {error}");
    }
    
    Environment.Exit(1);
  }
}
catch (Exception ex)
{
  Console.WriteLine($"❌ An error occurred: {ex.Message}");
  Environment.Exit(1);
}

// ScriptContext automatically restores directory on disposal