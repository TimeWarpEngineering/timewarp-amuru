#!/usr/bin/dotnet run
// DisableBranchProtection.cs - Disable branch protection on the default branch
#pragma warning disable IDE0005 // Using directive is unnecessary
#pragma warning restore IDE0005

// Get script directory using CallerFilePath
static string GetScriptDirectory([CallerFilePath] string scriptPath = "")
{
  return Path.GetDirectoryName(scriptPath) ?? "";
}

// Push current directory, change to script directory for relative paths
string originalDirectory = Directory.GetCurrentDirectory();
string scriptDir = GetScriptDirectory();
Directory.SetCurrentDirectory(scriptDir);

Console.WriteLine("🔓 Disabling branch protection on master branch...");
Console.WriteLine($"Script directory: {scriptDir}");
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
finally
{
  // Pop - restore original working directory
  Directory.SetCurrentDirectory(originalDirectory);
}