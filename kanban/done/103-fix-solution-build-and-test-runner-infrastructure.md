# Fix solution build and test runner infrastructure

## Description

The library builds clean and the suite is green (409 passed / 1 env skip / 0 failed on SDK 10.0.301), but the surrounding infrastructure has holes that bite contributors and CI.

## Checklist

- [x] Solution build fixed in 094-003: slnx lists both library projects, stale dev-cli entry removed; `dotnet build timewarp-amuru.slnx` clean
- [x] **No SDK pin**: repo has no `global.json`; under the machine-default SDK 11.0.100-preview.5 the library build fails with 329 IDE0055 formatting errors (CI pins `dotnet-version: 10.0.201` in `.github/workflows/workflow.yml`). Added `global.json` (10.0.301, rollForward latestFeature) 2026-07-05
- [x] **Aggregate runner now includes them** (417 tests, up from 365): `native/*.cs` and `repo-services/*.cs` added to the compile glob; root cause of the stale "require git repo" comment found and fixed — two tests (`SetLocation`, `Cd`) changed process CWD without restoring, breaking later `Git.FindRoot`-dependent tests; both now restore in try/finally. repo-services tests use `MockBehavior.Loose` (they intentionally mix mocked+real git). Previously omitted 52 tests: `tests/timewarp-amuru/multi-file-runners/Directory.Build.props` compile glob only covers `native/file-system/*.cs`, excluding `native/path-resolver.cs` (11 tests), `repo-services/*.cs` (41 tests) — all pass standalone today; the props comment claiming repo-services "require being run from within a git repo" is stale. Include them (json-rpc test files stay excluded until task 083 revives them — they're 100% commented out)
- [x] **AGENTS.md rewritten** (2026-07-05): correct build/test commands, package layout, new contracts (validation default, strict mocks, Shell.Run, CWD-restore rule): references `./Tests/RunTests.cs` and `Tests/Integration/` which don't exist; actual runner is `tests/timewarp-amuru/multi-file-runners/run-tests.cs` via `tools/dev-cli/endpoints/test-command.cs`. Update
- [x] Dead json-rpc source and test files DELETED (git history preserves them; 083 rebuilds fresh); commented csproj reference removed during the 094-003 split

## Notes

Found by multi-agent release review (2026-07-04); build/test numbers verified empirically on SDK 10.0.301.
