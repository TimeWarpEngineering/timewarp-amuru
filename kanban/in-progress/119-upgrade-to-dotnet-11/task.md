# Upgrade to .NET 11

## Description

Bump TimeWarp.Amuru from .NET 10 to .NET 11 across TFMs, central package versions, SDK pin, CI, docs, and analyzer stack. Do not start product-code work until .NET 11 is GA (or an explicit go-live RC decision is recorded). This task is the implementation kitchen; analysis below reflects master as of 2026-10-02.

As of ~2026-10-02: .NET 11 is **RC1** (`11.0.0-rc.1`, go-live license, 2026-09-08). **GA scheduled 2026-11-10** (STS, support roughly Nov 2026 through Nov 2028). Prefer waiting for GA unless product needs an early RC soak.

## Goal (implementer restatement)

Move TimeWarp.Amuru from a single `net10.0` target and the SDK 10.0.301 pin onto a single `net11.0` target and a .NET 11 SDK pin. Keep `rollForward: latestFeature`. Align the central analyzer packages, the docs that require .NET 10, and CI (which already reads `global.json`) with that SDK. Leave the build warnings-as-errors clean, the full test suite green, and both packages packing with AOT compatibility analyzers clean. A file-based script that pulls `TimeWarp.Amuru` with `#:package` must run on that SDK.

Acceptance for the product diff:

- `global.json` names a .NET 11 GA SDK. Root and `tools/` `Directory.Build.props` target `net11.0`. Source and samples inherit that TFM.
- Analyzer and tooling versions stay in `Directory.Packages.props` and match the SDK 11 analyzer generation (the known failure is IDE0055 when they drift).
- `.github/workflows/workflow.yml` still installs the SDK from `global.json`, and the task branch CI is green on `ubuntu-latest`.
- `documentation/user/installation.md`, `AGENTS.md`, and other hard ".NET 10 required" lines say .NET 11.
- `dotnet build timewarp-amuru.slnx`, `dev test`, and a pack of both packages succeed.
- No product diff until .NET 11 is GA, or until this kitchen records an explicit RC go-live. This walk records wait-for-GA.

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

- [x] Decide RC go-live vs wait-for-GA; record choice in Notes / commit message. Decision 2026-10-31: **wait for GA**. No product RC go-live.
- [ ] Update `global.json` `sdk.version` from `10.0.301` to .NET 11 SDK (GA preferred).
- [ ] Install matching SDK on TWE-001 / agent machines; confirm `dotnet --version`.
- [ ] Confirm `LangVersion=latest` + `AnalysisLevel=latest-all` + `EnableNETAnalyzers=true` behave under SDK 11 (root `Directory.Build.props`).
- [ ] Review `.editorconfig` experimental / style rules for SDK 11 analyzer changes (known risk: IDE0055 when SDK and analyzer package generation diverge).
- [ ] **Pre-GA soak (can start now on RC1, `11.0.100-rc.1` is installed):** run the full suite on the .NET 11 runtime (`DOTNET_ROLL_FORWARD=Major` or a temporary `global.json`) and watch for behavior changes under CliWrap's output reading: Windows redirected stdio moved to overlapped I/O, macOS spawn moved to posix_spawn, Linux handle inheritance uses close_range. Record pass/fail per OS in Notes.
- [x] **Decision: replace CliWrap internally with the .NET 11 Process APIs?** **Keep CliWrap for this TFM bump.** A swap is its own task. See Notes. .NET 11 adds `Process.Run/RunAsync`, `RunAndCaptureText[Async]`, deadlock-free `ReadAll*`, handle-based stdio redirection, `KillOnParentExit`, `ProcessExitStatus`, `SafeProcessHandle.WaitForExitOrKillOnCancellationAsync`, `Signal(PosixSignal)`. Task 091 already removed CliWrap from the public surface, so a later swap would be internal. Evaluated against the execution modes in `core/command-result.cs` (capture, stream, pipe, passthrough, select, timeout graceful/forceful from task 044) and the mock layer.

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

### Go-live decision (2026-10-31)

**Wait for GA.** Do not pin `global.json` to an RC SDK and do not retarget to `net11.0` on this walk.

Checked 2026-10-31 against [the .NET 11 download page](https://dotnet.microsoft.com/en-us/download/dotnet/11.0) and the [11.0 release notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/README.md):

- Latest published build is **11.0.0-rc.3** (SDK `11.0.100-rc.3.26522.148`, 2026-10-25). It is go-live licensed. It is not GA.
- GA is still scheduled **2026-11-10**. Support window on the release index is November 10, 2026 through November 9, 2028.
- This machine has SDKs through `11.0.100-rc.3.26522.148`. `dotnet --version` under the repo pin is `10.0.403` (`global.json` is `10.0.301` with `rollForward: latestFeature`). There is no stable `11.0.100` SDK installed because it is not published.
- `Microsoft.CodeAnalysis.NetAnalyzers` has no stable 11.0 package. The 11 line is `11.0.100-rc.3.26522.148`. The latest stable is `10.0.403` (CPM here is still `10.0.401`). Shipping the TFM bump on RC would pin CI and analyzers to prerelease builds that change again at GA, and it would be a breaking TFM for consumers while .NET 11 is not the supported GA runtime.
- No product note asks for an early RC soak-as-ship. The kitchen says to prefer GA unless that need is recorded. Task 129 shipped 2.0.0 on `net10.0` and left this upgrade for after GA.

Pre-GA soak (checklist item, Linux `close_range` / CliWrap output reading) was **not** run. It does not unblock the TFM bump, and the suite should be the GA verification run. RC3 is the installed candidate if a soak is wanted before November 10.

### CliWrap (2026-10-31)

**Keep** CliWrap 3.10.5 for the TFM bump. Do not partial-swap execution onto `Process.Run` / `RunAndCaptureText` in the same change.

`source/timewarp-amuru/core/command-result.cs` builds the public modes on CliWrap: event streams for low-buffer capture, CliWrap's pipe operator for `Pipe`, and `ExecuteAsync(forceful, graceful)` for the task 044 timeout (SIGINT at the timeout, kill after `TimeoutGracePeriod`, exit 124 / `TimeoutException`). The mock layer matches every mode except pipelines and must not read CliWrap's last-stage `TargetFilePath`. `TtyPassthroughAsync` is already the raw `Process` boundary, and PTY stays out of scope (.NET 12 research). The .NET 11 process APIs are additive, so they do not force a rewrite. Task 083 is the consumer that wants those primitives; a swap belongs on its own task after this bump is green.

### Pins observed 2026-10-31 (checklist text is the 2026-10-02 snapshot)

`Directory.Packages.props` now has `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` at `4.0.0-beta.6`, and self pins `TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` at `2.0.0`. A `net11.0` project can restore `net10.0` libraries through NuGet's forward TFM fallback, so sibling net11 builds are not a hard restore gate. The GA walk still confirms restore and AOT.

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
- Implement walk 2026-10-31: recorded wait-for-GA and keep-CliWrap. No .NET 11 product diff. Walk parked until the .NET 11 GA SDK is published.
- Same walk, audit boy-scout: `ganda repo audit --fix` scaffolded `glossary.md`, refreshed `.githooks/pre-push.cs`, and pinned `TimeWarp.Nuru` / `TimeWarp.Nuru.DevCli` to `4.0.0-beta.10`. DevCli 4.0.0-beta.10 makes `NuGetVersionService`, `IRepoConfigService`, and `IPackableProjectService` internal, so `WorkflowCommand.Handler` is internal to match the package handlers. why: Steve 2026-10-19, audit --fix is the request (task 156)