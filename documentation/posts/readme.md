# Amuru 2.0 posts (unpublished)

Drafts for the stable 2.0.0 release. Nothing here has been posted. Steven approves before
the blog stub, X, or Nostr. The GitHub Release and the NuGet packages are already public.
Do not edit them from this pack.

Headline: **TimeWarp.Amuru 2.0: Shell scripting in C#, without shelling out.**

| File | When | Channel |
|---|---|---|
| `blogs/2026-xx-xx-timewarp-amuru-2-release.md` | After Steven's OK | Blog, the link target |
| `blips/2026-xx-xx-timewarp-amuru-2-release.md` | Same hour, 20:00–22:00 Asia/Bangkok | X. Attach the card. No link in this post. |
| `blips/2026-xx-xx-timewarp-amuru-2-release-reply.md` | Reply to that X post, after the blog is live | X reply. Carries the blog link. |
| `blips/2026-xx-xx-timewarp-amuru-2-release-nostr.md` | With the X post | Nostr. Attach the card. Includes the blog link. |
| `blogs/2026-xx-xx-timewarp-amuru-2-follow-up.md` | 2–5 days later | Blog |
| `blips/2026-xx-xx-timewarp-amuru-2-follow-up.md` | With that blog post | X and Nostr. Includes the follow-up link. |
| `assets/timewarp-amuru-2-card.png` | With the launch posts | 1200×675 card and blog OG image |
| `video-script.md` | Only if a voice is available | Not recorded |

`xx` in the blog URLs is the publish month and day. Replace it in the blips, the follow-up
blog, and the FreezeTeam stub on the day Steven picks. Blips must stay at or under 280
characters so `ganda post` can take them. Do not run `ganda post` from this task.

The launch X post is the one that stays link-free. The card is the image. The reply under
it carries the blog URL. Nostr includes the URL in the note.

## Relays

`ganda post config show` (2026-10-14) lists the ganda Nostr account and these relays:

- `wss://rilo.nostria.app`
- `wss://ribo.nostria.app`

X credentials are configured in the same file. This task does not post to either network.

## FreezeTeam stub

The blog body lives in this repository. thefreezeteam.com pulls it with the `Git` shortcode.
The stub PR waits for Steven. Do not open it from this task.

Intended URL, once the day replaces `xx`:

`https://thefreezeteam.com/posts/steven-t-cramer/2026/xx/2026-xx-xx-timewarp-amuru-2-release`

Stub path in TheFreezeTeamBlog, beside a copy of `assets/timewarp-amuru-2-card.png`:

`Source/TheFreezeTeam.com/input/posts/steven-t-cramer/2026/xx/2026-xx-xx-timewarp-amuru-2-release.md`

The shortcode path is lowercase. GitHub's contents API is case-sensitive (TheFreezeTeamBlog
task 001). The path must match `documentation/posts/blogs/` in this repo. `Published` uses
`DD/MM/YYYY`, the same shape as the 2025 Nuru stub.

```markdown
---
Title: "TimeWarp.Amuru 2.0: Shell scripting in C#, without shelling out"
Published: xx/xx/2026
Tags:
  - CLI
  - .NET
  - TimeWarp
  - Open Source
Author: Steven T. Cramer
Excerpt: Search and replace in C# without shelling out to grep or sed, and a per-command timeout that returns exit 124.
Description: TimeWarp.Amuru 2.0.0 is the first stable 2.x. SelectString and ReplaceInFiles stay in process. Commands that would hang return exit 124. Breaking changes are in the upgrade guide.
Image: timewarp-amuru-2-card.png
---

<?! Git "TimeWarpEngineering" "timewarp-amuru" "documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-release.md" /?>
```

The follow-up stub is the same shape. Its shortcode path is
`documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-follow-up.md`. It can reuse the card.

## Card

`assets/timewarp-amuru-2-card.html` is the source. Headless Chrome screenshots it at
1200×675 and device scale 1. The three figures are the shipped contract (grep exit codes
0, 1, and 2; a dry-run unified diff that writes nothing; timeout exit 124). They are not
a benchmark. Re-render after the HTML changes. Do not typeset the contract in an image model.

```bash
google-chrome --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=1 \
  --window-size=1200,675 \
  --screenshot=documentation/posts/assets/timewarp-amuru-2-card.png \
  documentation/posts/assets/timewarp-amuru-2-card.html
```

Run that from the repository root. Then check the PNG is 1200 by 675.

On this machine `google-chrome` is a shim to a missing Playwright Chromium 1234. The
checked-in PNG was captured with Playwright Chromium 1243
(`~/.cache/ms-playwright/chromium-1243/chrome-linux64/chrome`) and the same flags, plus
`--no-sandbox --disable-dev-shm-usage`.
