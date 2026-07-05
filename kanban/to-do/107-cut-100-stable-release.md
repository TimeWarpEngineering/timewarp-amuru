# Cut 1.0.0 stable release

## Description

Capstone: ship **TimeWarp.Amuru 1.0.0 stable** (process-automation core). Per the 094 decision record (2026-07-05), 1.0 is CORE ONLY — the `TimeWarp.Amuru.Tools` package (DotNet/Git/Fzf wrappers + repo/nu-get services) ships alongside as `1.0.0-beta.x` and stabilizes on its own cadence.

Review basis: six-agent release review (2026-07-04), tasks 087-106.

## Checklist

### Core 1.0 gates (must be done)
- [x] 095 — TimeWarp.Terminal stable dependency (DONE 2026-07-05: bumped to 1.0.0, nuspec verified all-stable, suite green)
- [x] 094-001 — dead-surface deletes (Post, Installer, GenerateColor, ConvertTimestamp, CommandBuilderExtensions)
- [x] 094-002 — SshKeyHelper moved to Zana with bugs fixed (DONE 2026-07-05; ganda commit a7001fc)
- [x] 094-003 — Tools package split (DONE 2026-07-05: TimeWarp.Amuru.Tools 1.0.0-beta.1, per-package push, Shell.Run factory)
- [x] 090 + 041 — error-handling contract: default None, WithZeroExitCodeValidation opt-in, never-ran reports failure (DONE 2026-07-05)
- [x] 089 — CommandMock matching/fallthrough fixes (DONE 2026-07-05: strict by default, all modes intercepted)
- [x] 091 — CliWrap types out of public signatures (DONE 2026-07-05)
- [x] 092 — result-type unification (DONE 2026-07-05: ExecutionResult deleted, CommandOutput is THE result type with RunTime + WriteToConsole)
- [x] 093 (core half) — XML docs generated, enforced, and shipping in the core nupkg (DONE 2026-07-05; Tools half gates Tools stable)
- [x] 097 — core engine bugs (DONE 2026-07-05: race, cancellation, Windows .cs, ConfigureAwait+CA2007, ScriptContext nesting)
- [x] 084 — JSON-RPC disable finalized (DONE 2026-07-05 except the design-doc note, which rides 102): dead files deleted, pins pruned, AOT verified clean)

### Strongly recommended before tagging
- [x] 101 — release pipeline hardened (DONE 2026-07-05: tests+samples in release mode, tag guard, dispatch defused, CI determinism, skip-duplicate)
- [x] 102 — readme rewritten with verified samples; vision doc bannered; doc links fixed (DONE 2026-07-05 except optional doc-compile-check nicety)
- [x] 103 — build/test infrastructure complete (DONE 2026-07-05: slnx, global.json, runner covers all 417 tests, AGENTS.md rewritten)
- [x] 096 — AOT declared on both packages, validated end-to-end with a native publish (DONE 2026-07-05)
- [x] 098 — repo services fixed (DONE 2026-07-05: tracked-file guard + symlink-safe clean, version-contains check, versionsort, unlisted filter)

### Tools-stable gates (do NOT block core 1.0)
- 087 — invalid CLI flags; 088 — fzf stub; 099 — git standardization; 100 — validation-control rollout; 093/096 Tools portions

### Mechanical release steps
- [ ] Bump `source/Directory.Build.props` core `<Version>` to `1.0.0` in the release PR (Tools stays beta; confirm `ganda repo audit` exception per 094-003)
- [ ] Full suite + verify-samples green at the release commit
- [ ] Pack; inspect core nuspec: stable deps only, xml docs present, readme/icon present
- [ ] Tag `v1.0.0`, publish GitHub Release with notes (summarize the beta→stable journey + the core/Tools split)
- [ ] Verify nuget.org listing renders the readme
- [ ] Post-release: update consuming repos' dev-clis to add the TimeWarp.Amuru.Tools reference; 094-004 (PublicAPI analyzer baseline), announce, open post-1.0 track (083 json-rpc, Tools stabilization, 104/105/106 remainders)

## Notes

Non-goals for core 1.0 (deferred, additive later): json-rpc (083), native command expansion (020-027), Claude/Git fluent APIs (004/005), timeout support (044), kijamii (031/033 — receives ganda `services/post/`, per 094 decision Nostr never ships in Amuru).
