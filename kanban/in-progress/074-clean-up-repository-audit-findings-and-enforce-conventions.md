# Clean up repository audit findings and enforce conventions

## Description

Address all failing checks from `ganda repo audit` and perform manual audits to clean up the repository. This includes removing unused packages, fixing file naming conventions, and resolving baseline configuration issues.

## Checklist

### Automatic Audit Findings (ganda repo audit)

- [ ] Create `BannedSymbols.txt` in repository root
- [ ] Add banned symbols configuration to `Directory.Build.props`
- [ ] Add banned symbols configuration to `source/Directory.Build.props` (if needed)
- [ ] Fix dev-cli capabilities JSON parsing failure
- [ ] Add `#region Purpose` to source files missing it (13 files identified)
- [ ] Run `ganda repo audit` to verify all checks pass

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
- [ ] Fix CA1062: Validate parameters are non-null before using them
- [ ] Fix CA1031: Catch more specific exception types (not general `Exception`)

## Notes

### Current Audit Status (2026-03-16)

```
Passed: 4 | Failed: 5

FAIL: BannedSymbols.txt is missing
FAIL: Directory.Build.props is missing banned symbols config
FAIL: source/Directory.Build.props is missing banned symbols config
FAIL: Files missing #region Purpose: 13
FAIL: Capabilities JSON parsing failed
```

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
