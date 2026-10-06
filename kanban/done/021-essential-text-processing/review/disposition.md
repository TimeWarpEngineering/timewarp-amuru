# Disposition — task 021

**Date:** 2026-10-06
**Outcome:** clean
**Rounds:** 3
**Final open count:** 0

## Summary

Effort 3, general axis. In round 1, two general reviewers (one on correctness, one on tests and docs) raised 11 findings: 4 bugs, 4 suggestions and 3 nits. The bugs were lossy decoding that corrupted non-UTF-8 and binary files, a stray CR that flipped a file's newline style, output lost when an error occurred partway through a walk, and MaxMatches counting matches instead of lines. All 11 were fixed in 1842a0c. The round-2 re-review raised 4 more: 2 suggestions (an encoder exception escaping the per-file handling, and a blank path entry aborting the walk after a write) and 2 documentation nits. All 4 were fixed in b87a9c5. Round 3 verified those fixes and found nothing new. No finding was marked wontfix.

## Exception log

None.

## Escalations

None.
