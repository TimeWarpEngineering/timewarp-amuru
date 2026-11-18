# Round 1 — general
**Date:** 2026-11-18
**Scope reviewed:** branch `task/150-analyzer-shell-builder-on-a-tool-with-a-typed` vs `master` (`master...HEAD`). Analyzer project, verb table, pack target, Jaribu runfile, readme, and the dev-cli / test call sites moved onto typed builders. Recorded gates in `## Results` / `### How to validate` were read and not re-run.

## Summary

The analyzer project, nupkg layout, AMURU0001 / AMURU0002 messages, the DotNet both-directions table test, the required Jaribu cases, the readme, and the 3.0.0-beta.8 bump match the task. Dev-cli and the two edited tests call `DotNet.Build`, `DotNet.Run().WithFile`, `DotNet.NuGet().Push`, and `DotNet.MsBuild().WithGetProperty` with the same flags as the old `Shell.Builder("dotnet")` lines. AMURU0003 does not. Several git rows fire on a verb plus one token and name a helper whose real command is a different operation. The recorded test run (20/20) only locks the positive `worktree add` case.

## Issues

### Issue 1 — Severity: bug
- File: source/timewarp-amuru-analyzers/typed-builder-table.cs:133
- Description: AMURU0003 is specified as a git invocation that matches a typed `Git.*` helper. `TypedBuilderTable.Find` treats a row as a match when argument 0 equals `Verb` and, if set, `SubVerb` appears anywhere before `--` (`HasSubVerb`, line 211). `ShellBuilderAnalyzer` reports that row with no further check (shell-builder-analyzer.cs:124). These rows then name a helper that does not perform the invocation:
  - `show-ref` + `--verify` → `Git.BranchExistsAsync()`. That helper runs `show-ref --verify --quiet refs/heads/{branch}` (git.branch-exists.cs:44). `git show-ref --verify refs/tags/v1` is told to use it.
  - `rev-list` + `--count` → `Git.GetCommitsAheadAsync()`. That helper runs `rev-list --count {branch}..HEAD` (git.get-commits-ahead.cs:63). `git rev-list --count HEAD` is told to use it.
  - `remote` + `get-url` → `Git.GetRepositoryNameAsync()`. That helper runs `remote get-url origin` and returns a repository name (git.get-repository-name.cs:38). `git remote get-url upstream` is told to use it.
  - `remote` + `set-head` → `Git.SetRemoteHeadAutoAsync()`. That helper runs `remote set-head origin --auto` (git.remote-head.cs:43). `git remote set-head origin main` is told to use it.
  - `config` + `remote.origin.fetch` → `Git.ConfigureFetchRefspecAsync()`. That helper sets `remote.origin.fetch` to `+refs/heads/*:refs/remotes/origin/*` (git.fetch-refspec.cs:43). A get, or a set of any other value, is told to use it.
  - `fetch` with no sub-verb → `Git.FetchAsync()`. That helper runs `fetch {remote}` (git.fetch.cs:44). `git fetch --all` and `git fetch origin main` are told to use it.
  - `symbolic-ref` + `refs/remotes/origin/HEAD` → `Git.GetDefaultBranchAsync()`, which reads that ref (git.get-default-branch.cs:53). A set of the same ref (`symbolic-ref refs/remotes/origin/HEAD refs/remotes/origin/main`) is told to use it.
  - `rev-parse` + `--show-toplevel` → `Git.FindRoot()`, which walks directories for `.git` and does not run git (git.find-root.cs:30). Any extra `rev-parse` argument still matches.
  - `worktree` + `add` always names both `WorktreeAddAsync` (`worktree add <path> <branch>`) and `WorktreeAddNewBranchAsync` (`worktree add -b …`). `git worktree add -b new path` and `git worktree add --detach path` get the same message (typed-builder-table.cs:145).
  The git test only asserts that each row's method name exists (`EveryGitRow_Should_NameAnExistingHelper`) and that `worktree add` reports AMURU0003 (`GitWithHelper_Should_ReportInfo`). It does not assert these counterexamples. Default severity is info, so the dev-cli build staying green does not show the wrong advice. Raising AMURU0003 in `.editorconfig`, which the readme tells consumers to do, would warn on calls the named helper cannot replace.
- Suggestion: Match the helper's real argument pattern, and do not report when another argument shows a different operation (a ref that is not `refs/heads/…`, a rev-list range that is not `{branch}..HEAD`, a remote other than `origin`, `set-head` without `--auto`, `fetch` with `--all` or a refspec, a `symbolic-ref` set, `worktree add -b` versus `worktree add` without `-b`, `worktree add --detach`). For `FindRoot`, either require the arguments to be exactly `rev-parse --show-toplevel` and say that the helper is a filesystem walk, or drop the row. Add Jaribu cases for each counterexample (expect no AMURU0003) and keep a true positive per remaining row. `EveryGitRow_Should_NameAnExistingHelper` can stay; it does not cover this.
- Status: open
