# Bump consumed TimeWarp.Amuru package pins to 2.0.0

## Description

`Directory.Packages.props` pins the *consumed* packages used by the `.githooks` runfiles (`#:package TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` under CPM) at `2.0.0-beta.2`. `v2.0.0` was released 2026-10-14 and both packages were pushed to NuGet by the release run. Task 129 recorded this as the post-publish follow-up, same as 123 and 126 did for the betas.

2.0.0 is the beta.2 surface with no API change, so the hooks need a re-pin and a compile proof, not an API migration.

## Requirements

- Wait until NuGet flatcontainer lists `2.0.0` for both packages before restoring.
- `Directory.Packages.props`: `TimeWarp.Amuru` → `2.0.0`, `TimeWarp.Amuru.Tools` → `2.0.0`. Keep the "Self pins" comment.
- Each runfile under `.githooks/` (`pre-commit.cs`, `post-commit.cs`, `post-checkout.cs`, `post-merge.cs`, `pre-push.cs`) restores and compiles against 2.0.0: `dotnet build .githooks/<hook>.cs`, 0 warnings / 0 errors; paste the per-file table into Results as task 126 did.
- Do not edit hook bodies. They are byte-identical to ganda's `templates/repo-baseline/githooks/`; if the audit's hook check wants a change, record it in Results instead of diverging.
- Exercise: a throwaway commit in the task worktree shows `post-commit` attesting under the new pins (paste the `Attested tree` line), then `git reset --soft HEAD~1`. The branch push exercises `pre-push`.
- `./bin/dev build` and `ganda repo audit` clean. `source/Directory.Build.props` stays `2.0.0`.

## Checklist

- [ ] NuGet lists 2.0.0 for both packages
- [ ] Pins bumped
- [ ] 5 hooks compile against 2.0.0 (table in Results)
- [ ] Throwaway commit attested; reset; branch push passes pre-push
- [ ] `./bin/dev build`, `ganda repo audit` clean
- [ ] Results: pin diff, compile table, attest line
- [ ] PR open and CI green

## Notes

- Pattern: task 126 (beta.2 pins) Results.
- The only in-repo consumers of the published packages are the hooks; tests and samples reference the projects.

## Session

- Created: 3213244 (2026-10-14)
