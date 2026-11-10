# Audit baseline: pwsh workflow, coverage, suppression baseline, Nuru beta.12

## Description

`ganda repo audit` on master (2026-11-11, after task 142 merged) reports 4 Errors and 2 Warnings.
None came from 142; they are checks the installed ganda added. Bring master back to a clean audit
so the pre-PR gate passes for the next task.

Audit output (abridged):

- analyzer-suppressions (Error): `.timewarp/suppression-baseline.json` missing. `--fix` seeds it from the current tree.
- timewarp-signals-gitignore (Error): root `.gitignore` lacks `/.timewarp/signals/`. `--fix`.
- nuru (Error): TimeWarp.Nuru 4.0.0-beta.11 in `Directory.Packages.props`, latest 4.0.0-beta.12. `--fix` or edit by hand.
- workflow-pwsh-shell (Error): `.github/workflows/workflow.yml` needs top-level `defaults: run: shell: pwsh`; job `ci` steps "Run CI Pipeline" (`if [ "${{ github.event_name }}" == "release" ]`) and "Keep last two Packages artifacts" (`set -euo pipefail`) are bash and must be rewritten in pwsh by hand.
- memsearch-scaffold (Warning): `.githooks/{post-commit,post-merge,post-checkout,pre-commit,pre-push}.cs` outdated. `--fix` moves the pins.
- ci-code-coverage (Warning): CI test run collects no coverage. Either add `--results-directory artifacts/coverage --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml` to the Microsoft.Testing.Platform invocation plus an `actions/upload-artifact@v4` step for `artifacts/coverage`, with `"runner": "Microsoft.Testing.Platform"` under `test` in global.json when `dotnet test` runs the suite. If the test entry point is the dev-cli runner rather than `dotnet test`, and the flags cannot be threaded through, set `ci-code-coverage.severity = off` under `[ganda.audit]` in `.editorconfig` with a one-line reason and this task id.

## Goal (implementer restatement)

Bring this worktree back to a clean `ganda repo audit` after the checks the installed ganda added. No package version bump of TimeWarp.Amuru. 3.0.0-beta.3 stays the release that ships separately.

Acceptance: `ganda repo audit --fix` lands as-is, including the seeded `.timewarp/suppression-baseline.json`, the `/.timewarp/signals/` gitignore line, the memsearch githook pin move, and the TimeWarp.Nuru / TimeWarp.Nuru.DevCli pin at 4.0.0-beta.12. `.github/workflows/workflow.yml` defaults every `run:` step to pwsh. The "Run CI Pipeline" step still chooses publish mode when the event is `release` or when a workflow_dispatch break-glass sets mode and confirm to `release`, and it still runs the ordinary pipeline otherwise. The "Keep last two Packages artifacts" step still lists non-expired `Packages-*` artifacts, skips the prune when the artifact just uploaded is not listed yet, keeps the two newest, and deletes the rest. `./bin/dev build` and `./bin/dev test` pass on the beta.12 pin. ci-code-coverage is either collected and uploaded, or turned off under `[ganda.audit]` with a reason and this task id when the dev-cli runner cannot take the Microsoft.Testing.Platform coverage flags. A final `ganda repo audit` reports zero Errors.

## Checklist

- [x] `ganda repo audit --fix`; commit whatever it changes as-is (audit fixes are policy)
- [x] Rewrite the two bash steps in `workflow.yml` as pwsh and add `defaults: run: shell: pwsh`; keep behaviour identical (release-mode branch, artifact retention of the last two Packages artifacts)
- [x] Nuru pinned at 4.0.0-beta.12; build and tests green locally (`./bin/dev build`, `./bin/dev test`)
- [x] ci-code-coverage resolved: coverage collected and uploaded, or severity off with reason
- [x] `ganda repo audit` passes with zero Errors in the task worktree
- [ ] CI green on the PR, including the rewritten pwsh steps

## Session

