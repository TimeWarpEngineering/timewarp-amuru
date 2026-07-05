# Scope the 1.0 public API surface

## Description

Parent task / decision record. The package exposed ~125 public types; each is a semver commitment at 1.0. Decisions below were made 2026-07-05 (owner + Claude session), superseding parts of the earlier Grok review where noted. Execution is broken into child tasks 094-001..094-004.

**BLOCKER for 1.0** (children 001–003; 004 lands at/after release).

**Artifacts:** [`review-findings.md`](./review-findings.md) has the cross-repo consumer analysis. Its Phase 2 recommendation (internalize + `InternalsVisibleTo`) is **superseded** by the decisions below — no `InternalsVisibleTo` anywhere.

## Decision record (2026-07-05)

**Purpose statement:** TimeWarp.Amuru (public) = process automation — fluent process execution, testing mocks, native shell-replacement ops. Zana (private, in ganda repo) = TimeWarp-internal counterpart. Kijamii (tasks 031/033) = future public social library.

**Constraints that shaped the decisions:**
- EVERY TimeWarp repo has a dev-cli consuming the repo/version-check services — copies don't scale, and public repos cannot take dependencies on private packages (Zana/ganda), including in their build process. So those services must stay in a PUBLIC package.
- Owner prefers a single version per repo (`ganda repo audit` checks this) but accepts core/Tools diverging here: tools move often, core should be stable.

**Decisions:**

| Item | Decision |
|------|----------|
| Packaging | Two packages: `TimeWarp.Amuru` (core) + `TimeWarp.Amuru.Tools` (DotNet/Git/Fzf wrappers + repo/ + nu-get/ services). Same repo, same `TimeWarp.Amuru` root namespace. Subdivide Tools later only if needed (cheap while beta) |
| 1.0 stable | Core only. Tools stays beta until 087/099/100 land |
| `Post.cs` | Delete. Social → ganda `services/post/` now, Kijamii later. Nothing Nostr-shaped ships in Amuru |
| `GenerateColor.cs`, `ConvertTimestamp.cs`, `Installer.cs`, `CommandBuilderExtensions.cs` | Delete — zero callers verified in amuru + ganda |
| `SshKeyHelper.cs` | Move to Zana (ganda is private and can dep on Zana; ganda is the only consumer). Fix its bugs during the move, on ganda's board |
| `repo/`, `nu-get/` | Stay PUBLIC, move to the Tools package (see constraint above). Bugs fixed here via task 098 |
| `WorktreePorcelainParser` | Internalize (assembly-internal only, no ItV). `WorktreeEntry` stays public |
| `InternalsVisibleTo` | Not used — rejected as private-code-with-public-fragility |
| PublicAPI analyzers | Adopt at/after 1.0 release (094-004) |
| `json-rpc/*` | Stays non-public (084); native replacement post-1.0 (083) |

**Explicit keep-public list (core 1.0 surface):** `Shell`, `ShellBuilder`, `CommandOutput`, `CommandResult`, `ExecutionResult` (shape may change in 090-092), `CommandMock`/`MockSetup`, `Bash`, `PathResolver`, file-system commands, `ScriptContext`, `CliConfiguration`, `WorktreeEntry` (moves with Git to Tools).

## Checklist

- [ ] 094-001 — Delete dead public surface
- [ ] 094-002 — Move SshKeyHelper to Zana
- [ ] 094-003 — Split TimeWarp.Amuru.Tools package
- [ ] 094-004 — Adopt PublicAPI analyzers baseline (at/after release)
- [ ] Update downstream tasks when children land (093, 098, 100, 105, 107 already re-scoped 2026-07-05)

## Session

- Created: multi-agent release review (2026-07-04)
- Folderized + review findings: Grok (2026-07-04)
- Decision record + child breakdown: Claude session (2026-07-05)
