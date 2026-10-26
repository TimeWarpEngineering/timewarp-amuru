# Round 1 — merged findings
**Date:** 2026-10-14
**Sources:** general (orchestrator + delegated source fact-check)

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 1 | 0 | 0 |
| suggestion | 3 | 0 | 0 |
| nit | 1 | 0 | 0 |

## Issues

### M1 — Severity: bug — Status: open
- File: documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-follow-up.md:3
- Description: Public blog body contains internal instructions ("Post it 2–5 days after the launch post." at line 3; "It was not executed for this draft." at line 85) that the FreezeTeam `Git` shortcode would publish verbatim.
- Suggestion: Rewrite both sentences as reader-facing copy; keep timing in `documentation/posts/readme.md`.
- Source: general
- Disposition notes:

### M2 — Severity: suggestion — Status: open
- File: documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-release.md:32
- Description: Snippets use `CommandOutput` (namespace `TimeWarp.Amuru`) with only `TimeWarp.Amuru.Native.Text` / `using static …Bash` imported; copy-paste fails with CS0246. Release blog 32–38, 48–52, 84–90; follow-up 37–41, 73–79.
- Suggestion: Add `using TimeWarp.Amuru;` to those blocks (or use `var`).
- Source: general
- Disposition notes:

### M3 — Severity: suggestion — Status: open
- File: documentation/posts/blips/2026-xx-xx-timewarp-amuru-2-release.md:1
- Description: "A command that would hang returns exit 124." implies a default timeout; 2.0.0 has none.
- Suggestion: Condition it on a configured timeout; keep ≤ 280 characters.
- Source: general
- Disposition notes:

### M4 — Severity: suggestion — Status: open
- File: documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-release.md:129
- Description: Git breaking changes are in `TimeWarp.Amuru.Tools` but are listed without package attribution.
- Suggestion: Attribute the two Git bullets to `TimeWarp.Amuru.Tools`.
- Source: general
- Disposition notes:

### M5 — Severity: nit — Status: open
- File: documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-release.md:133
- Description: Tools package description omits the repo/NuGet services and calls the static `Git` helpers builders.
- Suggestion: Mirror the csproj description.
- Source: general
- Disposition notes:

## Duplicates / conflicts

- None. Single reviewer source. Fact-check items that verified TRUE (exit codes, TextMatch, binary sniff, dry-run preview, `.bak`/temp/BOM/newline/symlink, timeout defaults, exit 124, concurrency, Pipe, mock stdin, every listed breaking change, card claims, video script) raised no finding. "Without shelling out" is the agreed headline slogan and is scoped to the text commands in the body; not raised.
