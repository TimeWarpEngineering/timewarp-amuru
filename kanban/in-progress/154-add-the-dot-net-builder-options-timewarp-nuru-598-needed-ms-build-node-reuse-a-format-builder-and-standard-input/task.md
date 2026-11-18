# Add the DotNet builder options timewarp-nuru 598 needed: MsBuild node reuse, a Format builder, and standard input

## Goal (implementer restatement)

Nuru still builds three `dotnet` command lines by hand because the typed builders cannot say what those calls say. This task adds those missing pieces on the 3.0 prerelease line so a later nuru change can drop the fallbacks.

Acceptance criteria:

- `DotNet.MsBuild()` can turn MSBuild node reuse on or off. The command string contains `-nodeReuse:true` or `-nodeReuse:false`. Leaving the method unset omits the switch.
- `DotNet.Format()` and `DotNet.Format(path)` build `dotnet format` with a project or solution path, `--severity`, a repeatable `--exclude`, and `--verify-no-changes`. `Build()` returns a `CommandResult` whose `ToCommandString()` shows those tokens, and the builder has the same run, capture, and passthrough methods as `DotNet.MsBuild()`.
- `DotNet.Run()` and `DotNet.Builder()` take `WithStandardInput(string)`. The text is piped to the child, including an empty string as immediate EOF, and it does not appear in the command string. Null is rejected.
- `DotNet.Run()` already has `WithTimeout(TimeSpan)` from task 153. This walk does not add that method again.
- `format` leaves the analyzer's known-gap list. `Shell.Builder("dotnet")` with a `format` argument reports AMURU0001 and names `DotNet.Format()`.
- New public members are listed in `PublicAPI.Unshipped.txt`. This walk does not bump the package version or publish it. Moving nuru's call sites waits until a release that contains these members.

## Description

timewarp-nuru task 598 moved its raw `Shell.Builder("dotnet")` calls to the typed `DotNet.*`
builders for AMURU0001 (3.0.0-beta.9). Three calls could not use a typed builder because the
builder lacks an option. They run on a fallback today:

| Call (timewarp-nuru) | Missing in Amuru.Tools | Fallback in nuru |
|----------------------|------------------------|------------------|
| `dotnet msbuild <proj> -nologo -nodeReuse:false -getProperty:IsPackable,PackageId` (DevCli `packable-project-service.cs`) | `DotNetMsBuildBuilder` has no node-reuse option (`-nodeReuse:false`) | `DotNet.Builder().WithArguments("msbuild", …)` |
| `dotnet format <sln> --severity warn --exclude … [--verify-no-changes]` (`tools/dev-cli/endpoints/format-command.cs`) | no `DotNet.Format()` builder (AMURU0001 reports "dotnet verb has no typed builder") | `DotNet.Builder().WithArguments("format", …)` |
| `dotnet run <file> -- …` with text piped to stdin (`tests/…/routing-33b-json-args-stdin-and-defaults.cs`, `tests/timewarp-nuru-daemon-tests/daemon-03-served-app.cs`) | `DotNetRunBuilder` and `DotNetBuilder` have no `WithStandardInput(string)`; `DotNetRunBuilder` also has no `WithTimeout(TimeSpan)` | `Shell.Run("dotnet", args, options, standardInput)` |

## Requirements

- `DotNetMsBuildBuilder.WithNodeReuse(bool)` (emits `-nodeReuse:false` / `-nodeReuse:true`).
- `DotNet.Format()` builder: project/solution path, `--severity`, `--exclude` (repeatable),
  `--verify-no-changes`, plus the usual execution methods and `Build()` (for `ToCommandString()`).
- `WithStandardInput(string)` on `DotNetRunBuilder` and `DotNetBuilder`; `WithTimeout(TimeSpan)`
  on `DotNetRunBuilder`.

## Checklist

- [x] `DotNetMsBuildBuilder.WithNodeReuse(bool)`
- [x] `DotNet.Format()` builder
- [x] `WithStandardInput` on `DotNetRunBuilder` / `DotNetBuilder`, `WithTimeout` on `DotNetRunBuilder` (`WithTimeout` already shipped in task 153)
- [ ] Released, then timewarp-nuru moves the fallbacks to the typed builders

## Session

