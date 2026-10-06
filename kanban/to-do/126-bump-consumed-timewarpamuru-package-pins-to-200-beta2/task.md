# Bump consumed TimeWarp.Amuru package pins to 2.0.0-beta.2

## Description

`Directory.Packages.props` pins the *consumed* packages used by the `.githooks` runfiles (`#:package TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` under CPM) at `2.0.0-beta.1`. `v2.0.0-beta.2` is on NuGet for both packages (released and indexed 2026-10-06). Task 125 recorded this as the post-publish follow-up, same as 117, 122 and 123 did for their releases.

beta.2 is additive over beta.1 (native text commands, timeouts, Tools XML docs): no breaking changes, so the hooks need a re-pin and a compile proof, not an API migration.

## Requirements

- `Directory.Packages.props`: `TimeWarp.Amuru` → `2.0.0-beta.2`, `TimeWarp.Amuru.Tools` → `2.0.0-beta.2`. Keep the "Self pins" comment.
- Each runfile under `.githooks/` (`pre-commit.cs`, `post-commit.cs`, `post-checkout.cs`, `post-merge.cs`, `pre-push.cs`) restores and compiles against beta.2: `dotnet build .githooks/<hook>.cs`, 0 warnings / 0 errors; paste the per-file table into Results as task 123 did.
- Do not edit hook bodies. They are byte-identical to ganda's `templates/repo-baseline/githooks/` (task 344 attest-only shape); if the audit's hook check wants a change, record it in Results instead of diverging.
- Exercise: a throwaway commit in the task worktree shows `post-commit` attesting under the new pins (paste the `Attested tree` line), then `git reset --soft HEAD~1`. The branch push exercises `pre-push`.
- `./bin/dev build` and `ganda repo audit` clean. `source/Directory.Build.props` stays `2.0.0-beta.2`.

## Checklist

- [x] Pins bumped
- [x] 5 hooks compile against beta.2 (table in Results)
- [x] Throwaway commit attested; reset; branch push passes pre-push
- [x] `./bin/dev build`, `ganda repo audit` clean
- [x] Results: pin diff, compile table, attest line

## Notes

- Pattern: task 123 (beta.1 pins) Results.
- The only in-repo consumers of the published packages are the hooks; tests and samples reference the projects.

## Results

Consumed pins in `Directory.Packages.props` now match the published 2.0.0-beta.2 packages. The "Self pins" comment is unchanged. `source/Directory.Build.props` stays `<Version>2.0.0-beta.2</Version>`. No other package versions changed.

Pin diff:

```diff
-    <PackageVersion Include="TimeWarp.Amuru" Version="2.0.0-beta.1" />
-    <PackageVersion Include="TimeWarp.Amuru.Tools" Version="2.0.0-beta.1" />
+    <PackageVersion Include="TimeWarp.Amuru" Version="2.0.0-beta.2" />
+    <PackageVersion Include="TimeWarp.Amuru.Tools" Version="2.0.0-beta.2" />
```

### Template ownership

Each `.githooks/*.cs` file is byte-identical to `timewarp-ganda` `21af1887` `source/timewarp-ganda/templates/repo-baseline/githooks/` (`pre-commit.cs`, `post-commit.cs`, `post-checkout.cs`, `post-merge.cs`, `pre-push.cs`). No hook body was edited. `ganda repo audit` `memsearch-scaffold` passes ("Git hooks match the attest baseline"), so the audit did not ask for a hook change.

### Per-hook compile

`dotnet build .githooks/<hook>.cs` from the repo root. Each runfile's `project.assets.json` resolved `TimeWarp.Amuru/2.0.0-beta.2` and `TimeWarp.Amuru.Tools/2.0.0-beta.2`. Summary (`-v n -clp:Summary`):

