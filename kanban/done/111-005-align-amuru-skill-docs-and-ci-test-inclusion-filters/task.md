# Align amuru skill docs and CI test-inclusion filters

## Description

Parent **111** round-1 findings **M15, M21–M24, M26**. Docs that contradict the 1.0 contract, plus CI/test-runner footguns. M25 (`cliwrap-exit-code-tests/`) is wontfix on 111.

Pinned tree: `fbd5d276fc5a936136a55d981fc121a23b991493`. Evidence: parent `review/round-1/merged.md`.

## Requirements

- **M21 (bug)** `skills/amuru/SKILL.md:9`, `:268-297` — skill claims it is authoritative and still says default throw-on-nonzero, `ExecutionResult`, and `AsJsonRpcClient`. Contradicts `AGENTS.md` / 090–092 / 094-001. Align with `CommandOutput` and default validation `None`; drop JSON-RPC/`ExecutionResult`.
- **M15 (suggestion)** `source/timewarp-amuru-tools/dot-net-commands/dot-net.md:3-24` — documents `ExecuteAsync()`, `GetStringAsync()`, TimeWarp.Cli. Rewrite to `RunAsync`/`CaptureAsync` or delete if unpacked.
- **M22 (suggestion)** `.github/workflows/workflow.yml:7-17` and `:21-31` — path filters omit `BannedSymbols.txt`, `.editorconfig`, `source/.editorconfig`, `timewarp-amuru.slnx`.
- **M23 (suggestion)** `tests/timewarp-amuru/multi-file-runners/Directory.Build.props:14-25` — hand-maintained non-recursive Compile globs. Use recursive `**/*.cs` or a disk-vs-Compile check (the 103 failure class).
- **M24 (nit)** `msbuild/repository.props:8` — `TestsDirectory` is `Tests/`; on-disk is `tests/`.
- **M26 (nit)** `tests/timewarp-amuru/Directory.Build.props:27` — tests import banned `System.Console` statically. Remove it.

Do not clone **082** kebab, **105** missing tests, **094-004** PublicAPI analyzers, or **106** XML-doc polish (stale RunBuilder / CliConfiguration TimeWarp.Cli text).

## Checklist

- [x] M21 skill matches AGENTS.md error-handling and current types
- [x] M15 DotNet reference examples compile against current builders
- [x] M22 CI path filters cover BannedSymbols / editorconfig / slnx
- [x] M23 aggregate runner cannot silently omit a new test folder
- [x] M24 TestsDirectory casing
- [x] M26 remove static System.Console using from tests
- [x] `## Results` + `### How to validate`
- [x] Review disposition (2 rounds, effort 1, clean)

## Session

- Implementer: Grok session 01a0efee-2550-7c01-9a9b-a6fdec8e9be5 (2026-09-30)
- Review: Grok review oracle (2026-09-30)

## Results

M15, M21–M24, and M26 are aligned with the 1.0 contract. M25 stays wontfix on parent 111 (not touched).

- **M21.** [skills/amuru/SKILL.md](skills/amuru/SKILL.md) no longer claims to override other sources. Default validation is `None` (`CommandOutput.ExitCode` / `Success`; `WithZeroExitCodeValidation()` opts into throwing; never-ran is `CommandResult.NeverRanExitCode`). `PassthroughAsync` and `TtyPassthroughAsync` return `CommandOutput`. JSON-RPC / `AsJsonRpcClient` and `ExecutionResult` are gone. Also dropped non-existent `When` / `WhenNotNull` / `Unless` and `FindRootAsync`, and corrected `WithSingleFile` / `WithTrimmed` plus the Git result types the old samples named as `string` / `int`.
- **M15.** [source/timewarp-amuru-tools/dot-net-commands/dot-net.md](source/timewarp-amuru-tools/dot-net-commands/dot-net.md) is rewritten, not deleted. The file is source, not an unpacked package. Examples use `RunAsync` / `CaptureAsync` / `ToListAsync` and real builder names (`Outdated()`, `Vulnerable()`, `WithLockedMode()`, `WithSingleFile()`, `WithTrimmed()`). Failure stays a real `CommandOutput`, not an empty string. TimeWarp.Cli is gone.
- **M22.** Pull-request path filters in [.github/workflows/workflow.yml](.github/workflows/workflow.yml) include `BannedSymbols.txt`, `.editorconfig`, `source/.editorconfig`, and `timewarp-amuru.slnx`. Push to `master` stays unfiltered so every master commit still produces a `Packages-*` artifact.
- **M23.** [tests/timewarp-amuru/multi-file-runners/Directory.Build.props](tests/timewarp-amuru/multi-file-runners/Directory.Build.props) compiles `../single-file-tests/**/*.cs` instead of a hand-maintained folder list.
- **M24.** `TestsDirectory` in [msbuild/repository.props](msbuild/repository.props) is `tests/`.
- **M26.** The static `System.Console` using is removed from [tests/timewarp-amuru/Directory.Build.props](tests/timewarp-amuru/Directory.Build.props).

