# Bump both packages to 2.0.0-beta.1 and draft 2.0 release notes

## Description

Master is versioned `1.1.1`, identical to the latest NuGet and GitHub release, yet it carries five merged breaking changes (tasks 087, 088, 099, 100, 121) plus the PublicAPI baseline (094-004). `dev check-version` currently refuses with "Version 1.1.1 was already released". Nothing may ship from master until the version is bumped, and by semver the removals make the next release **2.0.0**.

This task bumps both packages (lockstep, task 117) to `2.0.0-beta.1`, promotes the PublicAPI Unshipped entries into Shipped per the 094-004 workflow, and drafts the 2.0 release notes from the Results already recorded on the merged tasks. It does **not** cut the release; the cockpit does that with `dev release` from master after this merges (see `documentation/developer/guides/releasing.md` and the tw-release skill).

## Requirements

- Single version source: `source/Directory.Build.props` `<Version>` from `1.1.1` to `2.0.0-beta.1`. No per-project `<Version>`; the workflow's lockstep gate must stay green.
- `dev check-version` reports `Version in source: 2.0.0-beta.1`, latest NuGet `1.1.1`, both packages checked, safe to release, exit 0.
- PublicAPI: for both `source/timewarp-amuru/public-api/` and `source/timewarp-amuru-tools/public-api/`, move every entry in `PublicAPI.Unshipped.txt` into `PublicAPI.Shipped.txt`; for each `*REMOVED*` line in Unshipped, delete the matching line from Shipped and drop the marker (Shipped may not contain `*REMOVED*`, RS0024). Unshipped ends as `#nullable enable` only. `./bin/dev build` 0 warnings / 0 errors.
- Release notes: create `documentation/release-notes/2.0.0.md` (or follow an existing release-notes convention if one exists in the repo; check `documentation/` and `readme.md` first and record what you found). Sections: Breaking changes (per package), Behavior changes, New, Fixed, Upgrade guide. Source every item from the merged kitchens under `kanban/done/`:
  - 087 Fix invalid CLI flags in dotnet builders: "Breaking public surface" paragraph in Results
  - 088 Fzf SelectWithFzf and input methods: Results + review M1 (empty-string stdin = immediate EOF)
  - 099 Git UpdateBranchAsync and result standardization: "Removed or renamed public members" list in Results
  - 100 Validation controls on all builders: "Removed / renamed members" in Results
  - 121 Passthrough stdin and Pipe options: "Release note" section in Results
  - 094-004 PublicAPI analyzers: adoption note for contributors (not user-facing API)
  - 332/333 in ganda are not amuru release items; omit.
- Upgrade guide: one line per removed/renamed member mapping old to new (e.g. `GetMasterWorktreePathAsync` → `GetDefaultWorktreePathAsync`; `WithConfig` → `WithConfigFile`; pack `WithFramework` removed; watch `WithInclude`/`WithExclude`/`WithProperty` removed; `BranchExistsAsync` returns a record; `WithStandardInput("")` semantics; `TtyPassthroughAsync` throws on configured stdin).
- Mention the Direct/Tools split only if the notes convention expects a package list; both packages ship at one version.
- Do not touch `Directory.Packages.props` consumed pins for `TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` (they reference published packages; 2.0.0-beta.1 does not exist on NuGet until the release is cut). Note this in Results as the post-publish follow-up, as task 117 did.
- Do not tag, do not run `dev release`, do not push tags.

## Checklist

- [x] `source/Directory.Build.props`: `<Version>2.0.0-beta.1</Version>`
- [x] `dev check-version` green (safe to release, both packages, exit 0); paste output into Results
- [x] PublicAPI promotion for core and Tools; Unshipped files reduced to the header; `./bin/dev build` clean
- [x] Release notes file written from the six kitchens above; every removed/renamed member appears in the Upgrade guide
- [x] `readme.md` / `AGENTS.md`: fix any text that still says 1.x-only or implies the old API names (spot-check `WithConfig`, `GetMasterWorktreePath`, `UpdateMasterWorktree`, `WithTargetFramework`, `SelectWithFzf` prose)
- [x] `dev workflow --mode merge` green: build, verify-samples, test (expect 604 passed, 1 skipped), nupkgs for both packages at 2.0.0-beta.1 under `artifacts/packages/`
- [x] `ganda repo audit` clean
- [x] Results: gate log (check-version, workflow, audit), release-notes path, post-publish follow-up note for the consumed pins

## Notes

- Lockstep rule and the workflow gate are documented in task 117 Results and `AGENTS.md`.
- PublicAPI promotion workflow is documented in `AGENTS.md` and `documentation/developer/guides/releasing.md` (task 094-004).
- After this merges, the cockpit runs the tw-release flow: `dev release` from a clean synced master cuts `v2.0.0-beta.1`, CI promotes the artifacts. Further changes before 2.0.0 GA go into Unshipped again and get promoted at the next bump.
- Why beta first: master has had five breaking PRs in two days with worker-authored tests; a prerelease lets consumers (ganda, nuru, flow) validate before 2.0.0 is final.

