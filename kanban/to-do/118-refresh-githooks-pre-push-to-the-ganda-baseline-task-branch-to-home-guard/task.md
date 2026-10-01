# Refresh githooks pre-push to the ganda baseline (task branch to home guard)

## Description

Refresh this repo's `.githooks/pre-push.cs` (and the `.githooks/pre-push` shim if the baseline
changed it) to the current ganda repo baseline.

Ganda task 323 (timewarp-ganda PR #197, merged 2026-10-01) added a guard to the baseline
pre-push hook. It refuses a push whose local ref is `refs/heads/task/*` and whose destination
is the home branch (`master` / `main`), and the message names the task branch. Raw-sha pushes,
such as the `kanban publish` merge commit, stay allowed. Until each repo picks up the new hook,
`ganda repo audit` warns `memsearch-scaffold: .githooks/pre-push.cs (outdated)`, and the repo
lacks the guard.

## Requirements

1. Apply the baseline with ganda itself, not by hand:
   `ganda repo audit --fix --checks memsearch-scaffold`. Confirm that `.githooks/pre-push.cs`
   now matches the baseline and the warning is gone.
2. Keep any repo-specific hook content the baseline intends to preserve. If `--fix` would drop a
   local customization, stop and record it in Notes rather than overwrite it.
3. Re-run `ganda repo audit` and fix anything else it reports (boyscout welcome), then commit.
4. Smoke-test the hook without touching home:
   - Pipe a fake pre-push line into the hook,
     `refs/heads/task/x <sha> refs/heads/master <sha>`, and confirm it is refused.
   - Pipe `<sha> <sha> refs/heads/master <sha>` and confirm it is allowed.

   Do not push to master to test.

## Checklist

- [x] `.githooks/pre-push.cs` refreshed via `ganda repo audit --fix --checks memsearch-scaffold`
- [x] Audit clean (no `memsearch-scaffold` warning)
- [x] Hook smoke test: task→home refused, raw sha→home allowed (stdin simulation only)
- [x] Gates per this repo's `tw-pr` (a hook-only change needs no full build unless the skill's
      scope table says otherwise)
- [x] Implementation review; host `open-pr`

## Notes

- One of a set of identical tasks filed in each repo that carries `.githooks/pre-push.cs`:
  amuru, architecture, bayline, ganda, kiini, mediator, nuru, state, taratibu.
- Do not start any app host. Run builds serially and call `dotnet build-server shutdown` before
  finishing.

## Results

- `ganda repo audit --fix --checks memsearch-scaffold` refreshed `.githooks/pre-push.cs`. The diff
  is additive only: a `taskToHome` guard plus two header comment lines. No repo-specific
  customization was dropped. The `.githooks/pre-push` shim was unchanged.
- The full `ganda repo audit` also reported `bin-dev` and `dev-cli-capabilities` errors because
  `bin/dev` was missing. `bin/` is gitignored, so this was local state only. Fixed it with
  `dotnet run --file tools/dev-cli/dev.cs -- self-install`. Audit now passes all checks.
- Gates: this is a hook-only change, so no product build or tests were needed. The hook runfile
  compiled and ran during the smoke test below.

- Review: 1 round, effort 1, roster general. Final counts: 0 bug / 0 suggestion / 0 nit (0 open,
  0 fixed, 0 wontfix). Disposition **clean**. Artifacts: `review/review-framework.md`,
  `review/round-1/merged.md`, `review/disposition.md`.

### How to validate

Smoke (from the task worktree; nothing is pushed):

```bash
ganda repo audit
S=$(git rev-parse HEAD)
echo "refs/heads/task/x $S refs/heads/master $S" | ./.githooks/pre-push origin url; echo "exit=$?"
echo "$S $S refs/heads/master $S" | ./.githooks/pre-push origin url; echo "exit=$?"
```

Expect:

- The audit prints `Repository passes all audit checks.` and shows no `memsearch-scaffold` warning.
- task→home is refused with `Refusing push of task branch to home: task/x -> master.` and `exit=1`.
- raw sha→home is allowed silently with `exit=0`.

Recorded output (2026-10-01):

```
Refusing push of task branch to home: task/x -> master.
Task branches publish to origin/<task branch> and land on home via PR.
Fix tracking: ganda repo audit --fix --checks task-branch-upstream, then git push.
Escape hatch (intentional only): git push --no-verify
exit=1
exit=0
```

## Session

- Created: 2026-10-01
- 2026-10-01: implement — hook refreshed via audit --fix, bin/dev self-installed, audit clean, smoke test passed
- 2026-10-01: review — effort 1 (general), round 1 no findings, disposition clean
