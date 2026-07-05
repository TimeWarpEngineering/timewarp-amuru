# Task 094 — Review findings (for implementing agent)

**Reviewer:** Grok (2026-07-04 session)  
**Repos cross-checked:** `timewarp-amuru/dev`, `timewarp-ganda/Cramer-2026-06-29-dev`  
**Paths below:** relative to `source/timewarp-amuru/` unless noted.

This document captures analysis and **recommended decisions** from a pre-implementation review. The implementing agent should treat these as the starting plan; adjust only if code or consumers have changed since this review.

---

## Executive summary

Task 094 is correctly prioritized as a **1.0 blocker**. Do it before task 093 (XML docs) and 096 (AOT) — every type removed now is documentation and trimming work avoided later.

The original checklist is accurate on bugs and awkward surface area. This review adds:

1. **Cross-repo consumer map** (ganda/dev-cli dependencies the flat task did not list).
2. **Concrete per-item verdicts** (delete / internalize / keep).
3. **Post + Nostr resolution** — delete Amuru stub; social posting lives in ganda; future public package is **TimeWarp.Kijamii** (tasks 031/033), not Amuru.
4. **Execution phases** so Phase 1 (safe deletes) can land as one PR before `InternalsVisibleTo` work.
5. **Gaps** — items not in the original checklist that should be explicitly kept public.

---

## Decision table

| Item | Verdict | Consumers | Notes |
|------|---------|-----------|-------|
| `Post.cs` | **Delete** | None | Ganda owns social (`services/post/`). Kijamii later. |
| `SshKeyHelper.cs` | **Internalize** (+ fix in follow-up) | **ganda** `ssh-key-helper` endpoint | Broken `GenerateKeyPairAsync`; ganda calls it today. |
| `Installer.cs` (Amuru) | **Delete** | None in amuru | Ganda uses `Zana.Installer`; duplicate. |
| `repo/*` + `nu-get/*` | **Internalize** (as one unit) | ganda dev-cli, ganda `repo-check-version`, amuru dev-cli `check-version` | Task 098 fixes bugs regardless. |
| `GenerateColor.cs` | **Delete** | None | Ganda reimplemented inline; Zana has own helper. |
| `ConvertTimestamp.cs` | **Delete** | None | Ganda reimplemented inline. |
| `WorktreePorcelainParser` | **Internalize** | Amuru tests only | Zana has separate `ParsePorcelain`. |
| `WorktreeEntry` | **Keep public** (for now) | Tests, XML examples | Naming polish → task 106. |
| `CommandBuilderExtensions` | **Delete** | None | Zero usage; ~14/74 builders implement `ICommandBuilder<T>`. |
| `CommandMock` / `MockSetup` | **Keep public** | Tests, consumers | Not in original checklist; core 1.0 story. |
| `fzf-command/*` | **Keep public** | Consumers | Fix stub in 088, don't cut. |
| `json-rpc/*` | **Stay non-public** | None (084 disabled) | Already commented out. |

---

## Must not ship as-is

### Post — DELETE

- Amuru `native/utilities/Post.cs` is a pre-migration stub (`ToNostr`/`ToX` print "Would post..." and return `true`).
- **No callers** in amuru or ganda.
- Canonical implementation: `timewarp-ganda/source/timewarp-ganda/services/post/` (`NostrPostService`, `XPostService`, config/crypto).
- CLI: `ganda post`, `ganda post config example`, etc. (ganda task 093).
- **Do not** implement Nostr in Amuru for 1.0. Social → **TimeWarp.Kijamii** (tasks 031/033) when ready.

### SshKeyHelper — INTERNALIZE (not delete)

**Bugs confirmed** (original task text is correct):

1. `GenerateKeyPairAsync` (:51-52) — entire flag string passed as one `WithArguments` element; `ssh-keygen` always fails.
2. `ChangePassphraseAsync` (:143-159) — stdin won't satisfy `ssh-keygen -p` TTY prompts; hangs in CI.
3. `ConvertKeyFormatAsync` (:195-201) — destructively rewrites input key in place.

**Live consumer — do not delete without replacement:**

```
timewarp-ganda/source/timewarp-ganda/endpoints/ssh-key-helper-command.cs
```

Ganda `global-usings.cs` imports `TimeWarp.Amuru.Native.Utilities`, so `ganda ssh-key-helper` hits this code path today.

**Recommended 094 action:**

- Mark `SshKeyHelper` `internal`.
- Add `InternalsVisibleTo` for ganda (or move implementation to Zana in a follow-up task).
- **Separate fix task** (not scoping): fix argv splitting or rewrite ganda command to use `Shell.Builder("ssh-keygen")` with discrete args.
- Do **not** ship as `[Experimental]` — broken is broken.
- Longer term: SSH key utilities belong in **Zana** `native/utilities/`, not Amuru 1.0 public surface.

---

## Decide keep / internalize / split

### Installer (Amuru) — DELETE

- `source/timewarp-amuru/Installer.cs` — **zero callers** in amuru repo.
- Ganda `install` command uses `Zana.Installer.InstallUtilitiesAsync` (`timewarp-zana/native/utilities/installer.cs`).
- Security issues cited in task (PowerShell injection, predictable temp path, hardcoded x64, cleanup outside `finally`) apply to **Zana's** copy if that command stays — not a reason to keep Amuru's duplicate.

### repo/ + nu-get/ — INTERNALIZE together

**Consumers verified:**

