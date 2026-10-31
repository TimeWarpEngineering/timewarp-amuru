# Upgrade to .NET 11

## Description

Bump TimeWarp.Amuru from .NET 10 to .NET 11 across TFMs, central package versions, SDK pin, CI, docs, and analyzer stack. Do not start product-code work until .NET 11 is GA (or an explicit go-live RC decision is recorded). This task is the implementation kitchen; analysis below reflects master as of 2026-10-02.

As of ~2026-10-02: .NET 11 is **RC1** (`11.0.0-rc.1`, go-live license, 2026-09-08). **GA scheduled 2026-11-10** (STS, support roughly Nov 2026 through Nov 2028). Prefer waiting for GA unless product needs an early RC soak.

## Open work (Steve, 2026-10-31)

The TFM bump, SDK pin, version `3.0.0`, and review disposition are already on this branch. Previous `## Results` do not close this pass. Do not redo the retarget and do not revert it.

Steve: update every current reference so it shows .NET 11.

Replace `net10.0` with `net11.0` in the live samples and tests:

- XML doc examples on the dotnet builders (`"net8.0", "net10.0"` becomes `"net8.0", "net11.0"`) under `source/timewarp-amuru-tools/dot-net-commands/`
- Samples in `source/timewarp-amuru-tools/dot-net-commands/dot-net.md`
- Every `WithFramework("net10.0")`, `WithTemplateArg("net10.0")`, and the matching expected command strings under `tests/timewarp-amuru/single-file-tests/dot-net-commands/`
- Product lines in `documentation/posts/blips/amuru-1.0.0.md` and `documentation/posts/blips/amuru-1.0.0-x.md` that say the library is for .NET 10
- The `<TargetFramework>net10.0</TargetFramework>` sample in `.agent/workspace/2025-12-06T18-30-00_ganda-migration-analysis.md`

Leave these strings. Changing them would make a shipped record false, or undo review finding M3:

- `documentation/release-notes/2.0.0.md` (`2.0.0` shipped `lib/net10.0`)
- `documentation/release-notes/3.0.0.md` (says `net10.0` is gone)
- `documentation/user/installation.md` lines that say `dnx` and `#:package` arrived with the .NET 10 SDK. The requirement lines stay .NET 11.
- `kanban/done/**` and this kitchen's decision history

The builder API still accepts any moniker. Only the sample text changes. Re-run the dotnet-command tests and a warnings-as-errors build.

## Goal (implementer restatement)

The `net11.0` hard-cut, the SDK pin, version `3.0.0`, and the review fixes are already on this branch. This pass does not retarget projects, does not change `global.json`, and does not touch analyzer or package pins. It only rewrites the live examples that still name `net10.0` as the current moniker. The builder methods still take any framework string. The method signatures stay the same.

Acceptance for this pass:

- XML doc examples under `source/timewarp-amuru-tools/dot-net-commands/` say `"net8.0", "net11.0"`.
- `dot-net.md` samples call `WithFramework("net11.0")`.
- Every `WithFramework("net10.0")` and `WithTemplateArg("net10.0")` under `tests/timewarp-amuru/single-file-tests/dot-net-commands/` becomes `net11.0`, and the expected command strings match that moniker.
- The product lines in `documentation/posts/blips/amuru-1.0.0.md` and `documentation/posts/blips/amuru-1.0.0-x.md` say the library is for .NET 11.
- The `<TargetFramework>` sample in `.agent/workspace/2025-12-06T18-30-00_ganda-migration-analysis.md` is `net11.0`.
- Shipped records stay as written: `documentation/release-notes/2.0.0.md`, `documentation/release-notes/3.0.0.md`, the `dnx` and `#:package` history lines in `documentation/user/installation.md`, `kanban/done/**`, and this kitchen's decision history.
- The dotnet-command tests pass, and a warnings-as-errors build reports 0 warnings and 0 errors.

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

