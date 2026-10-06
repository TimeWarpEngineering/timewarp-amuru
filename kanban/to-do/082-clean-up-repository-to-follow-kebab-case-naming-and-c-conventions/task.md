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
- [x] Record the exact PascalCase file list and the TODO-stub file list in Results before starting (`find source -name '*.cs' | xargs -n1 basename | grep -E '^[A-Z]'` and `grep -rl 'TODO: Add purpose description' source tools tests`)

### Tools package (`source/timewarp-amuru-tools/`), one commit per folder
- [x] `dot-net-commands/*.cs` renamed + regions
- [x] `dot-net-commands/tool/*.cs` renamed + regions
- [x] `git-commands/*.cs` renamed + regions
- [x] `fzf-command/*.cs` renamed + regions
- [x] `repo/*.cs`, any other PascalCase under Tools renamed + regions

### Core package (`source/timewarp-amuru/`), one commit per folder
- [x] root-level `AppContextExtensions.cs`, `CliConfiguration.cs`, `ScriptContext.cs` renamed + regions
- [x] `native/**` (`Bash.cs`, `file-system/commands/Commands.*.cs`, `file-system/direct/Direct.*.cs`, `PathResolver.cs`) renamed + regions
- [x] `testing/*.cs` (`CommandMock.cs`, `MockScope.cs`, `MockState.cs`, `MockSetup.cs`) renamed + regions
- [x] `interfaces/ICommandBuilder.cs` renamed + regions
- [x] `core/*.cs` region stubs (files are already kebab)

### Tools / tests / samples
- [x] `tools/dev-cli/**`, `tests/**`, `samples/**`: any remaining PascalCase `.cs` (expected none; confirm) and any TODO stubs

### Folded from 106
- [x] "RunBuilder" XML doc text fixed in `core/shell-builder.cs` (and `core/shell.cs` if present)
- [x] No "TimeWarp.Cli" text remains in source
- [x] `WithArguments` / `Pipe` nullability checked and left as shipped (PublicAPI text encodes `?` vs `!`; unifying it would edit `PublicAPI.Shipped.txt`)
- [x] readme casing consistent across csproj/props/disk
- [x] `PackageProjectUrl` confirmed present for both packages

### Verification
- [x] `grep` for every old basename returns only `kanban/` hits
- [x] Zero `TODO: Add purpose description` remain
- [x] `./bin/dev build` clean; full runner green; `ganda repo audit` clean; `public-api/` untouched
- [x] Results: before/after counts, per-folder commit list, list of 106 items confirmed already done vs fixed here

## Notes

- 106 archived 2026-10-06 as folded into this card. Task 007 was archived earlier as a duplicate.
- Reference files with good regions: `source/timewarp-amuru/core/command-extensions.cs`, `source/timewarp-amuru/core/command-result.cs`.
- Renames are noisy for `git blame`; use `git log --follow` / `git blame -C` afterward. That is the accepted cost of doing it in one PR.
- Do not rename `kanban/**` or anything under `documentation/` that is already kebab.

## Results

Measured on this worktree before the renames (same tree as master `75c8489` plus the kitchen rewrite `b57bc72`).

| Measure | Before | After |
|---|---:|---:|
| Tracked `.cs` under `source/` | 114 | 114 |
| On-disk `find source -name '*.cs'` (includes `obj/`) | 126 | obj varies after build |
| PascalCase `.cs` basenames under `source/` | 97 | 0 |
| Extra PascalCase `.cs` outside `source/` | 1 (`tests/timewarp-amuru/TestHelpers.cs`) | 0 |
| `TODO: Add purpose description` under `source/`, `tools/`, `tests/` | 37 | 0 |

`tools/dev-cli/**` and `samples/**` had no PascalCase `.cs` and no purpose stubs. `tests/**` had only `TestHelpers.cs`.

### PascalCase files before

