# Native text commands: SelectString and ReplaceInFiles

## Description

Add two native text primitives to the core package under `source/timewarp-amuru/native/text/`, following the existing `native/file-system/` shape (Commands with shell semantics, Direct with typed results, Bash aliases): **`SelectString`** (grep) and **`ReplaceInFiles`** (sed -i). Task **023** (Native text commands) is folded into this card and archived with a pointer.

**Rewritten 2026-10-06.** The original card sold this as "20–100x faster than spawning grep/sed". That is not the problem: across Amuru tools/hooks/tests, ganda source, and Nuru's dev-cli there are only two shell-outs to text tools, both in a shell-builder test. The real motivation is correctness and reuse: **ganda has 31 source files that hand-roll read-file → modify → write-file, 13 of them with `Regex`/`.Replace`** (the audit fixers). Each re-implements encoding, newline preservation, atomic write, and dry-run differently or not at all. One tested primitive replaces that pattern. Agents typing `sed` into a Bash tool are **not** the target; this API only helps code that is already C# (runfiles, dev-cli, ganda services, hooks).

Scope deliberately excludes JSON conversion (System.Text.Json exists), `wc`, sort, split/join, and the process/system-info/archive/tee families (024, 025, 027, 045: no callers).

## Requirements

### Shape (match `native/file-system/`)
- Files: `native/text/commands.select-string.cs`, `native/text/commands.replace-in-files.cs`, `native/text/direct.select-string.cs`, `native/text/direct.replace-in-files.cs`, `native/text/text-match.cs` (result record), `native/text/replace-result.cs`, plus shared helpers if needed. Kebab-case basenames; Purpose/Design regions (tw-agent-context-regions).
- `Commands.*` return `CommandOutput` (no throw, stderr + exit code on failure) so they compose with the rest of Amuru. `Direct.*` return typed results and may throw.
- Bash aliases in `native/aliases/bash.cs`: `Grep(...)` → `SelectString`, `Sed(...)` → `ReplaceInFiles`. Replace the commented-out Grep/Sed stubs there.
- Globbing: reuse `Direct.FindItem` / `FindCriteria` for multi-file input instead of a new glob engine. Accept a single path, an `IEnumerable<string>` of paths, or a root + glob pattern.

