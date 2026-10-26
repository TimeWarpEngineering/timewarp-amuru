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

- [x] 1. Check the shipped NuGet readmes for both packages: one-line pitch, install line without `--prerelease`, short example, migration notes, absolute links. They already shipped inside 2.0.0, so record gaps for the next patch rather than re-releasing.
- [x] 2. Confirm the published GitHub Release body has a migration section for the breaking changes. Draft an edit if it doesn't; don't apply it without Steven's OK.
- [x] 3. Blog post draft: `documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-release.md`, the hub the X and Nostr posts link to (pulled into thefreezeteam.com via the `Git` shortcode stub; the FreezeTeam stub PR waits for Steven).
- [x] 4. X post draft in `documentation/posts/blips/` (≤ 280 chars). The X post carries the release card and **no link**. Draft the follow-up reply that carries the blog link.
- [x] 5. Nostr post draft `documentation/posts/blips/2026-xx-xx-timewarp-amuru-2-release-nostr.md` (≤ 280 chars, links to the blog), posted through the ganda-configured Nostr account (`ganda post`). Confirm relays with `ganda post config show`.
- [x] 6. 1200x675 social card (HTML captured by headless Chrome, same pipeline as the Mediator 14 card), used on X, Nostr and as the blog OG image.
- [x] 7. (Optional) Short video (≤ 30 s) only if there's a sharp before/after story, e.g. grep/sed shell-outs replaced by `SelectString`/`ReplaceInFiles`, or a hung command stopped by `Timeout`. Script only if no voice is available.
- [x] 8. Follow-up post with a concrete example, 2–5 days after the launch posts.
- [ ] Get Steven's OK before anything is posted publicly.

SI-account posts are out of scope.

## Timing

1. Now: everything drafted and unpublished. The release is already live and indexed on nuget.org.
2. After Steven's OK: blog, then X (card, no link) with the blog link in the reply, then Nostr, around 8–10 PM Bangkok.
3. 2–5 days later: the follow-up post.

## Session

- Created: Grok Bot (2026-10-14)
- Implementation: Grok session 01a1386c-1440-7061-a34c-94e6404413fe (2026-10-14)
- Review round 1: Claude Opus 5.5 review oracle (ganda task work, headless), effort 2, roster general; source fact-check subagent a72301428ac5f6fb8 (Sonnet) (2026-10-14)

## Notes

Source: Steven, 2026-10-14, after the 2.0.0 release: "we need to now create the marketing task like we did for Mediator and Nuru". Mirrors timewarp-mediator task 015 (Mediator 14.0.0 marketing prep) and timewarp-nuru task 487 (Nuru 3.0 launch plan).

Unlike Mediator 015, this task starts after the release, so the readme and release body are checks, not pre-release work.

Headline used in the drafts: "TimeWarp.Amuru 2.0: Shell scripting in C#, without shelling out." The other two highlights (first stable 2.x, exit 124) are on the card and in the posts.

The published GitHub Release body already has `## Upgrade guide`. It matches `documentation/release-notes/2.0.0.md` through that section (the release omits the `# 2.0.0` heading). No edit was drafted and none was applied.

Shipped readme gaps, for the next patch. Both nuget.org 2.0.0 nupkgs carry the same `readme.md` bytes as the repository `readme.md` (sha256 `d5400850efec6d0b1a0afb05338c7c1df6822e9bd6956038bb1ebb7b32e36898`). This task does not edit that file.

- One-line pitch: the NuGet page opens with the repository badge block and a two-sentence description. It does not lead with the 2.0 pitch. `TimeWarp.Amuru.Tools` has no package-specific readme.
- Install line: `dotnet add package TimeWarp.Amuru` and `TimeWarp.Amuru.Tools` have no `--prerelease`. They also do not pin `--version 2.0.0`. The runfile sample uses `@<latest-version>`.
- Short example: Quick Start is a long catalog. `SelectString` / `ReplaceInFiles` are one bullet, not a sample.
- Migration notes: one sentence points at the relative path `documentation/release-notes/2.0.0.md`. The later "Migration Guide" link is `analysis/migration-guide.md`, which is the v0.x to v1.0 guide, not the 2.0 upgrade guide.
- Absolute links: these eleven links are relative and do not resolve on nuget.org: `documentation/release-notes/2.0.0.md`, `documentation/developer/reference/native-text-commands.md` (twice), `documentation/developer/how-to-guides/`, `documentation/conceptual/architectural-decision-records/overview.md`, `documentation/overview.md`, `analysis/migration-guide.md`, `source/timewarp-amuru/core/command-extensions.cs`, `source/timewarp-amuru/core/command-result.cs`, `samples/`, `kanban/overview.md`.

`ganda post config show` lists Nostr relays `wss://rilo.nostria.app` and `wss://ribo.nostria.app`. Both completed a TLS handshake on 2026-10-14. Nothing was posted.

Samples in the launch and follow-up blogs were run on Linux against nuget.org `TimeWarp.Amuru` 2.0.0 (`#:package TimeWarp.Amuru@2.0.0`). The grep/sed shell-out in the follow-up is the before picture and was not executed.

## Learnings

## Results

