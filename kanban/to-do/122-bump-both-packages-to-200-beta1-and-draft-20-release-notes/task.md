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

- [ ] `source/Directory.Build.props`: `<Version>2.0.0-beta.1</Version>`
- [ ] `dev check-version` green (safe to release, both packages, exit 0); paste output into Results
- [ ] PublicAPI promotion for core and Tools; Unshipped files reduced to the header; `./bin/dev build` clean
- [ ] Release notes file written from the six kitchens above; every removed/renamed member appears in the Upgrade guide
- [ ] `readme.md` / `AGENTS.md`: fix any text that still says 1.x-only or implies the old API names (spot-check `WithConfig`, `GetMasterWorktreePath`, `UpdateMasterWorktree`, `WithTargetFramework`, `SelectWithFzf` prose)
- [ ] `dev workflow --mode merge` green: build, verify-samples, test (expect 604 passed, 1 skipped), nupkgs for both packages at 2.0.0-beta.1 under `artifacts/packages/`
- [ ] `ganda repo audit` clean
- [ ] Results: gate log (check-version, workflow, audit), release-notes path, post-publish follow-up note for the consumed pins

## Notes

- Lockstep rule and the workflow gate are documented in task 117 Results and `AGENTS.md`.
- PublicAPI promotion workflow is documented in `AGENTS.md` and `documentation/developer/guides/releasing.md` (task 094-004).
- After this merges, the cockpit runs the tw-release flow: `dev release` from a clean synced master cuts `v2.0.0-beta.1`, CI promotes the artifacts. Further changes before 2.0.0 GA go into Unshipped again and get promoted at the next bump.
- Why beta first: master has had five breaking PRs in two days with worker-authored tests; a prerelease lets consumers (ganda, nuru, flow) validate before 2.0.0 is final.

## Session

- Created: 522eb63d (2026-10-05)
