# AGENTS.md

This file provides guidance to agents when working with code in this repository.

## Build/Test Commands
- **Build**: `dotnet build timewarp-amuru.slnx` (builds both packages: `TimeWarp.Amuru` core and `TimeWarp.Amuru.Tools`), or `./tools/dev-cli/dev.cs build`
- **Full test suite**: `cd tests/timewarp-amuru/multi-file-runners && dotnet run run-tests.cs` (aggregate multi-mode run of all single-file tests), or `./tools/dev-cli/dev.cs test`
- **Single test file**: any file under `tests/timewarp-amuru/single-file-tests/` is a runnable .NET 10 file-based app — `dotnet run <file>.cs`
- **SDK pin**: `global.json` pins SDK 10.0.x — building with a newer preview SDK fails style analysis (IDE0055)
- **Local development**: Use `#:package TimeWarp.Amuru@*-*` and `#:property RestoreNoCache true` in scripts for fresh package downloads

## Package Layout
- `source/timewarp-amuru/` — **TimeWarp.Amuru** (core): Shell/ShellBuilder/CommandResult/CommandOutput, CommandMock testing, native file-system ops, ScriptContext. Stable-1.0 track; XML docs (CS1591), CA2007 (ConfigureAwait), and AOT analyzers enforced
- `source/timewarp-amuru-tools/` — **TimeWarp.Amuru.Tools**: DotNet/Git/Fzf fluent builders + repo/nu-get services. Own `<Version>` in its csproj (beta cadence); repo version in `source/Directory.Build.props` is the CORE version

## Non-Obvious Patterns
- **Build scripts and dev-cli should use TimeWarp.Amuru**: Prefer `Shell.Builder` over raw `System.Diagnostics.Process`; only use raw process APIs in rare implementation boundaries like true TTY passthrough (`System.Console` and `ProcessStartInfo` are banned via BannedSymbols.txt)
- **Error-handling contract**: default validation is `None` — non-zero exits are reported via `CommandOutput.ExitCode`/`Success`, never thrown; `WithZeroExitCodeValidation()` opts into throwing. Commands that never ran report `CommandResult.NeverRanExitCode` (-1)
- **Mocking is strict by default**: inside `CommandMock.Enable()`, an unmocked command throws; use `CommandMock.Enable(MockBehavior.Loose)` for tests that intentionally mix mocked and real commands
- **Tool-wrapper construction**: builders create commands via the public `Shell.Run(executable, args, options, stdin)` — never via core internals (no InternalsVisibleTo)
- **C# script execution**: `.cs` files get `--` prefix inserted before arguments to prevent dotnet interception; on Windows they route through the `dotnet` host
- **Tests that change CurrentDirectory must restore it** (try/finally) — later tests in the aggregate run depend on running inside the repo
- **Runfile caches can serve STALE ProjectReference builds**: after editing library source, `dotnet run <test>.cs` may run against an old copy of the referenced project. If test behavior contradicts the source you just changed, clear the cache: `rm -rf ~/.local/share/dotnet/runfile/<test-name>-*`
- **Analyzer overrides**: Different analyzer settings in `tests/`, `samples/` directories; `source/.editorconfig` adds CA2007 for the library projects

## Code Style Rules
See `.ai/04-csharp-coding-standards.md` and `.editorconfig` for project-specific formatting requirements.
