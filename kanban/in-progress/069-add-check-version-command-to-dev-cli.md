# Add check-version command to dev-cli

## Description

Add a `check-version` command to the dev-cli tool that checks if the current package version already exists on NuGet.org. This command is useful for CI/CD validation and local development checks.

## Checklist

- [ ] Add `case "check-version":` entry in the switch statement in `tools/dev-cli/dev.cs`
- [ ] Implement `CheckVersionCommand(string[] args)` method that:
  - Extracts version from `Source/Directory.Build.props`
  - Runs `dotnet package search TimeWarp.Amuru --exact-match --prerelease --source https://api.nuget.org/v3/index.json`
  - Reports whether version exists on NuGet.org
  - Returns appropriate exit code (0 if not published, 1 if already published)
- [ ] Update `ShowHelp()` to include the new command
- [ ] Update `OutputCapabilities()` JSON to include the command
- [ ] Add `--capabilities` flag support for the command

## Notes

**Target file:** `tools/dev-cli/commands/check-version-command.cs`

**Reference implementations:**
- See timewarp-nuru and timewarp-terminal repositories for similar check-version implementations
- Current CI/CD version check logic is in `.github/workflows/ci-cd.yml` (lines 114-125)

**Version source:** `/home/steventcramer/worktrees/github.com/TimeWarpEngineering/timewarp-amuru/cramer-2026-01-29-dev/Source/Directory.Build.props` contains `<Version>1.0.0-beta.18</Version>`

**Current dev-cli pattern:** Uses simple switch-based dispatch in `tools/dev-cli/dev.cs` (264 lines). Commands are async methods following `CommandNameCommand(string[] args)` pattern.

**Exit codes:**
- 0 = Version not yet published (safe to publish)
- 1 = Version already exists on NuGet.org (would fail CI/CD)

## Implementation Plan

1. Create `tools/dev-cli/commands/check-version-command.cs` with the `CheckVersionCommand` method
2. Modify `tools/dev-cli/dev.cs` to:
   - Add `case "check-version":` in the switch statement
   - Reference the command from the commands folder
   - Add to `ShowHelp()` output
   - Add to `OutputCapabilities()` JSON

**Command behavior:**
- Read version from `Source/Directory.Build.props` (XML parsing to extract <Version> value)
- Execute: `dotnet package search TimeWarp.Amuru --exact-match --prerelease --source https://api.nuget.org/v3/index.json`
- Parse output to check if version exists
- Print status message with appropriate emoji
- Return exit code: 0 = version not published (safe), 1 = version already exists (would fail CI/CD)

**Exit codes:**
- 0: Version not yet on NuGet.org ✅
- 1: Version already published ❌
- 2: Error reading version or running search
