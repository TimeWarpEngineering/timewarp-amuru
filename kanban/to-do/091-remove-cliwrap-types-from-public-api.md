# Remove CliWrap types from public API

## Description

CliWrap types leak into the public API contract. Once 1.0 ships, CliWrap's type identity (and major version) is frozen into our contract — a CliWrap 4.x upgrade with breaking changes becomes our breaking change.

**BLOCKER for 1.0.**

## Checklist

- [ ] `core/command-options.cs:38` — `public CommandResultValidation? Validation { get; set; }` exposes CliWrap's enum. Replace with an Amuru-owned representation (coordinate with tasks 041/090 — the strongly-typed-methods approach may remove the need for a public enum entirely)
- [ ] `core/execution-result.cs:23` — public constructor takes `CliWrap.CommandResult`. Make the ctor internal (all construction is already internal)
- [ ] Audit for any other CliWrap types in public signatures (parameters, returns, base types)
- [ ] Verify with a public-API diff that CliWrap no longer appears in the surface

## Notes

Found by multi-agent release review (2026-07-04). Paths relative to `source/timewarp-amuru/`.
