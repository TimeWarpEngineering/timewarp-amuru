# Clean up repository audit findings and enforce conventions

## Description

Address all failing checks from `ganda repo audit` and perform manual audits to clean up the repository. This includes removing unused packages, fixing file naming conventions, and resolving baseline configuration issues.

## Checklist

### Automatic Audit Findings (ganda repo audit)

- [x] Create `BannedSymbols.txt` in repository root
- [x] Add banned symbols configuration to `Directory.Build.props`
- [x] Add banned symbols configuration to `source/Directory.Build.props` (if needed)
- [ ] Fix dev-cli capabilities JSON parsing failure (upstream bug in TimeWarp.Nuru - trailing commas in JSON)
- [x] Add `#region Purpose` to source files missing it (7 files in tools/dev-cli)
- [ ] Run `ganda repo audit` to verify all checks pass (blocked by Nuru JSON bug)

### Manual Audit Findings

- [x] Remove `Blockcore.Nostr.Client` from `Directory.Packages.props` (unused, planned for TimeWarp.Kijamii)
- [x] Remove `TimeWarp.Multiavatar` from `Directory.Packages.props` (extracted to own repo per task 032)
- [x] Verify no other unused packages remain
- [x] Update outdated NuGet packages (TimeWarp.Nuru, analyzers, ModelContextProtocol.Core)

### File Naming Conventions

- [x] Rename `Source/` directory to `source/` (kebab-case)
- [x] Update all references to `Source/` in code files
- [ ] Audit remaining files for kebab-case naming compliance
- [ ] Rename any other files not following kebab-case convention
- [ ] Update any references to renamed files

### Verification

- [ ] Run `ganda repo audit` - all checks should pass
- [ ] Run `./Scripts/Build.cs` - build should succeed
- [ ] Run `./Tests/RunTests.cs` - all tests should pass
- [ ] Commit changes with message: "chore: clean up audit findings and enforce conventions"

### Analyzer Fixes (Post-Cleanup)

- [ ] Remove temporary CA1062 suppression from `Directory.Build.props`
- [ ] Remove temporary CA1031 suppression from `Directory.Build.props`
- [ ] Remove temporary RS0030 suppression from `Directory.Build.props`
- [ ] Fix CA1062: Validate parameters are non-null before using them
- [ ] Fix CA1031: Catch more specific exception types (not general `Exception`)
- [ ] Fix RS0030: Migrate from `System.Console` to `TimeWarp.Terminal.ITerminal`

## Notes

### Current Audit Status (2026-03-16)

```
Passed: 8 | Failed: 1

PASS: baseline-envrc
PASS: baseline-bin-dev
PASS: baseline-banned-symbols
PASS: baseline-banned-api-analyzers
PASS: baseline-source-directory-build-props
PASS: baseline-msbuild-repository-props
PASS: baseline-directory-packages-props
PASS: baseline-region-annotations
FAIL: baseline-dev-cli-capabilities (Nuru JSON trailing comma bug)
```

### Nuru JSON Bug

`TimeWarp.Nuru 3.0.0-beta.62` outputs invalid JSON with trailing commas in the `--capabilities` output. This is an upstream bug that cannot be fixed in this repository.

Example of invalid output:
```json
{
  "commands": [
    { "pattern": "build", ... },  // <-- trailing comma
  ]
}
```

**Options:**
1. File bug on TimeWarp.Nuru repository
2. Update `ganda repo audit` to use lenient JSON parsing
3. Skip this check temporarily

### Unused Packages Analysis

Per analysis in `.agent/workspace/2026-03-16T10-30-00_directory-packages-usage-analysis.md`:

| Package | Status | Action |
|---------|--------|--------|
| `Blockcore.Nostr.Client` | Unused | Remove - planned for TimeWarp.Kijamii (task 031/033) |
| `TimeWarp.Multiavatar` | Unused | Remove - extracted to own repo (task 032) |

### File Naming Convention

This repository uses **kebab-case** for file naming:
- ✅ `my-file-name.cs`
- ❌ `MyFileName.cs`
- ❌ `my_file_name.cs`

### Related Tasks

- Task 032: Extract TimeWarp.Multiavatar to Own Repository (done)
- Task 033: Extract Kijamii to Own Repo (to-do) - mentions removing Blockcore.Nostr.Client

### BannedSymbols.txt Template

```
# Banned Symbols
# Add symbols that should not be used in this codebase

# Example:
# T:System.Console;Use TimeWarp.Terminal.IConsole instead
```

### Files Missing #region Purpose

Run `ganda repo audit` to get the current list of 13 files missing the Purpose region.
