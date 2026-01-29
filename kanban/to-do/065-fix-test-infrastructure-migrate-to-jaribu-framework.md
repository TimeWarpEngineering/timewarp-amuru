# Fix test infrastructure - migrate to Jaribu framework

## Summary

Migrate all tests in TimeWarp.Amuru from the custom Test.Helpers pattern to the proper Jaribu testing framework.

## Problem

Tests are using a custom `TimeWarp.Amuru.Test.Helpers` pattern instead of the proper Jaribu framework. This creates unnecessary technical debt and results in an extra NuGet package being published.

**Files Following Wrong Pattern (need migration):**

- `Tests/Integration/GitCommands/Git.BareAndWorktree.cs` (newly created - followed wrong pattern)
- `Tests/Integration/GitCommands/Git.DefaultBranch.cs` (existing - uses custom TestRunner)
- All other test files using Test.Helpers pattern

## Jaribu Framework Requirements

Migrate tests to use proper Jaribu patterns:
- `[TestTag]` attributes for test categorization
- `Should_` naming convention for test methods
- `Setup` and `CleanUp` methods for test lifecycle
- See `jaribu skill` for complete framework documentation

## Checklist

- [ ] Review jaribu skill documentation
- [ ] Migrate `Tests/Integration/GitCommands/Git.BareAndWorktree.cs` to Jaribu pattern
- [ ] Migrate `Tests/Integration/GitCommands/Git.DefaultBranch.cs` to Jaribu pattern
- [ ] Audit all test files for Test.Helpers pattern usage
- [ ] Migrate any remaining test files using custom pattern
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