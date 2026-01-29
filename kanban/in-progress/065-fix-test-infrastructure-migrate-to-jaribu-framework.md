# Fix test infrastructure - migrate to Jaribu framework

## Summary

Migrate all tests in TimeWarp.Amuru from the custom Test.Helpers pattern to the proper Jaribu testing framework.

## Problem

Tests are using a custom `TimeWarp.Amuru.Test.Helpers` pattern instead of the proper Jaribu framework. This creates unnecessary technical debt and results in an extra NuGet package being published.

**Files Following Wrong Pattern (need migration):**

### GitCommands (MIGRATED ✓)
- [x] `Tests/Integration/GitCommands/Git.BareAndWorktree.cs`
- [x] `Tests/Integration/GitCommands/Git.DefaultBranch.cs`

### Core (PENDING - 10 files)
- [ ] `Tests/Integration/Core/CommandExtensions.cs`
- [ ] `Tests/Integration/Core/CommandOptions.Cancellation.cs`
- [ ] `Tests/Integration/Core/CommandOptions.Configuration.cs`
- [ ] `Tests/Integration/Core/CommandResult.ErrorHandling.cs`
- [ ] `Tests/Integration/Core/CommandResult.Interactive.cs`
- [ ] `Tests/Integration/Core/CommandResult.OutputFormats.cs`
- [ ] `Tests/Integration/Core/CommandResult.Pipeline.cs`
- [ ] `Tests/Integration/Core/CommandResult.TtyPassthrough.cs`
- [ ] `Tests/Integration/Core/NewApiTests.cs`
- [ ] `Tests/Integration/Core/RunBuilder.cs`

### Other Directories (Need audit)
- [ ] Check `Tests/Integration/GwqCommand/`
- [ ] Check `Tests/Integration/GhqCommand/`
- [ ] Check `Tests/Integration/DotNetCommands/`

## Jaribu Framework Requirements

Migrate tests to use proper Jaribu patterns:
- `[TestTag]` attributes for test categorization
- `Should_` naming convention for test methods
- `Setup` and `CleanUp` methods for test lifecycle
- See `jaribu skill` for complete framework documentation

## Checklist

- [x] Review jaribu skill documentation
- [x] Add TimeWarp.Jaribu@1.0.0-beta.8 to Directory.Packages.props
- [x] Migrate `Tests/Integration/GitCommands/Git.BareAndWorktree.cs` to Jaribu pattern
- [x] Migrate `Tests/Integration/GitCommands/Git.DefaultBranch.cs` to Jaribu pattern
- [ ] Migrate Core test files (10 files - see list below)
- [ ] Migrate remaining test files using custom pattern
- [ ] Remove or deprecate Test.Helpers project
- [ ] Stop publishing Test.Helpers NuGet package
- [ ] Verify all tests pass after migration

## Notes

**Root Cause:** Existing tests established a custom pattern that was perpetuated instead of adopting the project's standard testing framework.

**Impact:**
- Unnecessary maintenance of duplicate infrastructure
- Extra NuGet package being published
- Tests not following project conventions
- Technical debt accumulating

**References:**
- See `jaribu skill` for proper framework usage
- Jaribu is the intended testing framework for TimeWarp.Amuru

## Results

[Document outcomes after completion]