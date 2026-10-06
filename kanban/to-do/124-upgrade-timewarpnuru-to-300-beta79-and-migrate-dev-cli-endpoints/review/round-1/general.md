# Round 1 — general
**Date:** 2026-10-06
**Scope reviewed:** Directory.Packages.props, tools/dev-cli/global-usings.cs, tools/dev-cli/endpoints/*.cs

## Summary

Pin bump plus mechanical migration of all five dev-cli endpoints to the beta.79 Mediator contracts: `ValueTask<Unit>` → `Task<Unit>`, static `Unit` import dropped in favour of explicit `Unit.Value`, the two `internal` endpoints made `public` (required by the generated public `Send` overloads), handler parameter renamed to `cancellationToken` (CA1725) and a `ThrowIfNull` guard added (CA1062). Grep confirms no remaining `ValueTask`, bare `Value`, `return default`, or `internal` endpoint in tools/dev-cli. No `#pragma`/`NoWarn` additions; source/ and public-api/ untouched; no unnecessary Mediator CPM pins. Low risk; implementer reports build, tests, workflow, AOT publish and audit green.

## Issues

None.
