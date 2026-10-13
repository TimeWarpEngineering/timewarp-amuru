# CsCheck pilot: quote-then-parse round-trip property for argument escaping

## Description

Add a CsCheck property test for Amuru argument escaping: generate random argument strings (spaces, quotes, backslashes, empty, unicode, shell metacharacters), quote them via Amuru, parse them back using the per-platform rules (Windows CommandLineToArgvW-style and POSIX), and assert round-trip equality. Quoting bugs mean broken commands and potential injection; properties plus shrinking find them cheaply.

Sibling pilot: Nuru task 497 (fuzz route parsing with random argv), https://github.com/TimeWarpEngineering/timewarp-nuru/blob/master/kanban/to-do/497-cscheck-pilot-fuzz-route-parsing-with-random-argv/task.md

## Context

- Busara (Wisdom) verdict 2026-10-13: GREEN for CsCheck as a **test-only** dependency, pilots first. We have no property-based testing today.
- Akili (Intelligence) evaluation: `/workspace/ideas/2026-10-13-cscheck-eval.md` (box). Verified CsCheck works in a Jaribu runfile.
- CsCheck: https://github.com/AnthonyLloyd/CsCheck (Apache-2.0, forkable if needed).

## Rules

- Pin **CsCheck 4.9.1** (no need to wait for 5.0; tests do not need AOT).
- Test-only. CsCheck must never be referenced by product/package code.
- Pin `CsCheck_Seed` in CI; print the seed on every failure so it can be replayed.
- Do not use `Faster` in build-blocking tests (CI timing is flaky).

## Kill criterion

If both pilots (this one and the sibling) find nothing and the generators are costly to maintain, stop at these two pilots.

## Deferred (until pilots show value)

- State reducer model-based tests
- Parse/format round-trip
- Terminal / Flexbox invariants

## Checklist

- [ ] Add CsCheck 4.9.1 to the test project/runfile only
- [ ] Generators for random argument lists, including edge characters
- [ ] Round-trip property: quote via Amuru, parse back, assert equality
- [ ] Run on Windows, Linux and macOS in CI
- [ ] Pin CsCheck_Seed in CI; print seed on failure
- [ ] Record findings (bugs found / invariants not covered by existing tests) for the kill-criterion review

## Notes

Not launched. Filed 2026-10-13 at Steven's request ("sure lets try").
