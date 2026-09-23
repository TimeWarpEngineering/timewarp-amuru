# Releasing TimeWarp.Amuru

How this repo cuts and publishes a release: the version SSOT, the normal flow from
version bump to NuGet, every gate the pipeline enforces and what to do when one
refuses, the break-glass path, and the trusted-publishing setup. A cold reader
should be able to execute a release from this document alone.

This guide documents **this repo's instantiation** of the org-wide TimeWarp
versioning/release convention (developed under timewarp-nuru kanban task 458 and
operated through the `tw-release` skill). It does not restate the convention itself,
only what is actually implemented here.

## Overview

- **Version SSOT:** `<Version>` in `source/Directory.Build.props`. There is no
  MinVer, no CI-injected version, and no per-project version overrides. The release
  tag is always `v{Version}`.
- **One version, lockstep.** The repo ships `TimeWarp.Amuru` (core) and
  `TimeWarp.Amuru.Tools`, and both share that single version and release together.
  Never add a `<Version>` to a csproj; a project that must not ship sets
  `IsPackable=false` with a stated reason.
- **Packable set is derived, not listed.** `IsPackable` and `PackageId` are read by
  real MSBuild evaluation (`dotnet msbuild -getProperty:IsPackable,PackageId`) over
  every `*.csproj` under `source/`. Adding or removing a packable project changes
  what ships automatically. Each project's effective `<Version>` is evaluated the
  same way only so the release pipeline can refuse a project that drifts from the
  props version.
- **Publish-state gate covers every packable project.** There is no per-repo
  package scope; `check-version` and `dev release` judge the whole derived set at
  the props version.
- **Humans type a version exactly once**, in the props-bump PR. The tag, the GitHub
  Release, every gate and the NuGet push derive from that one value.
- **Prerelease versions go through the same pipeline** with identical guards.

## Normal release flow

### 1. Bump the version and merge to master

Open a PR that changes `<Version>` in `source/Directory.Build.props` (it may ride a
feature PR or stand alone). Once it merges, the `push` event runs `workflow.yml` in
**merge** mode: `dev workflow` executes `clean → build → verify-samples → test`
(`GeneratePackageOnBuild` produces the nupkgs during Build). On success the
`Packages-{run_number}` artifact with every `artifacts/packages/*.nupkg` is uploaded.
Upload is green-`master` only, not PRs, failed runs, probe, release or break-glass
runs, with `if-no-files-found: error`. Older `Packages-*` artifacts are then deleted,
keeping the two newest (`retention-days: 7` is the backstop). This is the CI-tested
artifact a later release promotes; nothing is published yet.

Every `master` push runs CI (the `push` trigger has no paths filter) so that the
commit `dev release` tags always has a run to promote from. Pull requests keep the
paths filter.

### 2. Cut the release with `dev release`

`dev release` (from the `TimeWarp.Nuru.DevCli` package, `release-command.cs`) reads
`<Version>` from `source/Directory.Build.props` and runs eight guards, in order,
before creating anything. Guards 2–8 each print a `✓` line on success; the first
failure aborts with an operator-facing reason and a nonzero exit code:

1. **Props version readable** from `source/Directory.Build.props`.
2. **`gh` available and authenticated** (`gh auth status`).
3. **Working tree clean** (`git status --porcelain` is empty).
4. **On master** (not a feature branch, not detached `HEAD`).
5. **In sync with `origin/master`** after `git fetch origin master`: zero ahead,
   zero behind. Ahead-only means push first; behind-only means pull first; both
   nonzero is a divergence to reconcile by hand.
6. **Tag `v{Version}` available** locally and on origin. If it exists, resume from
   that commit via break-glass, or bump the version.
7. **Publish-state gate** over the derived packable set (`TimeWarp.Amuru` and
   `TimeWarp.Amuru.Tools`) at the props version: **None** published passes; **All**
   aborts ("bump the version"); **Partial** also aborts here (an untagged prior
   push; resume via break-glass from the original commit, or bump).
8. **A successful CI run of `workflow.yml` exists at `HEAD`**; otherwise the
   release event would fail at locate-run anyway.

