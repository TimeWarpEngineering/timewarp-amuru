# Roll ICommandBuilder and WithNoValidation across all builders

## Description

**Re-scoped 2026-07-05** per the 094 decision record: `CommandBuilderExtensions` is being DELETED (094-001, zero callers), which removes the original combinator motivation for `ICommandBuilder<T>`. What remains is the validation-consistency problem, and it now gates **Tools stable**, not core 1.0:

16 of 29 dot-net builder files lack `WithNoValidation` entirely. Since default validation throws on non-zero exit (task 090), `CaptureAsync()` on those builders can never return a failed `CommandOutput` and callers cannot opt out — while Build/Test/Pack/etc. can. Whatever 090 decides, all builders must expose the same validation controls before Tools goes stable.

## Checklist

- [ ] Blocked by task 090's contract decision (if the default flips to `None`, this becomes "expose `WithZeroExitCodeValidation` uniformly" instead)
- [ ] Add the decided validation methods to the non-covered builders: `FzfBuilder`, `DotNetWatchBuilder`, all `DotNetNuGet*` (12), `DotNetWorkload*` (13), `DotNetTool*` (8), `DotNetUserSecrets*` (6), `DotNetSln*` (5), `DotNetNew*` sub-builders (5), `DotNetReference*` (4), `DotNetDevCerts*` (2)
- [ ] Decide whether `ICommandBuilder<T>` itself still earns its keep with the combinators gone (14 implementers today); if not, consider removing the interface from the public surface in 094 instead of spreading it
- [ ] Fix naming drift while touching each: `WithConfig` (ListPackages) vs `WithConfigFile` (everywhere else); `WithFramework` vs `WithTargetFramework` (Watch)
- [ ] Tests: one validation-control test on a representative newly-covered builder

## Notes

Originally motivated by the API-surface review (2026-07-04); combinator half cancelled when `CommandBuilderExtensions` was verified to have zero callers in amuru + ganda. Gate: `TimeWarp.Amuru.Tools` stable, after the 094-003 split.
