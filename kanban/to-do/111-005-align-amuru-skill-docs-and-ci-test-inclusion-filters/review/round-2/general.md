# Round 2 — general
**Date:** 2026-09-30
**Scope reviewed:** post-fix diff of `skills/amuru/SKILL.md` and `source/timewarp-amuru-tools/dot-net-commands/dot-net.md` against round-1 M1–M4. No new product scan beyond that delta.

## Summary

M1–M4 are fixed. Fzf samples call `FromInput` / `FromCommand`. The skill names `TimeWarp.Amuru.Tools` for DotNet, Git, and Fzf. The DotNet reference limits `WithProperty` to the builders that define it and includes pack in `RunAndCaptureAsync`. No new findings on the fix delta.

## Issues

### Resolved prior

- M1 — fixed — `skills/amuru/SKILL.md` uses `FromInput` and `FromCommand`. No `WithInputItems` or `WithInputCommand`.
- M2 — fixed — `#:package TimeWarp.Amuru.Tools` is next to core, with a sentence that DotNet, Git, and Fzf are that package.
- M3 — fixed — `WithProperty` is limited to Build, Clean, Restore, Run, Test, Publish, and Pack. The sample stays on `DotNet.Build()`.
- M4 — fixed — `RunAndCaptureAsync` sentence includes pack and names the builders that only have `RunAsync` / `CaptureAsync`.
