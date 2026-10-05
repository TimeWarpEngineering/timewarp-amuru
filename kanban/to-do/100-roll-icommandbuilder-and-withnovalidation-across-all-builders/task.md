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
- [x] Add `WithZeroExitCodeValidation()` to `DotNetBuilder` base (`DotNet.Base.cs`) next to `WithNoValidation()`
- [x] Add both controls to every builder class in: `DotNet.DevCerts.cs`, `DotNet.NuGet.cs`, `DotNet.Reference.cs`, `DotNet.Sln.cs`, `DotNet.UserSecrets.cs`, `DotNet.Watch.cs`, `DotNet.Workload.cs`, `fzf-command/Fzf.cs`. Verify with `grep -L WithZeroExitCodeValidation` over `dot-net-commands/*.cs` and `fzf-command/Fzf.cs` that none remain
- [x] Confirm `DotNet.Tool.cs` and `DotNet.New.cs` sub-builders expose both (they already have `WithNoValidation`)
- [x] ~~Decide on `ICommandBuilder<T>`~~ — keep unchanged (see Decisions)
- [x] Rename `DotNetListPackagesBuilder.WithConfig` → `WithConfigFile`; rename `DotNetWatchBuilder.WithTargetFramework` → `WithFramework`; update tests/docs; `*REMOVED*` lines in Tools `PublicAPI.Unshipped.txt`
- [x] Update `public-api/PublicAPI.Unshipped.txt` for Tools (added members) so `./bin/dev build` is 0 warnings / 0 errors
- [x] Tests: for each newly covered file, one test that `WithZeroExitCodeValidation()` makes a failing command throw and the default does not (use the SDK where safe, mocks otherwise); extend `dot-net.cli-smoke.cs` if a real-SDK case is cheap
- [x] Full runner green: `dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs`; `ganda repo audit` clean

## Notes

Originally motivated by the API-surface review (2026-07-04); combinator half cancelled when `CommandBuilderExtensions` was verified to have zero callers in amuru + ganda. Paths relative to `source/timewarp-amuru-tools/`. 094-003 split and 094-004 baseline are done; this ships in the 2.0 cycle with 087/088/099.

## Results

Every Tools command builder exposes `WithNoValidation()` and `WithZeroExitCodeValidation()` with the same semantics as `ShellBuilder`. Default validation stays `None`. `ICommandBuilder<T>` is unchanged (still 10 implementers).

Leaf builders that received a `CommandOptions` copy keep a writable field so a later validation call replaces that instance. `DotNetWatchRunBuilder`, `DotNetWatchTestBuilder`, and `DotNetWatchBuildBuilder` forward both calls to `DotNetWatchBuilder`, and `Build` reads the options after that update. Tool and New sub-builders did not already have `WithNoValidation`; both methods were added there too.

`grep -L WithZeroExitCodeValidation` over `source/timewarp-amuru-tools/dot-net-commands/*.cs`, `dot-net-commands/tool/*.cs`, and `fzf-command/Fzf.cs` lists no files.

### Removed / renamed members

- `DotNetListPackagesBuilder.WithConfig(string config)` → `WithConfigFile(string config)`
- `DotNetWatchBuilder.WithTargetFramework(string framework)` → `WithFramework(string framework)`

Both old signatures are `*REMOVED*` in `source/timewarp-amuru-tools/public-api/PublicAPI.Unshipped.txt`. The new members and every added validation method are in that file. `./bin/dev build`: 0 warnings, 0 errors.

### How to validate

**Smoke**

```bash
dotnet run tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.validation.cs
dotnet run tests/timewarp-amuru/single-file-tests/fzf-command/fzf-builder.validation.cs
dotnet run tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.cli-smoke.cs -- --filter-method ZeroExitCodeValidation
```

**Expect**

- `dot-net.validation.cs`: 10 passed. For base, dev-certs, nuget list source, reference list, sln list, user-secrets list, watch --list run, workload list, new list, and tool list, the default capture has `Success == false` and a non-zero exit and does not throw. The same command with `WithZeroExitCodeValidation()` on the leaf builder throws `CliWrap.Exceptions.CommandExecutionException`.
- `fzf-builder.validation.cs`: 1 passed. `WithFilter("no-such-item")` over `alpha`/`beta` reports a non-zero exit by default and throws `CommandExecutionException` when strict validation is set.
- `Build_WithZeroExitCodeValidation_Should_ThrowWhenProjectIsMissing`: a missing project under `DotNet.Build().WithZeroExitCodeValidation()` throws `CommandExecutionException`. The existing missing-project smoke without that call still returns output.

**Automated**

```bash
cd tests/timewarp-amuru/multi-file-runners && dotnet run run-tests.cs
# expect: Failed: 0 (after review: Passed 565, Skipped 1)
./bin/dev build
# expect: 0 Warning(s), 0 Error(s)
ganda repo audit
# expect: Passed 32, Failed 0
```

### Review disposition

- Effort 3, roster: general. Rounds: 2 (round 2 = re-verify of fix delta).
- Final counts: bug 1 wontfix; suggestion 2 fixed; nit 1 fixed; 0 open.
- Disposition: **accepted-exceptions**. M1 (`Fzf.FromCommand` validation and options apply to the source command only, because `CommandResult.Pipe` has no options overload) predates this task and needs a new core API. It is documented in `Fzf.cs` and is not fixed here.
- Review fixes: parent-then-child and revert-to-no-validation tests in `dot-net.validation.cs` (12 passed), `[Timeout]` on the fzf validation test, and a sub-builder option-snapshot note in `dot-net.md`.
- Post-fix gates: `./bin/dev build` 0 warnings / 0 errors; full runner Passed 565, Skipped 1, Failed 0; `ganda repo audit` passes.
- Artifacts: `review/review-framework.md`, `review/round-1/{general,merged}.md`, `review/round-2/{general,merged}.md`, `review/disposition.md`.

## Session

- Kitchen refresh: 522eb63d (2026-10-05)
- Implementation: implementer-grok (2026-10-05)
- Review: review oracle Claude Opus 5.5 + general subagent (Sonnet) accaaa8e773eeec7f (2026-10-05)
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 200 — 2026-10-05T02:07:01Z
