# Bump both packages to 2.0.0-beta.2 and extend the 2.0 release notes

## Description

Master is `2.0.0-beta.1` (published 2026-10-05) plus six merged PRs. Two add public API: **021** native text commands (`SelectString`, `ReplaceInFiles`, Bash `Grep`/`Sed`) and **044** command timeouts (`CommandOptions.Timeout`, `WithTimeout`, `CommandOutput.TimedOut`, exit 124). Measured on master `ab865c6`: 98 unshipped entries in core, 2 in Tools, no `*REMOVED*` lines. The other four PRs (093 Tools XML docs, 082 kebab renames, 123 consumed pins, 124 Nuru beta.79 dev-cli) are non-API.

This task follows the task 122 pattern exactly: bump the lockstep version, promote Unshipped → Shipped, extend the release notes, prove the gates. It does **not** cut the release; the cockpit runs `dev release` from master afterward. Ganda's first consumer task (convert the three fixers named in 021 Results) waits for beta.2 on NuGet.

## Requirements

- `source/Directory.Build.props`: `<Version>2.0.0-beta.1</Version>` → `2.0.0-beta.2`. Single lockstep source; no per-project `<Version>`.
- `dev check-version`: `Version in source: 2.0.0-beta.2`, latest NuGet `2.0.0-beta.1`, both packages, safe to release, exit 0.
- PublicAPI: move every entry from `PublicAPI.Unshipped.txt` into `PublicAPI.Shipped.txt` for both packages (keep Shipped sorted the way the analyzer code fix emits it); Unshipped ends as `#nullable enable` only. `./bin/dev build` 0 warnings / 0 errors. Record the new Shipped line counts (beta.1: core 253, Tools 1620).
- Release notes: extend `documentation/release-notes/2.0.0.md` with a `# 2.0.0-beta.2` section **above** the beta.1 section (newest first), same structure: New (per package), Fixed, Behavior changes (if any), Upgrade guide (none expected: additive only). Source from the merged kitchens under `kanban/done/`:
  - 021 Results → New: `SelectString` / `ReplaceInFiles` (Commands, Direct, Bash aliases), options, exit-code contracts, write-safety guarantees (BOM/newline/trailing-newline preservation, atomic write, skip-unchanged, binary skip)
  - 044 Results → New: timeout API, result contract table (TimedOut + 124 under default validation, `TimeoutException` under strict, caller cancel unchanged), TTY and pipe behavior, Windows graceful note
  - 093 Results → Fixed/Docs: Tools package now ships `timewarp-amuru-tools.xml` (IntelliSense for every Tools member)
  - 124 Results → internal (dev-cli on Nuru beta.79); mention only under a "Repository" or "Internal" line if the convention has one, otherwise omit
  - 082, 123: internal; omit
- Keep the beta.2 section free of kitchen meta text (no "these notes are a draft", no task-id-only lines); the cockpit copies it into the GitHub Release body.
- Spot-check `readme.md` / `AGENTS.md` / `skills/amuru/SKILL.md` for "beta.1" literals that should say beta.2 or be version-neutral; do not touch the "install with `--prerelease` until 2.0.0 GA" guidance.
- Do not touch `Directory.Packages.props` consumed pins (`TimeWarp.Amuru` / `.Tools` at `2.0.0-beta.1` for the githooks runfiles): beta.2 is not on NuGet until the release is cut. Note the post-publish follow-up in Results, as 117/122/123 did.
- Do not tag, do not run `dev release`, do not push tags.

## Checklist

- [x] `<Version>2.0.0-beta.2</Version>` in `source/Directory.Build.props`
- [x] `dev check-version` green; output pasted into Results
- [x] PublicAPI promoted for core (98 entries) and Tools (2 entries); Unshipped files are header-only; build clean; new Shipped counts recorded
- [x] `documentation/release-notes/2.0.0.md` has a complete `# 2.0.0-beta.2` section at the top, sourced from 021/044/093
- [x] Doc spot-check for stale beta.1 literals
- [x] `./bin/dev workflow --mode merge` green: build, verify-samples, test (expect 720 passed, 1 skipped), nupkgs for both packages at 2.0.0-beta.2 under `artifacts/packages/`
- [x] `ganda repo audit` clean
- [x] Results: gate log, Shipped counts, release-notes section path, consumed-pins follow-up note

## Notes

- Pattern: task 122 (beta.1 bump) Results and `documentation/developer/guides/releasing.md`.
- After merge the cockpit runs: `dev release --dry-run`, `dev release`, then edits the GitHub Release body to the beta.2 section and marks it prerelease (as done for beta.1).
- Follow-ups after publish: amuru consumed-pins bump to beta.2 (like 123); ganda task to pin beta.2 and convert `global-usings-analyzer-check.cs`, `runfile-operations.cs` `EnsureRunfileDirectivesAsync`, and `EnsureRunfileShebangsAsync` to `ReplaceInFiles`.

## Results