`source/timewarp-amuru-tools/dot-net-commands/`: `DotNet.cs`, `DotNet.AddPackage.cs`, `DotNet.Base.cs`, `DotNet.Build.cs`, `DotNet.Clean.cs`, `DotNet.DevCerts.cs`, `DotNet.ListPackages.cs`, `DotNet.New.cs`, `DotNet.NuGet.cs`, `DotNet.Pack.cs`, `DotNet.PackageSearch.cs`, `DotNet.Publish.cs`, `DotNet.Reference.cs`, `DotNet.RemovePackage.cs`, `DotNet.Restore.cs`, `DotNet.Run.cs`, `DotNet.Sln.cs`, `DotNet.Test.cs`, `DotNet.UserSecrets.cs`, `DotNet.Watch.cs`, `DotNet.Workload.cs`.

`dot-net-commands/tool/`: `DotNet.Tool.cs`, `DotNet.Tool.Install.cs`, `DotNet.Tool.List.cs`, `DotNet.Tool.Restore.cs`, `DotNet.Tool.Run.cs`, `DotNet.Tool.Search.cs`, `DotNet.Tool.Uninstall.cs`, `DotNet.Tool.Update.cs`.

`git-commands/`: `Git.cs`, `Git.BranchExists.cs`, `Git.CloneBare.cs`, `Git.Fetch.cs`, `Git.FetchRefspec.cs`, `Git.FindRoot.cs`, `Git.GetCommitsAhead.cs`, `Git.GetDefaultBranch.cs`, `Git.GetRepositoryName.cs`, `Git.GetWorktreePath.cs`, `Git.IsWorktree.cs`, `Git.RemoteHead.cs`, `Git.SetUpstream.cs`, `Git.UpdateBranch.cs`, `Git.UpdateWorktree.cs`, `Git.WorktreeAdd.cs`, `Git.WorktreeAddNewBranch.cs`, `Git.WorktreeList.cs`, `Git.WorktreePorcelainParser.cs`, `Git.WorktreeRemove.cs`.

`fzf-command/`: `Fzf.cs`, `Fzf.DisplayOptions.cs`, `Fzf.Extensions.cs`, `Fzf.HistoryOptions.cs`, `Fzf.InputMethods.cs`, `Fzf.InterfaceOptions.cs`, `Fzf.LayoutOptions.cs`, `Fzf.PreviewOptions.cs`, `Fzf.ScriptingOptions.cs`, `Fzf.SearchOptions.cs`.

`repo/`: `IRepoCheckVersionService.cs`, `IRepoCleanService.cs`, `RepoCheckVersionService.cs`, `RepoCleanService.cs`.

Core root: `AppContextExtensions.cs`, `CliConfiguration.cs`, `ScriptContext.cs`.

`native/`: `PathResolver.cs`, `aliases/Bash.cs`, twelve `Commands.*.cs`, eleven `Direct.*.cs`.

`testing/`: `CommandMock.cs`, `MockBehavior.cs`, `MockScope.cs`, `MockSetup.cs`, `MockState.cs`.

`interfaces/ICommandBuilder.cs`. Tests: `TestHelpers.cs`.

### Purpose stubs before

`DotNet.AddPackage.cs`, `DotNet.Base.cs`, `DotNet.Clean.cs`, `DotNet.ListPackages.cs`, `DotNet.New.cs`, `DotNet.PackageSearch.cs`, `DotNet.Reference.cs`, `DotNet.RemovePackage.cs`, `DotNet.Sln.cs`, `DotNet.UserSecrets.cs`, `DotNet.Workload.cs`, all eight `tool/DotNet.Tool*.cs`, `Fzf.DisplayOptions.cs`, `Fzf.HistoryOptions.cs`, `Fzf.InputMethods.cs`, `Fzf.InterfaceOptions.cs`, `Fzf.ScriptingOptions.cs`, `Fzf.SearchOptions.cs`, `Git.CloneBare.cs`, `Git.FindRoot.cs`, `Git.GetRepositoryName.cs`, `Git.IsWorktree.cs`, `Git.WorktreeAdd.cs`, `Git.WorktreeAddNewBranch.cs`, `Git.WorktreePorcelainParser.cs`, `core/shell-builder.cs`, `interfaces/ICommandBuilder.cs`, `Commands.GetLocation.cs`, `Direct.GetLocation.cs`, `testing/MockSetup.cs`.