| Hook | Result |
| --- | --- |
| `pre-commit.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |
| `post-commit.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |
| `post-checkout.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |
| `post-merge.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |
| `pre-push.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |

Output lines (exit 0):

```text
pre-commit.cs -> .../runfile/pre-commit-fc7e4e82e4b194b28c8d1e9ffd9d4f5f2a11f2e9b1ae264b508d4989bb3c5778/bin/debug/pre-commit.dll
post-commit.cs -> .../runfile/post-commit-046e6e0a95fc26b0b9673ecef42b683b7cd3a183b74a2421a95d8d572eb13293/bin/debug/post-commit.dll
post-checkout.cs -> .../runfile/post-checkout-531f15a5df367dc1d8866f21e354a5eae26bca6e8be284ee2d65ed5cb48137bb/bin/debug/post-checkout.dll
post-merge.cs -> .../runfile/post-merge-789029cc5a54c98e8f1923a642734d51f9aa7eba54b0de51d6abe2c4ced12215/bin/debug/post-merge.dll
pre-push.cs -> .../runfile/pre-push-5db118a556bb585f5149323a44f8171395c8bb22def45c336661e62adfe2945a/bin/debug/pre-push.dll
```

### Hook exercise

The first throwaway (`0d1aa37bb98e43033fbeabff7d18f3fa3fb1ad5f`, pin file only) did not attest: post-commit reported a dirty tree because the uncommitted folderize was still in the worktree. A second throwaway on top of those pins (`673e0ecebc75e58f5a3af8251f982c951991a493`, tree `01e783146628514b098963b01921aaebafeaae02`) was clean and included `Directory.Packages.props` at `2.0.0-beta.2`. `post-commit` output:

```text
Attested tree 01e783146628 (key_id=tw-audit-1)
  tree:   01e783146628514b098963b01921aaebafeaae02
  commit: 673e0ecebc75e58f5a3af8251f982c951991a493
  branch: task/126-bump-consumed-timewarpamuru-package-pins-to-200-be
  check_set: e3b184c203dc0ba60e42d30b299d9d1e8593eeaf3c7906810fdd53edf27cabb5
  notes: pushed
  warning: Failed to post ganda/attestation commit status; Workflow rerun error: '0x1B' is an invalid start of a value. Path: $ | LineNumber: 0 | BytePositionInLine: 0.
```

`git notes --ref=refs/notes/ganda-audit show 01e783146628514b098963b01921aaebafeaae02` returned the ed25519 note (`key_id=tw-audit-1`). Both throwaways were removed with `git reset --soft` and are not on the branch. The pin diff and this folderized kitchen stayed in the index. The status-post warning is the unpushed SHA; the note was signed and pushed.

Manual `pre-push` (stdin `refs/heads/task/126-bump-consumed-timewarpamuru-package-pins-to-200-be b1bc22b26934e6c99b6f3125df9179f8db36bd8b refs/heads/task/126-bump-consumed-timewarpamuru-package-pins-to-200-be 000…0`) exited 0 with no refusal text. The push of this branch is the git-invoked confirmation (same hook, dest is the task ref, not `master`/`main`).

### Build and audit

`./bin/dev build`: Build succeeded. 0 Warning(s). 0 Error(s). Exit 0.

`ganda repo audit`: exit 0. Passed 31, Failed 0, Skipped 0. Message: "Repository passes all audit checks."

### How to validate

Smoke: from this worktree, `dotnet build .githooks/pre-commit.cs`, `dotnet build .githooks/post-commit.cs`, `dotnet build .githooks/post-checkout.cs`, `dotnet build .githooks/post-merge.cs`, `dotnet build .githooks/pre-push.cs`, `./bin/dev build`, and `ganda repo audit`.

Expect: each command exits 0. Hook builds report 0 warnings and 0 errors, and their restores resolve `TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` at `2.0.0-beta.2`. `source/Directory.Build.props` is still `2.0.0-beta.2`. `Directory.Packages.props` keeps the "Self pins" comment. A commit on this task branch runs `post-commit` and prints an `Attested tree` line (`key_id=tw-audit-1`). Pushing the task ref runs `pre-push` and exits 0.

## Session

- Created: 522eb63d (2026-10-06)
- Implementation: 01a11229 (2026-10-06)