| Type | Used by |
|------|---------|
| `RepoCheckVersionService`, `INuGetPackageService`, `NuGetPackageService` | ganda `repo-check-version-command`, ganda `service-registration.cs`, amuru `tools/dev-cli/endpoints/check-version-command.cs` |
| `RepoCleanService`, `IRepoCleanService` | ganda `tools/dev-cli/dev.cs` (DI registration) |

These encode TimeWarp-internal conventions (e.g. hardcoded `source/Directory.Build.props`). Not appropriate as semver-stable public API for external consumers.

**094 action:** `internal` on `repo/*`, `nu-get/*` (interfaces, services, models). `InternalsVisibleTo` for `TimeWarp.Ganda` and dev-cli friend assemblies.

**Task 098 still required** — delete-safety and version-check bugs affect the release pipeline independent of visibility.

**Post-1.0:** consider moving to Zana (kanban/workspace pattern).

### GenerateColor / ConvertTimestamp — DELETE

**Surprise:** ganda CLI does **not** use Amuru implementations.

- `ganda convert-timestamp` — inline `DateTimeOffset.FromUnixTimeSeconds` (~5 lines).
- `ganda generate-color` — inline SHA256 → RGB/HSL.
- Zana `repo-setup-service.cs` has its own `GenerateColor(string seed)`.

Amuru versions are richer but have **zero callers** in either repo. Task 028 leftovers.

### WorktreePorcelainParser — INTERNALIZE

- Public parser is an implementation detail; only amuru integration tests reference it directly.
- Zana `worktree-service.cs` calls `Git.WorktreeListPorcelainAsync` but implements **its own** `ParsePorcelain` — duplicate logic (dedupe is a follow-up, not 094).
- Keep `WorktreeEntry` public, or add `Git.ParseWorktreeList(string)` and internalize both.
- `WorktreeEntry` missing `Git` prefix → task 106 polish.

### CommandBuilderExtensions — DELETE

- Zero usage outside XML examples in the file itself.
- Only builders implementing `ICommandBuilder<T>` can use `When`/`WhenNotNull`/etc. (~14 dotnet builders + `ShellBuilder`).
- If cut: cancel or defer task 100.
- Task 043 is a **different** question (Ghq/Gwq/Fzf builder extensions).

---

## Explicit keep list (not in original checklist)

Confirm these stay **public** for 1.0:

| Surface | Rationale |
|---------|-----------|
| `Shell`, `ShellBuilder`, `CommandOutput`, `CommandResult`, `ExecutionResult` | Core execution API (tasks 090-092 may reshape) |
| `Git.*`, `DotNet.*` fluent builders | Primary consumer value |
| `Fzf.*` | Public; fix 088 stub, don't cut |
| `CommandMock`, `MockSetup` | Testing story; task 089 |
| `Bash`, `PathResolver`, file-system commands | Layer 4 aliases / native ops |
| `ScriptContext`, `CliConfiguration` | Script execution scope |
| `WorktreeEntry` | Paired with git worktree APIs |

---

## Wrap-up deliverables (expand original checkbox)

1. **`PublicAPI.Shipped.txt`** (or Roslyn Public API Analyzers) — checked in, enforced in CI.
2. **`InternalsVisibleTo`** in Amuru for friend assemblies (ganda, dev-cli).
3. **Diff summary** — types removed vs kept, with consumer notes.
4. Update task 093/105/100 checklists if decisions change downstream scope.

---

## Execution phases

### Phase 1 — Delete dead public surface (one PR, no `InternalsVisibleTo`)

- Delete: `Post.cs`, `GenerateColor.cs`, `ConvertTimestamp.cs`, Amuru `Installer.cs`, `CommandBuilderExtensions.cs`
- No behavior change for existing consumers of kept API.

### Phase 2 — Internalize ecosystem types

- `internal`: `SshKeyHelper`, `repo/*`, `nu-get/*`, `WorktreePorcelainParser`
- Add `InternalsVisibleTo` for ganda + dev-cli
- Coordinate ganda package reference / friend assembly name

### Phase 3 — Guards and handoff

- Land `PublicAPI.Shipped.txt`
- Unblock task 093

### Follow-up tasks (out of 094 scope)

- Fix `SshKeyHelper` bugs or move to Zana + fix ganda `ssh-key-helper`
- Task 098 repo/nuget correctness
- Dedupe Zana `ParsePorcelain` vs Amuru parser
- Zana installer security fixes if `ganda install` stays

---

## Downstream task impact

| Task | Effect |
|------|--------|
| 093 XML docs | Smaller CS1591 burn-down |
| 098 repo bugs | Still required |
| 100 ICommandBuilder rollout | Cancel if extensions deleted |
| 105 test coverage | Drops Post/Installer/utility items |
| 106 polish | WorktreeEntry naming remains |
| 107 release gate | 094 remains blocking |

---

## Type count

Original task cites **123 public types**. Spot check: **~125** `public class/interface/enum/struct/record` declarations — close enough.

---

## Open questions for implementing agent

1. **Friend assembly names** — confirm exact assembly names for `InternalsVisibleTo` (ganda editions may publish multiple assemblies).
2. **SshKeyHelper fix timing** — same PR as internalize, or immediate child task before ganda release?
3. **WorktreeEntry** — keep public with internal parser, or add `Git.ParseWorktreeList` facade?
4. **PublicAPI analyzer** — adopt `Microsoft.CodeAnalysis.PublicApiAnalyzers` or hand-maintained `PublicAPI.Shipped.txt`?

---

## Session

- Original task: multi-agent release review (2026-07-04)
- Folderized + review findings: Grok session (2026-07-04)