# Adopt DevCli dev release and promote-artifact release pipeline

## Description

timewarp-amuru still cuts releases the old way: a human creates a GitHub
Release by hand, and `workflow.yml` rebuilds and publishes on the
`release: published` event via `dev workflow --api-key`. Every other package
repo (nuru, terminal, source-generators, state, ganda) follows the org
convention from nuru task 458: the version is typed once in
`source/Directory.Build.props`, `dev release` (from the
`TimeWarp.Nuru.DevCli` package, `release-command.cs`) runs eight guards and
creates the tag + GitHub Release, and the release run PROMOTES the CI-built
`Packages-*` artifact from the master push run at the tag SHA
(tag-gate → check-version → locate-run → download-artifact → verify → push),
never rebuilding.

Bring amuru onto that convention so `/tw-release` works here unchanged. This
surfaced on 2026-09-23 when cutting v1.1.0 (task 115): `dev release` did not
exist and the release had to be created with `gh release create` by hand.

Reference implementation: `timewarp-terminal` (consumes the DevCli package,
`tools/dev-cli/Directory.Build.props` + `Directory.Packages.props`) and
`timewarp-nuru/documentation/developer/guides/releasing.md` for the guide shape.

## Checklist

- [ ] Add `TimeWarp.Nuru.DevCli` (≥ 3.0.0-beta.76, match the Nuru version already in `Directory.Packages.props`) to central package versions and reference it from `tools/dev-cli/Directory.Build.props`
- [ ] Drop the local `endpoints/clean-command.cs`, `check-version-command.cs`, and `self-install-command.cs` in favour of the DevCli content copies (or exclude them from `Compile` as terminal/nuru do); add `<Using Include="DevCli" />`
- [ ] `dev --help` lists `release`; `dev release --dry-run` on master evaluates all 8 guards read-only and passes
- [ ] Rewrite `endpoints/workflow-command.cs` so `--mode release` runs the promote pipeline (tag-gate → check-version → locate-run → download-artifact → verify → push) instead of rebuilding; keep `pr` and `merge` modes as `clean → build → verify-samples → test`
- [ ] Update `.github/workflows/workflow.yml`: add `mode: release` + `confirm: release` break-glass inputs with the validate step; release event and break-glass both call `dev workflow --mode release --api-key …`; pass `GH_TOKEN: ${{ github.token }}` so locate-run/download-artifact can read Actions artifacts; `--file` on `dotnet run`
- [ ] Upload-artifacts and keep-last-two conditions still skip PRs, release, break-glass, and probe
- [ ] Add `documentation/developer/guides/releasing.md` describing this repo's instantiation (copy the nuru guide's structure; state only what amuru actually implements)
- [ ] `dev workflow --mode merge` locally green; `ganda repo audit` clean
- [ ] Check-version keeps working under the new pipeline: props `<Version>` vs newest tag, refuses when equal
- [ ] PR open; after merge, prove it end to end on the next real release (do not cut a test release for this task)

## Session

- Created: 2401810 (2026-09-23)

## Notes

- Do not bump `<Version>` on this task; v1.1.0 was released from task 115. The first `dev release` under the new pipeline happens when the next version bump merges.
- Trusted publishing is already configured for this repo + `workflow.yml` (v1.1.0 published through OIDC), so no NuGet.org policy work is expected. `gh workflow run workflow.yml -f mode=probe` confirms.
- The Packages-* artifact for the tag commit must come from a green master push run; the release step never rebuilds. Keep `retention-days: 7` and the keep-last-two prune.
- Skill reference: `tw-release` in timewarp-flow (`claude/skills/tw-release/SKILL.md`) is the operator procedure this repo must satisfy.
