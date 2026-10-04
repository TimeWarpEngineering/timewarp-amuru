# Add missing test coverage for public API areas

## Description

Test suite is healthy (551 pass, 1 skip, no flakes as of 2026-10-05) with good coverage of core, dot-net-commands, git-commands, and fzf — but two public API areas still have ZERO tests. Deliberately excludes what other tasks already track: concurrency (001), long output (002).

**Trimmed 2026-10-05:** git-commands coverage landed in task 099 (PR #109); 098 regression tests exist under `tests/timewarp-amuru/single-file-tests/repo-services/`; Direct file-system tests exist for every member except GetLocation and SetLocation. What remains is below.

## Checklist

- [ ] `source/timewarp-amuru-tools/dot-net-commands/tool/` — all 8 builders (Install/List/Restore/Run/Search/Uninstall/Update) have no test file; every other DotNet command does (MEDIUM). Follow the `dot-net.*.cs` snapshot pattern and add each to `dot-net.cli-smoke.cs` where the real SDK can be exercised without side effects
- [x] ~~`extensions/CommandBuilderExtensions`~~ — being deleted in 094-001 (zero callers); no coverage needed
- [x] ~~git-commands untested members~~ — done in task 099 (PR #109): `git.find-root.cs`, `git.get-repository-name.cs`, `git.get-worktree-path.cs`, `git.is-worktree.cs`, `git.update-branch.cs`, `git.update-worktree.cs`
- [x] ~~`native/utilities/`~~ — `ConvertTimestamp`/`GenerateColor`/`Post`/`Installer` deleted in 094-001; `SshKeyHelper` moves to Zana with tests there (094-002)
- [ ] `native/file-system/direct/` — `Direct.GetLocation` and `Direct.SetLocation` have no direct.* test (LOW). GetChildItem, RemoveItem and the rest were covered by task 104/112
- [x] ~~Regression tests for the 098 fixes~~ — present in `repo-services/repo-clean-service.cs` and `repo-services/nuget-package-service.cs`
- [x] ~~Sweep after 097-099~~ — 097/098/099 all merged with their own tests; nothing left to sweep
- [ ] Full runner green: `dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs`

## Notes

Coverage map from multi-agent release review (2026-07-04): core 14 test files, configuration 4, dot-net-commands 20, git-commands 13, fzf 14, native file-system 7, script-support 4, mocks 1 direct + heavy indirect. Runner-exclusion problem was fixed in task 103. Paths relative to the repo root; dotnet builders live in the Tools package.

## Session

- Kitchen trim: 522eb63d (2026-10-05)
