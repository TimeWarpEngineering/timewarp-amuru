# Upgrade to .NET 11

## Description

Bump TimeWarp.Amuru from .NET 10 to .NET 11 across TFMs, central package versions, SDK pin, CI, docs, and analyzer stack. Do not start product-code work until .NET 11 is GA (or an explicit go-live RC decision is recorded). This task is the implementation kitchen; analysis below reflects master as of 2026-10-02.

As of ~2026-10-02: .NET 11 is **RC1** (`11.0.0-rc.1`, go-live license, 2026-09-08). **GA scheduled 2026-11-10** (STS, support roughly Nov 2026 through Nov 2028). Prefer waiting for GA unless product needs an early RC soak.

## Requirements

- Bump all project TFMs from `net10.0` to `net11.0` (no multi-TFM today; keep single TFM unless product decides otherwise).
- Bump `global.json` SDK from `10.0.301` to a .NET 11 GA (or approved RC) SDK; keep `rollForward: latestFeature` unless pin policy changes.
- Update Central Package Management (CPM) packages that pin analyzers / runtime-adjacent tooling for .NET 11 compatibility.
- Ensure CI (`.github/workflows/workflow.yml`) resolves SDK via updated `global.json` (`actions/setup-dotnet@v4` already uses `global-json-file`); verify `ubuntu-latest` runners can install SDK 11.
- Update docs that hard-require .NET 10 (`documentation/user/installation.md`, `AGENTS.md`, any README/overview mentions of file-based apps / SDK).
- Keep `TreatWarningsAsErrors` + `AnalysisLevel=latest-all` green; style failures (e.g. IDE0055) historically appear when SDK and analyzer packages drift — expect a coordinated bump.
- Re-run full test suite (`dev test` / multi-file runners) and AOT pack path (`IsAotCompatible=true` on both packages).
- No inventing Depends-on task ids; no known predecessor kanban task for this upgrade on the board as of analysis.
- Product implementation only via ganda on this task worktree; publish kanban first / separately from product commits as usual.

## Checklist

### 1. Toolchains (do first — unlocks local build)

- [ ] Decide RC go-live vs wait-for-GA; record choice in Notes / commit message.
- [ ] Update `global.json` `sdk.version` from `10.0.301` to .NET 11 SDK (GA preferred).
- [ ] Install matching SDK on TWE-001 / agent machines; confirm `dotnet --version`.
- [ ] Confirm `LangVersion=latest` + `AnalysisLevel=latest-all` + `EnableNETAnalyzers=true` behave under SDK 11 (root `Directory.Build.props`).
- [ ] Review `.editorconfig` experimental / style rules for SDK 11 analyzer changes (known risk: IDE0055 when SDK and analyzer package generation diverge).
- [ ] **Pre-GA soak (can start now on RC1, `11.0.100-rc.1` is installed):** run the full suite on the .NET 11 runtime (`DOTNET_ROLL_FORWARD=Major` or a temporary `global.json`) and watch for behavior changes under CliWrap's output reading: Windows redirected stdio moved to overlapped I/O, macOS spawn moved to posix_spawn, Linux handle inheritance uses close_range. Record pass/fail per OS in Notes.
- [ ] **Decision: replace CliWrap internally with the .NET 11 Process APIs?** .NET 11 adds `Process.Run/RunAsync`, `RunAndCaptureText[Async]`, deadlock-free `ReadAll*`, handle-based stdio redirection, `KillOnParentExit`, `ProcessExitStatus`, `SafeProcessHandle.WaitForExitOrKillOnCancellationAsync`, `Signal(PosixSignal)`. Task 091 already removed CliWrap from the public surface, so this is an internal swap. Evaluate against the execution modes in `core/command-result.cs` (capture, stream, pipe, passthrough, select, timeout graceful/forceful from task 044) and the mock layer; decide keep / swap / partial, record in Notes. Not a blocker for the TFM bump; may become its own task.

### 2. TFMs

- [ ] Root `Directory.Build.props`: `net10.0` to `net11.0`.
- [ ] `tools/Directory.Build.props`: same `net10.0` to `net11.0` (tools override TFM explicitly).
- [ ] Confirm `source/timewarp-amuru/timewarp-amuru.csproj` and `source/timewarp-amuru-tools/timewarp-amuru-tools.csproj` inherit TFM (no local override today).
- [ ] Confirm `samples/Directory.Build.props` inherits root TFM (samples only override packability).
- [ ] Confirm file-based tests under `tests/timewarp-amuru/` still run as .NET 11 file-based apps (`dotnet run <file>.cs`).

### 3. Packages (CPM — after TFM / SDK)

- [ ] `Microsoft.CodeAnalysis.NetAnalyzers` `10.0.401` to .NET 11-aligned version.
- [ ] Re-evaluate `Microsoft.CodeAnalysis.CSharp.CodeStyle` `5.9.0` and `Microsoft.CodeAnalysis.BannedApiAnalyzers` `5.6.0` for SDK 11 compatibility.
- [ ] Roslynator trio (`Roslynator.Analyzers` / `CodeAnalysis.Analyzers` / `Formatting.Analyzers`) all at `5.0.0` — bump if needed for SDK 11.
- [ ] `NuGet.Versioning` `7.9.0` — bump if NuGet client line requires it for SDK 11 tooling.
- [ ] External runtime deps: `CliWrap` `3.10.5`, `TimeWarp.Terminal` `1.0.2` — verify net11 restore / AOT.
- [ ] TimeWarp ecosystem pins (may need sibling upgrades first or coordinated releases): `TimeWarp.Jaribu` `1.0.0-beta.15`; `TimeWarp.Nuru` / `TimeWarp.Nuru.DevCli` `3.0.0-beta.76`; `TimeWarp.SourceGenerators` `1.0.0-beta.11`; `TimeWarp.Build.Tasks` `1.0.0`; self pins `TimeWarp.Amuru` `1.0.0`, `TimeWarp.Amuru.Tools` `1.0.0-beta.2` (CPM for githooks runfiles).
- [ ] `Shouldly` `4.3.0` — verify on net11 test host.
- [ ] Keep CPM in `Directory.Packages.props`; do not scatter versions into csproj.

