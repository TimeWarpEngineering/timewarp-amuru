# Pre-1.0 API polish sweep

## Description

Small consistency/hygiene items from the release review that are individually minor but breaking (or embarrassing) to fix after 1.0. Sweep them in one pass after the structural tasks (090-094) land.

## Checklist

### Naming/docs consistency
- [ ] `core/shell.cs:25`, `core/shell-builder.cs:18,25` — stale XML docs still say "RunBuilder" (pre-rename of `ShellBuilder`)
- [ ] `CliConfiguration.cs:10` — stale "TimeWarp.Cli" doc text
- [ ] `ShellBuilder.WithArguments(params string[])` non-nullable vs `CommandResult.Pipe(params string[]?)` nullable — inconsistent nullability on the same concept
- [ ] `core/output-line.cs` — `OutputLine` is a class without value equality; natural `record`; `Timestamp` should be `DateTimeOffset` (task 092 touches this too); `TtyPassthroughAsync` uses local `DateTimeOffset.Now` vs CliWrap's UTC timing
- [ ] Package metadata polish: add `PackageProjectUrl`, richer description (`timewarp-amuru.csproj:4-6`); standardize readme filename casing (csproj says `README.md`, disk has `readme.md`, props says `readme.md`)

### Repo hygiene
- [ ] 88 files carry identical `// TODO: Add purpose description` region stubs (agent-context-regions boilerplate) — backfill or strip before 1.0; visible via SourceLink/embedded sources
- [ ] Zero functional TODO/HACK/FIXME markers otherwise (verified) — keep it that way

## Notes

Found by multi-agent release review (2026-07-04). Verified good and needing no change: `Async` suffix consistency on Task-returning methods, execution verb taxonomy (`RunAsync`/`CaptureAsync`/`RunAndCaptureAsync`/`PassthroughAsync`/`SelectAsync`), `NuGetCacheType` enum completeness, license file + Unlicense expression match, icon/readme packed correctly.
