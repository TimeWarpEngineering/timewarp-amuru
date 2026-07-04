# Roll ICommandBuilder and WithNoValidation across all builders

## Description

`ICommandBuilder<T>` is adopted by only 14 of ~74 builder classes (ShellBuilder + 13 `DotNet*` builders). Consequences: the `CommandBuilderExtensions` combinators (`When`/`Unless`/`Tap`/...) silently don't work on ~80% of builders, and 16 of 29 dot-net builder files lack `WithNoValidation` entirely — since default validation throws on non-zero exit (task 090), `CaptureAsync()` on those builders can never return a failed `CommandOutput` and callers cannot opt out. Retrofitting post-1.0 changes failure semantics.

## Checklist

- [ ] Implement `ICommandBuilder<T>` (including `WithNoValidation`) on the non-implementers: `FzfBuilder`, `DotNetWatchBuilder`, all `DotNetNuGet*` (12), `DotNetWorkload*` (13), `DotNetTool*` (8), `DotNetUserSecrets*` (6), `DotNetSln*` (5), `DotNetNew*` sub-builders (5), `DotNetReference*` (4), `DotNetDevCerts*` (2)
- [ ] Fix naming drift while touching each: `WithConfig` (ListPackages) vs `WithConfigFile` (everywhere else); `WithFramework` vs `WithTargetFramework` (Watch)
- [ ] Depends on task 090's contract decision (if the default flips to `None`, the urgency drops but the interface rollout still matters for the combinators)
- [ ] Depends on task 043 / 094 deciding whether `CommandBuilderExtensions` ships at all
- [ ] Tests: at least one combinator + one `WithNoValidation` test on a representative newly-covered builder

## Notes

Found by multi-agent release review (2026-07-04); flagged independently by the API-surface and wrapper reviewers. Adding interface implementations is non-breaking, so this can technically ride 1.0.x — but the missing `WithNoValidation` methods interact with the frozen error contract, so decide before 1.0.
