# Round 5 — general
**Date:** 2026-11-09
**Scope reviewed:** fix delta `93204a3..e33d046`. That is commit `013b1cf` (zero grace keeps the child's exit code when the deadline cuts off a descendant), the kanban record `4451757`, and `e33d046` (executable bit on the new runfile). Product files: `command-result.cs`, `command-result.timeout.cs`, `command-execution.md`, `3.0.0-beta.2.md`, `shell-builder.with-timeout.cs`, and the new `shell-builder.with-timeout.descendants.cs`.

## Summary

The delta closes M5 with fix option (a) from round 4. With a zero grace period, `TimeoutSession.NoteChildExit` now sets `ChildExitedBeforeDeadline` when the direct child exits before the graceful signal. It skips `CancelAfter`, so the deadline stays at `Timeout`. The child-code catch in `ExecuteCliAsync` now also matches on `ChildExitedBeforeDeadline`. Zero grace starts two separate timers at the same instant, and that match covers the case where the forceful timer fires before the graceful one. An exit at or after the zero-grace signal, and a child that is still running, stay exit 124. The Design regions, the reference table, the release note, and the Goal restatement now describe the zero-grace rule the same way. The capture test file had crossed the 500-line cap, so the descendant probes moved to a new file. That file has its executable bit set (mode 100755). I found no new defects.

## Verification

- `shell-builder.with-timeout.descendants.cs`: 3/3 passed. The zero-grace descendant case took 1.18s, the original-deadline case 2.21s, and the drain-bound case 3.18s.
- `shell-builder.with-timeout.cs`: 16/16 passed. The slow-reader grace test took 7.08s.
- `shell-builder.with-timeout.modes.cs`: 14/14 passed. This file covers the zero-grace ignored-SIGINT kill.
- `command-result.concurrency.cs`: 5/5 passed. This file covers a zero-grace timeout under concurrency.
- Probes I ran against this branch with timeout 1s and grace 0:
  - `StreamStdoutAsync` on `sleep 15 & echo started`: ended in 1.1s, yielded `started`, exit 0, `TimedOut` false.
  - Strict `CaptureAsync` on the same command: ended in 1.1s, exit 0, `TimedOut` false, no throw.
  - `sleep 15`, where the child is still running at the deadline: ended in 1.3s, exit 124, `TimedOut` true.
- Line counts: `command-result.cs` is 1339 lines, which matches the baseline. `command-result.timeout.cs` is 424, `shell-builder.with-timeout.cs` 442, and the new descendants file 120.
- Goal restatement compared with the original task: the restatement covers every Requirement and the rolled-in 3.0.0-beta.2 bump. The descendant and drain-bound paragraphs came from the review fix loop (M3–M5). They are not a misread of the task, so I recorded no misread.

## Issues

<!-- None. -->
