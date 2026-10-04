# Fix invalid CLI flags in dotnet command builders

## Description

Release review (2026-07-04) verified several builder options against SDK 10.0.301 that emit flags the real `dotnet` CLI rejects or ignores. Any use of these options fails the command outright (or silently does nothing). These shipped through 34 betas because tests only snapshot the emitted argument string — `tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.pack.cs:95` asserts the broken `--tl on` form.

**BLOCKER for 1.0.**

## Checklist

### Hard failures (verified against SDK 10.0.301)
- [x] `WithTerminalLogger` emits `--tl <mode>` as two tokens; CLI parses `<mode>` as a positional PROJECT arg (`MSB1009`). Use `--tl:<mode>`. Six copy-paste sites:
  - [x] `dot-net-commands/DotNet.Build.cs:341-342`
  - [x] `dot-net-commands/DotNet.Restore.cs:296-297`
  - [x] `dot-net-commands/DotNet.Run.cs:376-377`
  - [x] `dot-net-commands/DotNet.Test.cs:380-381`
  - [x] `dot-net-commands/DotNet.Publish.cs:433-434`
  - [x] `dot-net-commands/DotNet.Pack.cs:344-345`
- [x] `DotNet.Test.cs:247` — `WithCollect()` emits bare `--collect`; CLI requires a data-collector name. Change to `WithCollect(string dataCollector)`
- [x] `DotNet.Pack.cs:85` — `WithFramework` emits `--framework`, unsupported by `dotnet pack` (MSB1001). Remove
- [x] `DotNet.DevCerts.cs:108` — `WithExport()` emits `--export`, which doesn't exist; export is via `-ep|--export-path`. Fix the `WithExport().WithExportPath()` pairing
- [x] `DotNet.NuGet.cs:1159` — `nuget why` builder's `WithProject` emits `--project`; the CLI takes project as positional. Fix
- [x] `DotNet.NuGet.cs:494-495` — `nuget delete` builder emits `--configfile`, not accepted by delete (copy-paste from push). Remove; also add `--non-interactive` support (delete prompts by default and stalls `CaptureAsync()`)

### Silent no-ops
- [x] `DotNet.Watch.cs:155-177` — `WithInclude`/`WithExclude`/`WithProperty` emit flags `dotnet watch` doesn't support (verified silently swallowed). Remove or fix
- [x] `DotNet.Run.cs` — `WithProject` and `WithFile` are mutually exclusive on the real CLI; builder emits both. Add guard

### Regression prevention
- [x] Fix the wrong assertion in `dot-net.pack.cs:95` and any other tests asserting broken flag strings
- [x] Add at least one smoke test per builder that actually executes the emitted command against the pinned SDK (not just string snapshots)

## Results

The builders now emit flags SDK 10.0.x accepts.

- `WithTerminalLogger` emits one argument, `--tl:<mode>`, on build, restore, run, test, publish, and pack.
- `WithCollect(string dataCollector)` emits `--collect` and the collector name.
- `DotNetPackBuilder.WithFramework` is removed. `dotnet pack` rejects `--framework` with MSB1001.
- `WithExport().WithExportPath(path)` emits `--export-path` only. `WithExport()` without a path throws.
- `nuget why` `WithProject` is a positional argument before the package id.
- `nuget delete` no longer emits `--configfile`. `WithNonInteractive()` emits `--non-interactive`.
- `dotnet watch` no longer has `WithInclude`, `WithExclude`, or `WithProperty`.
- `dotnet run` throws when `WithProject` and `WithFile` are both set.

Breaking public surface: removed pack `WithFramework`, watch `WithInclude` / `WithExclude` / `WithProperty`, and nuget delete `WithConfigFile`. `WithCollect()` now requires a collector name. `WithExport()` without `WithExportPath` throws. `WithProject` and `WithFile` on run throw together.

### How to validate

Smoke: `dotnet run tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.cli-smoke.cs` from the repo root.

Expect: all 10 smoke tests pass. Each one runs the builder against the SDK selected by a copy of `global.json` (10.0 feature band). Build, restore, publish, and pack with `--tl:off` in an empty directory report MSB1003 and not MSB1009. `dotnet run --tl:off` reports that it could not find a project, not MSB1009. `dotnet test` accepts `--collect "XPlat Code Coverage"`. `dotnet dev-certs https --check` completes without an unrecognized-option error. `dotnet nuget why` on `timewarp-amuru-tools.csproj` reports that the project does not depend on `TimeWarp.Amuru.NotADependency`. `dotnet nuget delete --non-interactive` against an empty source returns Not Found without prompting. `dotnet watch --list run` in an empty directory reports that no project file was found.

Snapshot coverage is in the existing `dot-net.*.cs` command tests. The aggregate runner is `cd tests/timewarp-amuru/multi-file-runners && dotnet run run-tests.cs`.

## Session

- Implementation: grok task-work (2026-10-04)

## Notes

Found by multi-agent release review. Verified-clean areas: Tool/* builders, PackageSearch, ListPackages, Workload, New, UserSecrets, Sln, Reference, AddPackage/RemovePackage ordering, NuGet source commands.
