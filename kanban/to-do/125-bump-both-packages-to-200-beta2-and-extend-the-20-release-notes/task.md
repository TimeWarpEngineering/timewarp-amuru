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

- [ ] `<Version>2.0.0-beta.2</Version>` in `source/Directory.Build.props`
- [ ] `dev check-version` green; output pasted into Results
- [ ] PublicAPI promoted for core (98 entries) and Tools (2 entries); Unshipped files are header-only; build clean; new Shipped counts recorded
- [ ] `documentation/release-notes/2.0.0.md` has a complete `# 2.0.0-beta.2` section at the top, sourced from 021/044/093
- [ ] Doc spot-check for stale beta.1 literals
- [ ] `./bin/dev workflow --mode merge` green: build, verify-samples, test (expect 720 passed, 1 skipped), nupkgs for both packages at 2.0.0-beta.2 under `artifacts/packages/`
- [ ] `ganda repo audit` clean
- [ ] Results: gate log, Shipped counts, release-notes section path, consumed-pins follow-up note

## Notes

- Pattern: task 122 (beta.1 bump) Results and `documentation/developer/guides/releasing.md`.
- After merge the cockpit runs: `dev release --dry-run`, `dev release`, then edits the GitHub Release body to the beta.2 section and marks it prerelease (as done for beta.1).
- Follow-ups after publish: amuru consumed-pins bump to beta.2 (like 123); ganda task to pin beta.2 and convert `global-usings-analyzer-check.cs`, `runfile-operations.cs` `EnsureRunfileDirectivesAsync`, and `EnsureRunfileShebangsAsync` to `ReplaceInFiles`.

## Session

- Created: 522eb63d (2026-10-06)
