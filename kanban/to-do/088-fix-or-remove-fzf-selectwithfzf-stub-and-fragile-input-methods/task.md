# Fix or remove Fzf SelectWithFzf stub and fragile input methods

## Description

`SelectWithFzf()` silently discards every configured fzf option: `source/timewarp-amuru-tools/fzf-command/Fzf.Extensions.cs:28-33` — `ExtractFzfArguments` is a stub returning `[]` (comment admits "simplified implementation"). `cmd.SelectWithFzf(f => f.WithMulti().WithHeight(20))` runs plain `fzf` with no options and no error. A silent stub is worse than an absent API. 1.1.1 already shipped with it, so this is a correctness bug in the released Tools package (same class as task 087).

**Decision (2026-10-04): implement, do not remove.** The builder already models every fzf option correctly; the stub is the only gap. `SelectWithFzf` must pass the configured arguments through to the real `fzf` process.

## Checklist

- [x] Implement `ExtractFzfArguments` properly: resolve the configured fzf arguments from the builder and pass them to `command.Pipe("fzf", …)`
- [x] `Fzf.cs:68-69` — `FromInput` feeds items via `/bin/echo <joined>`: items starting with `-n`/`-e`/`-E` are eaten as echo flags; no `echo` on Windows. Pipe via stdin instead
- [x] `Fzf.cs:74` — `FromFiles` uses Unix `find`; broken on Windows
- [x] `Fzf.cs:79-84` — `FromCommand` splits the command string on spaces; quoted arguments (`git log --format="%h %s"`) are mangled
- [x] `Fzf.PreviewOptions.cs:48` vs `Fzf.LayoutOptions.cs:91` — `WithPreviewLabelPos(int)` vs `WithBorderLabelPos(string)` type drift; fzf accepts `N[:top|bottom]` for both. Align
- [x] Add at least one real-execution test for `SelectWithFzf` option pass-through

## Results

`SelectWithFzf` forwards the builder's option flags to `fzf`. Input no longer depends on `echo`, Unix `find`, or splitting a command on spaces.

- `ExtractFzfArguments` copies the builder's argument list into `command.Pipe("fzf", …)`.
- `FromInput` writes each item as its own stdin line, so `-n`, `-e`, and `-E` stay data on Windows and Unix.
- `FromFiles` walks the working directory for a file-name glob and feeds `./relative` paths on stdin. A pattern that contains a directory separator throws.
- `FromCommand` keeps quoted text, including spaces, as one argument. Backslashes stay literal.
- `WithPreviewLabelPos` and `WithBorderLabelPos` each accept fzf's `N[:top|bottom]` string and a column `int`.
- An empty `standardInput` string is attached as immediate EOF. Null still means the caller owns stdin.

### How to validate

Smoke: from the repo root, `dotnet run tests/timewarp-amuru/single-file-tests/fzf-command/fzf-extensions.select-with-fzf.cs` and `dotnet run tests/timewarp-amuru/single-file-tests/fzf-command/fzf-builder.from-input.cs`.

Expect: both files pass. `ConfiguredOptions_Should_ReachTheFzfProcess` shows the fzf process received `--multi`, `--height=20`, and `--prompt=Select output: `. When `fzf` is on `PATH`, `FilterOption_Should_FilterRealFzfMatches` returns `alp`, `alpha`, and `alpine`, and does not return `beta`. `EchoFlagItems_Should_StayOnStdin` returns `-n`, `-e`, `-E`, and `keep` on separate lines. `QuotedCommand_Should_KeepSpacesInOneArgument` returns `--format=%h %s`. `Files_Should_ListMatchingNamesOnly` includes `./one.cs` and `./nested/two.cs` and excludes `skip.txt`.

The aggregate runner is `dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs`. On 2026-10-04 it reported 530 passed, 1 skipped, 0 failed. The skip is the pre-existing `GetCommitsAheadOfDefaultBranch` case.

### Review disposition

- Rounds: 1. Effort 2, roster: general.
- Final counts: bug 0. Suggestion 0 open, 2 wontfix. Nit 0 open, 1 wontfix.
- Disposition: **accepted-exceptions**, 0 open.
  - M1: `WithStandardInput("")` now means immediate EOF. The change is intended and documented; the changelog note belongs to the release PR.
  - M2: Passthrough and TtyPassthrough drop configured stdin. This predates the task and lives in core, so it is out of scope.
  - M3: The redundant `UseStdin` branch stays. Removing it causes CS0414.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Notes

Found by multi-agent release review (2026-07-04). All other fzf flags were verified correct against fzf's real option set. Existing fzf tests cover arg-building well but only one test executes fzf for real. Paths relative to `source/timewarp-amuru-tools/` (fzf moved to the Tools package in the 094 split). Tests live under `tests/timewarp-amuru/single-file-tests/`.

## Session

- Kitchen refresh: 522eb63d (2026-10-04)
- Implementation: grok task-work (2026-10-04)
- Review: claude review oracle, effort 2, general (2026-10-04)
- Review oracle: review by implementer-claude (claude, model claude-opus-5-5), session not reported, max-turns 120 — 2026-10-04T09:10:55Z
