# CsCheck pilot: quote-then-parse round-trip property for argument escaping

## Description

Add a CsCheck property test for Amuru argument escaping: generate random argument strings (spaces, quotes, backslashes, empty, unicode, shell metacharacters), quote them via Amuru, parse them back using the per-platform rules (Windows CommandLineToArgvW-style and POSIX), and assert round-trip equality. Quoting bugs mean broken commands and potential injection; properties plus shrinking find them cheaply.

Sibling pilot: Nuru task 497 (fuzz route parsing with random argv), https://github.com/TimeWarpEngineering/timewarp-nuru/blob/master/kanban/to-do/497-cscheck-pilot-fuzz-route-parsing-with-random-argv/task.md

## Context

- Busara (Wisdom) verdict 2026-10-13: GREEN for CsCheck as a **test-only** dependency, pilots first. We have no property-based testing today.
- Akili (Intelligence) evaluation: `/workspace/ideas/2026-10-13-cscheck-eval.md` (box). Verified CsCheck works in a Jaribu runfile.
- CsCheck: https://github.com/AnthonyLloyd/CsCheck (Apache-2.0, forkable if needed).

## Where the quoting lives (groom 2026-11-15)

Amuru has no argument quoting layer of its own. `ShellBuilder.WithArguments(params string[])`
(`source/timewarp-amuru/core/shell-builder.cs`) hands the array to CliWrap, whose `ArgumentsBuilder`
does the per-platform escaping. The property therefore tests Amuru's contract on top of CliWrap:
pass random strings through `WithArguments`, run a child that echoes its argv one per line (a small
runfile under `tests/`), and assert the child saw the same strings. If task 119 later swaps CliWrap
for the .NET 11 process APIs, this test is the guard that the swap keeps the contract.

## Rules

- Pin **CsCheck 4.9.1** (no need to wait for 5.0; tests do not need AOT).
- Test-only. CsCheck must never be referenced by product/package code.
- Pin `CsCheck_Seed` in CI; print the seed on every failure so it can be replayed.
- Do not use `Faster` in build-blocking tests (CI timing is flaky).

## Kill criterion

If both pilots (this one and the sibling) find nothing and the generators are costly to maintain, stop at these two pilots.

## Deferred (until pilots show value)

- State reducer model-based tests
- Parse/format round-trip
- Terminal / Flexbox invariants

## Goal (implementer restatement)

Add Amuru's first property-based test (CsCheck 4.9.1, test-only). Random argument lists that
include spaces, quotes, backslashes, empty strings, unicode, and shell metacharacters go through
`Shell.Builder(...).WithArguments(...)` to a real child process that reports the argv it received.
The test asserts the child saw exactly the strings we passed. That checks the contract Amuru gets
from CliWrap's per-platform escaping, and it guards a later CliWrap swap (task 119).

Acceptance:
- CsCheck 4.9.1 is a central `PackageVersion` and only test code references it (no product csproj).
- Generators cover the edge characters above, including empty strings and empty lists.
- The round-trip property passes on Linux in the aggregate `./bin/dev test` run.
- CI pins `CsCheck_Seed`; a failure prints the seed so it can be replayed.
- No `Faster` in the build-blocking test.
- Results record findings for the kill-criterion review, and the Windows path status.

## Checklist

- [x] Add CsCheck 4.9.1 to the test project/runfile only
- [x] Generators for random argument lists, including edge characters
- [x] Round-trip property: quote via Amuru, parse back, assert equality
- [ ] Run on Linux in CI (the only runner today); run the Windows path locally and record the result in Results. Add a Windows runner only if the pilot finds a platform-specific bug (Linux done; Windows not run, no Windows host here — see Results / Findings)
- [x] Pin CsCheck_Seed in CI; print seed on failure
- [x] Record findings (bugs found / invariants not covered by existing tests) for the kill-criterion review

## Review

- Round 1: Done

## Notes

Not launched. Filed 2026-10-13 at Steven's request ("sure lets try").

## Results

### What changed

- `Directory.Packages.props`: central `CsCheck` 4.9.1, commented test-only. The only reference is
  `#:package CsCheck` in the new test file. No `source/` project references it.
- `tests/timewarp-amuru/single-file-tests/core/argv-echo/argv-echo.cs` (+ its own
  `Directory.Build.props`, which stops the props walk like `mcp-stdio-server`): child that prints
  the argument count, the Base64 of each argument's UTF-8 bytes one per line, then `end`. Base64
  keeps newlines, quotes, and console code pages out of the report. `end` keeps a trailing empty
  argument from being lost.
