# Split TimeWarp.Amuru.Tools package

## Description

Split the repo into two packages so the process-automation core can go 1.0 stable without freezing the tool-wrapper surface (~4x larger, tracks external CLIs that change under us, holds the 087/099/100 work):

- **TimeWarp.Amuru** (core, → 1.0 stable): `core/`, `testing/`, `interfaces/`, `native/` (file-system, aliases, PathResolver), `ScriptContext`, `CliConfiguration`, `AppContextExtensions`
- **TimeWarp.Amuru.Tools** (→ stays beta until 087/099/100 land): `dot-net-commands/`, `git-commands/`, `fzf-command/`, `repo/`, `nu-get/`

Root namespace stays `TimeWarp.Amuru` for both — consumers only add a package reference, no code edits. `repo/`/`nu-get/` stay PUBLIC in Tools because every TimeWarp repo's dev-cli consumes them and public repos cannot depend on private packages (Zana) — see parent 094 decision record.

## Checklist

- [x] Create `source/timewarp-amuru-tools/` project; move `dot-net-commands/`, `git-commands/`, `fzf-command/`, `repo/`, `nu-get/` (namespaces unchanged)
- [x] Tools references core; core drops the `NuGet.Versioning` dependency (verified in nuspec) (only repo/nu-get use it). `TimeWarp.Terminal` STAYS a core dependency (settled 2026-07-05: Terminal is TimeWarp-stack foundational — `BannedSymbols.txt` bans `System.Console`; core uses `TimeWarpTerminal.Default` for TTY passthrough streams, mock echo, and RunAsync pipes, not just `WriteToConsole`; and it's 1.0 stable now so no NU5104 concern)
- [x] Package metadata for Tools (id, description, tags, readme section, icon)
- [x] Versioning convention implemented: repo version = core; Tools csproj overrides at 1.0.0-beta.1 (no audit exception needed — `ganda repo audit` has no version check yet; this defines the convention): `source/Directory.Build.props` `<Version>` remains THE repo version (core inherits it; audit and release pipeline read it), and `timewarp-amuru-tools.csproj` sets its own `<Version>` which overrides the imported default while Tools is on beta cadence
- [x] Release pipeline pushes BOTH packages with per-package version resolution (`workflow-command.cs`); `dev build` now builds the solution; stale dev-cli.csproj slnx entry removed (`tools/dev-cli/endpoints/workflow-command.cs` reads only `source/Directory.Build.props`) to resolve version PER PACKAGE so Tools pushes its own version
- [x] `timewarp-amuru.slnx` lists both projects; dev-cli and tests reference both via Directory.Build.props
- [x] Test tree unchanged (single tree references both projects; aggregate runner covers both — 364/365 green)
- [x] Readme changes moved to task 102 (readme rewrite owns them)
- [x] Consuming-repo updates moved to task 107 post-release steps (needs published packages first)
- [x] Verified: core nuspec = CliWrap 3.10.1 + TimeWarp.Terminal 1.0.0 (all stable); Tools = core + NuGet.Versioning

## Notes

Decision 2026-07-05 (parent 094): two packages, not per-tool — subdividing a beta Tools package later is cheap and breaks no promises. Gates: this task blocks core 1.0 (093's doc scope and 096's AOT scope depend on where types land). Tasks 087/099/100 become Tools-stability gates, not core-1.0 gates.

## Session

- Created: 2026-07-05 (decision record in parent 094 task.md)

## Results

Completed 2026-07-05. New public API added deliberately: `Shell.Run(executable, args, options, stdin)` — the supported construction path for tool-wrapper builders (Tools uses it instead of core's internal `CommandExtensions`; no InternalsVisibleTo).
