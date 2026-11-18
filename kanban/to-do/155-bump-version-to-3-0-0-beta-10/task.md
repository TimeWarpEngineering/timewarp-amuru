# Bump version to 3.0.0-beta.10

## Description

Master carries the builder options from tasks 153 and 154, but
`source/Directory.Build.props` still says `3.0.0-beta.9`, which is already
published. Change `<Version>` to `3.0.0-beta.10` so `dev release` can ship them.

Change that one line only. No other product edits.

Make no hand commits on the branch after the bump commit: an unattested branch
tip becomes the merge tree and fails the release job's attestation gate.

## Checklist

- [ ] `<Version>` in `source/Directory.Build.props` is `3.0.0-beta.10`
- [ ] `./bin/dev build` succeeds
- [ ] `./bin/dev check-version` reports beta.10 as unpublished

## Session

- Created: 93a23df6-1180-44ac-9a03-f537cd480249 (2026-11-18)

## Notes

Steve 2026-11-18 "release it". The release itself (`dev release`) is cut by the
dispatching session after this merges and master CI is green; it is not part of
this task.
