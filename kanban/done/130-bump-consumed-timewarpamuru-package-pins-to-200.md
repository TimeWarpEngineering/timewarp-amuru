# Bump consumed TimeWarp.Amuru package pins to 2.0.0

## Description

`Directory.Packages.props` pins the *consumed* packages used by the `.githooks` runfiles (`#:package TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` under CPM) at `2.0.0-beta.2`. `v2.0.0` was released 2026-10-14 and both packages were pushed to NuGet by the release run. Task 129 recorded this as the post-publish follow-up, same as 123 and 126 did for the betas.

2.0.0 is the beta.2 surface with no API change, so the hooks need a re-pin and a compile proof, not an API migration.

## Requirements

- Wait until NuGet flatcontainer lists `2.0.0` for both packages before restoring.
- `Directory.Packages.props`: `TimeWarp.Amuru` → `2.0.0`, `TimeWarp.Amuru.Tools` → `2.0.0`. Keep the "Self pins" comment.
- Each runfile under `.githooks/` (`pre-commit.cs`, `post-commit.cs`, `post-checkout.cs`, `post-merge.cs`, `pre-push.cs`) restores and compiles against 2.0.0: `dotnet build .githooks/<hook>.cs`, 0 warnings / 0 errors; paste the per-file table into Results as task 126 did.
- Do not edit hook bodies. They are byte-identical to ganda's `templates/repo-baseline/githooks/`; if the audit's hook check wants a change, record it in Results instead of diverging.
- Exercise: a throwaway commit in the task worktree shows `post-commit` attesting under the new pins (paste the `Attested tree` line), then `git reset --soft HEAD~1`. The branch push exercises `pre-push`.
- `./bin/dev build` and `ganda repo audit` clean. `source/Directory.Build.props` stays `2.0.0`.

## Checklist

- [x] NuGet lists 2.0.0 for both packages
- [x] Pins bumped
- [x] 5 hooks compile against 2.0.0 (table in Results)
- [x] Pin commit attested (the real commit, no throwaway needed); branch push passes pre-push
- [x] `./bin/dev build`, `ganda repo audit` clean
- [x] Results: pin diff, compile table, attest line
- [ ] PR open and CI green

## Notes

- Pattern: task 126 (beta.2 pins) Results.
- The only in-repo consumers of the published packages are the hooks; tests and samples reference the projects.

## Results

Consumed pins in `Directory.Packages.props` now match the published 2.0.0 packages. The "Self pins" comment is unchanged. `source/Directory.Build.props` stays `<Version>2.0.0</Version>`. No other package versions changed. NuGet flatcontainer listed `2.0.0` for both packages before restore.

Pin diff:

```diff
-    <PackageVersion Include="TimeWarp.Amuru" Version="2.0.0-beta.2" />
-    <PackageVersion Include="TimeWarp.Amuru.Tools" Version="2.0.0-beta.2" />
+    <PackageVersion Include="TimeWarp.Amuru" Version="2.0.0" />
+    <PackageVersion Include="TimeWarp.Amuru.Tools" Version="2.0.0" />
```

### Template ownership

No `.githooks/*.cs` body was edited. `ganda repo audit` passes, so it did not ask for a hook change.

### Per-hook compile

`dotnet build .githooks/<hook>.cs` from the repo root. Each runfile's `project.assets.json` resolved `TimeWarp.Amuru/2.0.0` and `TimeWarp.Amuru.Tools/2.0.0`.

| Hook | Result |
| --- | --- |
| `pre-commit.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |
| `post-commit.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |
| `post-checkout.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |
| `post-merge.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |
| `pre-push.cs` | Build succeeded. 0 Warning(s). 0 Error(s). |

### Hook exercise

The pin commit itself (`ec856f526ff59d29bd43918fb81a74653d23f75b`) ran `post-commit` on a clean tree and attested, so no throwaway commit was needed:

```text
Attested tree 42e185297ac2 (key_id=tw-audit-1)
  tree:   42e185297ac29275bcfc2bf255ef929680648c3f
  commit: ec856f526ff59d29bd43918fb81a74653d23f75b
  branch: task/130-bump-consumed-timewarpamuru-package-pins-to-200
  check_set: d45d1f3ce13c1fcc2b3585092006fbd2e42821ff8de449d033cafa2880b0400b
  notes: pushed
```

`git notes --ref=refs/notes/ganda-audit show` returned the ed25519 note for that tree. The branch push exercises `pre-push`.

### Build and audit

`./bin/dev build`: Build succeeded. 0 Warning(s). 0 Error(s). Exit 0.

`ganda repo audit`: exit 0. "Repository passes all audit checks." No `--fix` was needed.

### How to validate

From this worktree run `dotnet build .githooks/<hook>.cs` for each of the five hooks, `./bin/dev build`, and `ganda repo audit`. Each exits 0, hook restores resolve both packages at `2.0.0`, and `source/Directory.Build.props` is still `2.0.0`.

## Session

- Created: 3213244 (2026-10-14)
- Implementation: ec856f5 (2026-10-14)