### 4. Workflows / CI images

- [ ] `.github/workflows/workflow.yml`: keep `actions/setup-dotnet@v4` + `global-json-file: global.json` (no hardcoded SDK version today).
- [ ] After `global.json` bump, run a green CI on the task branch; confirm SDK 11 installs on `ubuntu-latest`.
- [ ] No Dockerfiles / container images in-repo to bump (none found).
- [ ] No explicit test matrix of TFMs today (single job) — no matrix expansion required unless multi-TFM is chosen.
- [ ] `dev-cli` path (`dotnet run --file tools/dev-cli/dev.cs -- workflow`) must work under SDK 11.
- [ ] Packages artifact / release promotion path unchanged (verify nupkgs still emit for both packages).

### 5. Docs and agent guidance

- [ ] `documentation/user/installation.md`: ".NET 10 File-Based Apps", "#:package is native to .NET 10", "requires .NET 10", download link to 10.0 — update to .NET 11 / 11.0.
- [ ] `AGENTS.md`: SDK pin note (`global.json` pins SDK 10.0.x; preview SDK fails IDE0055) to 11.0.x; "runnable .NET 10 file-based app" to .NET 11.
- [ ] Spot-check `readme.md` / `overview.md` / skills for leftover net10 language.

### 6. Verification

- [ ] `dotnet build timewarp-amuru.slnx` (warnings-as-errors clean).
- [ ] Full `dev test` / multi-file runners green.
- [ ] Pack both packages; AOT-compat analyzers clean.
- [ ] Manual smoke: file-based script with `#:package TimeWarp.Amuru` on SDK 11.

## Notes

### Current values found (master checkout, 2026-10-02)

| Area | Current |
|------|---------|
| Default TFM | `net10.0` in root `Directory.Build.props` |
| Tools TFM | `net10.0` in `tools/Directory.Build.props` |
| Product csproj TFM | inherited (no override) — TimeWarp.Amuru, TimeWarp.Amuru.Tools |
| Package version | `1.1.1` in `source/Directory.Build.props` |
| SDK pin | `global.json` → `10.0.301`, `rollForward: latestFeature` |
| CI | single job `ubuntu-latest`; `setup-dotnet@v4` + `global-json-file` |
| Containers | none |
| Analyzers (CPM) | NetAnalyzers `10.0.401`; CodeStyle `5.9.0`; BannedApi `5.6.0`; Roslynator `5.0.0` |
| Analysis policy | `AnalysisMode=All`, `AnalysisLevel=latest-all`, `TreatWarningsAsErrors=true` |

### Ordered dependency list (must update first, then later)

1. **Toolchains**: `global.json` SDK 10.0.301 → 11.x; local/CI SDK install; analyzer/style alignment.
2. **TFMs**: `net10.0` → `net11.0` in root + `tools/` Directory.Build.props (samples/source inherit).
3. **Packages**: CPM analyzer pins (especially `Microsoft.CodeAnalysis.NetAnalyzers` 10.0.401) and TimeWarp/ecosystem packages that target net10; then CliWrap / NuGet.Versioning / test deps.
4. **Workflows / CI**: no hardcoded SDK — inherits `global.json`; verify runner + `dev-cli` workflow under 11; no Docker images.
5. **Docs**: installation guide + AGENTS.md net10 requirements.

### Breaking / risk notes

- **.NET 11 Process API (researched 2026-10-07):** purely additive, nothing removed; no `System.Diagnostics.Process` breaking change found; `PosixSignal` gains `SIGKILL` and `SIGTSTP`'s numeric value changed (irrelevant unless hard-coded). Risk is runtime behavior under CliWrap, not source compatibility. PTY support is .NET 12 research only (dotnet/runtime #128565), so `TtyPassthroughAsync` stays as is. Task 083 depends on this task to use the new primitives.

- **STS timeline**: .NET 10 is LTS (support to 2028-11); .NET 11 is STS (~2 years from GA). Upgrading consumers to net11 is a **breaking TFM bump** for library consumers still on net10 — consider whether to multi-target `net10.0;net11.0` temporarily or hard-cut.
- **Analyzer / IDE0055**: AGENTS.md already warns preview SDK vs pin causes style failures; bump SDK and NetAnalyzers/CodeStyle together.
- **AOT**: both packages set `IsAotCompatible=true` — re-validate under net11.
- **File-based apps / `#:package`**: docs market these as .NET 10 features; confirm behavior unchanged on 11.
- **Ecosystem blockers**: `TimeWarp.Nuru`, `TimeWarp.Jaribu`, `TimeWarp.SourceGenerators`, `TimeWarp.Terminal` may need net11 builds before Amuru can restore cleanly — check sibling repos before forcing the bump.
- **No Depends-on**: no existing predecessor task id found on the Amuru board for this upgrade.

### Session

- Created: 506058 (2026-10-02)
- Analysis machine: TWE-001 (WSL), master worktree read-only; kitchen authored in task-119 worktree only.