- [x] Decide RC go-live vs wait-for-GA; record choice in Notes / commit message. Decision 2026-10-31, revised the same day: **RC go-live**. Steve: do not leave this repo on .NET 10. The earlier wait-for-GA note is void.
- [x] Update `global.json` `sdk.version` from `10.0.301` to .NET 11 SDK (GA preferred). Pinned `11.0.100-rc.3.26522.148` with `allowPrerelease: true`. No GA SDK is published.
- [x] Install matching SDK on TWE-001 / agent machines; confirm `dotnet --version`. This machine already had `11.0.100-rc.3.26522.148`. `dotnet --version` in the repo prints that version.
- [x] Confirm `LangVersion=latest` + `AnalysisLevel=latest-all` + `EnableNETAnalyzers=true` behave under SDK 11 (root `Directory.Build.props`). Release build is 0 warnings. Char searches pass `StringComparison.Ordinal` on the char overload; see Notes.
- [x] Review `.editorconfig` experimental / style rules for SDK 11 analyzer changes (known risk: IDE0055 when SDK and analyzer package generation diverge). No `.editorconfig` change. NetAnalyzers matches the SDK build, and IDE0055 did not fire.
- [x] **Pre-GA soak:** full suite on the .NET 11 runtime. Linux pass (732 passed, 1 skipped, 0 failed) under SDK `11.0.100-rc.3.26522.148`. Windows and macOS were not run on this host. See Notes.
- [x] **Decision: replace CliWrap internally with the .NET 11 Process APIs?** **Keep CliWrap for this TFM bump.** A swap is its own task. See Notes. .NET 11 adds `Process.Run/RunAsync`, `RunAndCaptureText[Async]`, deadlock-free `ReadAll*`, handle-based stdio redirection, `KillOnParentExit`, `ProcessExitStatus`, `SafeProcessHandle.WaitForExitOrKillOnCancellationAsync`, `Signal(PosixSignal)`. Task 091 already removed CliWrap from the public surface, so a later swap would be internal. Evaluated against the execution modes in `core/command-result.cs` (capture, stream, pipe, passthrough, select, timeout graceful/forceful from task 044) and the mock layer.

### 2. TFMs

- [x] Root `Directory.Build.props`: `net10.0` to `net11.0`.
- [x] `tools/Directory.Build.props`: same `net10.0` to `net11.0` (tools override TFM explicitly).
- [x] Confirm `source/timewarp-amuru/timewarp-amuru.csproj` and `source/timewarp-amuru-tools/timewarp-amuru-tools.csproj` inherit TFM (no local override today).
- [x] Confirm `samples/Directory.Build.props` inherits root TFM (samples only override packability).
- [x] Confirm file-based tests under `tests/timewarp-amuru/` still run as .NET 11 file-based apps (`dotnet run <file>.cs`). The workflow test step ran them on SDK 11.

### 3. Packages (CPM — after TFM / SDK)

- [x] `Microsoft.CodeAnalysis.NetAnalyzers` `10.0.401` to .NET 11-aligned version. Pinned `11.0.100-rc.3.26522.148`. No stable 11.0 package exists.
- [x] Re-evaluate `Microsoft.CodeAnalysis.CSharp.CodeStyle` `5.9.0` and `Microsoft.CodeAnalysis.BannedApiAnalyzers` `5.6.0` for SDK 11 compatibility. Both are the newest stable packages. Left in place. PublicApiAnalyzers stays `5.6.0` for the same reason.
- [x] Roslynator trio (`Roslynator.Analyzers` / `CodeAnalysis.Analyzers` / `Formatting.Analyzers`) all at `5.0.0` — bump if needed for SDK 11. `5.0.1` exists and was not required. The SDK 11 build is clean on `5.0.0`.
- [x] `NuGet.Versioning` `7.9.0` — bump if NuGet client line requires it for SDK 11 tooling. `7.9.0` is the newest release. Left in place.
- [x] External runtime deps: `CliWrap` `3.10.5`, `TimeWarp.Terminal` `1.0.2` — verify net11 restore / AOT. Both restored. Release build with `IsAotCompatible=true` is 0 warnings.
- [x] TimeWarp ecosystem pins stay on their current CPM versions. `net10.0` libraries restore onto `net11.0` through TFM fallback. Jaribu `1.0.0-beta.15`, Nuru and Nuru.DevCli `4.0.0-beta.10`, SourceGenerators `1.0.0-beta.11`, Build.Tasks `1.0.0`. Githook self pins stay at the published `2.0.0` until `3.0.0` is on the feed.
- [x] `Shouldly` `4.3.0` — verify on net11 test host. Suite green. `5.0.0` is still preview, so `4.3.0` stays.
- [x] Keep CPM in `Directory.Packages.props`; do not scatter versions into csproj.

### 4. Workflows / CI images