- Created: 1966318 (2026-11-18), filed from timewarp-nuru 598
- Implementation: 01a1ed7f-49c5-7ff2-85b5-5f708b1d31ae (2026-11-18)
- `ganda repo audit --fix` wrote the gitignored `.vscode/settings.json` window title for `vscode-window-icon`. That file stays untracked.
- The first commit's attest hook failed `runfile-executable` on the two new runfiles. Both are now executable, matching the other dotnet runfiles.
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1ed7f-49c5-7ff2-85b5-5f708b1d31ae, max-turns 500, Done — 2026-11-18T07:29:46Z

## Results

`DotNet.MsBuild()` takes `WithNodeReuse(bool)` and emits `-nodeReuse:true` or `-nodeReuse:false`. Omitting it leaves the switch off the command line. `WithGetProperty("IsPackable,PackageId")` still emits one `-getProperty:` token, so the nuru packable query is `dotnet msbuild app.csproj -getProperty:IsPackable,PackageId -nologo -nodeReuse:false`.

`DotNet.Format()` and `DotNet.Format(path)` build `dotnet format`. `WithSeverity` emits `--severity`, each `WithExclude` emits its own `--exclude`, and `WithVerifyNoChanges` emits `--verify-no-changes`. The nuru check shape is `dotnet format timewarp-nuru.slnx --severity warn --exclude **/benchmarks/** --verify-no-changes`. `Build()` returns a `CommandResult`. The run, capture, and passthrough methods match `DotNet.MsBuild()`.

`DotNet.Run()` and `DotNet.Builder()` take `WithStandardInput(string)`. The text is passed to `Shell.Run` and does not appear in the command string. An empty string is immediate EOF. Null throws `ArgumentNullException`. `DotNetRunBuilder.WithTimeout` was already public from task 153 and is unchanged.

`format` left `TypedBuilderTable.DotNetGapVerbs`. `Shell.Builder("dotnet")` with a `format` argument is AMURU0001 and names `DotNet.Format()`. The known-gap example in the analyzer test is now `exec`.

The new members are in `PublicAPI.Unshipped.txt`. The package version stays `3.0.0-beta.9`. This walk does not publish the package, so the release checklist item stays open. Nuru's three fallbacks stay where they are until a release that contains these members.

### How to validate

Smoke: `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.format.cs`

Expect: 7 passed. The nuru check case prints `dotnet format timewarp-nuru.slnx --severity warn --exclude **/benchmarks/** --verify-no-changes`.

- `./bin/dev build` — succeeded, 0 warnings, 0 errors.
- `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.ms-build.cs` — exit 0, 15 passed. Standalone runfile (`#if !JARIBU_MULTI`).
- `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.format.cs` — exit 0, 7 passed. Standalone runfile (`#if !JARIBU_MULTI`).
- `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.standard-input.cs` — exit 0, 6 passed (4 on `DotNetRunBuilder`, 2 on `DotNetBuilder`). Standalone runfile (`#if !JARIBU_MULTI`).
- `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.run.cs` — exit 0, 13 passed. Covers `DotNetRunBuilder.Build` after the stdin argument was added. The test file was not edited. Standalone runfile (`#if !JARIBU_MULTI`).
- `dotnet run --file tests/timewarp-amuru/single-file-tests/analyzers/typed-builder-table.cs` — exit 0, 7 passed. Standalone runfile (`#if !JARIBU_MULTI`).
- `dotnet run --file tests/timewarp-amuru/single-file-tests/analyzers/shell-builder-analyzer.cs` — exit 0, 24 passed. Standalone runfile (`#if !JARIBU_MULTI`).
- `ganda repo audit` — exit 0, Passed 43, Failed 1 advisory (`vscode-window-icon`: `.vscode/settings.json` missing).
- `ganda repo audit --fix` — wrote the gitignored `.vscode/settings.json` and set `window.title` to include `timewarp-amuru`. The audit inside that command passed: Passed 44, Failed 0. The Windows taskbar icon was not applied; this machine has no Windows-side ganda.
- Post-commit `ganda repo attest` on `53afb14` — failed `runfile-executable`: `dot-net.format.cs` and `dot-net.standard-input.cs` were mode `100644`. Both are now `100755`. The follow-up commit's hook attests.
- Fix pass `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.format.cs` — exit 0, 7 passed.
- Fix pass `dotnet run --file tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.standard-input.cs` — exit 0, 6 passed.

## Notes

Source: timewarp-nuru task 598 (Amuru 3.0.0-beta.9 bump). The nuru fallbacks carry a comment
naming this task.
