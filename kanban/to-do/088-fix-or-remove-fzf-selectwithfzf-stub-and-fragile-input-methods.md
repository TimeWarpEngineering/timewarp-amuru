# Fix or remove Fzf SelectWithFzf stub and fragile input methods

## Description

`SelectWithFzf()` silently discards every configured fzf option: `source/timewarp-amuru-tools/fzf-command/Fzf.Extensions.cs:28-33` — `ExtractFzfArguments` is a stub returning `[]` (comment admits "simplified implementation"). `cmd.SelectWithFzf(f => f.WithMulti().WithHeight(20))` runs plain `fzf` with no options and no error. A silent stub is worse than an absent API. 1.1.1 already shipped with it, so this is a correctness bug in the released Tools package (same class as task 087).

**Decision (2026-10-04): implement, do not remove.** The builder already models every fzf option correctly; the stub is the only gap. `SelectWithFzf` must pass the configured arguments through to the real `fzf` process.

## Checklist

- [ ] Implement `ExtractFzfArguments` properly: resolve the configured fzf arguments from the builder and pass them to `command.Pipe("fzf", …)`
- [ ] `Fzf.cs:68-69` — `FromInput` feeds items via `/bin/echo <joined>`: items starting with `-n`/`-e`/`-E` are eaten as echo flags; no `echo` on Windows. Pipe via stdin instead
- [ ] `Fzf.cs:74` — `FromFiles` uses Unix `find`; broken on Windows
- [ ] `Fzf.cs:79-84` — `FromCommand` splits the command string on spaces; quoted arguments (`git log --format="%h %s"`) are mangled
- [ ] `Fzf.PreviewOptions.cs:48` vs `Fzf.LayoutOptions.cs:91` — `WithPreviewLabelPos(int)` vs `WithBorderLabelPos(string)` type drift; fzf accepts `N[:top|bottom]` for both. Align
- [ ] Add at least one real-execution test for `SelectWithFzf` option pass-through

## Notes

Found by multi-agent release review (2026-07-04). All other fzf flags were verified correct against fzf's real option set. Existing fzf tests cover arg-building well but only one test executes fzf for real. Paths relative to `source/timewarp-amuru-tools/` (fzf moved to the Tools package in the 094 split). Tests live under `tests/timewarp-amuru/single-file-tests/`.

## Session

- Kitchen refresh: 522eb63d (2026-10-04)
