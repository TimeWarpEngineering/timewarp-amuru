# Round 1 — general
**Date:** 2026-10-14
**Scope reviewed:** same as framework (`master...task/132-…`, commit `2764ba6`)

## Summary

The pack is drafted, unpublished, and the technical claims check out against the `v2.0.0`
source: SelectString exit codes, TextMatch members, binary sniff, ReplaceInFiles dry run /
preview format / `.bak` / temp-then-move / BOM / newline / symlink behavior, timeout
defaults and exit 124, concurrency contract, Pipe overload, `GetConfiguredStandardInput`,
and every listed breaking change were confirmed with file:line evidence. Blips are within
280 characters, the card is 1200×675, and the video script is 80 words. The defects are
in what the reader sees: the follow-up blog body carries internal publishing instructions
that the `Git` shortcode will render on thefreezeteam.com, the code snippets do not
compile as shown, and two statements are looser than the shipped behavior.

## Issues

### Issue 1 — Severity: bug
- File: documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-follow-up.md:3
- Description: The public blog body contains internal scheduling instructions: "This is the concrete 2.0.0 example. Post it 2–5 days after the launch post." Line 85 likewise ends with "It was not executed for this draft." The FreezeTeam stub pulls this file verbatim through the `Git` shortcode, so both sentences would publish on thefreezeteam.com. The release blog has no such leak.
- Suggestion: Rewrite line 3 as reader-facing copy (e.g. "A concrete example for TimeWarp.Amuru 2.0.0. The launch post is the hub:") and drop "for this draft" from line 85 (e.g. "The Before block is the shell-out those samples replace."). Keep the 2–5 day timing in `documentation/posts/readme.md`, where it already is.
- Status: open

### Issue 2 — Severity: suggestion
- File: documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-release.md:32
- Description: The snippets declare `CommandOutput` but import only `TimeWarp.Amuru.Native.Text` (or only `using static TimeWarp.Amuru.Native.Aliases.Bash;`). `CommandOutput` is in `TimeWarp.Amuru` (`source/timewarp-amuru/core/command-output.cs:17`), and the package flows no global usings to consumers, so a reader who copies a block gets CS0246. Affected: release blog Search (32–38), Bash (48–52), Commands.ReplaceInFiles (84–90); follow-up After blocks (37–41, 73–79). The samples were evidently run with both usings in scope.
- Suggestion: Add `using TimeWarp.Amuru;` to the first Native.Text block in each post (and the `Bash` block), or switch the declarations to `var`.
- Status: open

### Issue 3 — Severity: suggestion
- File: documentation/posts/blips/2026-xx-xx-timewarp-amuru-2-release.md:1
- Description: "A command that would hang returns exit 124." reads as automatic. 2.0.0 has no default timeout (`CommandOptions.Timeout` null means no limit; the release blog itself says so at line 100). The Nostr note ("a timeout that returns exit 124") and the blog ("takes a timeout") are accurate; the launch X post is the only overstatement and it is the most-read piece.
- Suggestion: e.g. "Give a command a timeout and a hang returns exit 124." Re-check the 280-character limit (currently 247).
- Status: open

### Issue 4 — Severity: suggestion
- File: documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-release.md:129
- Description: The Git breaking changes (lines 129–130) sit in a generic "What broke" list, but the `Git` helpers ship in `TimeWarp.Amuru.Tools` (`source/timewarp-amuru-tools/git-commands/`, `PublicAPI.Shipped.txt:113,126`), not core. A reader on core `TimeWarp.Amuru` only cannot tell these do not affect them; the Tools bullet (131) is the only one attributed to a package.
- Suggestion: Prefix the two Git bullets with "On `TimeWarp.Amuru.Tools`," or group them with the dotnet builder bullet under a Tools sub-list.
- Status: open

### Issue 5 — Severity: nit
- File: documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-release.md:133
- Description: "`TimeWarp.Amuru.Tools` is the fluent builders for `dotnet`, `git`, and `fzf`." The package description is "Fluent builders for common CLI tools (dotnet, git, fzf) plus TimeWarp repo services" (`source/timewarp-amuru-tools/timewarp-amuru-tools.csproj:6`); it also ships NuGet and repo services, and `Git` is a static helper class rather than a builder.
- Suggestion: Mirror the csproj description, e.g. "`TimeWarp.Amuru.Tools` adds fluent builders for `dotnet`, `git`, and `fzf`, plus TimeWarp repo services."
- Status: open