## Results

Both packages are `2.0.0-beta.1` from `source/Directory.Build.props`. No per-project `<Version>`. PublicAPI Unshipped for core and Tools is only `#nullable enable`. Shipped line counts (`wc -l`, header included): core 253 (was 250; three additions), Tools 1620 (was 1491; two `*REMOVED*` lines dropped, 131 additions). `./bin/dev build`: 0 warnings, 0 errors.

Release notes: `documentation/release-notes/2.0.0.md`. There was no `documentation/release-notes/` tree. Task 101 keeps GitHub Release bodies as the channel and does not set `PackageReleaseNotes`. Blips under `documentation/posts/blips/` are announcements, not a changelog. The new file is the draft for the `v2.0.0-beta.1` GitHub Release body. It is titled for the beta and lists breaking changes per package, behavior changes, new members, fixes, and an upgrade guide. Every removed or renamed member from kitchens 087, 088, 099, 100, and 121 is in that guide, including `WithStandardInput("")` and the `TtyPassthroughAsync` throw. Task 094-004 is a contributor note, not a user-facing API change. Ganda 332/333 are omitted.

Spot-check: `readme.md` and `AGENTS.md` do not mention `WithConfig`, `GetMasterWorktreePath`, `UpdateMasterWorktree`, or `WithTargetFramework`. `SelectWithFzf` remains the real API (task 088 implemented it). `AGENTS.md` no longer says "Stable-1.0 track" or "pending 2.0 surface"; Shipped is the `2.0.0-beta.1` baseline. `readme.md` no longer passes `--prerelease` on Tools alone or calls only core "stable". `skills/amuru/SKILL.md` no longer says "1.0 contract"; it points at the release notes.

Not changed on purpose: `Directory.Packages.props` still pins the consumed packages at `TimeWarp.Amuru` `1.0.0` and `TimeWarp.Amuru.Tools` `1.0.0-beta.2` (`.githooks` runfiles). `2.0.0-beta.1` does not exist on NuGet until the cockpit cuts the release. Post-publish follow-up: bump those consumed pins after `v2.0.0-beta.1` is on NuGet, the same follow-up task 117 recorded for `1.1.1`.

No tag. `dev release` was not run.

Gate log (2026-10-05, this worktree):

`./bin/dev check-version` exit 0:

```text
Version in source: 2.0.0-beta.1
Latest NuGet version: 1.1.1
Packages checked: TimeWarp.Amuru, TimeWarp.Amuru.Tools

✓ Version in source is new — safe to release.
```

`./bin/dev workflow --mode merge` exit 0. Pipeline `clean -> build -> verify-samples -> test` succeeded. Build 0 warnings, 0 errors. Both samples compiled. Tests: Passed 620, Failed 0, Skipped 1, Total 621. The kitchen's "604 passed" was the count when the task was written; the suite has grown (including task 105) and still has a single skip and zero failures. `artifacts/packages/` contains `TimeWarp.Amuru.2.0.0-beta.1.nupkg` and `TimeWarp.Amuru.Tools.2.0.0-beta.1.nupkg`.

`ganda repo audit`: Passed 32, Failed 0. "Repository passes all audit checks."

### How to validate

Smoke:

```bash
grep -n '<Version>' source/Directory.Build.props
./bin/dev check-version
test "$(wc -l < source/timewarp-amuru/public-api/PublicAPI.Unshipped.txt)" = 1
test "$(wc -l < source/timewarp-amuru-tools/public-api/PublicAPI.Unshipped.txt)" = 1
./bin/dev workflow --mode merge
ls artifacts/packages/TimeWarp.Amuru.2.0.0-beta.1.nupkg artifacts/packages/TimeWarp.Amuru.Tools.2.0.0-beta.1.nupkg
ganda repo audit
```

Expect:

- `<Version>2.0.0-beta.1</Version>` is the only `<Version>` under `source/`.
- check-version prints `Version in source: 2.0.0-beta.1`, `Latest NuGet version: 1.1.1`, both packages, and `safe to release`, exit 0.
- Each Unshipped file is only `#nullable enable`. Shipped contains no `*REMOVED*`.
- Merge workflow prints `Pipeline SUCCEEDED`. The suite prints Passed 620, Skipped 1, Failed 0.
- Both `2.0.0-beta.1` nupkgs exist.
- `ganda repo audit` prints Failed: 0.

## Session

- Created: 522eb63d (2026-10-05)
- Implementation: 01a10a53-3590-78c2-a429-c41a30400516 (2026-10-05)
