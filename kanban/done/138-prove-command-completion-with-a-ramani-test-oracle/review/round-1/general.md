# Round 1 — general
**Date:** 2026-11-08
**Scope reviewed:** branch at b44b799 vs merge base c62412b (see review-framework.md)

## Summary

The change adds a test-only Ramani machine (`CommandCompletionMachine`) with five states and four signals, plus 19 tests that step every accepted transition and every refusal. It matches the task Requirements and the implementer restatement. All 20 state and signal pairs are either a transition or a per-state `Refuse<T>(reason)`. Behaviors are `static` lambdas, and the validation and TTY flags live on the context. The TTY start checks run in the same order as `TtyPassthroughAsync` (`source/timewarp-amuru/core/command-result.cs:847-857`): configured stdin with `is not null` (so empty counts), then pipeline, then mock. The messages match the product's private constants word for word. Production code is unchanged, and `source/timewarp-amuru-tools` has no Ramani reference. The one risk is how the generator is wired in: the package version is typed out in the analyzer path.

Verified: `dotnet run tests/timewarp-amuru/single-file-tests/core/command-completion-machine.send-async.cs` gives 19 passed, 0 failed.

## Issues

### Issue 1 — Severity: suggestion
- File: tests/timewarp-amuru/Directory.Build.props:123 (the `AddCommandCompletionRamaniGenerator` target)
- Description: The analyzer path types out `timewarp.ramani/1.0.0-beta.23`, a second copy of the version in `Directory.Packages.props:16`. Every test runfile compiles `command-completion/*.cs`, so every test needs the generator. When the central version is bumped (the audit's package-freshness fix does this on its own, and that change is committed as policy), this path still names beta.23. On a clean CI package cache, beta.23 is not restored, the analyzer is missing, the generated partial members (`Start`, `SendAsync`, `State`, `HostCancellation`) disappear, and every test file fails to compile with an error that does not point at the cause. On a warm local cache, the old generator runs quietly against the new runtime library.
- Suggestion: Take the version from the central item instead of typing it again. For example, inside the target, set a property to `@(PackageVersion->WithMetadataValue('Identity','TimeWarp.Ramani')->'%(Version)')` and build the path from it, lowercasing the version with `ToLowerInvariant()` because NuGet folders are lowercase. Add `<Error Condition="!Exists('…')" Text="…" />` so a missing generator fails with a clear message.
- Status: open

## Considered and not raised

- **SM010 suppression (`tests/.editorconfig`).** Under the suppression rule this was checked as a possible bug. SM010 warns that a non-final state with no outgoing transitions "discards every signal". Here `Exited`, `TimedOut`, and `Canceled` refuse every declared signal with a reason. That is what the Requirements ask for: "Every other pair in this signal set is a refusal with a reason, not an implicit ignore." A Ramani `Final(state)` has no body: it takes no `Entry` and no `Refuse` (the runtime docs say the handled signal types are "empty for a final state"). So a final state would lose both the recorded result and the refusal reasons. The suppression applies to one file and gives its reason and the task id. It does not skip the work SM010 measures, because no signal is silently discarded. Possible upstream follow-up in Ramani: SM010 should not fire when an `.Exhaustive()` machine's state refuses its whole signal set.
- **Runner disagreement on interrupt-then-exit.** The oracle records `Exited` with the child's code, as Requirement 4 says. The runners report 124. That difference is written up in Notes, and the runners were left alone, as Requirement 12 says. Not a defect in this change.
- **Duplicated message strings.** The product constants are `private`, so the oracle has to repeat the text. Tests compare the strings exactly.
- **Goal restatement vs task.** The restatement matches the Requirements: states, signals, the cancellation-wins race, the strict-only throw, the TTY exemption, the start-refusal order, and explicit refusals. No misread.
