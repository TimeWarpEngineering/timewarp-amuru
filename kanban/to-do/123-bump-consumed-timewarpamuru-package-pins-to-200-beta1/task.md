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

- [x] Pins bumped in `Directory.Packages.props`
- [x] Each of the 5 hook runfiles compiles against 2.0.0-beta.1 (paste the per-file result into Results)
- [x] API usage fixed where 2.0 broke it, or confirmed none needed; template-ownership check recorded per edited hook
- [x] Throwaway commit proves `post-commit` attests under the new pins; branch push proves `pre-push`
- [x] `./bin/dev build` and `ganda repo audit` clean
- [x] Results: pin diff, per-hook compile log, hook exercise log, any ganda template follow-up

## Notes

- Origin: task 117 Results ("that pin is a follow-up after publish") and task 122 Results (same note for 2.0.0-beta.1).
- The hooks are the only in-repo consumers of the published packages; tests and samples reference the project, not the package.
- Release notes with the upgrade guide: `documentation/release-notes/2.0.0.md`.

## Results

Consumed pins in `Directory.Packages.props` now match the published 2.0.0-beta.1 packages. The "Self pins" comment is unchanged. `source/Directory.Build.props` stays `<Version>2.0.0-beta.1</Version>`. No other package versions changed.

Pin diff:

```diff
-    <PackageVersion Include="TimeWarp.Amuru" Version="1.0.0" />
-    <PackageVersion Include="TimeWarp.Amuru.Tools" Version="1.0.0-beta.2" />
+    <PackageVersion Include="TimeWarp.Amuru" Version="2.0.0-beta.1" />
+    <PackageVersion Include="TimeWarp.Amuru.Tools" Version="2.0.0-beta.1" />
```

### Template ownership and API usage

Each `.githooks/*.cs` file is byte-identical to `timewarp-ganda` `f9d709fb` `source/timewarp-ganda/templates/repo-baseline/githooks/` (`pre-commit.cs`, `post-commit.cs`, `post-checkout.cs`, `post-merge.cs`, `pre-push.cs`). No hook body was edited.

None of the hooks call a 2.0 breaking member (`BranchExistsAsync`, `UpdateBranchAsync`, `GetWorktreePathAsync`, `GetMasterWorktreePathAsync`, `UpdateMasterWorktreeAsync`, `WithConfig`, `WithTargetFramework`, `WithStandardInput`, or the other upgrade-guide renames). They use `Git.FindRoot`, `Shell.Builder`, `WithArguments`, `WithWorkingDirectory`, `WithNoValidation`, `CaptureAsync`, and `RunAsync`. No ganda template follow-up for an API fix.

`ganda repo audit` reports an advisory `memsearch-scaffold` warning: installed ganda `1.0.0-beta.37` embeds post-commit/post-merge/post-checkout bodies that drop `memsearch index-repo` (task 339), while git `f9d709fb` templates still index. Those files stay on the git template. Rewriting them to the embedded bytes would drop the memsearch half of this task's hook exercise and would diverge from the template the next git refresh installs. Audit exit code is 0 ("Repository passes — 1 advisory").

### Per-hook compile

`dotnet build .githooks/<hook>.cs` from the repo root. Each runfile's `project.assets.json` resolved `TimeWarp.Amuru/2.0.0-beta.1` and `TimeWarp.Amuru.Tools/2.0.0-beta.1`. Summary (`-v n -clp:Summary`):

| Hook | Result |
| --- | --- |
| `pre-commit.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |
| `post-commit.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |
| `post-checkout.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |
| `post-merge.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |
| `pre-push.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |

First restore/compile lines (exit 0):

```text
pre-commit.cs -> .../runfile/pre-commit-06b993d2e478bcb5353748d521d4c87c484c58328b1b67b62738f8de6ee27feb/bin/debug/pre-commit.dll
post-commit.cs -> .../runfile/post-commit-ff95ca71bef2901bdda3183c00563ef653c70a6ac66340f4c81a344c97cdae4e/bin/debug/post-commit.dll
post-checkout.cs -> .../runfile/post-checkout-1789d3b5bac4d4c8a44943c80ea418b988f76d7074aca93be141229777f4e842/bin/debug/post-checkout.dll
post-merge.cs -> .../runfile/post-merge-05826c5d01efda2a1e582a9e39048478d3205cce7f6e7f956aa8fbdfd3adfd26/bin/debug/post-merge.dll
pre-push.cs -> .../runfile/pre-push-56382bf65cda7f260f1a36960f05bcfa41925d48b64a46a6fb8c8ddb07e81dbc/bin/debug/pre-push.dll
```

### Hook exercise

Throwaway commit `aae71915c4417b7985c39f658bafa0f3949b5a7e` (tree `3e2967c55f549336f03645ee0778323fcbf359b7`) on this task branch, with the 2.0.0-beta.1 pins in the working tree so the hook runfile restored those packages. `post-commit` output:

```text
Started background memsearch index
Attested tree 3e2967c55f54 (key_id=tw-audit-1)
  tree:   3e2967c55f549336f03645ee0778323fcbf359b7
  commit: aae71915c4417b7985c39f658bafa0f3949b5a7e
  branch: task/123-bump-consumed-timewarpamuru-package-pins-to-200-be
  check_set: 0f56ee7e9b9d460c5526e4fa3ea443f7eede04d345dedf34edbb65488d99ce35
  notes: pushed
  warning: Failed to post ganda/attestation commit status; Workflow rerun error: '0x1B' is an invalid start of a value. Path: $ | LineNumber: 0 | BytePositionInLine: 0.
```

`git notes --ref=refs/notes/ganda-audit show 3e2967c55f549336f03645ee0778323fcbf359b7` returned the ed25519 note (`key_id=tw-audit-1`). `.githooks/memsearch-index.log` gained a second `index ... collections=ms_timewarp_amuru_8e86d5f1` line (log is gitignored). The commit was removed with `git reset --soft HEAD~1` and is not on the branch. The status-post warning is the unpushed SHA; the note was signed and pushed.

Manual `pre-push` (stdin `refs/heads/task/123-bump-consumed-timewarpamuru-package-pins-to-200-be <local-sha> refs/heads/task/123-... 000…0`) exited 0 with no refusal text. The push of this branch is the git-invoked confirmation (same hook, dest is the task ref, not `master`/`main`).

### Build and audit

`./bin/dev build`: Build succeeded. 0 Warning(s). 0 Error(s). Exit 0.

`ganda repo audit`: exit 0. Passed 31, Failed 1, Skipped 0. The only failure is the non-blocking `memsearch-scaffold` warning described above. Message: "Repository passes — 1 advisory (non-blocking) warning(s)."

### How to validate

Smoke: from this worktree, `dotnet build .githooks/pre-commit.cs`, `dotnet build .githooks/post-commit.cs`, `dotnet build .githooks/post-checkout.cs`, `dotnet build .githooks/post-merge.cs`, `dotnet build .githooks/pre-push.cs`, and `./bin/dev build`.

Expect: each command exits 0 with 0 warnings and 0 errors. Hook restores resolve `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` at `2.0.0-beta.1`. `source/Directory.Build.props` is still `2.0.0-beta.1`. A commit on this task branch runs `post-commit` (memsearch index line plus `ganda repo attest` note on `refs/notes/ganda-audit`). Pushing the task ref runs `pre-push` and exits 0.

## Session

- Created: 522eb63d (2026-10-06)
- Implementation: 01a10ebb (2026-10-06)
