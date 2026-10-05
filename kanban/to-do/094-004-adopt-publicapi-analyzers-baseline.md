# Adopt PublicAPI analyzers baseline

## Description

Once the 1.0 surface ships, guard it: adopt `Microsoft.CodeAnalysis.PublicApiAnalyzers` with checked-in `PublicAPI.Shipped.txt`/`PublicAPI.Unshipped.txt` per packable project, so every surface change is deliberate and reviewable. Fits the repo's existing analyzer posture (BannedApiAnalyzers already in `Directory.Build.props`).

Timing per owner decision (2026-07-05): at/after the 1.0 release — not a release gate. 1.0.0, 1.1.0 and 1.1.1 have shipped; both packages are stable and released in lockstep (task 117), so Tools is no longer beta.

**Decision (2026-10-05):** baseline both packages from the current master surface. Master already carries the breaking changes from tasks 087, 088 and 099 that the next release (2.0.0 by semver) will ship, so the baseline is the pending 2.0 surface, not 1.1.1. Put that surface in `PublicAPI.Shipped.txt` for both `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools`; leave `PublicAPI.Unshipped.txt` empty. Document that each release moves Unshipped entries into Shipped. Do not hand-reconstruct the 1.1.1 surface; the removed members are already listed in the 087/088/099 Results.

This task must merge before tasks 100, 121 and 105 launch, because they add public members and must record them in Unshipped.

## Checklist

- [ ] Add `Microsoft.CodeAnalysis.PublicApiAnalyzers` to `Directory.Packages.props` (CPM) and reference it from `source/timewarp-amuru/timewarp-amuru.csproj` and `source/timewarp-amuru-tools/timewarp-amuru-tools.csproj` (PrivateAssets=all, analyzer only). Do not apply it to tests, samples, tools/dev-cli
- [ ] Generate `PublicAPI.Shipped.txt` for both packages from current master (use the analyzer's RS0016 code fix "Add all items to public API" or `dotnet format analyzers --diagnostics RS0016`), then move everything from Unshipped to Shipped and leave Unshipped with only the `#nullable enable` header
- [ ] Both files use `#nullable enable`; verify RS0017 (removed API) and RS0016 (undeclared API) produce errors by temporarily removing one line and building
- [ ] Keep `IsAotCompatible` / existing analyzers unchanged; `dev build` warnings-as-errors clean; full runner green
- [ ] Ensure RS0016/RS0017 run as errors in CI (already `TreatWarningsAsErrors`)
- [ ] Document the workflow in `Agents.md` (and `documentation/developer/` if a contributing doc exists): adding API = add to Unshipped; removing API = `*REMOVED*` line in Unshipped; release = move Unshipped into Shipped. Mention the IDE/code-fix path and the `dotnet format analyzers` path
- [ ] Note in Results the line counts of each Shipped file so the release PR can sanity-check the surface size

## Session

- Created: 2026-07-05 (decision record in parent 094 task.md)
- Kitchen refresh: 522eb63d (2026-10-05)
