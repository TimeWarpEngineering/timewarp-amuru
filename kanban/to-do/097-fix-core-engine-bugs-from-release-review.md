# Fix core engine bugs from release review

## Description

Concrete correctness bugs in the core execution engine found by the 2026-07-04 release review (contract-level decisions are split out to task 090). The worst is a data race that corrupts `StreamToFileAsync` output for any command writing to both stdout and stderr.

## Checklist

### Major
- [x] `StreamToFileAsync` shared-FileStream race fixed: line writes serialized under a lock via delegate pipes (2026-07-05)
- [ ] `core/command-extensions.cs:66-71` — `.cs` file-based-app execution relies on Unix shebang + exec bit; on Windows it throws `Win32Exception` at execution time with no guard or useful error message. Guard + document, or route via `dotnet run` on Windows
- [ ] Library-wide missing `ConfigureAwait(false)` (all awaits in `core/command-result.cs` except line 202, all of `core/shell-builder.cs`, `Installer.cs`) — deadlocks consumers who block on a `SynchronizationContext`

### Minor
- [x] `SelectAsync` no longer swallows `OperationCanceledException`; cancellation propagates (2026-07-05)
- [ ] `ScriptContext.cs:24-29,90-99` — second `ScriptContext` instance is never tracked: its `Dispose()` no-ops (`Current == this` false), so directory restore and `onExit` never run; static `Current` also unsynchronized. Support nesting or throw on second creation
- [ ] `CliConfiguration.cs:14` — command-path overrides are process-global mutable statics (unlike `CommandMock`'s AsyncLocal); parallel tests race. AsyncLocal overlay or document as process-wide. Also stale "TimeWarp.Cli" doc at :10
- [ ] `core/command-output.cs:110-120,140-167` — `GetLines()` uses `RemoveEmptyEntries` (blank lines silently vanish — breaks blank-line-delimited output like `git log` blocks) and the string-based ctor fabricates ordering (all stdout before all stderr). Decide/document/fix
- [ ] `core/command-output.cs:105` — `OutputLines` exposes the private `List<OutputLine>` castable to mutable `List<>`; wrap in `AsReadOnly()`
- [ ] `core/command-result.cs:93-104` — `PassthroughAsync`: child exiting without draining stdin leaves a pending console-stdin read that can swallow the parent app's next keystroke. Investigate/document

## Notes

Verified against decompiled CliWrap 3.10.1. The classic process deadlock/orphan bugs are absent — CliWrap handles concurrent stream pumping, tree-kill on cancellation, and stdin teardown correctly. Paths relative to `source/timewarp-amuru/`.