Each stub is now a one-line Purpose plus a Design region that states a decision for that file. Files that already had real regions were renamed only.

### Per-folder commits

| Commit | Folder |
|---|---|
| `157881c` | `source/timewarp-amuru-tools/dot-net-commands/` (not `tool/`) |
| `6e80ae7` | `dot-net-commands/tool/` |
| `4f5ebe5` | `git-commands/` |
| `3ab3803` | `fzf-command/` |
| `5638624` | `repo/` |
| `dac6af5` | core root (`app-context-extensions.cs`, `cli-configuration.cs`, `script-context.cs`) |
| `942acc1` | `native/**` |
| `0d7cc1b` | `testing/` |
| `af986a2` | `interfaces/icommand-builder.cs` |
| `a64c84e` | `core/shell-builder.cs` regions and the RunBuilder doc fix |
| `051b5af` | `tests/timewarp-amuru/test-helpers.cs` and its `Directory.Build.props` include |
| `252986d` | both csproj readme includes |
| `42c4af6` | documentation basename references |

Renames are recorded as git renames (96–100% similarity). Type names, namespaces, and members are unchanged.

### Folded from 106

- **Fixed here:** `core/shell-builder.cs` constructor doc said "RunBuilder". It now says `ShellBuilder`. `core/shell.cs` had no RunBuilder text.
- **Fixed here:** both packable csproj files said `README.md` while `source/Directory.Build.props` and the file on disk say `readme.md`. They now pack `readme.md`. Both built nuspecs contain `<readme>readme.md</readme>`.
- **Already done:** no `TimeWarp.Cli` text remains under `source/`. `cli-configuration.cs` already documents TimeWarp.Amuru. The package-search tests still pass the string `TimeWarp.Cli` as a search term. Kanban and historical docs still say the old product name; those were not rewritten.
- **Already done:** `PackageProjectUrl` is `https://timewarp.software/projects/timewarp-amuru/` in `source/Directory.Build.props`. Neither csproj clears it. Both packed nuspecs emit that `projectUrl`. `ganda repo audit` reports the PackageProjectUrl check passed.
- **Left as shipped:** `CommandResult.Pipe` and `DotNetRunBuilder.WithArguments` are `params string[]?`. `ShellBuilder.WithArguments`, `ShellBuilder.Pipe`, and `DotNetBuilder.WithArguments` are `params string[]`. `PublicAPI.Shipped.txt` already records that difference as `string![]?` versus `string![]!`. Making them match would change the shipped public-API text, which this no-release PR must not do. `git diff --stat origin/master -- source/*/public-api/` is empty.

Old basenames were updated in `documentation/conceptual/architectural-layers.md` and `documentation/developer/progressive-enhancement-pattern.md`. A search of each old basename outside `kanban/` returns no hits.

### How to validate

Smoke:

```bash
./bin/dev build
dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs
ganda repo audit
git diff --stat origin/master -- source/timewarp-amuru/public-api source/timewarp-amuru-tools/public-api
rg -n "TODO: Add purpose description" source tools tests samples
```

Expect:

- Build succeeds with 0 warnings and 0 errors.
- The runner prints `Passed: 620` and `Skipped: 1` (total 621) and exits 0.
- `ganda repo audit` reports Passed 32, Failed 0, including `kebab-path-names`.
- The public-api diff is empty.
- The purpose-stub search prints nothing.

## Session

- Created: ses_27dd18c7effe1K4rnFRhnQezjn (2026-04-13)
- Returned to to-do: 01a06a4a-807d-7143-9d21-330f32238619 (2026-09-04)
- Rewritten and 106 folded in: 522eb63d (2026-10-06)
- Implemented renames, regions, and the 106 checks: 42c4af6 (2026-10-06)
