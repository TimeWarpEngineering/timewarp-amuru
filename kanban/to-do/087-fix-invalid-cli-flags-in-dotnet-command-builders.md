# Fix invalid CLI flags in dotnet command builders

## Description

Release review (2026-07-04) verified several builder options against SDK 10.0.301 that emit flags the real `dotnet` CLI rejects or ignores. Any use of these options fails the command outright (or silently does nothing). These shipped through 34 betas because tests only snapshot the emitted argument string — `tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.pack.cs:95` asserts the broken `--tl on` form.

**BLOCKER for 1.0.**

## Checklist

### Hard failures (verified against SDK 10.0.301)
- [ ] `WithTerminalLogger` emits `--tl <mode>` as two tokens; CLI parses `<mode>` as a positional PROJECT arg (`MSB1009`). Use `--tl:<mode>`. Six copy-paste sites:
  - [ ] `dot-net-commands/DotNet.Build.cs:341-342`
  - [ ] `dot-net-commands/DotNet.Restore.cs:296-297`
  - [ ] `dot-net-commands/DotNet.Run.cs:376-377`
  - [ ] `dot-net-commands/DotNet.Test.cs:380-381`
  - [ ] `dot-net-commands/DotNet.Publish.cs:433-434`
  - [ ] `dot-net-commands/DotNet.Pack.cs:344-345`
- [ ] `DotNet.Test.cs:247` — `WithCollect()` emits bare `--collect`; CLI requires a data-collector name. Change to `WithCollect(string dataCollector)`
- [ ] `DotNet.Pack.cs:85` — `WithFramework` emits `--framework`, unsupported by `dotnet pack` (MSB1001). Remove
- [ ] `DotNet.DevCerts.cs:108` — `WithExport()` emits `--export`, which doesn't exist; export is via `-ep|--export-path`. Fix the `WithExport().WithExportPath()` pairing
- [ ] `DotNet.NuGet.cs:1159` — `nuget why` builder's `WithProject` emits `--project`; the CLI takes project as positional. Fix
- [ ] `DotNet.NuGet.cs:494-495` — `nuget delete` builder emits `--configfile`, not accepted by delete (copy-paste from push). Remove; also add `--non-interactive` support (delete prompts by default and stalls `CaptureAsync()`)

### Silent no-ops
- [ ] `DotNet.Watch.cs:155-177` — `WithInclude`/`WithExclude`/`WithProperty` emit flags `dotnet watch` doesn't support (verified silently swallowed). Remove or fix
- [ ] `DotNet.Run.cs` — `WithProject` and `WithFile` are mutually exclusive on the real CLI; builder emits both. Add guard

### Regression prevention
- [ ] Fix the wrong assertion in `dot-net.pack.cs:95` and any other tests asserting broken flag strings
- [ ] Add at least one smoke test per builder that actually executes the emitted command against the pinned SDK (not just string snapshots)

## Notes

Found by multi-agent release review. Verified-clean areas: Tool/* builders, PackageSearch, ListPackages, Workload, New, UserSecrets, Sln, Reference, AddPackage/RemovePackage ordering, NuGet source commands.