Drafted the 2.0.0 launch pack and left it unpublished. The NuGet packages and the GitHub Release were already public, so this task did not change them.

- **Headline.** "TimeWarp.Amuru 2.0: Shell scripting in C#, without shelling out."
- **NuGet readmes.** Checked, not edited. Both nuget.org 2.0.0 packages ship the repository `readme.md` (same sha256). Gaps for the next patch are in Notes: no 2.0 one-line pitch, install lines omit `--version 2.0.0`, no short native-text example, the migration link is the v0.x to v1.0 guide, and eleven links are relative.
- **GitHub Release.** [v2.0.0](https://github.com/TimeWarpEngineering/timewarp-amuru/releases/tag/v2.0.0) already has `## Upgrade guide` covering the breaking changes. No draft edit. Nothing was applied.
- **Blog.** `documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-release.md`. Intended URL once the date replaces `xx`: `https://thefreezeteam.com/posts/steven-t-cramer/2026/xx/2026-xx-xx-timewarp-amuru-2-release`. The FreezeTeam stub text is in `documentation/posts/readme.md`. The stub PR was not opened.
- **X.** `documentation/posts/blips/2026-xx-xx-timewarp-amuru-2-release.md` (247 characters, no link). Reply: `documentation/posts/blips/2026-xx-xx-timewarp-amuru-2-release-reply.md` (123 characters, blog link).
- **Nostr.** `documentation/posts/blips/2026-xx-xx-timewarp-amuru-2-release-nostr.md` (264 characters, blog link). Relays from `ganda post config show`: `wss://rilo.nostria.app`, `wss://ribo.nostria.app`. Both completed TLS. `ganda post` was not run.
- **Card.** `documentation/posts/assets/timewarp-amuru-2-card.png`, 1200×675, from `timewarp-amuru-2-card.html`. Figures are the shipped contract (exit codes 0, 1, 2; dry-run diff; exit 124), not a benchmark.
- **Follow-up.** `documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-follow-up.md` and `documentation/posts/blips/2026-xx-xx-timewarp-amuru-2-follow-up.md` (270 characters). Before is a grep/sed shell-out. After is `SelectString` and `ReplaceInFiles`, including the dry-run diff checked against nuget.org 2.0.0.
- **Video.** `documentation/posts/video-script.md` only (80 words). No mp4: no ElevenLabs key and no local TTS on this machine.
- **Not done.** Steven's OK. After that: FreezeTeam stub, then the blog, then X (card, no link) with the blog link in the reply, then Nostr, around 20:00–22:00 Asia/Bangkok. The follow-up is 2–5 days later. Replace `xx` in the blips and the stub on the publish day.

### Review

- **Rounds.** 1 so far. Effort 2, roster `general`, plus a source fact-check of every technical claim against `v2.0.0`.
- **Final counts (round 1).** bug 1 open; suggestion 3 open; nit 1 open; 0 fixed; 0 wontfix.
- **Disposition.** Pending. 5 findings are open, so there is no `review/disposition.md` yet. The fix pass stays on task 132. Round 2 re-reviews the fixes.
- **Open findings.**
  - M1 (bug): the follow-up blog body includes internal instructions ("Post it 2–5 days after…", "…for this draft") that would be published.
  - M2: the snippets need `using TimeWarp.Amuru;` for `CommandOutput`.
  - M3: the X post implies a default timeout.
  - M4: the Git breaking changes aren't attributed to Tools.
  - M5: the Tools package description.
- **Paths.**
  - `review/review-framework.md`
  - `review/round-1/general.md`
  - `review/round-1/merged.md` (the live ledger)

### How to validate

**Smoke**

```bash
python3 - <<'PY'
from pathlib import Path
p = Path("documentation/posts/assets/timewarp-amuru-2-card.png")
data = p.read_bytes()
w = int.from_bytes(data[16:20], "big")
h = int.from_bytes(data[20:24], "big")
print(f"{w}x{h}")
for f in sorted(Path("documentation/posts/blips").glob("2026-xx-xx-timewarp-amuru-2-*.md")):
    body = f.read_text().strip("\n")
    print(len(body), "url" if "http" in body else "no-url", f.name)
x = Path("documentation/posts/blips/2026-xx-xx-timewarp-amuru-2-release.md").read_text()
assert "http" not in x
PY
rg -n "Upgrade guide" documentation/release-notes/2.0.0.md
gh release view v2.0.0 --repo TimeWarpEngineering/timewarp-amuru --json body --jq '.body | contains("## Upgrade guide")'
```

**Expect**

- The PNG line is `1200x675`.
- Blip counts are 280 or less: launch X 247 and `no-url`, reply 123 and `url`, Nostr 264 and `url`, follow-up 270 and `url`.
- `rg` finds `Upgrade guide` in the release notes. The `gh` line prints `true`.
- `documentation/posts/readme.md` names both Nostr relays and contains the unpublished FreezeTeam stub with the lowercase shortcode path `documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-release.md`.
- `readme.md` is unchanged from `origin/master`.

**Not in scope.** No public post, no `ganda post`, no GitHub Release edit, no NuGet readme change, no FreezeTeam pull request, no mp4.
