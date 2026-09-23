# Round 2 — general
**Date:** 2026-09-23
**Scope reviewed:** fix delta for M1/M2 (uncommitted `git diff`) on branch task/115-... vs origin/master

## Summary

Both round-1 findings are fixed cleanly and minimally: the fix delta touches only the three lines/bytes
needed (`.editorconfig`, `Directory.Packages.props`, `.memsearch.toml`), with no collateral changes.

## Prior findings

- M1 — Severity: suggestion — Status: fixed — `dotnet_diagnostic.TW0007.filename = global-usings.cs` is
  now at .editorconfig:303, grouped with the Global Usings Analyzer lines (globalusingsanalyzer0001-3,
  lines 299-301) and still between the `[*.cs]` header (line 37) and the next header `[Scripts/*.cs]`
  (now line 306, confirmed via `grep -n "^\["`), i.e. still correctly scoped to `[*.cs]`. It carries an
  explicit comment: `# TW0007 file-level usings (TimeWarp.SourceGenerators) — repo-wide ([*.cs]),
  intentionally not scoped to Scripts/`. The misleading heading-adjacency footgun is gone.
- M2 — Severity: nit — Status: fixed — `tail -c 3 Directory.Packages.props | xxd` → `743e 0a` (`t>\n`);
  `tail -c 3 .memsearch.toml | xxd` → `2222 0a` (`""\n`). Both files now end with a trailing newline.

## New issues

None.
