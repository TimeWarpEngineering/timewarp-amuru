# Cleanup Amuru After Migration

## Description

Remove TimeWarp.Ganda from the Amuru repository after successful migration to the private Ganda repository. Update CI/CD and documentation to reflect the change.

## Parent

Migration Analysis: `.agent/workspace/2025-12-06T18-30-00_ganda-migration-analysis.md`

## Requirements

- Only proceed after Ganda repo is fully functional
- Amuru CI/CD must continue to work without Ganda
- Public API of Amuru must remain unchanged

## Checklist

### Remove Ganda Project
- [ ] Delete `Source/TimeWarp.Ganda/` directory
- [ ] Remove Ganda from solution file (if applicable)

### Update CI/CD
- [ ] Remove Ganda build steps from `.github/workflows/ci-cd.yml`
- [ ] Remove Ganda package publishing
- [ ] Verify Amuru-only build succeeds

### Update Documentation
- [ ] Update `readme.md` to note Ganda moved to separate repo
- [ ] Update any cross-references to Ganda
- [ ] Add note about TimeWarp.Zana for private utilities

### Validation
- [ ] Full CI/CD pipeline passes
- [ ] TimeWarp.Amuru package builds correctly
- [ ] No broken references remain

## Notes

After cleanup, Amuru should contain only:
- Core shell execution (`Shell.Builder()`)
- Command builders (`DotNet`, `Fzf`, etc.)
- Public native utilities (`Native/Utilities/`)
- Public standalone executables (`exe/`)

Ganda is now a private consumer of Amuru, not part of it.