- [x] `.github/workflows/workflow.yml`: keep `actions/setup-dotnet@v4` + `global-json-file: global.json` (no hardcoded SDK version today).
- [x] After `global.json` bump, run a green CI on the task branch; confirm SDK 11 installs on `ubuntu-latest`. `workflow_dispatch` mode `merge` on `6bdf9c2`: [run 41181587366](https://github.com/TimeWarpEngineering/timewarp-amuru/actions/runs/41181587366). `ubuntu-latest` installed SDK `11.0.100-rc.3.26522.148` from `global.json`. Build 0 warnings. Tests 732 passed, 1 skipped. Both `3.0.0` nupkgs were created. Package upload stays master-only, so that step was skipped on this ref.
- [x] No Dockerfiles / container images in-repo to bump (none found).
- [x] No explicit test matrix of TFMs today (single job) — no matrix expansion required unless multi-TFM is chosen.
- [x] `dev-cli` path (`dotnet run --file tools/dev-cli/dev.cs -- workflow`) must work under SDK 11. `workflow --mode pr` succeeded. The handler constructor is public so the mediator can activate it. The class stays internal.
- [x] Packages artifact / release promotion path unchanged (verify nupkgs still emit for both packages). Release build wrote `TimeWarp.Amuru.3.0.0.nupkg` and `TimeWarp.Amuru.Tools.3.0.0.nupkg`, both `lib/net11.0`.

### 5. Docs and agent guidance

- [x] `documentation/user/installation.md`: ".NET 10 File-Based Apps", "#:package is native to .NET 10", "requires .NET 10", download link to 10.0 — update to .NET 11 / 11.0.
- [x] `AGENTS.md`: SDK pin note (`global.json` pins SDK 10.0.x; preview SDK fails IDE0055) to 11.0.x; "runnable .NET 10 file-based app" to .NET 11.
- [x] Spot-check `readme.md` / `overview.md` / skills for leftover net10 language. Readme, the two how-to guides, and architectural-layers now say .NET 11. `skills/tw-amuru` and both overview files had no .NET 10 requirement.
- [x] Steve 2026-10-31: sample monikers still say `net10.0`. See **Open work**. Replace those current samples, tests, blips, and the analysis snippet with `net11.0`. Do not rewrite the 2.0.0 release notes or the `dnx` / `#:package` history lines. Done this pass. Builder XML docs, `dot-net.md`, the dotnet-command tests, both 1.0.0 blips, and the migration-analysis `<TargetFramework>` sample now say `net11.0`. The cli-smoke Design note names the 11.0 feature band, matching `global.json`. Shipped records were left as written.

### 6. Verification

- [x] `dotnet build timewarp-amuru.slnx` (warnings-as-errors clean). Release build inside `workflow --mode pr` is 0 warnings, 0 errors.
- [x] Full `dev test` / multi-file runners green. 732 passed, 1 skipped, 0 failed. This pass ran `dotnet run --file tests/timewarp-amuru/multi-file-runners/run-tests.cs` again after the sample moniker edits: same totals, including every `DotNetCommands` class and the live `nuget why` smoke.
- [x] Pack both packages; AOT-compat analyzers clean. `IsAotCompatible=true` build is 0 warnings. Both nupkgs contain `lib/net11.0`.
- [x] Manual smoke: file-based script with `#:package TimeWarp.Amuru@3.0.0` on SDK `11.0.100-rc.3.26522.148` captured `amuru-net11-smoke` from the local package.

## Review

- Round 1: open M1, M2, M3. Fix pass implemented all three.
- Round 2: Done
## Notes

### Go-live decision (2026-10-31)

**RC go-live. Hard-cut to `net11.0`.** Steve 2026-10-31: do not leave this repo on .NET 10. The earlier "wait for GA" decision from the same day is void.

Pin `global.json` to the installed go-live SDK `11.0.100-rc.3.26522.148`, or a newer 11.0 SDK if one is present. Retarget root and `tools/` to `net11.0` only. Do not multi-target `net10.0`. Bump analyzer packages onto the matching 11.0 line, including the RC NetAnalyzers package when no stable 11.0 package exists. Update docs that require .NET 10.

CliWrap stays for this bump. That is the process library, not a `net10.0` target. A `net10.0` NuGet dependency may remain only when it restores onto `net11.0` through TFM fallback and no `net11.0` build of that package exists.

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
- Steve 2026-10-31 overruled that park: RC go-live, hard-cut off `net10.0`. Next walk retargets.
- Steve 2026-10-31: current samples, tests, blips, and the analysis snippet still say `net10.0`. Update those references to `net11.0`. Leave shipped-version records.
- Same walk, audit boy-scout: `ganda repo audit --fix` scaffolded `glossary.md`, refreshed `.githooks/pre-push.cs`, and pinned `TimeWarp.Nuru` / `TimeWarp.Nuru.DevCli` to `4.0.0-beta.10`. DevCli 4.0.0-beta.10 makes `NuGetVersionService`, `IRepoConfigService`, and `IPackableProjectService` internal, so `WorkflowCommand.Handler` is internal to match the package handlers. why: Steve 2026-10-19, audit --fix is the request (task 156)
- Implement walk 2026-10-31 (RC go-live): hard-cut to `net11.0`, SDK `11.0.100-rc.3.26522.148`, NetAnalyzers `11.0.100-rc.3.26522.148`, package version `3.0.0`. The workflow handler constructor is public again. An internal constructor is invisible to the mediator, which is why `dev workflow` threw. The class stays internal.
- Same walk: SDK 11 file-based apps restore as RID-specific AOT tool packages. Root `GeneratePackageOnBuild` then fails the hook with "GeneratePackageOnBuild is not supported for native compilation." `.githooks/Directory.Build.props` sets `IsPackable` and `GeneratePackageOnBuild` to false, the same opt-out samples already use. The product csprojs still pack.

### CA1307 and CA1865 (2026-10-31)

SDK 11 NetAnalyzers flags `StartsWith(char)`, `EndsWith(char)`, and `LastIndexOf(char, int)` as CA1307. The char overloads that take `StringComparison` exist on `net11.0`. The five call sites pass `StringComparison.Ordinal`. That form compiles clean: neither CA1307 nor CA1865 fires. An earlier note on this task said those overloads were missing. That claim was wrong, and the `#pragma` blocks that rested on it are gone.

### Pre-GA soak (2026-10-31)

| OS | Result |
|----|--------|
| Linux (this host, SDK `11.0.100-rc.3.26522.148`) | Pass. `workflow --mode pr`: 732 passed, 1 skipped, 0 failed. CliWrap capture, stream, pipe, passthrough, and timeout tests are in that run. |
| Windows | Not run. No Windows host on this walk. |
| macOS | Not run. No macOS host on this walk. |

### Version

`<Version>` is `3.0.0`. `2.0.0` is the published package and the git tag `v2.0.0`. `dev check-version` reports `3.0.0` as new against NuGet `2.0.0`. The symbol surface is unchanged, so both `PublicAPI.Unshipped.txt` files stay header-only. Githook CPM pins stay at `2.0.0` until `3.0.0` is on the feed. `documentation/release-notes/3.0.0.md` records the TFM break. This walk does not publish.

## Session

- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a19093-be62-7253-8b37-b2aa28346f32, max-turns 500, Blocked — 2026-10-31T06:21:31Z
- Implement oracle: RC go-live product diff by implementer-grok (grok-4.7), session 01a190ce-c522-7673-84d7-a41f8639ad2e, 2026-10-31. Local workflow green. Ubuntu CI run 41181587366 on `6bdf9c2` is green.
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a190ce-c522-7673-84d7-a41f8639ad2e, max-turns 500, Done — 2026-10-31T07:45:41Z
- Review oracle round 1: general reviewer, Claude Code (claude-opus-5-5), effort 2 (by-diff, 513 lines), turn cap 120, 2026-10-31. Findings: 1 bug and 2 nits, all open. Fix pass needed. See `review/round-1/merged.md`.
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 1414ca54-3135-46c5-9aa6-cf3d3454ed17, max-turns 120 — 2026-10-31T07:49:05Z
- Fix pass (implement-fix-2): M1 passes `StringComparison.Ordinal` at the five char searches and drops the CA1307 pragmas. M2 drops CA1812 from the dev-cli `NoWarn` and suppresses the three mediator handlers that still fire it. M3 corrects the `dnx` and `#:package` history in `installation.md`.
- Review oracle round 2: general reviewer, Claude Code (claude-opus-5-5), effort 1 (by-diff, 188 lines), turn cap 80, 2026-10-31. M1, M2, and M3 are fixed, and there are no new findings. Disposition is clean. See `review/round-2/merged.md` and `review/disposition.md`.
- Implement oracle: sample monikers updated to `net11.0` by implementer-grok (grok-4.7), session 01a191aa-9292-7250-9de4-ecd6e73b64e7, 2026-10-31. Release build 0 warnings. Multi-file runner 732 passed, 1 skipped, 0 failed.

## Blocked

Resolved 2026-10-31 by Steve: RC go-live on SDK `11.0.100-rc.3.26522.148`. Do not wait for the GA SDK or a stable NetAnalyzers 11.0 package. Use the matching RC analyzer package.

## Results

Hard-cut TimeWarp.Amuru to `net11.0` on SDK `11.0.100-rc.3.26522.148` (`rollForward: latestFeature`, `allowPrerelease: true`). `Microsoft.CodeAnalysis.NetAnalyzers` is that same RC build. Package version is `3.0.0` because `2.0.0` is already published and the TFM change is breaking. CliWrap stays. No `.editorconfig` change. The five char searches pass `StringComparison.Ordinal` on `StartsWith(char, StringComparison)`, `EndsWith(char, StringComparison)`, and `LastIndexOf(char, int, StringComparison)`. Those overloads exist on `net11.0`, and the CA1307 pragmas are gone.

`dotnet run --file tools/dev-cli/dev.cs -- workflow --mode pr` on this host: clean, release build 0 warnings, both samples compiled, tests 732 passed / 1 skipped / 0 failed. `ganda repo audit` passed (35 passed, 0 failed, 2 skipped). `dev check-version` says `3.0.0` is new against NuGet `2.0.0`.

The review fix pass ran that same workflow and `ganda repo audit` again. Release build 0 warnings and 0 errors. Tests 732 passed, 1 skipped, 0 failed. The workflow printed `Pipeline SUCCEEDED`. Audit passed (35 passed, 0 failed, 2 skipped).

`WorkflowCommand.Handler` keeps an internal class and a public constructor. The mediator cannot activate an internal constructor, so `dev workflow` failed until that constructor was public.

SDK 11 treats a file-based app as a RID-specific AOT tool package. With root `GeneratePackageOnBuild` still true, `git commit` died in the pre-commit hook: native compilation refuses that property. `.githooks/Directory.Build.props` turns packing off for the hooks. Samples and the dev CLI already opted out. `dotnet run --file .githooks/pre-commit.cs` then exits 0 on this task branch. Each hook under `.githooks/*.cs` builds with that opt-out.

Ubuntu CI is green. A branch push does not match `workflow.yml` (`push` is master only, and `pull_request` is not open yet), so this walk dispatched `workflow_dispatch` mode `merge` on `6bdf9c2`. [Run 41181587366](https://github.com/TimeWarpEngineering/timewarp-amuru/actions/runs/41181587366) installed SDK `11.0.100-rc.3.26522.148`, built with 0 warnings, passed 732 tests, skipped 1, and packed both `3.0.0` nupkgs. The Packages artifact upload is master-only, so it did not run on this ref.

### Review

Round 1 ran on 2026-10-31 with one general reviewer at effort 2. The fix pass (`0b1cccd`) implemented every open finding. Round 2 ran on 2026-10-31 with one general reviewer at effort 1 over the fix delta `75b0006..0b1cccd`. It re-verified M1, M2, and M3 as fixed and raised no new findings. A Release `--no-incremental` build of `timewarp-amuru.slnx` gave 0 warnings and 0 errors. The `tools/dev-cli/dev.cs` Release build is clean, so the assembly CA1812 targets resolve. `./bin/dev test` passed with 1 skipped.

**Disposition: clean.** Rounds: 2. Roster: general (effort 2, then effort 1). Final counts: bug 0 open / 1 fixed / 0 wontfix; suggestion 0 / 0 / 0; nit 0 open / 2 fixed / 0 wontfix. No wontfix and no escalations.

- **M1 (bug):** the five call sites pass `StringComparison.Ordinal` and no longer suppress CA1307. Notes, the checklist, and the Results sentence above match that.
- **M2 (nit):** CA1812 is not in the dev-cli `NoWarn`. A Release build of `tools/dev-cli/dev.cs` with that id enabled reports exactly three handlers: `WorkflowCommand.Handler`, and `ReleaseCommand.Handler` plus `SelfInstallCommand.Handler` from TimeWarp.Nuru.DevCli 4.0.0-beta.10 content. The first carries `[SuppressMessage]` on the type. The other two are assembly suppressions targeted at `~T:DevCli.ReleaseCommand.Handler` and `~T:DevCli.SelfInstallCommand.Handler`, because the content files live in the NuGet cache. `CleanCommand.Handler` and `CheckVersionCommand.Handler` do not fire CA1812. A dead internal class anywhere else in this CLI is reported again.
- **M3 (nit):** `installation.md` says `#:package` and `dnx` are .NET SDK commands from .NET 10 onward. The requirement lines still say .NET 11.

Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/round-2/general.md`, `review/round-2/merged.md` (final ledger), and `review/disposition.md`.

### Sample monikers (2026-10-31)

Live examples that still named `net10.0` now name `net11.0`. That is the XML doc examples on the dotnet builders, the samples in `dot-net.md`, every `WithFramework` / `WithTemplateArg` pair and its expected command string under `tests/timewarp-amuru/single-file-tests/dot-net-commands/`, the product lines in both 1.0.0 blips, and the `<TargetFramework>` sample in `.agent/workspace/2025-12-06T18-30-00_ganda-migration-analysis.md`. The builder methods still accept any moniker. `documentation/release-notes/2.0.0.md`, `documentation/release-notes/3.0.0.md`, the `dnx` and `#:package` history lines in `installation.md`, and `kanban/done/**` were left as written. The cli-smoke Design note now says the copied `global.json` keeps roll-forward on the 11.0 feature band.

`dotnet build timewarp-amuru.slnx -c Release --no-incremental -warnaserror` reported 0 warnings and 0 errors, and it packed both `3.0.0` nupkgs. `dotnet run --file tests/timewarp-amuru/multi-file-runners/run-tests.cs` reported Total 733, Passed 732, Skipped 1, and exit 0. The `--filter-tag DotNetCommands` argument was ignored by `RunAllTests()`, so the run was the full suite. `CliSmoke_Given_` passed 18, including `NuGetWhy_Should_AcceptPositionalProject` with `--framework net11.0`.

### How to validate

Smoke:

```bash
rg -n 'net10\.0' \
  source/timewarp-amuru-tools/dot-net-commands \
  tests/timewarp-amuru/single-file-tests/dot-net-commands \
  documentation/posts/blips \
  .agent/workspace/2025-12-06T18-30-00_ganda-migration-analysis.md
dotnet build timewarp-amuru.slnx -c Release -warnaserror
dotnet run --file tests/timewarp-amuru/multi-file-runners/run-tests.cs
dotnet --version
dotnet build timewarp-amuru.slnx
dotnet run --file tools/dev-cli/dev.cs -- workflow --mode pr
```

Expect: `rg` prints no `net10.0` hits in those live sample paths. Release and the plain build each report 0 warnings and 0 errors. The multi-file runner grand total is 732 passed, 1 skipped, 0 failed. `dotnet --version` prints `11.0.100-rc.3.26522.148` or a newer installed 11.0 SDK in that feature band. The workflow prints `Pipeline SUCCEEDED`. `artifacts/packages` contains `TimeWarp.Amuru.3.0.0.nupkg` and `TimeWarp.Amuru.Tools.3.0.0.nupkg`, and each lists `lib/net11.0`. This pass ran the `rg` check, the Release `-warnaserror` build, and the multi-file runner. It did not re-run `workflow --mode pr`.

File-based package smoke, after that pack. Save this as a `.cs` file outside the repo so the repo `nuget.config` `<clear />` does not hide the GitHub packages feed, then `dotnet run --file` it:

```csharp
#!/usr/bin/env -S dotnet --
#:package TimeWarp.Amuru@3.0.0
#:property RestoreAdditionalProjectSources=/absolute/path/to/artifacts/packages
#:property RestoreNoCache=true

using TimeWarp.Amuru;

CommandOutput result = await Shell.Builder("echo").WithArguments("amuru-net11-smoke").CaptureAsync();
if (!result.Success || !result.Stdout.Contains("amuru-net11-smoke", StringComparison.Ordinal))
{
  return 1;
}

Console.WriteLine("smoke-ok");
return 0;
```

Expect `smoke-ok` and exit 0 on SDK `11.0.100-rc.3.26522.148`. This walk ran that script and saw `smoke-ok tfm-package=3.0.0 stdout=amuru-net11-smoke`.
