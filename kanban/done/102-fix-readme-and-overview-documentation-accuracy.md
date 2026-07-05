# Fix readme and overview documentation accuracy

## Description

The readme ships INSIDE the nupkg, so its non-compiling samples land on the nuget.org package page. Spot-check against `source/timewarp-amuru/core/` found the flagship examples use APIs that don't exist.

## Checklist

### readme.md (MAJOR — ships in the package)
- [x] Multi-arg `Shell.Builder(...)` eliminated — readme fully rewritten 2026-07-05 with every sample verified against the live API
- [x] Error-handling section rewritten for the NEW contract (default None, `WithZeroExitCodeValidation()`, `NeverRanExitCode`)
- [x] `.WithCancellationToken` removed — tokens passed to execution methods
- [x] `.WithTimeout` removed from samples (CTS pattern shown; 044 still tracks real timeout support)
- [x] `result.Lines` → `GetLines()`/`OutputLines`
- [x] All relative links point at real lowercase paths; Zana/ganda entries corrected; stale workflow badge (release-build.yml) fixed to workflow.yml; Testing section now leads with CommandMock (strict default) with CliConfiguration as the path-override alternative; When()/extension-methods section removed (deleted API)

### overview.md (MAJOR)
- [x] overview.md given a prominent STATUS banner marking it as the historical design/vision document, pointing to the readme and collocated design regions for the shipped API

### Package split (from 094-003)
- [x] Installation shows both packages; Tools examples (DotNet/Git/Fzf) in their own section noting the package

### documentation/
- [x] `documentation/overview.md` links fixed to lowercase paths and real sample filenames
- [x] Cross-repo links now point at the ganda GitHub repo; dead Architecture/Source paths fixed
- [x] json-rpc design doc has a STATUS: DISABLED banner referencing tasks 083/084
- [ ] Consider a doc-sample compile check (post-1.0 nicety — samples/ already compile against live source via `#:project`)

## Notes

Found by multi-agent release review (2026-07-04). Verified correct already: `CaptureAsync()`→`CommandOutput` members, `CliConfiguration.*`, `DotNet.*`, `Fzf.*`, `When/Unless/Tap` docs, the ganda rename (task 030) fully applied, samples/ current.
