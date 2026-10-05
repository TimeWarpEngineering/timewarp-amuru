# Add missing test coverage for public API areas

## Description

Test suite is healthy (551 pass, 1 skip, no flakes as of 2026-10-05) with good coverage of core, dot-net-commands, git-commands, and fzf — but two public API areas still have ZERO tests. Deliberately excludes what other tasks already track: concurrency (001), long output (002).

**Trimmed 2026-10-05:** git-commands coverage landed in task 099 (PR #109); 098 regression tests exist under `tests/timewarp-amuru/single-file-tests/repo-services/`; Direct file-system tests exist for every member except GetLocation and SetLocation. What remains is below.

## Checklist

- [x] `source/timewarp-amuru-tools/dot-net-commands/tool/` — all 8 builders (Install/List/Restore/Run/Search/Uninstall/Update) have no test file; every other DotNet command does (MEDIUM). Follow the `dot-net.*.cs` snapshot pattern and add each to `dot-net.cli-smoke.cs` where the real SDK can be exercised without side effects
- [x] ~~`extensions/CommandBuilderExtensions`~~ — being deleted in 094-001 (zero callers); no coverage needed
- [x] ~~git-commands untested members~~ — done in task 099 (PR #109): `git.find-root.cs`, `git.get-repository-name.cs`, `git.get-worktree-path.cs`, `git.is-worktree.cs`, `git.update-branch.cs`, `git.update-worktree.cs`
- [x] ~~`native/utilities/`~~ — `ConvertTimestamp`/`GenerateColor`/`Post`/`Installer` deleted in 094-001; `SshKeyHelper` moves to Zana with tests there (094-002)
- [x] `native/file-system/direct/` — `Direct.GetLocation` and `Direct.SetLocation` have no direct.* test (LOW). GetChildItem, RemoveItem and the rest were covered by task 104/112
- [x] ~~Regression tests for the 098 fixes~~ — present in `repo-services/repo-clean-service.cs` and `repo-services/nuget-package-service.cs`
- [x] ~~Sweep after 097-099~~ — 097/098/099 all merged with their own tests; nothing left to sweep
- [x] Full runner green: `dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs`

## Notes

Coverage map from multi-agent release review (2026-07-04): core 14 test files, configuration 4, dot-net-commands 20, git-commands 13, fzf 14, native file-system 7, script-support 4, mocks 1 direct + heavy indirect. Runner-exclusion problem was fixed in task 103. Paths relative to the repo root; dotnet builders live in the Tools package.

## Results

`dotnet tool` install, list, restore, run, search, uninstall, and update now have command-string tests in `dot-net.tool.cs`. The parent `DotNet.Tool()` builder is covered there too: it creates each sub-builder, and working directory, environment variables, and `WithNoValidation` stay off the command string. `dot-net.cli-smoke.cs` runs each subcommand against the SDK in an empty directory. Install and update use a NuGet config whose sources are cleared to an empty local folder, so the smoke fails the lookup before anything is installed. List, restore, run, and uninstall only read or reject a missing manifest, command, or package. Search asks the public feed for a term with no hits.

`Direct.GetLocation` / `Pwd` and `Direct.SetLocation` / `Cd` have `direct.get-location.cs` and `direct.set-location.cs`. SetLocation tests restore the process working directory.

### How to validate

**Smoke**

```bash
dotnet run tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.tool.cs
dotnet run tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.cli-smoke.cs
dotnet run tests/timewarp-amuru/single-file-tests/native/file-system/direct.get-location.cs
dotnet run tests/timewarp-amuru/single-file-tests/native/file-system/direct.set-location.cs
```

**Expect**

- `dot-net.tool.cs`: 28 passed. Basic and full flag sets match builder argument order for install, list, restore, run, search, uninstall, and update. A null package id, command name, or search term throws `ArgumentNullException`. The later of `Global()` and `Local()` is the only scope flag. Blank optional values omit their flags.
- `dot-net.cli-smoke.cs`: 18 passed. Tool list on an empty tool path exits 0 and prints `Package Id`. Tool restore with no manifest exits 0 and reports that no tools were restored. Tool run of `not-a-real-tool` fails with the missing-command message. Tool search for `zzznone-amuru` exits 0 and reports no results. Uninstall of a missing package fails. Install and update of a missing package fail, mention the empty local source, and leave the tool directory uncreated.
- `direct.get-location.cs`: 1 passed. `GetLocation` and `Pwd` match `Environment.CurrentDirectory`.
- `direct.set-location.cs`: 3 passed. `SetLocation` and `Cd` switch to a temp directory. A missing directory throws `DirectoryNotFoundException` and leaves the working directory unchanged.

**Automated**

```bash
dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs
# expect: Passed 604, Skipped 1, Failed 0 (Total 605)
ganda repo audit
# expect: exit 0
```

## Session

- Kitchen trim: 522eb63d (2026-10-05)
- Implementation: task 105 tests (2026-10-05)