### How to validate

**Smoke**

```bash
rg -n 'ExecutionResult|AsJsonRpcClient|FindRootAsync|TimeWarp\.Cli|\.ExecuteAsync\(|\.GetStringAsync\(|\.GetLinesAsync\(|WithInputItems|WithInputCommand' \
  skills/amuru/SKILL.md source/timewarp-amuru-tools/dot-net-commands/dot-net.md
rg -n 'TimeWarp\.Amuru\.Tools' skills/amuru/SKILL.md
rg -n 'BannedSymbols.txt|source/\.editorconfig|timewarp-amuru\.slnx' .github/workflows/workflow.yml
rg -n 'TestsDirectory' msbuild/repository.props
rg -n 'System\.Console' tests/timewarp-amuru/Directory.Build.props
rg -n 'single-file-tests/\*\*/\*\.cs' tests/timewarp-amuru/multi-file-runners/Directory.Build.props
```

**Expect**

- First `rg`: no matches.
- Skill names `TimeWarp.Amuru.Tools`. Fzf samples use `FromInput` / `FromCommand`. DotNet reference limits `WithProperty` and includes pack on `RunAndCaptureAsync`.
- Workflow: those three names (and `.editorconfig`) are under `pull_request.paths`. `push` to `master` has no `paths` key.
- `TestsDirectory` ends in `tests/`.
- No `System.Console` using in the test props.
- The aggregate runner include is `../single-file-tests/**/*.cs`.

**Automated**

```bash
dotnet build timewarp-amuru.slnx --nologo
cd tests/timewarp-amuru/multi-file-runners && dotnet run run-tests.cs
```

**Expect:** solution build 0 warnings, 0 errors. Aggregate runner: Total 508, Passed 507, Skipped 1, Failed 0 (includes `native/file-system` and `repo-services`, which the old non-recursive list had to name by hand).

**Not in scope:** M25 `cliwrap-exit-code-tests/`, kebab renames (082), missing tests (105), PublicAPI analyzers (094-004), XML-doc polish (106). A PR that only changes `skills/**` still does not run this workflow.

### Review disposition

Effort 1, roster `general`, 2 rounds. Final counts: bug 0 open / 3 fixed / 0 wontfix; suggestion 0 open / 1 fixed / 0 wontfix; nit 0. Outcome **clean**. No wontfix and no escalation.

Round 1 accepted M15 and M21–M24 / M26 and filed **M1** (fzf `WithInputItems` / `WithInputCommand`), **M2** (skill omitted `TimeWarp.Amuru.Tools`), **M3** (`WithProperty` claimed on every DotNet builder), and **M4** (pack omitted from `RunAndCaptureAsync`). Fixed on this task. Round 2 re-checked M1–M4 against the doc delta; no new findings.

- Framework: `kanban/to-do/111-005-align-amuru-skill-docs-and-ci-test-inclusion-filters/review/review-framework.md`
- Last merged: `kanban/to-do/111-005-align-amuru-skill-docs-and-ci-test-inclusion-filters/review/round-2/merged.md`
- Disposition: `kanban/to-do/111-005-align-amuru-skill-docs-and-ci-test-inclusion-filters/review/disposition.md`

## Notes

Parent: **111**. Sources: `review/round-1/tests-infra.md`, `tools-builders.md`.

Review trail: `review/review-framework.md`, `review/round-2/merged.md`, `review/disposition.md`. M1–M4 fixed here; disposition clean.
