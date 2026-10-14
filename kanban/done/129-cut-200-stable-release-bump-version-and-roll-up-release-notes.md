# Cut 2.0.0 stable release: bump version and roll up release notes

## Description

Master is `2.0.0-beta.2` (published 2026-10-06) plus task 001 (concurrency and large-output tests, plus `CommandResult` doc comments) and kanban/audit commits. No public API changed since beta.2: both `PublicAPI.Unshipped.txt` files are header-only. All product runtime dependencies are stable (CliWrap 3.10.5, TimeWarp.Terminal 1.0.2, NuGet.Versioning). Open tasks 083 (additive JSON-RPC, depends on 119), 119 (.NET 11, GA 2026-11-10), and 128 (test-only CsCheck pilot) are not release blockers; 2.0.0 ships on net10.0 and the .NET 11 move is a later 2.x multi-target or 3.0 decision.

This task follows the task 122 / 125 pattern: bump the lockstep version, roll the release notes up to a stable `# 2.0.0` section, remove prerelease install guidance, prove the gates. It does **not** cut the release; the cockpit runs `dev release` from master afterward.

## Requirements

- `source/Directory.Build.props`: `<Version>2.0.0-beta.2</Version>` → `2.0.0`. Single lockstep source; no per-project `<Version>`.
- `dev check-version`: `Version in source: 2.0.0`, latest NuGet `2.0.0-beta.2`, both packages, safe to release, exit 0.
- PublicAPI: Unshipped is already header-only for both packages; confirm nothing to promote and record Shipped line counts (beta.2: core 351, Tools 1622).
- Release notes: `documentation/release-notes/2.0.0.md` gets a `# 2.0.0` section **above** the beta sections (newest first). It is the GitHub Release body for the stable release, so it must stand alone: a one-paragraph summary of 2.0 (both packages ship together, first stable 2.x, installs without `--prerelease`), then the **consolidated** Breaking changes, Behavior changes, New (per package), Fixed, and Upgrade guide merged from the beta.1 and beta.2 sections, plus the task 001 contract (`CommandResult` execution methods may be called concurrently; `LastOutput` is last-writer-wins and not meaningful under concurrent use; capture buffers everything, stream keeps one line). Keep the beta.1 and beta.2 sections below unchanged as history. No kitchen meta text, no task-id-only lines.
- Remove "install with `--prerelease` until 2.0.0 is final" guidance from `readme.md` and any other user-facing doc; name `2.0.0` where a version literal is needed. Spot-check `AGENTS.md` and `skills/amuru/SKILL.md` for `beta.2` literals.
- Do not touch `Directory.Packages.props` consumed pins (`TimeWarp.Amuru` / `.Tools` at `2.0.0-beta.2` for the githooks runfiles): 2.0.0 is not on NuGet until the release is cut. Note the post-publish follow-up in Results.
- Do not tag, do not run `dev release`, do not push tags.

## Checklist

- [x] `<Version>2.0.0</Version>` in `source/Directory.Build.props`
- [x] `dev check-version` green; output pasted into Results
- [x] PublicAPI Unshipped confirmed header-only for both packages; Shipped counts recorded
- [x] `documentation/release-notes/2.0.0.md` has a complete stand-alone `# 2.0.0` section at the top
- [x] Prerelease install guidance removed from `readme.md`; doc spot-check for stale beta literals
- [x] `./bin/dev workflow --mode merge` green: build, verify-samples, test, nupkgs for both packages at 2.0.0 under `artifacts/packages/`
- [x] `ganda repo audit` clean
- [x] Results: gate log, Shipped counts, release-notes section path, consumed-pins follow-up note
- [x] PR open and CI green

## Notes

- Pattern: task 125 Results and `documentation/developer/guides/releasing.md`.
- After merge the cockpit runs: `dev release --dry-run`, `dev release`, then sets the GitHub Release body to the `# 2.0.0` section, not marked prerelease.
- Follow-ups after publish: consumed-pins bump to 2.0.0 (like 126); decide .NET 11 shape for task 119 (multi-target net10+net11 in 2.x, or net11-only as 3.0).

## Session

- Created: 3213244 (2026-10-14)
- Implementation: claude (2026-10-14)
- Audit: `ganda repo audit --fix` bumped TimeWarp.Nuru and TimeWarp.Nuru.DevCli in Directory.Packages.props from 3.0.0-beta.79 to 3.0.0-beta.80 (dev-cli only); rebuilt bin/dev with `./bin/dev self-install`; audit then clean.

## Results

Both packages are `2.0.0` from `source/Directory.Build.props`, the only `<Version>` under `source/`. PublicAPI Unshipped for core and Tools is only `#nullable enable`, so there was nothing to promote. Shipped line counts (`wc -l`): core 351, Tools 1622 (unchanged from beta.2). Build: 0 warnings, 0 errors.

Release notes: `documentation/release-notes/2.0.0.md` has a stand-alone `# 2.0.0` section above `# 2.0.0-beta.2` and `# 2.0.0-beta.1`, which are unchanged. It consolidates Breaking changes, Behavior changes, New (per package, plus the `CommandResult` concurrency and output contract), Fixed, and the Upgrade guide.

Docs touched: `readme.md` (prerelease install guidance removed), `AGENTS.md` (Shipped baseline is the `2.0.0` surface), `skills/amuru/SKILL.md` (`--prerelease` dropped from the version search). `documentation/user/installation.md` mentions `--prerelease` only for the unrelated Ganda tool and was left alone.

Audit fix: `Directory.Packages.props` TimeWarp.Nuru and TimeWarp.Nuru.DevCli bumped to 3.0.0-beta.80 (dev-cli only).

Not changed on purpose: consumed pins for `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` in `Directory.Packages.props` stay at `2.0.0-beta.2` until 2.0.0 is on NuGet. Post-publish follow-up: bump them to `2.0.0`.

No tag. `dev release` was not run.

### Gate log

`./bin/dev check-version` exit 0:

```
Version in source: 2.0.0
Latest NuGet version: 2.0.0-beta.2
Packages checked: TimeWarp.Amuru, TimeWarp.Amuru.Tools

✓ Version in source is new — safe to release.
```

`./bin/dev workflow --mode merge` exit 0. Pipeline `clean -> build -> verify-samples -> test` SUCCEEDED. Tests: Passed 732, Failed 0, Skipped 1, Total 733. `artifacts/packages/` contains `TimeWarp.Amuru.2.0.0.nupkg` and `TimeWarp.Amuru.Tools.2.0.0.nupkg`.

`ganda repo audit` clean after the fix (Passed 32, Failed 0).
