# Cut 1.0.0 stable release

## Description

Capstone task: the actual act of shipping TimeWarp.Amuru 1.0.0 stable, once the blocking work is done. The full release-readiness review (2026-07-04, six-agent sweep of core/API/wrappers/native/tests/packaging) is encoded in tasks 087-106; this task tracks the gate list and the mechanical release steps.

## Checklist

### Blocking gates (must be done)
- [ ] 095 — TimeWarp.Terminal stable dependency (NU5104; external gate)
- [ ] 087 — invalid CLI flags in dotnet builders
- [ ] 088 — SelectWithFzf stub
- [ ] 089 — CommandMock real-execution fallthrough
- [ ] 090 — error-handling contract decision (with 041)
- [ ] 091 — CliWrap types out of public API
- [ ] 092 — result-type unification
- [ ] 093 — XML documentation in package
- [ ] 094 — 1.0 API surface scoped (Post/SshKeyHelper resolved)
- [ ] 084 — JSON-RPC disable finalized (ship 1.0 WITHOUT json-rpc; 083 lands post-1.0 as additive)

### Strongly recommended before tagging
- [ ] 097 — core engine bugs (at minimum StreamToFileAsync race)
- [ ] 098 — repo services (version-check bug affects this very pipeline)
- [ ] 099 — UpdateBranchAsync + git API consistency
- [ ] 101 — release pipeline runs tests; tag-vs-version guard
- [ ] 102 — readme samples compile (readme ships in the nupkg)
- [ ] 103 — slnx/global.json/test-runner fixes
- [ ] 096 — AOT declaration

### Mechanical release steps
- [ ] Bump `source/Directory.Build.props` `<Version>` to `1.0.0` in the release PR
- [ ] Full suite green at the release commit; verify-samples green
- [ ] Pack locally; inspect nuspec: stable deps only, xml docs present, readme/icon present
- [ ] Tag `v1.0.0`, publish GitHub Release with release notes (summarize the beta→stable journey)
- [ ] Verify nuget.org listing page renders the readme correctly
- [ ] Post-release: announce; open post-1.0 track (083 json-rpc replacement, 100 builder interface rollout if deferred, 104-106 remainders)

## Notes

Non-goals for 1.0 (explicitly deferred, additive later): json-rpc (083), native command expansion (020-027), Claude/Git fluent APIs (004/005), timeout support (044 — additive), kijamii (031/033).