Run `dev release --dry-run` first: every guard is evaluated read-only (even the
`git fetch origin master`) and the exact tag/push/`gh release create` commands are
printed without creating anything.

With guards passed and not `--dry-run`, `dev release`:

1. Creates an **annotated** tag `v{Version}` at the verified `HEAD` commit.
2. Pushes the tag to `origin`. If the push fails the local tag is deleted and the
   command refuses; nothing was made public.
3. Runs `gh release create v{Version} --title v{Version} --generate-notes --verify-tag`.
   If this fails after the tag was pushed, the tag is **not** rolled back and
   re-running `dev release` refuses at guard 6 by design; the command prints the
   exact `gh release create` invocation to retry by hand.

**Stale-binary gotcha:** `bin/dev` is an AOT snapshot. If `release` is missing from
`dev --help`, run `dev self-install`, or use
`dotnet run --file tools/dev-cli/dev.cs -- release`, which always compiles fresh.

### 3. `release: published` runs the promotion pipeline

Publishing the GitHub Release fires the `release` event; `workflow.yml` runs
`dev workflow --mode release --api-key …`. This pipeline does **not** rebuild
anything; it promotes the exact bytes CI already built and tested in step 1:

`tag-gate → check-version → locate-run → download-artifact → verify → push`

**Step 1/6 — Release Gate (tag assertions).** Three checks, each a distinct
failure class:

- *Tag assertion* (only on a real `release` event): `GITHUB_REF_NAME` must equal
  `v{Version}` from props exactly. Refusal: **"release tag does not match source
  version"**.
- *Tag pin*: if tag `v{Version}` exists as a local ref, `HEAD` must be at that tag's
  commit. Refusal: **"tag pin mismatch"**; a resume is running from a different
  commit than the one that was tagged.
- *Ancestor-of-master*: `HEAD` must be reachable from `origin/master` (or `master`)
  via `git merge-base --is-ancestor`. Refusal: **"commit not on master"** or
  **"master ref unresolvable"** (a shallow checkout; `workflow.yml` uses
  `fetch-depth: 0`).

This repo does not run an attestation gate at release time.

**Step 2/6 — Check Version.** Runs the standalone `check-version` gate on the
derived packable set: `None` and `Partial` proceed, only `All` aborts with
**"version already released"**. Then the packable set is derived and each
project's effective `<Version>` is evaluated; an empty set aborts **"no packable
projects found"**, an unevaluable version aborts **"package version unresolvable"**,
and any project whose evaluated version differs from the props version aborts
**"package version differs from props version"** (a stray per-csproj `<Version>`;
remove it, or set `IsPackable=false` with a stated reason). The log prints the set,
for example `Packable set (2) at 1.1.1: TimeWarp.Amuru, TimeWarp.Amuru.Tools`.

**Step 3/6 — Locate CI Run.** Resolves `HEAD` and asks
`gh run list --workflow workflow.yml --commit <sha> --status success` for
candidates. `pull_request` runs are excluded outright; a `push` run is preferred
over a `release` run at the same sha, then newest first. Refusals: **"gh CLI
unavailable"**, **"gh run list failed"** (stderr surfaced), **"no successful CI run
found"** (fix and `gh run rerun <id>`).

**Step 4/6 — Download Artifact.** Walks the candidates for a non-expired
`Packages-*` artifact and downloads it into `artifacts/packages/` (cleared first).
Refusals: **"every candidate CI run's Packages-* artifact has expired"** or **"no
candidate CI run … uploaded a Packages-* artifact"**; both are fixed by
`gh run rerun <run-id>`, which rebuilds the same commit.

**Step 5/6 — Verify Package Set.** The downloaded `.nupkg` file names must equal
`{PackageId}.{Version}.nupkg` for every packable project at the props version. Any
missing or unexpected file aborts **"downloaded package set does not match derived
packable set"**; typically the CI run predates the version bump.

