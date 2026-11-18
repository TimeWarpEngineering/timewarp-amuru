# Bump version to 3.0.0-beta.10

## Description

Master carries the builder options from tasks 153 and 154, but
`source/Directory.Build.props` still says `3.0.0-beta.9`, which is already
published. Change `<Version>` to `3.0.0-beta.10` so `dev release` can ship them.

Change that one line only. No other product edits.

Make no hand commits on the branch after the bump commit: an unattested branch
tip becomes the merge tree and fails the release job's attestation gate.

## Goal (implementer restatement)

Change `<Version>` in `source/Directory.Build.props` from `3.0.0-beta.9` (already
published) to `3.0.0-beta.10` so the builder options from tasks 153 and 154 can ship.

Acceptance: that one line is the only product edit; `./bin/dev build` passes;
`./bin/dev check-version` shows 3.0.0-beta.10 as not yet published; `ganda repo audit` passes.

## Checklist

- [x] `<Version>` in `source/Directory.Build.props` is `3.0.0-beta.10`
- [x] `./bin/dev build` succeeds
- [x] `./bin/dev check-version` reports beta.10 as unpublished

## Session

- Created: 93a23df6-1180-44ac-9a03-f537cd480249 (2026-11-18)
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session a2bee1e1-1468-47a2-95c8-e3d2a1982976, max-turns 500, Done — 2026-11-18T09:16:36Z
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7), session 01a1edf1-b9e5-7a13-a8a3-0bea78e54917, max-turns 80, effort 1, roster general, artifacts general.md, merged.md, disposition.md — 2026-11-18
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1edf1-b9e5-7a13-a8a3-0bea78e54917, max-turns 80, artifacts general.md, merged.md, disposition.md — 2026-11-18T09:23:07Z

## Results

`source/Directory.Build.props` `<Version>` changed `3.0.0-beta.9` → `3.0.0-beta.10`.
No other product edits.

Gates:

- `./bin/dev build` — succeeded, 0 warnings, 0 errors; packages built as 3.0.0-beta.10.
- Test files — none added or edited; the change is an MSBuild property with no type under
  test, so no covering test file applies. CI runs the full suite on the PR.
- `./bin/dev check-version` — source 3.0.0-beta.10, latest NuGet 3.0.0-beta.9, "Version in
  source is new — safe to release."
- `ganda repo audit` — passes (exit 0), one advisory warning: `vscode-window-icon`
  (`.vscode/settings.json` missing). Pre-existing and non-blocking; left alone because the
  task allows only the version line as a product edit.

### How to validate

Smoke: `grep -n '<Version>' source/Directory.Build.props && ./bin/dev check-version`

Expect: `<Version>3.0.0-beta.10</Version>` and "Version in source is new — safe to release."

### Review

- Rounds: 1. Effort 1, roster: general (review oracle, Grok 4.7).
- Final counts: bug 0, suggestion 0, nit 0. None open, fixed, or wontfix.
- Disposition: **clean**.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.
- The reviewer ran no build, test, or audit. It reviewed the diff from merge-base `15ab788` to tip `2ff481b` and the recorded results above. `git notes --ref=refs/notes/ganda-audit show` on the bump, the tip, and the merge-base found no note.

## Learnings

Misread: the implementer restatement adds `ganda repo audit` to acceptance. The original checklist does not. The restatement also leaves out the description's rule against a hand commit after the bump commit. The product edit still matches the task: `<Version>` in `source/Directory.Build.props` is the only product change.

## Review

- Round 1: Done

## Notes

Steve 2026-11-18 "release it". The release itself (`dev release`) is cut by the
dispatching session after this merges and master CI is green; it is not part of
this task.
