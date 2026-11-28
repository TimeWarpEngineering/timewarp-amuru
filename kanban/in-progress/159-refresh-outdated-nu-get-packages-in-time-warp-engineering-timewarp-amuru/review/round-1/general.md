# Round 1 — general
**Date:** 2026-11-28
**Scope reviewed:** branch `task/159-refresh-outdated-nu-get-packages-in-time-warp` vs `master` (commit `caefa23`, plus kanban commits)

## Summary

The change bumps one library pin (`TimeWarp.Ramani` `1.0.0-beta.25` → `1.0.0-beta.26`) in
`Directory.Packages.props`, sets `<Version>` to `3.0.0-beta.11` in `source/Directory.Build.props`,
and promotes every `PublicAPI.Unshipped.txt` line into `PublicAPI.Shipped.txt` on both packable
projects. Risk is low. No issues found.

## Checks performed

- Goal restatement vs task: matches. The task asks for the Ramani bump and the version bump; the
  restatement adds the PublicAPI promotion that a version-bump PR carries and keeps self pins on
  the published `3.0.0-beta.10`. No misread.
- `Directory.Packages.props`: only the Ramani line changed. Self pins
  (`TimeWarp.Amuru`, `TimeWarp.Amuru.Tools`) stay at `3.0.0-beta.10`, as the restatement says.
- `source/Directory.Build.props`: `<Version>3.0.0-beta.11</Version>` with a `-beta.N` suffix.
- PublicAPI promotion is a pure move. For each project I compared the sorted non-blank lines of
  master's `PublicAPI.Unshipped.txt` (minus `#nullable enable`) with the sorted lines this branch
  adds to `PublicAPI.Shipped.txt`: 109/109 identical (`timewarp-amuru`) and 321/321 identical
  (`timewarp-amuru-tools`). No Shipped lines were removed and no duplicate lines exist in either
  Shipped file. Both Unshipped files are now only `#nullable enable`.
- No other file in the repo (outside kanban and PublicAPI) pins Ramani or the Amuru version, so
  nothing was left behind.
- Recorded gates in `## Results` / `### How to validate` cover the change: build (0 warnings,
  beta.11 nupkgs), the four Ramani-backed runfiles (command-completion and JSON-RPC session
  machine / client), `./bin/dev check-version`, and `ganda repo audit` (exit 0, one pre-existing
  advisory warning).

Commands run (read-only): `git diff master...HEAD`, sorted-line `diff` of the PublicAPI move,
`rg` for other version references, `git merge-tree` against master.

## Issues

None.

## Notes (not findings)

- `git merge-tree --write-tree HEAD master` reports an add/add conflict on this task's
  `task.md` only: master's two `land kanban 159` commits added a short copy of the card while
  this branch added the full card. Product files merge cleanly. The kitchen is host-owned, so the
  host land step resolves this; it is not a product finding.
