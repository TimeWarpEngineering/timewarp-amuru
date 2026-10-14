# Marketing for the TimeWarp.Amuru 2.0.0 release

## Description

Prepare the marketing pack for `TimeWarp.Amuru` 2.0.0 and `TimeWarp.Amuru.Tools` 2.0.0 so every piece is drafted and unpublished until Steven approves it. Masoko (Markets) owns the marketing pieces.

- **Released:** both packages are on nuget.org since 2026-10-14 08:55 Asia/Bangkok. The [v2.0.0 GitHub Release](https://github.com/TimeWarpEngineering/timewarp-amuru/releases/tag/v2.0.0) is published.
- **Working headline:** to be chosen from the highlights below (first stable 2.x; shell scripting in C# without shelling out to grep/sed; commands that can't hang forever).
- **Breaking changes:** yes. The posts must point to a migration section.
- **Nothing gets posted publicly without Steven's OK.** Every claim must match shipped 2.0.0 behavior and the release notes.

### Highlights (from the v2.0.0 release notes)

- First stable 2.x (baseline 1.1.1). Both packages ship together at `2.0.0`; install without `--prerelease`.
- Native text search and in-place replace (`TimeWarp.Amuru.Native.Text`):
  - `SelectString` / Bash `Grep`: grep-compatible exit codes, `TextMatch` streaming with context lines, binary-file skip.
  - `ReplaceInFiles` / Bash `Sed`: dry run with a unified-diff `Preview`, `.bak` backup, BOM and newline-style preservation, temp-file-then-move writes, unchanged files not rewritten, symlinks kept as links.
- Per-command timeouts: `CommandOptions.Timeout`, grace period (default 5 s), SIGINT then process-tree kill, exit 124, `CommandOutput.TimedOut`, `MockSetup.TimesOut()`.
- Documented concurrency and output contract for `CommandResult`.
- `Pipe` overload taking `CommandOptions`; `CommandMock.GetConfiguredStandardInput`.
- Breaking: `WithStandardInput("")` is immediate EOF; `TtyPassthroughAsync` throws on configured stdin or pipelines; several `Amuru.Tools` dotnet builder methods removed or renamed; Git helpers no longer default to `master` and return typed result records instead of `bool`/`string`.

## Checklist

- [ ] 1. Check the shipped NuGet readmes for both packages: one-line pitch, install line without `--prerelease`, short example, migration notes, absolute links. They already shipped inside 2.0.0, so record gaps for the next patch rather than re-releasing.
- [ ] 2. Confirm the published GitHub Release body has a migration section for the breaking changes. Draft an edit if it doesn't; don't apply it without Steven's OK.
- [ ] 3. Blog post draft: `documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-release.md`, the hub the X and Nostr posts link to (pulled into thefreezeteam.com via the `Git` shortcode stub; the FreezeTeam stub PR waits for Steven).
- [ ] 4. X post draft in `documentation/posts/blips/` (≤ 280 chars). The X post carries the release card and **no link**. Draft the follow-up reply that carries the blog link.
- [ ] 5. Nostr post draft `documentation/posts/blips/2026-xx-xx-timewarp-amuru-2-release-nostr.md` (≤ 280 chars, links to the blog), posted through the ganda-configured Nostr account (`ganda post`). Confirm relays with `ganda post config show`.
- [ ] 6. 1200x675 social card (HTML captured by headless Chrome, same pipeline as the Mediator 14 card), used on X, Nostr and as the blog OG image.
- [ ] 7. (Optional) Short video (≤ 30 s) only if there's a sharp before/after story, e.g. grep/sed shell-outs replaced by `SelectString`/`ReplaceInFiles`, or a hung command stopped by `Timeout`. Script only if no voice is available.
- [ ] 8. Follow-up post with a concrete example, 2–5 days after the launch posts.
- [ ] Get Steven's OK before anything is posted publicly.

SI-account posts are out of scope.

## Timing

1. Now: everything drafted and unpublished. The release is already live and indexed on nuget.org.
2. After Steven's OK: blog, then X (card, no link) with the blog link in the reply, then Nostr, around 8–10 PM Bangkok.
3. 2–5 days later: the follow-up post.

## Session

- Created: Grok Bot (2026-10-14)

## Notes

Source: Steven, 2026-10-14, after the 2.0.0 release: "we need to now create the marketing task like we did for Mediator and Nuru". Mirrors timewarp-mediator task 015 (Mediator 14.0.0 marketing prep) and timewarp-nuru task 487 (Nuru 3.0 launch plan).

Unlike Mediator 015, this task starts after the release, so the readme and release body are checks, not pre-release work.

## Learnings

## Results
