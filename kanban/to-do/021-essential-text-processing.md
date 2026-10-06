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

- [ ] `native/text/` files created with Purpose/Design regions, kebab basenames
- [ ] `SelectString` Direct (streaming `TextMatch`) and Commands (grep-format output, grep exit codes)
- [ ] `ReplaceInFiles` Direct (`ReplaceResult`, dry run, backup, encoding/newline/trailing-newline preservation, atomic write, skip-unchanged) and Commands
- [ ] Bash aliases `Grep` / `Sed` wired; commented stubs removed
- [ ] Globbing via existing `FindItem` / `FindCriteria`
- [ ] PublicAPI Unshipped updated; XML docs complete; `./bin/dev build` 0 warnings / 0 errors
- [ ] Tests listed above pass; full runner green (`dotnet run tests/timewarp-amuru/multi-file-runners/run-tests.cs`)
- [ ] Docs + readme line
- [ ] Results: API summary, test list, the 3 ganda fixer candidates with line refs
- [ ] `ganda repo audit` clean

## Notes

- 023 archived 2026-10-06 as folded here. 024/025/027/045 remain separate decisions (recommended archive).
- Reference shapes: `native/file-system/commands/commands.get-content.cs`, `native/file-system/direct/direct.find-item.cs`, `native/aliases/bash.cs`.
- Design cue from PowerShell `Select-String` for naming and from GNU grep/sed for exit-code contracts. Do not try to be sed: no scripts, no addresses, just pattern → replacement over files.
- 2.0 is in beta (`2.0.0-beta.1` published 2026-10-05); additive API is fine and will ride the next beta.

## Session

- Created: 2025-12-12
- Rewritten and 023 folded in: 522eb63d (2026-10-06)
