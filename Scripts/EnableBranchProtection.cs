#!/usr/bin/dotnet --
#:project ../source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:property LangVersion=preview
#:property EnablePreviewFeatures=true

// EnableBranchProtection.cs - Enable branch protection on the default branch

using TimeWarp.Amuru;

// Use ScriptContext to manage directory changes
using var context = ScriptContext.FromEntryPoint();

Console.WriteLine("🔒 Enabling branch protection on master branch...");
Console.WriteLine($"Script directory: {context.ScriptDirectory}");
Console.WriteLine($"Working from: {Directory.GetCurrentDirectory()}");

try
{
  // Create protection rules JSON
  var protectionRules = new
  {
    required_status_checks = (object?)null,
    enforce_admins = true,
    required_pull_request_reviews = new
    {
      required_approving_review_count = 1,
      dismiss_stale_reviews = true,
      require_code_owner_reviews = false
    },
    restrictions = (object?)null,
    allow_force_pushes = false,
    allow_deletions = false
  };

  string jsonContent = JsonSerializer.Serialize(protectionRules, new JsonSerializerOptions 
  { 
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
  });

  // Write to temp file
  string tempFile = Path.GetTempFileName();
  await File.WriteAllTextAsync(tempFile, jsonContent);

  Console.WriteLine("📋 Protection rules:");
  Console.WriteLine(jsonContent);

  // Apply protection using gh CLI
  var process = new Process
  {
    StartInfo = new ProcessStartInfo
    {
      FileName = "gh",
      Arguments = "api /repos/TimeWarpEngineering/timewarp-cli/branches/master/protection " +
                  "--method PUT " +
                  "--header \"Accept: application/vnd.github+json\" " +
                  $"--input \"{tempFile}\"",
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

  // Clean up temp file
  File.Delete(tempFile);

  if (process.ExitCode == 0)
  {
    Console.WriteLine("✅ Branch protection enabled successfully!");
    Console.WriteLine("\nProtection settings:");
    Console.WriteLine("- Require pull request reviews: ✅");
    Console.WriteLine("- Required approvals: 1");
    Console.WriteLine("- Dismiss stale reviews: ✅");
    Console.WriteLine("- Enforce for administrators: ✅");
    Console.WriteLine("- Prevent force pushes: ✅");
    Console.WriteLine("- Prevent branch deletion: ✅");
  }
  else
  {
    Console.WriteLine($"❌ Failed to enable branch protection. Exit code: {process.ExitCode}");
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