# Adopt PublicAPI analyzers baseline

## Description

Once the 1.0 surface ships, guard it: adopt `Microsoft.CodeAnalysis.PublicApiAnalyzers` with checked-in `PublicAPI.Shipped.txt`/`PublicAPI.Unshipped.txt` per packable project, so every surface change is deliberate and reviewable. Fits the repo's existing analyzer posture (BannedApiAnalyzers already in `Directory.Build.props`).

Timing per owner decision (2026-07-05): at/after the 1.0 release — not a release gate. 1.0.0, 1.1.0 and 1.1.1 have shipped; both packages are stable and released in lockstep (task 117), so Tools is no longer beta.

**Decision (2026-10-05):** baseline both packages from the current master surface. Master already carries the breaking changes from tasks 087, 088 and 099 that the next release (2.0.0 by semver) will ship, so the baseline is the pending 2.0 surface, not 1.1.1. Put that surface in `PublicAPI.Shipped.txt` for both `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools`; leave `PublicAPI.Unshipped.txt` empty. Document that each release moves Unshipped entries into Shipped. Do not hand-reconstruct the 1.1.1 surface; the removed members are already listed in the 087/088/099 Results.

This task must merge before tasks 100, 121 and 105 launch, because they add public members and must record them in Unshipped.

## Checklist

- [x] Add `Microsoft.CodeAnalysis.PublicApiAnalyzers` to `Directory.Packages.props` (CPM) and reference it from `source/timewarp-amuru/timewarp-amuru.csproj` and `source/timewarp-amuru-tools/timewarp-amuru-tools.csproj` (PrivateAssets=all, analyzer only). Do not apply it to tests, samples, tools/dev-cli
- [x] Generate `PublicAPI.Shipped.txt` for both packages from current master (use the analyzer's RS0016 code fix "Add all items to public API" or `dotnet format analyzers --diagnostics RS0016`), then move everything from Unshipped to Shipped and leave Unshipped with only the `#nullable enable` header
- [x] Both files use `#nullable enable`; verify RS0017 (removed API) and RS0016 (undeclared API) produce errors by temporarily removing one line and building
- [x] Keep `IsAotCompatible` / existing analyzers unchanged; `dev build` warnings-as-errors clean; full runner green
- [x] Ensure RS0016/RS0017 run as errors in CI (already `TreatWarningsAsErrors`)
- [x] Document the workflow in `Agents.md` (and `documentation/developer/` if a contributing doc exists): adding API = add to Unshipped; removing API = `*REMOVED*` line in Unshipped; release = move Unshipped into Shipped. Mention the IDE/code-fix path and the `dotnet format analyzers` path
- [x] Note in Results the line counts of each Shipped file so the release PR can sanity-check the surface size

## Session

- Created: 2026-07-05 (decision record in parent 094 task.md)
- Kitchen refresh: 522eb63d (2026-10-05)
- Implementer: grok session 01a10997-db2d-7462-88fa-af6de304fe5a (2026-10-05)
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-05T01:20:45Z

## Results

Adopted `Microsoft.CodeAnalysis.PublicApiAnalyzers` 5.6.0 on the two packable projects and baselined the current master surface into Shipped. Unshipped is only `#nullable enable`.

Line counts (`wc -l`, `#nullable enable` header included):

| File | Lines |
| --- | ---: |
| `source/timewarp-amuru/public-api/PublicAPI.Shipped.txt` | 250 |
| `source/timewarp-amuru/public-api/PublicAPI.Unshipped.txt` | 1 |
| `source/timewarp-amuru-tools/public-api/PublicAPI.Shipped.txt` | 1491 |
| `source/timewarp-amuru-tools/public-api/PublicAPI.Unshipped.txt` | 1 |

API entries are 249 (core) and 1490 (tools).

The files live under `public-api/` because Roslyn requires the basenames `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`, which are not kebab-case. Each csproj lists them as `AdditionalFiles`. `.editorconfig` prunes the `public-api` directory from `kebab-path-names`. The analyzer reads them by basename; a clean Release build is the proof.

`IsAotCompatible` and the existing analyzer references are unchanged. RS0026 is `none` in `source/.editorconfig` because the baseline includes optional-parameter overload sets (`GetChildItem` and several `Git` helpers). Splitting those overloads would be a breaking redesign.

RS0016 and RS0017 are warnings. Root `TreatWarningsAsErrors` promotes them to errors, so CI fails the build. Checked on `TimeWarp.Amuru` and then restored:

- Dropping one Shipped line failed the build with RS0016 (`CliConfiguration.AllCommandPaths`).
- A symbol written only in Unshipped (`TimeWarp.Amuru.PublicApiBaselineProbe`) failed the build with RS0017.
- A `*REMOVED*` line in Shipped failed with RS0024 ("The shipped API file can't have removed members").
- A `*REMOVED*` line whose symbol is still in source failed with RS0050.

The packed nupkgs do not depend on the analyzer and do not contain the PublicAPI files.

There is no contributing guide under `documentation/developer/`. The add/remove/release workflow is in `AGENTS.md`. The release promotion step is also in `documentation/developer/guides/releasing.md`, because Shipped cannot contain `*REMOVED*` lines: a release deletes the matching Shipped entry and drops the `*REMOVED*` line instead of copying it.

### How to validate

**Smoke**

```bash
./bin/dev build
wc -l source/timewarp-amuru/public-api/PublicAPI.Shipped.txt source/timewarp-amuru-tools/public-api/PublicAPI.Shipped.txt
# temporarily drop one Shipped line, then restore it
```

**Expect**

- `./bin/dev build` succeeds with 0 warnings and 0 errors.
- Line counts are 250 (`TimeWarp.Amuru`) and 1491 (`TimeWarp.Amuru.Tools`).
- Each `PublicAPI.Unshipped.txt` is only `#nullable enable`.
- Removing one Shipped line makes `dotnet build source/timewarp-amuru/timewarp-amuru.csproj` fail with error RS0016. Restore the line before committing.

**Automated gate**

```bash
./bin/dev test
ganda repo audit
```

Expect the runner to finish with 0 failed (this session: 552 total, 551 passed, 1 skipped) and the audit to report Failed: 0.

**Not in scope:** reconstructing the 1.1.1 surface. Removed members from tasks 087, 088, and 099 stay recorded on those tasks. This baseline is the pending 2.0 surface.

### Review

- Rounds: 1; effort 3 (by-diff); roster: general (review oracle, claude-opus-5-5, 2026-10-05)
- Final counts: bug 0, suggestion 0, nit 0 (0 open, 0 fixed, 0 wontfix)
- Disposition: **clean**
- Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`
