# Cut 1.0.0 stable release

## Description

Capstone: ship **TimeWarp.Amuru 1.0.0 stable** (process-automation core). Per the 094 decision record (2026-07-05), 1.0 is CORE ONLY — the `TimeWarp.Amuru.Tools` package (DotNet/Git/Fzf wrappers + repo/nu-get services) ships alongside as `1.0.0-beta.x` and stabilizes on its own cadence.

Review basis: six-agent release review (2026-07-04), tasks 087-106.

## Checklist

### Core 1.0 gates (must be done)
- [x] 095 — TimeWarp.Terminal stable dependency (DONE 2026-07-05: bumped to 1.0.0, nuspec verified all-stable, suite green)
- [x] 094-001 — dead-surface deletes (Post, Installer, GenerateColor, ConvertTimestamp, CommandBuilderExtensions)
- [ ] 094-002 — SshKeyHelper moved to Zana (ganda side lands first)
- [ ] 094-003 — Tools package split (core drops NuGet.Versioning; 087/099/100 stop gating core)
- [x] 090 + 041 — error-handling contract: default None, WithZeroExitCodeValidation opt-in, never-ran reports failure (DONE 2026-07-05)
- [x] 089 — CommandMock matching/fallthrough fixes (DONE 2026-07-05: strict by default, all modes intercepted)
- [x] 091 — CliWrap types out of public signatures (DONE 2026-07-05)
- [ ] 092 — result-type unification (Terminal coupling settled 2026-07-05: stays; Terminal is stack-foundational and stable)
- [ ] 093 — XML docs for the core surface
- [ ] 097 — core engine bugs (at minimum the `StreamToFileAsync` race)
- [ ] 084 — JSON-RPC disable finalized (core 1.0 ships WITHOUT json-rpc; 083 post-1.0 additive)

### Strongly recommended before tagging
- [ ] 101 — release pipeline runs tests; tag-vs-version guard; workflow_dispatch defused
- [ ] 102 — readme samples compile (readme ships in the nupkg)
- [ ] 103 — slnx fix, runner exclusions, AGENTS.md (global.json DONE 2026-07-05)
- [ ] 096 — AOT declaration on core
- [ ] 098 — repo services fixes (rides in Tools, but the version-check bug affects THIS pipeline via dev-cli)

### Tools-stable gates (do NOT block core 1.0)
- 087 — invalid CLI flags; 088 — fzf stub; 099 — git standardization; 100 — validation-control rollout; 093/096 Tools portions

### Mechanical release steps
- [ ] Bump `source/Directory.Build.props` core `<Version>` to `1.0.0` in the release PR (Tools stays beta; confirm `ganda repo audit` exception per 094-003)
- [ ] Full suite + verify-samples green at the release commit
- [ ] Pack; inspect core nuspec: stable deps only, xml docs present, readme/icon present
- [ ] Tag `v1.0.0`, publish GitHub Release with notes (summarize the beta→stable journey + the core/Tools split)
- [ ] Verify nuget.org listing renders the readme
- [ ] Post-release: 094-004 (PublicAPI analyzer baseline), announce, open post-1.0 track (083 json-rpc, Tools stabilization, 104/105/106 remainders)

## Notes

Non-goals for core 1.0 (deferred, additive later): json-rpc (083), native command expansion (020-027), Claude/Git fluent APIs (004/005), timeout support (044), kijamii (031/033 — receives ganda `services/post/`, per 094 decision Nostr never ships in Amuru).
