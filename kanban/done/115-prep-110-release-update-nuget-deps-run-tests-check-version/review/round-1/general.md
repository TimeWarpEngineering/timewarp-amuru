# Round 1 — general
**Date:** 2026-09-23
**Scope reviewed:** branch task/115-... vs origin/master

## Summary

The change is a clean, mechanical prep for the 1.1.0 release: a deps bump commit (10 packages,
including a Roslynator 4→5 major with zero code fallout) and an audit-cleanup commit (106 shebang
rewrites, new `.githooks/*.cs` runfiles, props/editorconfig/vscode scaffolding, one README rename).
I verified the highest-risk items — the root `Directory.Build.props` now pulling `TimeWarp.SourceGenerators`
and `TimeWarp.Build.Tasks` into every project (including file-based runfiles under tests/tools/samples),
the `TimeWarp.Amuru`/`TimeWarp.Amuru.Tools` self-pins consumed by the new git hooks, and the hook logic
itself (stdin parsing, home-branch/refs-ganda short-circuits, exit codes) — and none of them are broken:
no duplicate `PackageReference`, no missing public API in the pinned 1.0.0/1.0.0-beta.2 packages, hook
logic matches its header comments, `.githooks/memsearch-index.log` is correctly gitignored, the README
rename has no inbound links, and the task.md file/gate claims match the diff. Risk is low; the two
findings below are a placement footgun in `.editorconfig` and a pre-existing final-newline gap that now
also affects a diff-touched/new file.

## Issues

### Issue 1 — Severity: suggestion
- File: .editorconfig:303-306
- Description: The new `TW0007.filename = global-usings.cs` line was inserted directly under the
  `#### Build Scripts Analyzer Settings ####` heading, one line before the `[Scripts/*.cs]` section
  header it visually appears to belong to:
  ```
  #### Build Scripts Analyzer Settings ####
  # TW0007 file-level usings (TimeWarp.SourceGenerators)
  dotnet_diagnostic.TW0007.filename = global-usings.cs
  [Scripts/*.cs]
  ```
  EditorConfig properties apply to the section header that *precedes* them, so this line actually lands
  in the broad `[*.cs]` section opened at line 37 — not `[Scripts/*.cs]`. In practice this happens to be
  the *correct* scope: `global-usings.cs` files exist outside `Scripts/*.cs` too
  (`source/timewarp-amuru/global-usings.cs`, `source/timewarp-amuru-tools/global-usings.cs`,
  `tools/dev-cli/global-usings.cs`), and the build is verified clean (0 warnings/0 errors per task.md),
  so the effective behavior is fine today. But the heading/placement is misleading: a future maintainer
  reading "Build Scripts Analyzer Settings" above this line could reasonably move it inside
  `[Scripts/*.cs]` to "fix" the apparent misplacement, which would silently narrow the scope and likely
  reintroduce TW0007 noise on the `source/**/global-usings.cs` files.
- Suggestion: Either move the `TW0007.filename` line above the `#### Build Scripts Analyzer Settings ####`
  heading (grouping it with the existing `globalusingsanalyzer0001.filename` line at line 299, which has
  the same repo-wide intent), or add a one-line comment noting it's intentionally global/not scoped to
  `[Scripts/*.cs]`.
- Status: open

### Issue 2 — Severity: nit
- File: Directory.Packages.props:20, .memsearch.toml:50
- Description: `.editorconfig`'s `[*]` section sets `insert_final_newline = true` for all files with no
  extension exclusion (.editorconfig:24-25). `Directory.Packages.props` (touched by both the deps and
  audit commits in this diff) and the newly-added `.memsearch.toml` both end without a trailing newline
  (confirmed via `xxd` on the file tails — `.../Version>` and `.../"` are the last bytes, no `\n`),
  violating that rule.
- Suggestion: Append a trailing newline to both files (or run the repo's normal formatter/EditorConfig
  fixer over them) so they match the declared `insert_final_newline = true` convention.
- Status: open
