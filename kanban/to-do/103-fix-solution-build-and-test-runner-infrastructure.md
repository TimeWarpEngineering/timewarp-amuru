# Fix solution build and test runner infrastructure

## Description

The library builds clean and the suite is green (409 passed / 1 env skip / 0 failed on SDK 10.0.301), but the surrounding infrastructure has holes that bite contributors and CI.

## Checklist

- [ ] **Solution build is broken**: `timewarp-amuru.slnx` still references `tools/dev-cli/dev-cli.csproj` (MSB3202: not found) — dev-cli was converted to a file-based app (`tools/dev-cli/dev.cs`). Remove the stale entry
- [ ] **No SDK pin**: repo has no `global.json`; under the machine-default SDK 11.0.100-preview.5 the library build fails with 329 IDE0055 formatting errors (CI pins `dotnet-version: 10.0.201` in `.github/workflows/workflow.yml`). Add `global.json` so local builds match CI
- [ ] **Aggregate test runner silently omits 52 passing tests**: `tests/timewarp-amuru/multi-file-runners/Directory.Build.props` compile glob only covers `native/file-system/*.cs`, excluding `native/path-resolver.cs` (11 tests), `repo-services/*.cs` (41 tests) — all pass standalone today; the props comment claiming repo-services "require being run from within a git repo" is stale. Include them (json-rpc test files stay excluded until task 083 revives them — they're 100% commented out)
- [ ] **AGENTS.md build/test instructions are stale**: references `./Tests/RunTests.cs` and `Tests/Integration/` which don't exist; actual runner is `tests/timewarp-amuru/multi-file-runners/run-tests.cs` via `tools/dev-cli/endpoints/test-command.cs`. Update
- [ ] Delete or clearly header-comment the dead commented-out `json-rpc/` source files and the commented `StreamJsonRpc` reference in `timewarp-amuru.csproj:14` (084 cleanup)

## Notes

Found by multi-agent release review (2026-07-04); build/test numbers verified empirically on SDK 10.0.301.
