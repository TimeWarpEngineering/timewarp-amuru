# Remove CliWrap types from public API

## Description

CliWrap types leak into the public API contract. Once 1.0 ships, CliWrap's type identity (and major version) is frozen into our contract — a CliWrap 4.x upgrade with breaking changes becomes our breaking change.

**BLOCKER for 1.0.**

## Checklist

- [x] `command-options.cs` — `Validation` property made internal (done in 090 commit; strongly-typed With* methods are the public surface)
- [x] `execution-result.cs:23` — `ExecutionResult` constructor made internal (all construction was already internal)
- [x] Audited: no remaining CliWrap types (Command, CommandResultValidation, PipeSource/Target) in public signatures
- [x] Verified via grep sweep; formal PublicAPI baseline lands with 094-004 post-release

## Notes

Found by multi-agent release review (2026-07-04). Paths relative to `source/timewarp-amuru/`.
