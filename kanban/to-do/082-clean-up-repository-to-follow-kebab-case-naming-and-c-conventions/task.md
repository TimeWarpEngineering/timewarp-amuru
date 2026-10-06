# Clean up repository to follow kebab-case naming and C# conventions

## Description

Rename the remaining PascalCase `.cs` files to kebab-case and replace every remaining `// TODO: Add purpose description` region stub with real Purpose/Design text, per the tw-csharp skill. Task **106** (pre-1.0 API polish sweep) is folded into this card; its still-open items are listed below and 106 is archived with a pointer here.

**Rewritten 2026-10-06.** The earlier checklist named folders that moved to the Tools package in the 094 split (`dot-net-commands`, `git-commands`, `fzf-command` are under `source/timewarp-amuru-tools/`) and files that no longer exist (`Installer.cs`, `CommandBuilderExtensions.cs`, `json-rpc/*`, `native/utilities/*`). Measured on master `75c8489`:

| Measure | Count |
|---|---:|
| `.cs` files under `source/` | 126 |
| still PascalCase basenames | 97 |
| files with `// TODO: Add purpose description` | 37 |

**Ship as one PR, no release.** File names and region comments do not change the public API, so `public-api/PublicAPI.Unshipped.txt` must stay untouched for both packages (the analyzer will fail the build if a signature changes by accident). Commit **per folder** (one `git mv` + region pass per commit) so the review can read it; the PR is still one.

## Requirements

- Every rename is `git mv` (history preserved). Type names, namespaces, and member names do not change. Only basenames change, to kebab-case per tw-csharp. Partial-class files keep their dotted shape in kebab form (`DotNet.NuGet.cs` → `dot-net.nu-get.cs`; `Git.UpdateBranch.cs` → `git.update-branch.cs`; `Fzf.LayoutOptions.cs` → `fzf.layout-options.cs`; `Direct.GetContent.cs` → `direct.get-content.cs`; `ICommandBuilder.cs` → `icommand-builder.cs`). Match the pattern already used by `core/` and `nu-get/` (done in tasks 080/081) and by the test tree (`tests/**` is already kebab).
- Exceptions per tw-csharp stay PascalCase: `Directory.Build.props`, `Directory.Packages.props`, `*.csproj`, `AGENTS.md`, `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt` (Roslyn requires those basenames; `public-api/` is already excluded from the kebab audit check).
- After the renames, fix every reference to an old file name: XML `<see href>`/doc comments, `documentation/**`, `readme.md`, `skills/amuru/SKILL.md`, `AGENTS.md`, kanban is **not** rewritten (history). `grep -rn` for each old basename must return only `kanban/` hits.
- Region stubs: replace each `// TODO: Add purpose description` with a real `#region Purpose` one-liner and a `#region Design` block that states the non-obvious decisions for that file (tw-agent-context-regions skill). No copy-paste between files; if a file is a trivial partial with nothing to say, a one-line Purpose and no Design region is acceptable. Zero stubs remain afterward.
- Folded from 106, verify each and fix if still present (most of 106 was done by 092/097/117): stale "RunBuilder" in `core/shell-builder.cs` XML docs (confirmed present at line 18 on master); any remaining "TimeWarp.Cli" doc text; `WithArguments(params string[])` vs `Pipe(params string[]?)` nullability on the same concept (pick one, prefer non-nullable, note that changing a parameter's nullability annotation does **not** change the PublicAPI text, verify by building); readme filename casing consistency between csproj/props and disk (`readme.md`); `PackageProjectUrl` is already set in `source/Directory.Build.props` for both packages (confirm, then check off).
- `./bin/dev build` 0 warnings / 0 errors; `dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs` green (expect 620 passed, 1 skipped); `ganda repo audit` clean; `git diff --stat origin/master -- source/*/public-api/` empty.

## Checklist

### Measure
- [ ] Record the exact PascalCase file list and the TODO-stub file list in Results before starting (`find source -name '*.cs' | xargs -n1 basename | grep -E '^[A-Z]'` and `grep -rl 'TODO: Add purpose description' source tools tests`)

### Tools package (`source/timewarp-amuru-tools/`), one commit per folder
- [ ] `dot-net-commands/*.cs` renamed + regions
- [ ] `dot-net-commands/tool/*.cs` renamed + regions
- [ ] `git-commands/*.cs` renamed + regions
- [ ] `fzf-command/*.cs` renamed + regions
- [ ] `repo/*.cs`, any other PascalCase under Tools renamed + regions

### Core package (`source/timewarp-amuru/`), one commit per folder
- [ ] root-level `AppContextExtensions.cs`, `CliConfiguration.cs`, `ScriptContext.cs` renamed + regions
- [ ] `native/**` (`Bash.cs`, `file-system/commands/Commands.*.cs`, `file-system/direct/Direct.*.cs`, `PathResolver.cs`) renamed + regions
- [ ] `testing/*.cs` (`CommandMock.cs`, `MockScope.cs`, `MockState.cs`, `MockSetup.cs`) renamed + regions
- [ ] `interfaces/ICommandBuilder.cs` renamed + regions
- [ ] `core/*.cs` region stubs (files are already kebab)

### Tools / tests / samples
- [ ] `tools/dev-cli/**`, `tests/**`, `samples/**`: any remaining PascalCase `.cs` (expected none; confirm) and any TODO stubs

### Folded from 106
- [ ] "RunBuilder" XML doc text fixed in `core/shell-builder.cs` (and `core/shell.cs` if present)
- [ ] No "TimeWarp.Cli" text remains in source
- [ ] `WithArguments` / `Pipe` params nullability made consistent; PublicAPI text unchanged
- [ ] readme casing consistent across csproj/props/disk
- [ ] `PackageProjectUrl` confirmed present for both packages

### Verification
- [ ] `grep` for every old basename returns only `kanban/` hits
- [ ] Zero `TODO: Add purpose description` remain
- [ ] `./bin/dev build` clean; full runner green; `ganda repo audit` clean; `public-api/` untouched
- [ ] Results: before/after counts, per-folder commit list, list of 106 items confirmed already done vs fixed here

## Notes

- 106 archived 2026-10-06 as folded into this card. Task 007 was archived earlier as a duplicate.
- Reference files with good regions: `source/timewarp-amuru/core/command-extensions.cs`, `source/timewarp-amuru/core/command-result.cs`.
- Renames are noisy for `git blame`; use `git log --follow` / `git blame -C` afterward. That is the accepted cost of doing it in one PR.
- Do not rename `kanban/**` or anything under `documentation/` that is already kebab.

## Session

- Created: ses_27dd18c7effe1K4rnFRhnQezjn (2026-04-13)
- Returned to to-do: 01a06a4a-807d-7143-9d21-330f32238619 (2026-09-04)
- Rewritten and 106 folded in: 522eb63d (2026-10-06)