- `tests/timewarp-amuru/single-file-tests/core/shell-builder.with-arguments.round-trip.cs`:
  builds the child once into `$TMP/timewarp-amuru-128-argv-echo` (rebuilt when the source is
  newer), then runs it through `Shell.Builder(echo).WithArguments(...)`. Four tests:
  - `KnownEdgeCases_Should_ReachChildUnchanged`: fixed list (empty, spaces, quotes, backslash runs,
    trailing backslashes, `$HOME`, backticks, `$(id)`, `; rm -rf / &`, pipes and redirects, globs,
    tab, newline, CJK, emoji, `-`, `--`).
  - `EmptyList_Should_ReachChildAsNoArguments`, `TrailingEmptyArguments_Should_ReachChild`.
  - `RandomArguments_Should_ReachChildUnchanged`: CsCheck `SampleAsync`, 100 iterations (default),
    4 threads. Generator mixes printable ASCII, the `EdgeCharacters` set (whitespace, quotes, backslash, backtick, and shell metacharacters), BMP
    unicode U+00A0..U+D7FF, and astral code points; 0..12 pieces per argument, 0..6 arguments.
    No NUL and no lone surrogates (neither can cross an OS argv). No `Faster`.
- `tests/timewarp-amuru/multi-file-runners/Directory.Build.props`: `Compile Remove` for the child
  (its top-level statements would collide with the runner's).
- `.github/workflows/workflow.yml`: `CsCheck_Seed: 012XdpNskFd2` on the CI step. CsCheck uses that
  seed for the **first** sample only; the other 99 samples stay random per run. A failure message
  carries the shrunk case's own seed (`Set seed: "..." or -e CsCheck_Seed=... to reproduce`), and
  the test also prints the pinned seed before rethrowing.
- Audit boy-scout: `ganda repo audit --fix` set the VS Code window-icon config in
  `.vscode/settings.json` (gitignored, so nothing to commit).
- The first commit's post-commit attest failed `runfile-executable`: the two new shebang runfiles
  were committed without the executable bit. Fixed with `chmod +x` in a follow-up commit.

### Findings (for the kill-criterion review)

- **Bugs found: none.** 100 iterations in the aggregate run, plus a local sweep with
  `CsCheck_Iter=3000`, all passed on Linux. CliWrap's POSIX path hands argv through without a
  shell, so the Linux result is expected; the Windows quoting path is where bugs would live.
- **Detection works:** a mutation run (assertion changed to drop backslashes) failed, shrank to
  `["\\$><"]` in 3 shrinks, and printed the replay seed.
- **Invariants not covered before:** no existing test sent quotes, backslash runs, metacharacters,
  newlines, empty or trailing-empty arguments, or non-BMP unicode through `WithArguments` to a
  real child and checked the child's argv byte-for-byte. This test is also the guard for task 119.
- **Windows path: not run.** This machine is WSL2 with no Windows interop (`dotnet.exe`, `cmd.exe`,
  `pwsh.exe` not on `PATH`). The test is written to run there unchanged (it builds
  `argv-echo.exe`, and the child reports UTF-8 Base64 so the code page does not matter). Run it on
  a Windows host with `dotnet run tests/timewarp-amuru/single-file-tests/core/shell-builder.with-arguments.round-trip.cs`
  before deciding the kill criterion. Per the checklist, no Windows runner was added.
- **Maintenance cost so far:** one generator (about 10 lines) and a 20-line child.

### Gates

- `./bin/dev build` — passed (0 warnings, 0 errors).
- `./bin/dev test` — passed: 879 total, 878 passed, 1 skipped (pre-existing "Requires full git
  history - skipped in CI environments"). `WithArgumentsRoundTrip_Given_` 4/4,
  "Passed 100 iterations."
- `ganda repo audit` — passed with 1 advisory warning (`vscode-window-icon`); after
  `ganda repo audit --fix`, rerun: "Repository passes all audit checks." (43 passed, 0 failed,
  2 skipped).

### How to validate

Smoke:

```bash
dotnet run tests/timewarp-amuru/single-file-tests/core/shell-builder.with-arguments.round-trip.cs
CsCheck_Seed=012XdpNskFd2 CsCheck_Iter=1000 dotnet run tests/timewarp-amuru/single-file-tests/core/shell-builder.with-arguments.round-trip.cs
grep -rn "CsCheck" source/ || echo "no product reference"
```

Expect: 4 tests pass, the random test prints "Passed 100 iterations." (then 1,000), and the
grep prints "no product reference". To see a failure report, change the property's
`received.ShouldBe(arguments)` to compare against a mangled copy: the run fails with a shrunk
case and a `CsCheck_Seed=...` replay line.

### Review

- Round 1. Effort 2. Roster: general. Reviewer session Grok `01a1dfe8-ed25-78c2-8d66-58b55ef32a20`.
- Counts: bug 0 open / 0 fixed / 0 wontfix; suggestion 0 / 0 / 0; nit 0 / 0 / 0. Final open count 0.
- Disposition: clean. No wontfix. No escalation.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.
- The implementer restatement matches the groomed child-process contract. No misread recorded.

## Session

- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-11-15T15:52:18Z
- Review oracle: Grok session `01a1dfe8-ed25-78c2-8d66-58b55ef32a20` (2026-11-15), effort 2, roster general, round 1, disposition clean
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1dfe8-ed25-78c2-8d66-58b55ef32a20, max-turns 120, artifacts general.md, merged.md, disposition.md — 2026-11-15T16:07:39Z
