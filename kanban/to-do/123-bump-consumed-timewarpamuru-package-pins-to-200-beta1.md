# Bump consumed TimeWarp.Amuru package pins to 2.0.0-beta.1

## Description

`Directory.Packages.props` pins the *consumed* packages used by the `.githooks` runfiles (`#:package TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` under CPM) at `TimeWarp.Amuru` `1.0.0` and `TimeWarp.Amuru.Tools` `1.0.0-beta.2`. Task 117 and task 122 both recorded this as the post-publish follow-up. `v2.0.0-beta.1` is now on NuGet for both packages (released 2026-10-05, indexed and confirmed), so the pins can move.

Because 2.0 has breaking changes (tasks 087, 088, 099, 100, 121; see `documentation/release-notes/2.0.0.md`), the hooks must be compiled and exercised against the new version, not just re-pinned.

## Requirements

- `Directory.Packages.props`: `TimeWarp.Amuru` → `2.0.0-beta.1`, `TimeWarp.Amuru.Tools` → `2.0.0-beta.1`. Keep the "Self pins" comment.
- Every runfile under `.githooks/` (`pre-commit.cs`, `post-commit.cs`, `post-checkout.cs`, `post-merge.cs`, `pre-push.cs`) restores and compiles against the new pins: `dotnet build .githooks/<hook>.cs` (or `dotnet run --file` with a harmless invocation) for each, 0 warnings / 0 errors.
- If any hook uses an API that changed in 2.0 (check the upgrade guide: `BranchExistsAsync` now returns a record; `UpdateBranchAsync`/`GetWorktreePathAsync` require a branch name; `GetMasterWorktreePathAsync`/`UpdateMasterWorktreeAsync` removed; `WithConfig`/`WithTargetFramework` renamed; `WithStandardInput("")` semantics), fix the usage. **Before editing a hook body**, check whether the file is byte-identical to ganda's template under `timewarp-ganda/master/templates/repo-baseline/githooks/` (the attest hook installs from there). If it is template-owned, record in Results that the template needs the same change and open no edits that the next template refresh would revert without a ganda task to carry them; if the hook has already diverged locally, edit it here.
- Exercise the hooks for real: make a throwaway commit in the task worktree and confirm `post-commit` runs (memsearch index + `ganda repo attest` signs), then reset the throwaway. `pre-push` runs on the push of this branch; confirm it passes.
- `ganda repo audit` clean (the `githooks` / baseline checks must still pass).
- No other version changes. `source/Directory.Build.props` stays `2.0.0-beta.1`.

## Checklist

- [ ] Pins bumped in `Directory.Packages.props`
- [ ] Each of the 5 hook runfiles compiles against 2.0.0-beta.1 (paste the per-file result into Results)
- [ ] API usage fixed where 2.0 broke it, or confirmed none needed; template-ownership check recorded per edited hook
- [ ] Throwaway commit proves `post-commit` attests under the new pins; branch push proves `pre-push`
- [ ] `./bin/dev build` and `ganda repo audit` clean
- [ ] Results: pin diff, per-hook compile log, hook exercise log, any ganda template follow-up

## Notes

- Origin: task 117 Results ("that pin is a follow-up after publish") and task 122 Results (same note for 2.0.0-beta.1).
- The hooks are the only in-repo consumers of the published packages; tests and samples reference the project, not the package.
- Release notes with the upgrade guide: `documentation/release-notes/2.0.0.md`.

## Session

- Created: 522eb63d (2026-10-06)
