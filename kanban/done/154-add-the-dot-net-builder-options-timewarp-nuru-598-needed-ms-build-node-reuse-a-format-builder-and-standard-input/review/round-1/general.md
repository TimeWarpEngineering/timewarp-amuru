# Round 1 — general
**Date:** 2026-11-18
**Scope reviewed:** same as the framework (`e6ad360`..`fb34a5e`, product paths, tests, docs, skill)

## Summary

The change adds `DotNetMsBuildBuilder.WithNodeReuse(bool)`, a new `DotNetFormatBuilder` (`DotNet.Format()` / `DotNet.Format(path)`), and `WithStandardInput(string)` on `DotNetRunBuilder` and `DotNetBuilder`. It also moves `format` from `TypedBuilderTable.DotNetGapVerbs` to the typed-builder rows. Risk is low: every change is additive, and the stdin path reuses the existing `Shell.Run(..., standardInput)` overload that `ShellBuilder.WithStandardInput` already uses. No issues found.

## What was checked

- **Goal restatement vs. task.** The restatement matches the task's three rows and Requirements. It says `DotNetRunBuilder.WithTimeout` already shipped in task 153. That is correct: `DotNetRunBuilder.WithTimeout(System.TimeSpan)` is already in `PublicAPI.Unshipped.txt` from task 153, and the checklist item says so. No misread.
- **Node reuse** (`dot-net.ms-build.cs`). `bool?` field; omitted means no switch, the last call wins. The token comes right after `-nologo`. The test checks nuru's exact packable shape, `dotnet msbuild app.csproj -getProperty:IsPackable,PackageId -nologo -nodeReuse:false`. MSBuild switches are case-insensitive, so `-nodeReuse:` is valid.
- **Format builder** (`dot-net.format.cs`). Positional path, `--severity <level>` (last call wins), repeatable `--exclude <path>` in call order, `--verify-no-changes`. Blank inputs throw. The execution members (`RunAsync`, `CaptureAsync`, `RunAndCaptureAsync`, `PassthroughAsync`, `TtyPassthroughAsync`, `SelectAsync`) and option members (`WithWorkingDirectory`, `WithEnvironmentVariable`, `WithNoValidation`, `WithZeroExitCodeValidation`) match `DotNetMsBuildBuilder` one for one. There is no telemetry call. That fits the repo: only the build-family verbs (build, pack, publish, test, msbuild) emit telemetry, and clean and restore do not either.
- **Standard input.** Both builders store `string?` and call `Shell.Run("dotnet", args, Options, StandardInput)`. Null throws `ArgumentNullException`. Empty is passed through as configured stdin, which is immediate EOF. Omitting the call passes null, so the behavior is unchanged. `TtyPassthroughAsync` already refuses configured stdin (`command-result.standard-streams.cs:137`), so the new path cannot reach a TTY by mistake.
- **Analyzer.** The `format` row is added to `TypedBuilderTable`, and `format` is removed from `DotNetGapVerbs`. The analyzer test now expects AMURU0001 naming `DotNet.Format()`, and the AMURU0002 known-gap example moved to `exec`. Nothing was weakened: the gap-verb assertion still checks a real gap verb, and the change is the stated contract change.
- **Public API.** All 18 `DotNetFormatBuilder` members plus `DotNet.Format` ×2, `WithNodeReuse`, and `WithStandardInput` ×2 are in `PublicAPI.Unshipped.txt`.
- **Tests and aggregate runner.** The new runfiles are picked up by the `../single-file-tests/**/*.cs` glob in `multi-file-runners/Directory.Build.props`. The class names (`DotNet_.Format_Given_`, `DotNetRunBuilder_.WithStandardInput_Given_`, `DotNetBuilder_.WithStandardInput_Given_`) do not collide with any other test class (checked with `rg`). Both new runfiles are mode 100755.
- **Docs.** `dot-net.md` and `skills/tw-amuru/SKILL.md` cover the new members. Each changed source file has its Design region updated.
- **Recorded gates.** `## Results` / `### How to validate` record `./bin/dev build` (0 warnings), each changed or new test runfile run standalone and green, and `ganda repo audit` passing. That covers the change. The aggregate `./bin/dev test` run is not recorded; CI runs it, and the collision check above was the main risk there.

No targeted command was run beyond read-only `git`/`rg` inspection.

## Issues

None.