Both packages are `2.0.0-beta.2` from `source/Directory.Build.props`. That is the only `<Version>` under `source/`. No per-project `<Version>`. PublicAPI Unshipped for core and Tools is only `#nullable enable`. No `*REMOVED*` lines. Shipped line counts (`wc -l`, header included): core 351 (was 253; 98 additions), Tools 1622 (was 1620; 2 additions). Lines are ordered with the PublicApiAnalyzers code-fix comparer (ordinal ignore case, then ordinal). That also places the beta.1 additions that had been prepended (`CommandResult.Pipe`, `ShellBuilder.Pipe`, and the Tools `WithNoValidation` / `WithZeroExitCodeValidation` lines) where the code fix emits them. `./bin/dev build`, inside the merge workflow: 0 warnings, 0 errors.

Release notes: `documentation/release-notes/2.0.0.md`, section `# 2.0.0-beta.2` above `# 2.0.0-beta.1`. New covers `SelectString` / `ReplaceInFiles` (Commands, Direct, Bash `Grep`/`Sed`), options, exit codes, and write safety, plus the timeout API, result-contract table, pipe window, and the Windows graceful note. Fixed records that Tools ships `timewarp-amuru-tools.xml`. Upgrade guide is none. Tasks 082, 123, and 124 are omitted: the notes have no Repository or Internal heading, and 124 is the dev-cli Nuru pin.

Spot-check: `readme.md` still says to add `--prerelease` until 2.0.0 is final, and names `2.0.0-beta.2` as the package that flag installs. `AGENTS.md` says the Shipped baseline is the `2.0.0-beta.2` surface. `skills/amuru/SKILL.md` points at `documentation/release-notes/2.0.0.md` for the 2.0 notes, including breaking changes, without pinning beta.1.

Not changed on purpose: `Directory.Packages.props` still pins the consumed packages at `TimeWarp.Amuru` `2.0.0-beta.1` and `TimeWarp.Amuru.Tools` `2.0.0-beta.1` (`.githooks` runfiles). `2.0.0-beta.2` is not on NuGet until the cockpit cuts the release. Post-publish follow-up: bump those consumed pins after `v2.0.0-beta.2` is on NuGet, the same follow-up tasks 117, 122, and 123 recorded. A ganda task can then pin beta.2 and convert `global-usings-analyzer-check.cs`, `runfile-operations.cs` `EnsureRunfileDirectivesAsync`, and `EnsureRunfileShebangsAsync` to `ReplaceInFiles`.

No tag. `dev release` was not run.

### Gate log

`./bin/dev check-version` exit 0:

```
Version in source: 2.0.0-beta.2
Latest NuGet version: 2.0.0-beta.1
Source is 1 prerelease increment ahead of v2.0.0-beta.1
Packages checked: TimeWarp.Amuru, TimeWarp.Amuru.Tools

✓ Version in source is new — safe to release.
```

`./bin/dev workflow --mode merge` exit 0. Pipeline `clean -> build -> verify-samples -> test` succeeded. Build 0 warnings, 0 errors. Both samples compiled. Tests: Passed 723, Failed 0, Skipped 1, Total 724. The kitchen's "720 passed" was the count when task 044 recorded its run; this tree still has a single skip (`GetCommitsAheadOfDefaultBranch`) and zero failures. `artifacts/packages/` contains `TimeWarp.Amuru.2.0.0-beta.2.nupkg` and `TimeWarp.Amuru.Tools.2.0.0-beta.2.nupkg`.

`ganda repo audit` exit 0. Passed 31, Failed 0, Skipped 0.

### How to validate

Smoke:

```bash
./bin/dev check-version
./bin/dev workflow --mode merge
test "$(wc -l < source/timewarp-amuru/public-api/PublicAPI.Unshipped.txt)" = 1
test "$(wc -l < source/timewarp-amuru-tools/public-api/PublicAPI.Unshipped.txt)" = 1
test "$(wc -l < source/timewarp-amuru/public-api/PublicAPI.Shipped.txt)" = 351
test "$(wc -l < source/timewarp-amuru-tools/public-api/PublicAPI.Shipped.txt)" = 1622
ls artifacts/packages/TimeWarp.Amuru.2.0.0-beta.2.nupkg artifacts/packages/TimeWarp.Amuru.Tools.2.0.0-beta.2.nupkg
```

Expect:

- `<Version>2.0.0-beta.2</Version>` is the only `<Version>` under `source/`.
- check-version prints `Version in source: 2.0.0-beta.2`, `Latest NuGet version: 2.0.0-beta.1`, both packages, and `safe to release`, exit 0.
- workflow exits 0. Build reports 0 warnings and 0 errors. Tests pass with one skip and zero failures.
- Both Unshipped files are the single line `#nullable enable`. Shipped counts are 351 (core) and 1622 (Tools).
- Both `2.0.0-beta.2` nupkgs exist.
- `Directory.Packages.props` still pins `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` at `2.0.0-beta.1`.

### Review disposition

- Rounds: 1; roster: general; effort 2 (Budget.ByDiff, 640 lines)
- Final counts: bug 0, suggestion 0, nit 0 (0 open, 0 fixed, 0 wontfix)
- Disposition: **clean**
- Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`

## Session

- Created: 522eb63d (2026-10-06)
- Implementation: grok (2026-10-06)
- Review: claude review oracle, effort 2, general (2026-10-06) — clean
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 120 — 2026-10-06T16:30:37Z
