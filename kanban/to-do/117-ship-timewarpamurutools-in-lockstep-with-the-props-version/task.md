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

- [ ] Delete `<Version>` from `source/timewarp-amuru-tools/timewarp-amuru-tools.csproj` so it inherits the props version
- [ ] `.timewarp/dev.jsonc`: remove the `checkVersionConfig.packages` scope and the "own cadence" comment so check-version and the publish-state gate cover every packable project (delete the file if nothing else remains)
- [ ] `tools/dev-cli/endpoints/workflow-command.cs`: remove the independent-cadence comments and any code path that exists only to tolerate a second version (per-package version resolution may stay if it is how the packable set is derived, but the verify step must refuse when any package version differs from the props version). Keep `--skip-duplicate` only if the nuru DevCli reference pipeline uses it; otherwise drop it
- [ ] `documentation/developer/guides/releasing.md`: rewrite the two-package section and the "Tools-only release" paragraph to state one version, lockstep, and delete every mention of Tools riding its own cadence
- [ ] `AGENTS.md`: replace the "Own `<Version>` in its csproj" text on the Tools bullet, and add a rule under the build/release section: "All packages ship together at the single `<Version>` in `source/Directory.Build.props`. Never add a `<Version>` to a csproj. A project that must not ship sets `IsPackable=false` with a stated reason."
- [ ] Bump `<Version>` in `source/Directory.Build.props` from 1.1.0 to 1.1.1
- [ ] `dev check-version` passes for both packages at 1.1.1
- [ ] `dev workflow --mode merge` green locally; `ganda repo audit` clean
- [ ] `dev release --dry-run` from the task branch reports only the on-master guard as failing (tree clean, tag v1.1.1 absent, 1.1.1 unpublished for both packages)
- [ ] PR open

## Session

- Created: 2026-09-23 (cockpit dispatch)

## Notes

- Steve, 2026-09-23, on PR #98: "why two versions ... all the monorepos ship together." The split was introduced by the worker on task 116 and must be fully reverted, not documented.
- 1.1.1 is a patch bump because no library code changes; it exists so Tools gets a release at the props version. Tools currently has 1.0.0-beta.2 on NuGet; 1.1.1 is higher, so no ordering problem.
- After merge the cockpit runs `dev release` from clean synced master. Do not tag or release on this task.
- Reference for the lockstep convention: `timewarp-nuru/documentation/developer/guides/releasing.md` ("One version, lockstep").
