# Roll ICommandBuilder and WithNoValidation across all builders

## Description

**Re-scoped 2026-07-05** per the 094 decision record: `CommandBuilderExtensions` is being DELETED (094-001, zero callers), which removes the original combinator motivation for `ICommandBuilder<T>`. What remains is the validation-consistency problem, and it now gates **Tools stable**, not core 1.0:

**Refreshed 2026-10-05.** Task 090 decided: default validation is `None` (non-zero exit is reported via `ExitCode`/`Success`, never thrown); `WithZeroExitCodeValidation()` is the strict opt-in. Core `ShellBuilder` exposes both `WithNoValidation()` and `WithZeroExitCodeValidation()`. `DotNetBuilder` base exposes only `WithNoValidation()`, and these Tools builder files expose neither: `DotNet.DevCerts.cs`, `DotNet.NuGet.cs`, `DotNet.Reference.cs`, `DotNet.Sln.cs`, `DotNet.UserSecrets.cs`, `DotNet.Watch.cs`, `DotNet.Workload.cs`, plus `fzf-command/Fzf.cs`. Result: a caller cannot opt in to strict validation on those builders at all. Both packages are stable (1.1.1), so this is a gap in the released Tools API, fixed in the 2.0 cycle alongside 087/088/099.

**Decisions:**
- Expose **both** `WithNoValidation()` and `WithZeroExitCodeValidation()` on every builder, same names and semantics as `ShellBuilder`. Add `WithZeroExitCodeValidation()` to `DotNetBuilder` base so derived builders inherit it where the base is used.
- Keep `ICommandBuilder<T>` exactly as it is (10 implementers). Do not spread it to more builders and do not remove it: the PublicAPI baseline (task 094-004) is now in force and removal gains nothing.
- Naming drift: rename `DotNetListPackagesBuilder.WithConfig` → `WithConfigFile` and `DotNetWatchBuilder.WithTargetFramework` → `WithFramework` to match every other builder. These are breaking; record them under Results as removed/renamed members.
- PublicAPI analyzers are live: every added member goes in the package's `public-api/PublicAPI.Unshipped.txt`; every removed/renamed member gets a `*REMOVED*` line there. The build fails otherwise (RS0016/RS0017).

## Checklist

- [x] ~~Blocked by task 090~~ — resolved: default `None`; expose both controls uniformly (see Decisions)
- [ ] Add `WithZeroExitCodeValidation()` to `DotNetBuilder` base (`DotNet.Base.cs`) next to `WithNoValidation()`
- [ ] Add both controls to every builder class in: `DotNet.DevCerts.cs`, `DotNet.NuGet.cs`, `DotNet.Reference.cs`, `DotNet.Sln.cs`, `DotNet.UserSecrets.cs`, `DotNet.Watch.cs`, `DotNet.Workload.cs`, `fzf-command/Fzf.cs`. Verify with `grep -L WithZeroExitCodeValidation` over `dot-net-commands/*.cs` and `fzf-command/Fzf.cs` that none remain
- [ ] Confirm `DotNet.Tool.cs` and `DotNet.New.cs` sub-builders expose both (they already have `WithNoValidation`)
- [x] ~~Decide on `ICommandBuilder<T>`~~ — keep unchanged (see Decisions)
- [ ] Rename `DotNetListPackagesBuilder.WithConfig` → `WithConfigFile`; rename `DotNetWatchBuilder.WithTargetFramework` → `WithFramework`; update tests/docs; `*REMOVED*` lines in Tools `PublicAPI.Unshipped.txt`
- [ ] Update `public-api/PublicAPI.Unshipped.txt` for Tools (added members) so `./bin/dev build` is 0 warnings / 0 errors
- [ ] Tests: for each newly covered file, one test that `WithZeroExitCodeValidation()` makes a failing command throw and the default does not (use the SDK where safe, mocks otherwise); extend `dot-net.cli-smoke.cs` if a real-SDK case is cheap
- [ ] Full runner green: `dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs`; `ganda repo audit` clean

## Notes

Originally motivated by the API-surface review (2026-07-04); combinator half cancelled when `CommandBuilderExtensions` was verified to have zero callers in amuru + ganda. Paths relative to `source/timewarp-amuru-tools/`. 094-003 split and 094-004 baseline are done; this ships in the 2.0 cycle with 087/088/099.

## Session

- Kitchen refresh: 522eb63d (2026-10-05)
