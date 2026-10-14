# 30-second script — not recorded

There is a sharp before/after: a script that shells out to `grep` and `sed -i`, then
`SelectString` and `ReplaceInFiles` in process. This file is the script only.

No mp4. This machine has neither an ElevenLabs key nor a local voice (`espeak`, `espeak-ng`,
`piper`, and `festival` are absent). The task allows the clip only with one of those. Do not
invent a voiceover, and do not post a silent file as if it were the launch video. The
follow-up blog is the concrete example until a voice exists.

The words below are 80 words. At 160 words a minute that is 30 seconds. It was not
timed with a voice. On screen, show the two
snippets from `documentation/posts/blogs/2026-xx-xx-timewarp-amuru-2-follow-up.md`, then the
card `documentation/posts/assets/timewarp-amuru-2-card.png`.

## Words

Scripts call grep, then sed -i. The quoting, and whether those tools exist, is yours.

Amuru 2.0 keeps that work in C#. SelectString uses grep's exit codes: 0, 1, and 2.
ReplaceInFiles can dry-run first. You get a unified diff, and the file stays untouched.
The real edit writes a .bak only when the text changes.

A command that might hang takes WithTimeout. You get TimedOut and exit 124.

2.0 breaks callers. The upgrade guide is on the v2.0.0 release.
