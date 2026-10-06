# Bump consumed TimeWarp.Amuru package pins to 2.0.0-beta.2

## Description

`Directory.Packages.props` pins the *consumed* packages used by the `.githooks` runfiles (`#:package TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` under CPM) at `2.0.0-beta.1`. `v2.0.0-beta.2` is on NuGet for both packages (released and indexed 2026-10-06). Task 125 recorded this as the post-publish follow-up, same as 117, 122 and 123 did for their releases.

beta.2 is additive over beta.1 (native text commands, timeouts, Tools XML docs): no breaking changes, so the hooks need a re-pin and a compile proof, not an API migration.

## Requirements

- `Directory.Packages.props`: `TimeWarp.Amuru` → `2.0.0-beta.2`, `TimeWarp.Amuru.Tools` → `2.0.0-beta.2`. Keep the "Self pins" comment.
- Each runfile under `.githooks/` (`pre-commit.cs`, `post-commit.cs`, `post-checkout.cs`, `post-merge.cs`, `pre-push.cs`) restores and compiles against beta.2: `dotnet build .githooks/<hook>.cs`, 0 warnings / 0 errors; paste the per-file table into Results as task 123 did.
- Do not edit hook bodies. They are byte-identical to ganda's `templates/repo-baseline/githooks/` (task 344 attest-only shape); if the audit's hook check wants a change, record it in Results instead of diverging.
- Exercise: a throwaway commit in the task worktree shows `post-commit` attesting under the new pins (paste the `Attested tree` line), then `git reset --soft HEAD~1`. The branch push exercises `pre-push`.
- `./bin/dev build` and `ganda repo audit` clean. `source/Directory.Build.props` stays `2.0.0-beta.2`.

## Checklist

- [ ] Pins bumped
- [ ] 5 hooks compile against beta.2 (table in Results)
- [ ] Throwaway commit attested; reset; branch push passes pre-push
- [ ] `./bin/dev build`, `ganda repo audit` clean
- [ ] Results: pin diff, compile table, attest line

## Notes

- Pattern: task 123 (beta.1 pins) Results.
- The only in-repo consumers of the published packages are the hooks; tests and samples reference the projects.

## Session

- Created: 522eb63d (2026-10-06)
