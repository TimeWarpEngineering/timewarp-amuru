# Fix or remove Fzf SelectWithFzf stub and fragile input methods

## Description

`SelectWithFzf()` silently discards every configured fzf option: `fzf-command/Fzf.Extensions.cs:28-33` — `ExtractFzfArguments` is a stub returning `[]` (comment admits "simplified implementation"). `cmd.SelectWithFzf(f => f.WithMulti().WithHeight(20))` runs plain `fzf` with no options and no error. A silent stub is worse than an absent API — finish it or cut it before 1.0.

**BLOCKER for 1.0** (the stub); input-method items are majors/minors.

## Checklist

- [ ] Implement `ExtractFzfArguments` properly (or remove `SelectWithFzf` from the 1.0 surface)
- [ ] `Fzf.cs:68-69` — `FromInput` feeds items via `/bin/echo <joined>`: items starting with `-n`/`-e`/`-E` are eaten as echo flags; no `echo` on Windows. Pipe via stdin instead
- [ ] `Fzf.cs:74` — `FromFiles` uses Unix `find`; broken on Windows
- [ ] `Fzf.cs:79-84` — `FromCommand` splits the command string on spaces; quoted arguments (`git log --format="%h %s"`) are mangled
- [ ] `Fzf.PreviewOptions.cs:48` vs `Fzf.LayoutOptions.cs:91` — `WithPreviewLabelPos(int)` vs `WithBorderLabelPos(string)` type drift; fzf accepts `N[:top|bottom]` for both. Align
- [ ] Add at least one real-execution test for `SelectWithFzf` option pass-through

## Notes

Found by multi-agent release review (2026-07-04). All other fzf flags were verified correct against fzf's real option set. Existing fzf tests cover arg-building well but only one test executes fzf for real. Paths relative to `source/timewarp-amuru/`.
