# Ship TimeWarp.Amuru.Tools in lockstep with the props version

## Description

Task 116 (PR #98) adopted `dev release` but kept `TimeWarp.Amuru.Tools` on its
own version (`<Version>1.0.0-beta.2</Version>` in its csproj) and scoped the
publish-state gate to the core package only. That is wrong. **Every packable
project in a TimeWarp repo ships at the single `<Version>` in
`source/Directory.Build.props`. There is one version. There are no per-csproj
overrides and no independent cadences.** Remove the split everywhere it was
wired, bump the props version to 1.1.1 so both packages release together, and
open the PR. This PR is the version-bump PR for the first real `dev release`
under the new pipeline; the release itself is cut from master after merge.

## Checklist

- [x] Delete `<Version>` from `source/timewarp-amuru-tools/timewarp-amuru-tools.csproj` so it inherits the props version
- [x] `.timewarp/dev.jsonc`: remove the `checkVersionConfig.packages` scope and the "own cadence" comment so check-version and the publish-state gate cover every packable project (delete the file if nothing else remains)
- [x] `tools/dev-cli/endpoints/workflow-command.cs`: remove the independent-cadence comments and any code path that exists only to tolerate a second version (per-package version resolution may stay if it is how the packable set is derived, but the verify step must refuse when any package version differs from the props version). Keep `--skip-duplicate` only if the nuru DevCli reference pipeline uses it; otherwise drop it
- [x] `documentation/developer/guides/releasing.md`: rewrite the two-package section and the "Tools-only release" paragraph to state one version, lockstep, and delete every mention of Tools riding its own cadence
- [x] `AGENTS.md`: replace the "Own `<Version>` in its csproj" text on the Tools bullet, and add a rule under the build/release section: "All packages ship together at the single `<Version>` in `source/Directory.Build.props`. Never add a `<Version>` to a csproj. A project that must not ship sets `IsPackable=false` with a stated reason."
- [x] Bump `<Version>` in `source/Directory.Build.props` from 1.1.0 to 1.1.1
- [x] `dev check-version` passes for both packages at 1.1.1
- [x] `dev workflow --mode merge` green locally; `ganda repo audit` clean
- [x] `dev release --dry-run` from the task branch reports only the on-master guard as failing (tree clean, tag v1.1.1 absent, 1.1.1 unpublished for both packages)
- [ ] PR open

## Session

- Created: 2026-09-23 (cockpit dispatch)
- 2026-09-23: implementer (Claude Fable 5.1, ganda task work) — lockstep changes landed in commit `7d0300c` on `task/117-…`; check-version, merge workflow and repo audit green; release dry-run refuses only at the on-master guard. PR open is the host open-pr node.

## Notes

- Steve, 2026-09-23, on PR #98: "why two versions ... all the monorepos ship together." The split was introduced by the worker on task 116 and must be fully reverted, not documented.
- 1.1.1 is a patch bump because no library code changes; it exists so Tools gets a release at the props version. Tools currently has 1.0.0-beta.2 on NuGet; 1.1.1 is higher, so no ordering problem.
- After merge the cockpit runs `dev release` from clean synced master. Do not tag or release on this task.
- Reference for the lockstep convention: `timewarp-nuru/documentation/developer/guides/releasing.md` ("One version, lockstep").

## Results

One version, lockstep. Both packages now inherit `<Version>1.1.1</Version>` from
`source/Directory.Build.props`; nothing in the repo tolerates a second version.

- `source/timewarp-amuru-tools/timewarp-amuru-tools.csproj`: `<Version>1.0.0-beta.2</Version>` and its "own beta cadence" comment deleted. `dotnet msbuild -getProperty:Version` now evaluates `1.1.1` for both projects.
- `.timewarp/dev.jsonc`: deleted (the `checkVersionConfig.packages` scope was its only content). `dev check-version` now reports `Packages checked: TimeWarp.Amuru, TimeWarp.Amuru.Tools` with no override nudge.
- `tools/dev-cli/endpoints/workflow-command.cs`: Design comment rewritten to state lockstep. Per-project `<Version>` evaluation stays, but Step 2 now has a lockstep gate: any packable project whose evaluated version differs from the props version aborts with `package version differs from props version` and an operator message (remove the csproj `<Version>`, or `IsPackable=false` with a reason). The set banner prints `Packable set (N) at {props}: …`. `--skip-duplicate` is kept because the nuru DevCli reference pipeline uses it for partial-publish resume; its comment now says exactly that instead of "Tools rides its own cadence".
- `documentation/developer/guides/releasing.md`: Overview bullets replaced with "One version, lockstep" (mirrors timewarp-nuru's guide); guard 7, Step 2/6, Step 5/6, Step 6/6 and the partial-publish section rewritten for the whole derived set; the "Tools-only release" appendix item replaced by "No per-package releases".
- `AGENTS.md`: Tools bullet now says it ships at the same `<Version>` as core; new "One version, lockstep" rule under Build/Test Commands with the exact wording from the checklist.
- `readme.md`: "own release cadence" on the Two Packages bullet replaced with "both ship together at one version".
- `source/Directory.Build.props`: 1.1.0 → 1.1.1.

Not changed on purpose: `Directory.Packages.props` still pins the *consumed* `TimeWarp.Amuru.Tools` at `1.0.0-beta.2` for tests/samples that reference the published package; 1.1.1 does not exist on NuGet until the cockpit cuts the release from master, so that pin is a follow-up after publish.

Gate log (2026-09-23, task worktree, commit `7d0300c`):

- `dev check-version` → `Version in source: 1.1.1`, `Latest NuGet version: 1.1.0`, both packages checked, "safe to release", exit 0.
- `dev workflow --mode merge` → clean/build/verify-samples/test, 494 passed / 1 skipped, `Pipeline SUCCEEDED`; `artifacts/packages/` holds `TimeWarp.Amuru.1.1.1.nupkg` and `TimeWarp.Amuru.Tools.1.1.1.nupkg`.
- `ganda repo audit` → "Repository passes all audit checks" (after `dev self-install` produced the gitignored `bin/dev`; the only failures before that were `bin-dev` / `dev-cli-capabilities`).
- `dev release --dry-run` → `✓ gh authenticated`, `✓ working tree clean`, then refuses: "release must be cut from master". Guards stop at the first failure, so tag/publish-state were confirmed separately: `git tag -l 'v1.1.*'` and `git ls-remote --tags origin 'v1.1.*'` show only `v1.1.0`; check-version above shows 1.1.1 unpublished for both packages.

### How to validate

Smoke:

```bash
cd /home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-amuru/task-117-ship-timewarpamurutools-in-lockstep-with-the-props
grep -rn "<Version>" source/                       # only source/Directory.Build.props
dotnet msbuild source/timewarp-amuru-tools/timewarp-amuru-tools.csproj -getProperty:Version -nologo
dotnet run --file tools/dev-cli/dev.cs -- check-version
dotnet run --file tools/dev-cli/dev.cs -- workflow --mode merge
ganda repo audit
dotnet run --file tools/dev-cli/dev.cs -- release --dry-run
grep -rn -i --exclude-dir=obj "cadence\|dev.jsonc\|beta.2" AGENTS.md readme.md documentation/ tools/ source/
```

Expect:

- `grep "<Version>"` lists exactly one hit: `source/Directory.Build.props` with `1.1.1`.
- MSBuild prints `1.1.1` for the Tools project.
- check-version prints `Packages checked: TimeWarp.Amuru, TimeWarp.Amuru.Tools` and `✓ Version in source is new — safe to release.` with no `checkVersionConfig` nudge.
- Merge workflow ends `Pipeline SUCCEEDED`; `artifacts/packages/` contains `TimeWarp.Amuru.1.1.1.nupkg` and `TimeWarp.Amuru.Tools.1.1.1.nupkg`.
- Audit ends `Repository passes all audit checks.` (run `dotnet run --file tools/dev-cli/dev.cs -- self-install` first if `bin/dev` is missing in a fresh worktree).
- Dry-run passes `gh authenticated` and `working tree clean`, then fails only with "release must be cut from master".
- The final grep returns no hits (the `.timewarp` directory no longer exists; `Directory.Packages.props` still pins the consumed Tools package at `1.0.0-beta.2` until 1.1.1 is on NuGet).
- After merge, on clean synced master: `dev release --dry-run` passes all eight guards and prints the `v1.1.1` tag/release commands; the release event's Step 2 banner reads `Packable set (2) at 1.1.1: TimeWarp.Amuru, TimeWarp.Amuru.Tools`.
