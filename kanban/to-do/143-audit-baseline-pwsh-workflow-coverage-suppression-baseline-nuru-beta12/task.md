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

## Checklist

- [ ] `ganda repo audit --fix`; commit whatever it changes as-is (audit fixes are policy)
- [ ] Rewrite the two bash steps in `workflow.yml` as pwsh and add `defaults: run: shell: pwsh`; keep behaviour identical (release-mode branch, artifact retention of the last two Packages artifacts)
- [ ] Nuru pinned at 4.0.0-beta.12; build and tests green locally (`./bin/dev build`, `./bin/dev test`)
- [ ] ci-code-coverage resolved: coverage collected and uploaded, or severity off with reason
- [ ] `ganda repo audit` passes with zero Errors in the task worktree
- [ ] CI green on the PR, including the rewritten pwsh steps

## Session

- Created: a875b3a1-947b-46fa-a5cb-29aeb3cc8391 (2026-11-10)

## Notes

No version bump. 3.0.0-beta.3 is already on master and releases separately; this task is CI/audit hygiene only.
The pwsh conversion must be checked by reading the step logic, not just by making the audit pass: the
release-mode branch decides whether the pipeline runs in publish mode.
