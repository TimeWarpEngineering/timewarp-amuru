#!/usr/bin/dotnet --

#:project ../Source/TimeWarp.Amuru/TimeWarp.Amuru.csproj
#:package TimeWarp.Nuru
#:property TrimMode=partial
#:property NoWarn=IL2104;IL3053;IL2087

using TimeWarp.Amuru;
using TimeWarp.Nuru;
using static System.Console;

NuruAppBuilder builder = new();

builder.AddAutoHelp();

builder.AddDefaultRoute
(
  UpdateMaster,
  "Update the master branch worktree and show commits ahead"
);

NuruApp app = builder.Build();
return await app.RunAsync(args);

static async Task<int> UpdateMaster()
{
  WriteLine("Updating master...");

  GitBranchUpdateResult updateResult = await Git.UpdateMasterAsync();

  if (!updateResult.Success)
  {
    WriteLine($"Error: {updateResult.ErrorMessage}");
    return 1;
  }

  WriteLine(updateResult.BranchPath != null
    ? $"Successfully updated master at: {updateResult.BranchPath}"
    : "Successfully updated master");

  WriteLine();

  // Now check how many commits ahead we are
  GitCommitCountResult countResult = await Git.GetCommitsAheadOfMasterAsync();

  if (!countResult.Success)
  {
    WriteLine($"Warning: Could not determine commits ahead of master: {countResult.ErrorMessage}");
    return 0; // Still exit successfully since master was updated
  }

  if (countResult.Count == 0)
  {
    WriteLine("Current branch is up to date with master");
  }
  else if (countResult.Count == 1)
  {
    WriteLine("Current branch is 1 commit ahead of master");
  }
  else
  {
    WriteLine($"Current branch is {countResult.Count} commits ahead of master");
  }

  return 0;
}
