# Round 1 — general
**Date:** 2026-11-11
**Scope reviewed:** branch vs master. The full product diff was read. `.timewarp/suppression-baseline.json` was spot-checked only, because it is audit-seeded and therefore policy.

## Summary

The change clears the `ganda repo audit` baseline. It applies the `--fix` output as-is, moves the dev-cli handlers to the Nuru beta.12 `IRequestHandler`/`ValueTask` shape, rewrites the two bash workflow steps in pwsh, and turns ci-code-coverage off with a reason and this task id. Overall risk is low. CI release gating is the sensitive part, and the pwsh rewrite keeps that behavior. The one finding is a test assertion that depends on SDK behavior.

## Verified (no finding)

- **Run CI Pipeline**: the old bash had a `release` branch, a break-glass `workflow_dispatch` branch (mode=release and confirm=release), and a default branch. The new pwsh is `$env:GITHUB_EVENT_NAME -eq 'release' -or $env:BREAK_GLASS_RELEASE -eq 'true'`, and GitHub renders the boolean expression as `true`/`false`. A release event or a confirmed break-glass dispatch runs `workflow --mode release --api-key <OIDC key>`. Every other run calls `workflow` with no key. Dispatch with mode=release but no confirm still stops at the earlier "Validate break-glass confirmation" step. The OIDC key now arrives through an env var instead of being interpolated into the script text, which is an improvement. `$LASTEXITCODE` is propagated.
- **Keep last two Packages artifacts**: `LC_ALL=C sort -r` on whole TSV lines is equivalent to an ordinal sort followed by `Reverse()`. The just-uploaded check uses an exact, case-sensitive match on field 3 (`-ceq`), matching `grep -qxF`. It keeps the two newest, deletes the rest, and skips lines with an empty id. A `gh` failure exits non-zero, which matches `set -euo pipefail`.
- **Other `run:` steps** now run under pwsh. These are `echo "::error::…"; exit 1` and the probe `echo`, and both behave the same in pwsh. All three multi-line `run:` blocks parse with the PowerShell 7 parser and produce 0 errors (checked locally).
- **ci-code-coverage off** is under `[ganda.audit]` (`.editorconfig:334` onward) with a reason and "task 143". The condition the task names holds: `tools/dev-cli/endpoints/test-command.cs:42` runs `dotnet run run-tests.cs`, not Microsoft.Testing.Platform. This is the opt-out the task sanctions, not a silent suppression.
- **Dev-cli handlers**: none of the five still references `ICommandHandler` or a `Task<Unit> Handle`. Each change is a mechanical signature change.
- **Goal restatement vs task**: consistent. The restatement adds the no-version-bump constraint from Notes and spells out the behavior to keep. No misread.

## Issues

### Issue 1 — Severity: suggestion
- File: tests/timewarp-amuru/single-file-tests/dot-net-commands/dot-net.cli-smoke.cs:399
- Description: The old `Directory.Exists(toolPath).ShouldBeFalse()` became `Directory.EnumerateFileSystemEntries(toolPath, …)`. That call throws `DirectoryNotFoundException` when the tool-path directory does not exist. The directory exists only because SDK 11.0.100 now creates it. The previous assertion relied on the opposite behavior, so this depends on the SDK version. If CI's SDK, or a later one, stops creating the directory, the test fails with an exception instead of passing in the very case it is meant to accept. The one-line lambda also mixes `||` and `&&` without parentheses, which makes it hard to read. This does not weaken the test: it still fails if any package entry is installed.
- Suggestion: Guard with `!Directory.Exists(toolPath) || !Directory.EnumerateFileSystemEntries(...).Any(...)`, and move the predicate into a named local with explicit parentheses.
- Status: open
