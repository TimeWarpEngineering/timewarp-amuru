# Fix core engine bugs from release review

## Description

Concrete correctness bugs in the core execution engine found by the 2026-07-04 release review (contract-level decisions are split out to task 090). The worst is a data race that corrupts `StreamToFileAsync` output for any command writing to both stdout and stderr.

## Checklist

### Major
- [x] `StreamToFileAsync` shared-FileStream race fixed: line writes serialized under a lock via delegate pipes (2026-07-05)
- [x] `.cs` execution on Windows now routes through the dotnet host (`dotnet <script.cs> -- <args>`); Unix shebang path unchanged (2026-07-05)
- [x] `ConfigureAwait(false)` applied library-wide and ENFORCED: CA2007 is an error via `source/timewarp-amuru/.editorconfig` (2026-07-05)

### Minor
- [x] `SelectAsync` no longer swallows `OperationCanceledException`; cancellation propagates (2026-07-05)
- [x] `ScriptContext` now supports nesting via an instance stack: each Dispose restores its own directory/onExit innermost-first; static state lock-guarded; XML docs added (2026-07-05)
- [x] `CliConfiguration` documented as process-wide by design (use CommandMock for per-test isolation); stale "TimeWarp.Cli" doc fixed (2026-07-05)
- [x] `GetLines()` family now preserves interior blank lines (trailing newline still produces no empty entry); string-ctor stdout-before-stderr ordering documented (2026-07-05)
- [x] `OutputLines` wrapped in `AsReadOnly()` (2026-07-05)
- [x] `PassthroughAsync` pending-stdin-read caveat documented; TtyPassthroughAsync recommended when child ignores stdin (2026-07-05)

## Notes

Verified against decompiled CliWrap 3.10.1. The classic process deadlock/orphan bugs are absent — CliWrap handles concurrent stream pumping, tree-kill on cancellation, and stdin teardown correctly. Paths relative to `source/timewarp-amuru/`.