- Created: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-10)
- Implementation: 01a1c6a2-e428-7d33-aa6c-cbeee1b6df92 (2026-11-11)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1c6a2-e428-7d33-aa6c-cbeee1b6df92, max-turns 500, Done — 2026-11-10T18:29:55Z
- Review oracle: claude-opus-5-5 (Claude Code headless, ganda task work), effort 2, roster general, turn cap 120 — round 1, disposition accepted-exceptions (2026-11-11)
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 120, artifacts general.md, merged.md, disposition.md — 2026-11-10T18:32:43Z
- Installed ganda changed during this walk: was 51719784 bytes 2b68168f4096b6ead5ba35c3ac18bdcb70e3e05a0c22f189da7b7369e773a185; now 51756808 bytes a18f3dc25f245e28614a218db4f6294c4e02bfabf5529c0b4a7a94f7b048c6ec

## Results

`ganda repo audit --fix` seeded `.timewarp/suppression-baseline.json` (118 suppressions), ignored `/.timewarp/signals/`, refreshed the memsearch githook pins, and pinned `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` to 4.0.0-beta.12. That same fix also moved `TimeWarp.Terminal` from 1.0.2 to 2.0.0-beta.1. It wrote `defaults: run: shell: pwsh` and left the two bash steps. Those steps are now pwsh. A release event and a confirmed break-glass dispatch both run `workflow --mode release` with the OIDC key. Every other run calls `workflow` with no key. The prune step still lists non-expired `Packages-*` artifacts, skips when the artifact just uploaded is not listed, keeps the two newest, and deletes the rest. A local stand-in for `gh` kept `Packages-4` and `Packages-3` and deleted `Packages-2` and `Packages-1`.

Beta.12 removes `ICommandHandler`. The five dev-cli handlers implement `IRequestHandler` and return `ValueTask<Unit>`. `dotnet run --file tools/dev-cli/dev.cs -- --help` compiles. ci-code-coverage is `severity = off`: the suite is `dotnet run` of the file-based runner, not Microsoft.Testing.Platform, and `--fix` reported the invocation could not be edited.

`./bin/dev build` succeeded with 0 warnings. `./bin/dev test` passed 838, skipped 1. `verify-samples` compiled all 3 samples. SDK 11.0.100 creates the `--tool-path` directory and a `.store` lock when `dotnet tool install` cannot find the package, so `ToolInstall_Should_RejectUnknownPackageWithoutInstalling` now asserts that no package entry was installed. The file stays at its 510-line baseline. `ganda repo audit` then exited 0: Passed 43, Failed 0, Skipped 2. No TimeWarp.Amuru version bump.

The pull-request CI run is not available from this walk. The host opens the PR.

### Review disposition

One round at effort 2, general reviewer only. Final counts: 0 bugs, 1 suggestion (wontfix), 0 nits. 0 open. Outcome: **accepted-exceptions**.

- The pwsh rewrites keep the release, break-glass, and default branching and the keep-last-two prune behavior. All `run:` blocks parse under PowerShell 7.
- The ci-code-coverage opt-out is the one this task sanctions. The `--fix` output is policy.
- M1 (suggestion, wontfix): `dot-net.cli-smoke.cs:399` enumerates the tool-path directory without checking that it exists, so it depends on the SDK version. It cannot cause a false pass. Reopen if PR CI throws `DirectoryNotFoundException` there.
- Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

### How to validate

Smoke: from this worktree, run `ganda repo audit`, `./bin/dev build`, and `./bin/dev test`.

Expect: audit exits 0 and prints `Failed: 0`. Build exits 0 with 0 warnings. Test exits 0 and prints `Passed: 838` and `Skipped: 1`. `Directory.Packages.props` pins `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` at `4.0.0-beta.12`. `.github/workflows/workflow.yml` sets `defaults: run: shell: pwsh`, and the Run CI Pipeline step passes `--mode release` only when `GITHUB_EVENT_NAME` is `release` or `BREAK_GLASS_RELEASE` is `true`.

## Review

- Round 1: Done

## Notes

No version bump. 3.0.0-beta.3 is already on master and releases separately; this task is CI/audit hygiene only.
The pwsh conversion must be checked by reading the step logic, not just by making the audit pass: the
release-mode branch decides whether the pipeline runs in publish mode.
