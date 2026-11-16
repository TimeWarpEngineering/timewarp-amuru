# Adopt the shared pull request build classifier and the one-workflow CI standard

## Description

Bring timewarp-amuru to the org CI standard (ganda 476 / 479 / 565 / 587, audited by ganda 655): `workflow.yml` has
no `paths` / `paths-ignore` filter on `pull_request`, so every pull request reports `CI`, and `dev workflow`
calls the shared classifier `PullRequestBuildClassifier` from `TimeWarp.Nuru.DevCli` 4.0.0-beta.15
(timewarp-nuru 595) to skip build and test on a pull request that touches no build path (kanban, docs, ...).

Tracked by timewarp-ganda 656 (architecture reposet rollout).

Survey 2026-11-16 for this repo: has a `pull_request` paths filter; `dev workflow` does not classify. Work: remove the filter, add the classifier call.

## Requirements

1. Run `ganda repo audit --fix` first and commit what it changes.
2. Bump the `TimeWarp.Nuru.DevCli` pin to `4.0.0-beta.15` (and `TimeWarp.Nuru` to the same version if pinned).
3. In the `workflow` command's pull-request path, call `PullRequestBuildClassifier.DecideAsync` first. On skip,
   print its message and exit 0. Push and release paths are unchanged.
4. Remove `paths` / `paths-ignore` from the `pull_request` trigger of `.github/workflows/workflow.yml`. Keep any
   `push` filter and add a comment saying why (a kanban-only merge must not rebuild and upload packages).
5. If this repo builds from roots outside the classifier defaults (`source`, `tests`, `tools`, `msbuild`), declare
   them through the classifier's config (`PullRequestBuildConfig`), not by copying or editing the rule.
6. No local copy of the classification rule remains in `tools/dev-cli/`.
7. `ganda repo audit` passes, including both CI-standard checks from ganda 655.

## Goal (implementer restatement)

Every pull request into this repo must report a `CI` check, including kanban-only and docs-only ones, so
`ganda pr merge` never needs `--allow-no-ci`. To get there, `workflow.yml` drops its `pull_request` paths filter.
`dev workflow` then decides in C# whether to build. It does this with the shared
`PullRequestBuildClassifier.DecideAsync` from `TimeWarp.Nuru.DevCli` 4.0.0-beta.15, not with a local copy of the rule.

Acceptance:
- `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` are pinned at 4.0.0-beta.15.
- The pr/merge path of `dev workflow` calls the classifier first. On skip it prints the message and exits 0.
  The release path is unchanged.
- `pull_request` has no `paths` filter. `push` keeps its existing comment and has no filter, as before.
- Paths that must build but that the default rule calls non-build go in `.timewarp/dev.jsonc` `pullRequestBuild.buildPaths`.
  These are `readme.md` and `assets/logo.png` (both packed) and `samples` (built by verify-samples).
- `ganda repo audit` passes, including `workflow-file` and `dev-workflow-classifier`.

## Checklist

- [x] `ganda repo audit --fix` run and committed (no changes; only the two CI-standard checks failed)
- [x] DevCli pin at 4.0.0-beta.15
- [x] `dev workflow` calls the shared classifier on pull requests; skip exits 0 with its message
- [x] No `paths` filter on `pull_request`; `push` has no filter (existing why comment kept)
- [x] Extra build roots (if any) declared through config
- [x] `ganda repo audit` clean
- [ ] This task's pull request shows a `CI` run (record the URL in Results)

## Results

- `Directory.Packages.props`: `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` 4.0.0-beta.14 → 4.0.0-beta.15.
- `tools/dev-cli/endpoints/workflow-command.cs`: the pr/merge path first calls
  `PullRequestBuildClassifier.DecideAsync(repoRoot, ct)` and prints its message. On skip it returns with exit 0.
  The release path is unchanged. The Design region documents the classifier step.
- Line cap: adding the classifier call took the file past its 867-line baseline (TWA0038). The release-path
  outcome types (records and enums) moved to `workflow-command.outcomes.cs`, and `WorkflowCommand` and `Handler`
  became `partial`. The main file is now 839 lines. No behaviour changed.
- `.github/workflows/workflow.yml`: the `pull_request` `paths` list is gone and a comment explains why.
  `push` had no filter before and still has none. Its comment says every master commit needs a Packages-* artifact
  for the release promotion. That is why this repo does not filter `push` the way the task text suggested.
- `.timewarp/dev.jsonc` (new): `pullRequestBuild.buildPaths` = `readme.md` and `assets/logo.png` (both packed into
  the nupkgs) and `samples/**` (compiled by verify-samples). The classifier rule is not copied locally.
- Review round 1 fixes: M1, `documentation/developer/guides/releasing.md` no longer says pull requests keep
  the paths filter. It now says `pull_request` has no paths filter, every pull request reports `CI`, and
  `dev workflow` skips build and test only when `PullRequestBuildClassifier` finds only declared non-build
  paths (extra build paths in `.timewarp/dev.jsonc`). M2, the dev CLI was compiled through
  `dotnet run --file tools/dev-cli/dev.cs` (the CI path) and both smokes are recorded under `### Gates`.