### `SelectString` (grep)
- Inputs: pattern (regex by default; `SimpleMatch` option for literal), file(s) or a string/stream of content.
- Options: case-insensitive, invert match, line numbers (always on in results), context lines before/after, max matches, include/exclude file globs.
- Direct result: `IAsyncEnumerable<TextMatch>` with `Path`, `LineNumber`, `Line`, `Match` (the `System.Text.RegularExpressions.Match` or captured groups), `ContextBefore`/`ContextAfter`. Streams line-by-line; never reads a whole file into memory for the match path.
- Commands result: `CommandOutput` whose stdout is `path:line:text` per match (grep format), exit 0 on matches, 1 on none, 2 on error (grep's contract).

### `ReplaceInFiles` (sed -i)
- Inputs: pattern (regex; literal option), replacement (supports `$1` / `${name}` groups), file(s) as above.
- Options: **dry run** (report what would change, touch nothing), **backup** (`.bak` beside the file), max replacements per file, case-insensitive, multiline/singleline regex options.
- Preservation: keep the file's encoding (detect BOM; default UTF-8 no BOM), keep its newline style (CRLF vs LF, detected from the first newline), keep a trailing newline if present. **Atomic write**: write to a temp file in the same directory then move over the original. Unchanged files are not rewritten (no mtime churn).
- Direct result: `ReplaceResult` per file: `Path`, `ReplacementCount`, `Changed`, `Preview` (unified-diff-style lines when dry run), `BackupPath`.
- Commands result: `CommandOutput` listing `path: N replacement(s)` per changed file; exit 0 even when nothing matched (sed's contract), non-zero on IO/regex error.

### Non-functional
- AOT/trim clean (`IsAotCompatible=true` on the core package); regex via `RegexOptions.Compiled` is fine, no `Regex` source generator required.
- New public members go in `source/timewarp-amuru/public-api/PublicAPI.Unshipped.txt` (RS0016 fails the build otherwise). XML docs on every public member (CS1591 is an error on core).
- Tests under `tests/timewarp-amuru/single-file-tests/native/text/`: regex and literal match, invert, context, multi-file via glob, exit codes; replace with groups, dry run leaves files untouched and reports preview, backup created, CRLF and BOM preserved, trailing newline preserved, unchanged file not rewritten (compare mtime), atomic write (temp file not left behind), encoding round-trip for a UTF-8 file with non-ASCII.
- Docs: a short section in `documentation/` beside the file-system native docs and a line in `readme.md`'s native list.

### First consumer (proof, not scope creep)
- In Results, name 3 ganda audit fixers (`timewarp-ganda/master/source/timewarp-ganda/services/audit/checks/*.cs`) whose read-modify-write would collapse to `ReplaceInFiles`, with the lines they would replace. Do **not** edit ganda here; that is a ganda task after the next Amuru beta ships.

## Checklist

- [x] `native/text/` files created with Purpose/Design regions, kebab basenames
- [x] `SelectString` Direct (streaming `TextMatch`) and Commands (grep-format output, grep exit codes)
- [x] `ReplaceInFiles` Direct (`ReplaceResult`, dry run, backup, encoding/newline/trailing-newline preservation, atomic write, skip-unchanged) and Commands
- [x] Bash aliases `Grep` / `Sed` wired; commented stubs removed
- [x] Globbing via existing `FindItem` / `FindCriteria`
- [x] PublicAPI Unshipped updated; XML docs complete; `./bin/dev build` 0 warnings / 0 errors
- [x] Tests listed above pass; full runner green (`dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs`)
- [x] Docs + readme line
- [x] Results: API summary, test list, the 3 ganda fixer candidates with line refs
- [x] `ganda repo audit` clean

## Notes

- 023 archived 2026-10-06 as folded here. 024/025/027/045 remain separate decisions (recommended archive).
- Reference shapes: `native/file-system/commands/commands.get-content.cs`, `native/file-system/direct/direct.find-item.cs`, `native/aliases/bash.cs`.
- Design cue from PowerShell `Select-String` for naming and from GNU grep/sed for exit-code contracts. Do not try to be sed: no scripts, no addresses, just pattern → replacement over files.
- 2.0 is in beta (`2.0.0-beta.1` published 2026-10-05); additive API is fine and will ride the next beta.

## Session

- Created: 2025-12-12
- Rewritten and 023 folded in: 522eb63d (2026-10-06)
- Implemented SelectString and ReplaceInFiles (2026-10-06)
- Implementation review (2026-10-06): review oracle, Claude Code headless (ganda task work); effort 3, general axis; reviewers general + general-tests (round 1), general (rounds 2–3)

## Results

`TimeWarp.Amuru.Native.Text` adds grep and in-place replace for C# callers. `Commands` returns `CommandOutput` and does not throw. `Direct` returns typed results and throws. Globs go through `FindItem` / `FindCriteria`. The new surface is in `source/timewarp-amuru/public-api/PublicAPI.Unshipped.txt` and rides the next 2.0 beta.

### API

- `Commands.SelectString` / `Direct.SelectString`: pattern plus a path, an `IEnumerable<string>` of paths, a root and glob, or a `TextReader` / `Stream`. Direct streams `TextMatch` (`Path`, `LineNumber`, `Line`, `Match`, `ContextBefore`, `ContextAfter`) one line at a time. Commands writes `path:line:text` and uses grep exit codes: 0 matched, 1 none, 2 error. A reader or stream with no path is labeled `-`. There is no string-content overload; pass a `StringReader`.
- `Commands.ReplaceInFiles` / `Direct.ReplaceInFiles`: pattern, replacement (`$1` / `${name}`), and the same file inputs. Direct yields one `ReplaceResult` per file (`Path`, `ReplacementCount`, `Changed`, `Preview`, `BackupPath`), including files that did not change. Commands writes `path: N replacement(s)` for each changed file. Exit 0 when the walk finishes, including no matches. Exit 1 on a regex or I/O error.
- Options: literal match, case, invert, context, per-file limits, include/exclude globs, dry run, `.bak` backup, multiline, singleline. Dry run fills a unified diff and writes nothing. Backup is `path.bak` and only when the text changes. Encoding (BOM, otherwise UTF-8 without one), the first newline style, and a trailing newline are kept. Unchanged files are not rewritten. The write is a temp file in the same directory, then a move.
- Bash: `Grep` / `GrepDirect` and `Sed` / `SedDirect`. The commented Grep and Sed stubs are gone.

### Tests

Under `tests/timewarp-amuru/single-file-tests/native/text/`:

- `commands.select-string.cs`: regex and literal stdout, no-match exit 1, bad pattern and missing path exit 2, invert, glob, several paths, reader label `-`, two hits on one line.
- `direct.select-string.cs`: regex and literal `TextMatch`, case, invert, context order, same-line match order, max matches, glob, exclude, captures, reader, bad pattern throws, missing file and missing root throw.
- `commands.replace-in-files.cs`: count line and exit 0, no-match exit 0, bad pattern and missing path exit 1, dry run leaves the file, glob.
- `direct.replace-in-files.cs`: `$1` and `${name}`, literal, max replacements, multiline, dry-run bytes and preview, backup, CRLF plus UTF-8 BOM, trailing newline, unchanged mtime, no temp file left, UTF-8 `café` without a BOM, every path visited.
- `bash-aliases.cs`: `Grep` exit codes and `Sed` replacement.

### Ganda fixer candidates

Not edited here. After the next Amuru beta, these read-modify-write paths can call `ReplaceInFiles`:

1. `timewarp-ganda/master/source/timewarp-ganda/services/audit/checks/global-usings-analyzer-check.cs` lines 142–149. Reads `.editorconfig`, runs `EnsureGlobalUsingsAnalyzerFilenameKey`, and `File.WriteAllTextAsync` when the text differs.
2. `checks/runfile-project-directives-check.cs` `FixAsync` lines 123–127 calls `operations/runfile-operations.cs` `EnsureRunfileDirectivesAsync` lines 60–103. That method reads the runfile, matches `ProjectDirectiveRegex`, `line.Replace`s the project path, and writes the file back.
3. `checks/runfile-shebang-check.cs` `FixAsync` lines 66–69 calls `EnsureRunfileShebangsAsync` lines 137–138. That method reads the file and writes `ReplaceFirstLine` (lines 148–151).

The path decision stays in ganda. The read, newline and encoding preservation, and write are what this API takes over.

### Review disposition

- **Outcome:** `clean`. 3 rounds, effort 3 (general axis). Round 1 had two general reviewers, one on correctness and one on tests and docs. Rounds 2 and 3 were re-reviews.
- **Final counts:** 4 bugs, 6 suggestions and 5 nits were raised; all 15 are fixed. None is open and none is `wontfix`.
- **Round 1 (M1–M11, fixed in 1842a0c):**
  - Bugs:
    - Decoding was lossy, which corrupted non-UTF-8 and binary files. Decoding is now strict and files with a NUL byte are skipped as binary.
    - A lone CR flipped a file's newline style. Detection now picks CRLF or LF only.
    - An error partway through the Commands walk lost the output so far. The error is now handled per path and the walk continues.
    - `MaxMatches` counted matches. It now counts lines, like grep `-m`, and grep output prints each line once.
  - Suggestion: a symlink is now followed to its target.
  - Tests: per-file limits, the documented replace behaviors, and the options that had no test.
- **Round 2 (M12–M15, fixed in b87a9c5):**
  - An unencodable replacement result is now a per-file `InvalidDataException`.
  - A path list is validated before any write.
  - The docs now say where the backup goes for a symlink and that BOM-less UTF-16/32 files are skipped.
- **Round 3:** M12–M15 verified, nothing new. The text tests pass 66 of 66, and the full runner reports 686 passed and 1 skipped (the existing git-history test).
- **Paths:** `review/review-framework.md`, `review/round-3/merged.md` (the final ledger) and `review/disposition.md`.

### How to validate

Smoke:

```bash
dotnet run tests/timewarp-amuru/single-file-tests/native/text/commands.select-string.cs
dotnet run tests/timewarp-amuru/single-file-tests/native/text/direct.replace-in-files.cs
```

Expect: both processes exit 0. The first reports 12 passed. The second reports 23 passed, including CRLF plus BOM, trailing newline, unchanged mtime, and no leftover temp file.
