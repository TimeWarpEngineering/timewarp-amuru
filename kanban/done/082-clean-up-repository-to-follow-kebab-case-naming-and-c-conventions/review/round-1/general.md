# Round 1 — general
**Date:** 2026-10-06
**Scope reviewed:** same as framework

## Summary

The change renames 97 source files and one test file with `git mv` (types, namespaces and members unchanged), replaces every purpose stub, fixes the ShellBuilder constructor doc, and makes both csproj files pack `readme.md` as it is cased on disk. The rename risk is low: the build includes files by glob, and the only explicit include (`tests/timewarp-amuru/Directory.Build.props`) was updated. No build file still names `README.md`. The main risk is that a new Design comment misstates the code. Every claim was checked against its file. Two were wrong or imprecise. The rest match.

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-amuru-tools/git-commands/git.worktree-porcelain-parser.cs:7
- Description: The Design comment says the final record is flushed after the loop "rather than on a blank separator" because the split discards empty lines. In the code, records are flushed when the next `worktree ` line arrives. A whitespace-only blank-line branch still exists and still flushes, for example on a bare `\r` from CRLF output. Only the last record depends on the post-loop flush.
- Suggestion: Describe the three flush points accurately.
- Status: open

### Issue 2 — Severity: nit
- File: source/timewarp-amuru-tools/git-commands/git.worktree-add.cs:7
- Description: The comment says "failure carries stderr". The code uses the trimmed stderr, or "Failed to add worktree" when stderr is blank.
- Suggestion: Mention the fallback message.
- Status: open