- Boy-scout fix: the post-commit attest failed `package-pin-currency` because the self pins `TimeWarp.Amuru` and
  `TimeWarp.Amuru.Tools` were at 3.0.0-beta.4 (only the `.githooks` runfiles use them). Both are now 3.0.0-beta.5.
- `ganda repo audit --fix` at the start changed nothing. It reported only `workflow-file` and
  `dev-workflow-classifier`, which this task fixes.

### Gates

| Gate | Command | Result |
|------|---------|--------|
| Build | `./bin/dev build` | pass (0 warnings, 0 errors) |
| Test | `./bin/dev test` | pass: 879 total, 878 passed, 1 skipped |
| Audit | `ganda repo audit` | pass: all checks, including `workflow-file` and `dev-workflow-classifier` |

Fix pass for review round 1 (2026-11-17), re-run after the M1/M2 edits:

| Gate | Command | Result |
|------|---------|--------|
| Dev CLI compile + skip smoke (as CI) | `GITHUB_EVENT_NAME=pull_request GITHUB_EVENT_PATH=<e9200fe..9ff24c1> dotnet run --file tools/dev-cli/dev.cs -- workflow` | compiled; printed `Pull request changes only declared non-build paths. Skipping build and test. The rule is PullRequestBuildClassifier (TimeWarp.Nuru.DevCli). (1 changed paths)`; `exit=0` |
| Dev CLI build-path smoke (as CI) | same, event `8b95c33^1..8b95c33` | printed `Build path changed: .github/workflows/workflow.yml. Full build.`, ran clean → build → verify-samples → test (879 total, 878 passed, 1 skipped), `Pipeline SUCCEEDED`, `exit=0` |
| Build | `./bin/dev build` | pass (0 warnings, 0 errors) |
| Test | `./bin/dev test` | pass: 879 total, 878 passed, 1 skipped |
| Audit | `ganda repo audit` | pass: "Repository passes all audit checks.", exit 0 |

### How to validate

Smoke (skip path, kanban-only diff). This compiles the dev CLI the way CI does (`workflow.yml` runs
`dotnet run --file tools/dev-cli/dev.cs -- workflow`); `./bin/dev workflow` is the cached local binary:

```bash
printf '{"pull_request":{"base":{"sha":"%s"},"head":{"sha":"%s"}}}' $(git rev-parse e9200fe) $(git rev-parse 9ff24c1) > /tmp/ev.json
GITHUB_EVENT_NAME=pull_request GITHUB_EVENT_PATH=/tmp/ev.json dotnet run --file tools/dev-cli/dev.cs -- workflow; echo "exit=$?"
```

Expect: `Pull request changes only declared non-build paths. Skipping build and test. ... (1 changed paths)` and `exit=0`.

Smoke (build path): use base `8b95c33^1` and head `8b95c33` in the same command.
Expect: `Build path changed: .github/workflows/workflow.yml. Full build.` followed by the clean → build → verify-samples → test pipeline.

Run locally, `./bin/dev workflow` prints `No GITHUB_EVENT_NAME (local run). Full build.` and then builds.

CI: this task's pull request should show a `CI` run that builds, because it changes `tools/` and `.github/workflows`.
The host's open-pr node opens that PR, so its URL goes here after that node runs.

### Review

Round 1 (effort 2, roster general) is open. Counts: bug 2 open, suggestion 0, nit 0. Disposition is not written; M1 and M2 are open. Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.

## Review

- Round 1: Blocked — open M1, M2

## Notes


After merge, the ganda 656 tracker proves a kanban-only pull request here reports `CI` success and merges with
plain `ganda pr merge` (no `--allow-no-ci`).

## Learnings

- Misread (wording): Requirement 4 says to keep any `push` filter and add a comment that a kanban-only merge must not rebuild and upload packages. The restatement says `push` keeps its existing comment and has no filter. This repo's `push` trigger already had no paths filter. Its comment says every master commit needs a `Packages-*` artifact so `dev release` can promote it. The implementation kept that comment. The template sentence describes the opposite policy from this repo's release promotion. Requirement 3 says the pull-request path calls the classifier and the push path stays unchanged. The restatement puts the call on the pr/merge path. `PullRequestBuildClassifier.DecideAsync` builds for every event other than `pull_request`, so a push still builds. No product miss on either point.

## Session

- Created: 64677b22-d705-4b25-b537-4b53732bcb92 (2026-11-16)
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 7ebb37dc-78d0-4e0d-868c-d4eb848e9094, max-turns 500, Done — 2026-11-16T18:28:59Z
- Review oracle: round 1 by reviewer-grok (grok, vendor xai, model grok-4.7), session 01a1e59e-bed5-7411-81e2-eaa624aaa5eb, max-turns 120, effort 2, roster general — 2026-11-17
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1e59e-bed5-7411-81e2-eaa624aaa5eb, max-turns 120, artifacts general.md, merged.md — 2026-11-16T18:38:12Z