**Step 6/6 — Push.** Pushes each verified `.nupkg` to nuget.org with
`dotnet nuget push --skip-duplicate` using the short-lived OIDC key.
`--skip-duplicate` makes a resumed push idempotent at the HTTP layer for packages
already published; it does not verify byte identity, which comes from the tag-pin
check in Step 1. A push failure fails the pipeline. After a successful push the pipeline dispatches a
`repository_dispatch` to `timewarp-software` so the site rebuilds; that dispatch is
best effort and never fails a release (the site rebuilds nightly).

Success prints `Pipeline SUCCEEDED - Packages published to NuGet.org`.

## Why promotion, not rebuild

The published bytes are byte-identical to the bytes the merge-CI run built, ran
`verify-samples` and `test` against, and uploaded. There is no rebuild at release
time and no chance of SDK drift between merge and release changing the output.
"Published" and "tested" are the same artifact by construction.

## Break-glass

A bare `workflow_dispatch` (default inputs) runs in **merge** mode and never
publishes. Publishing via dispatch requires two inputs together: `mode: release`
**and** `confirm: release` (typed exactly). The "Validate break-glass confirmation"
step fails the run immediately if `mode` is `release` but `confirm` does not match.
Only when both are set does the OIDC NuGet login run and the workflow invoke
`dev workflow --mode release --api-key …`.

**When to use it:** recovering or resuming a release that did not complete, for
example the `release: published` webhook never fired, or the promotion pipeline
aborted partway (expired artifact after a fix, transient `gh` failure, partial
NuGet push).

**Tag-pin forces a same-commit resume.** Because `dev release` creates and pushes
the tag before anything is published, any resume must run from the commit the tag
points at; the tag-pin check aborts otherwise with "tag pin mismatch".

### Partial-publish resume

If an earlier attempt pushed some but not all packages under one version (for
example core landed and the run failed before the Tools push), `check-version`
reports `Partial` and the run resumes; `--skip-duplicate` no-ops the
already-published package and pushes the rest. This is safe only from the same
commit as the original push, which the tag-pin check guarantees when the release
was cut through `dev release`. If the source has changed since the partial push,
bump the version instead of resuming.

## Trusted publishing

NuGet credentials are never stored as a repository secret. `workflow.yml` grants
the job `id-token: write` and, only on a `release` event or a confirmed break-glass
dispatch, runs `nuget/login@v1` with `user: TimeWarp.Enterprises`. The OIDC token
is exchanged for a short-lived NuGet API key
(`steps.nuget-login.outputs.NUGET_API_KEY`) passed to `dev workflow --api-key …`.
The policy authorizing this (repo + `workflow.yml` + package) lives in NuGet.org's
Trusted Publishing settings, not in this repo.

### Verifying a policy without releasing: the probe

```bash
gh workflow run workflow.yml --repo TimeWarpEngineering/timewarp-amuru -f mode=probe
```

Runs only the `nuget/login` OIDC exchange and stops: no build, no publish. Success
proves an active trusted-publishing policy matches this repo and `workflow.yml`;
failure at the login step means the policy is missing or misconfigured.

## Operator / maintenance appendix

- **Tokens in the release job.** `GH_TOKEN` is the job token so locate-run and
  download-artifact can read this repo's Actions artifacts (`actions: write` is
  granted for the keep-last-two prune). The cross-repo rebuild dispatch uses a
  separate short-lived GitHub App installation token passed as
  `REBUILD_DISPATCH_TOKEN` (org variable `REBUILD_APP_ID`, org secret
  `REBUILD_APP_PRIVATE_KEY`); when the app is not configured the dispatch degrades
  to a warning.
- **Artifact retention.** Green `master` CI uploads `Packages-{run_number}` with
  `retention-days: 7` and then keeps only the two newest. If the artifact for a SHA
  is gone, `gh run rerun <run-id>` regenerates it from the same commit. Do not pack
  at release time.
- **No per-package releases.** There is one version and one release for both
  packages. A change that touches only Tools still ships as a props bump that
  releases core and Tools together at the new version; a project that must not
  ship sets `IsPackable=false` with a stated reason rather than carrying its own
  `<Version>`.
