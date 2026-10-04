# Round 1 — general
**Date:** 2026-10-04
**Scope reviewed:** branch vs master: Fzf.cs, Fzf.Extensions.cs, Fzf.LayoutOptions.cs, Fzf.PreviewOptions.cs, core/command-extensions.cs, four fzf test files. Call sites checked: Shell.Run, ShellBuilder.WithStandardInput, CommandResult.Pipe/SelectAsync/PassthroughAsync/TtyPassthroughAsync.

## Summary

The stub is gone: `SelectWithFzf` copies the builder's argument list into the fzf pipe stage, and a stand-in fzf test proves the flags reach the process. Input methods no longer depend on `echo`, Unix `find`, or naive space splitting. The label-pos overloads keep the old `int` signature, so the change stays source and binary compatible. No correctness bugs found. Fzf tests pass locally (2/2 and 9/9) and the Tools project builds with 0 warnings.

## Issues

### Issue 1 — Severity: suggestion
- File: source/timewarp-amuru/core/command-extensions.cs:97
- Description: Behavior change in the core package. `ShellBuilder.WithStandardInput("")` used to leave stdin inherited. It now attaches immediate EOF. Every in-repo caller wants this (no caller passes "" expecting inheritance), but it is visible to consumers of the core package.
- Suggestion: Mention it in release notes or the changelog.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-amuru/core/command-result.cs:210
- Description: `PassthroughAsync` replaces stdin with the console, and `TtyPassthroughAsync` does not redirect stdin at all. So `FzfBuilder.FromInput/FromFiles/FromCommand(...).PassthroughAsync()` or `.TtyPassthroughAsync()` drop the configured input. The echo pipeline had the same problem before this change, so it is not a regression. `SelectAsync`/`GetSelectionAsync` keep the stdin pipe and work.
- Suggestion: Track as follow-up; out of scope for 088 (task targets input source construction, not execution-mode stdin handling).
- Status: open

### Issue 3 — Severity: nit
- File: source/timewarp-amuru-tools/fzf-command/Fzf.cs:88-93
- Description: The `UseStdin` branch is identical to the fallthrough return.
- Suggestion: Collapse the two returns.
- Status: open
