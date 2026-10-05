# Round 1 — general
**Date:** 2026-10-05
**Scope reviewed:** branch task/100-roll-icommandbuilder-and-withnovalidation-across-a vs master

## Summary
All 29 builder-bearing files in dot-net-commands, tool and Fzf.cs expose both methods on every public builder class (counts verified per file; Options fields made non-readonly; semantics match ShellBuilder: replace Options, return this). PublicAPI.Unshipped has the *REMOVED* lines and the renamed members; no stale references to the old names remain outside Shipped. Remaining findings are a validation gap in the Fzf FromCommand pipeline and thin test coverage. Tests were not executed.

## Issues
### Issue 1 — Severity: bug
- File: source/timewarp-amuru-tools/fzf-command/Fzf.cs:107
- Description: In the FromCommand path, `Shell.Run(executable, commandArguments, Options).Pipe("fzf", fzfArguments)` applies Options (including the new validation) only to the source command stage. The fzf stage is created via `Pipe` with default options, so `WithZeroExitCodeValidation()` does not make an fzf no-match/cancel (exit 1/130) throw on this path, unlike the other input paths.
- Suggestion: Verify how the piped stage is validated; either apply the validation to the final stage (e.g. a Pipe overload taking options) or document the limitation, and add a test for the FromCommand path.
- Status: open

### Issue 2 — Severity: suggestion
- File: tests/timewarp-amuru/single-file-tests/fzf-command/fzf-builder.validation.cs:20
- Description: Only the FromInput path is tested; the FromCommand, glob and stdin paths in Build are untested. The test also lacks a `[Timeout]` unlike the dotnet tests, and fails if fzf is not installed.
- Suggestion: Add a FromCommand case (would expose Issue 1) and a Timeout attribute.
- Status: open

### Issue 3 — Severity: suggestion
- File: tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.validation.cs:120
- Description: Tests only call `WithZeroExitCodeValidation` on the leaf builder, one builder per family (of ~70 classes), and never `WithNoValidation` after strict (the revert case) nor strict set on the parent before creating a child (DevCerts/Sln/Tool parents). A regression where a parent's validation is not propagated to children, or WithNoValidation is a no-op, would pass.
- Suggestion: Add a parent-then-child case (e.g. `DotNet.Sln().WithZeroExitCodeValidation().List()`) and a `.WithZeroExitCodeValidation().WithNoValidation()` case; consider a reflection test asserting every public builder in the Tools assembly has both methods.
- Status: open

### Issue 4 — Severity: nit
- File: source/timewarp-amuru-tools/dot-net-commands/DotNet.DevCerts.cs:7
- Description: Design note says child builders "keep a writable copy"; parent validation set after a child is created does not affect that child (children snapshot Options at creation). This is consistent with ShellBuilder-style immutability but is undocumented in the XML docs of the parent methods.
- Suggestion: Optionally note "call before creating sub-builders" in dot-net.md.
- Status: open
