# Add missing test coverage for public API areas

## Description

Test suite is healthy (409 pass, no flakes) with good coverage of core, dot-net-commands, git-commands, and fzf arg-building — but several public API areas have ZERO tests. Deliberately excludes what other tasks already track: concurrency (001), long output (002), builder smoke tests (087), fzf execution (088), mock regressions (089).

## Checklist

- [ ] `dot-net-commands/tool/` — all 8 builders (Install/List/Restore/Run/Search/Uninstall/Update) have no test file; every other DotNet command does (MEDIUM)
- [x] ~~`extensions/CommandBuilderExtensions`~~ — being deleted in 094-001 (zero callers); no coverage needed
- [ ] git-commands untested members: `Git.FindRoot`, `Git.GetRepositoryName`, `Git.GetWorktreePath`, `Git.IsWorktree`, `Git.UpdateBranch`, `Git.UpdateWorktree` (MEDIUM — coordinate with task 099 fixes)
- [x] ~~`native/utilities/`~~ — `ConvertTimestamp`/`GenerateColor`/`Post`/`Installer` deleted in 094-001; `SshKeyHelper` moves to Zana with tests there (094-002)
- [ ] `native/file-system/direct/` — only `Direct.GetContent` tested; GetChildItem/GetLocation/RemoveItem/SetLocation untested (LOW — coordinate with task 104)
- [ ] Add coverage as areas get fixed by tasks 097-099 (regression tests belong with those fixes; this task sweeps what remains)

## Notes

Coverage map from multi-agent release review (2026-07-04): core 14 test files, configuration 4, dot-net-commands 20, git-commands 13, fzf 14, native file-system 7, script-support 4, mocks 1 direct + heavy indirect. Runner-exclusion problem is tracked in task 103.
