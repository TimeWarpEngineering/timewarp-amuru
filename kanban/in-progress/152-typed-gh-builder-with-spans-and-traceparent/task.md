# Typed Gh builder with spans and TRACEPARENT

## Description

The GitHub CLI emits no OpenTelemetry of its own, so a `gh` call made through
`Shell.Builder("gh")` is invisible in SigNoz: no span, no duration, no parent. A typed
`Gh` builder in Amuru gives every consumer one place to open a span per call, tag it with the
subcommand and repo, propagate `TRACEPARENT` (task 149), and shape the arguments. It also
gives the task 150 analyzer a replacement to name for `Shell.Builder("gh")`.

Org-wide scan on 2026-11-18 of the master worktrees under
`~/worktrees/github.com/TimeWarpEngineering/*`: 51 call sites in 12 repos (timewarp-ganda 8
files, timewarp-nuru 4, the rest one or two each). Subcommands seen:

| Subcommand | Call sites | Notes |
|------------|-----------:|-------|
| run list | 12 | workflow runs, usually `--json` + `--jq` or `--limit` |
| api | 12 | `repos/{owner}/{repo}/actions/runs/{id}/artifacts`, `repos/.../dispatches` |
| run download | 11 | artifacts by run id, `--name`, `--dir` |
| auth token | 5 | token for a NuGet or API call |
| auth status | 2 | preflight |
| issue list / issue create | 2 | |
| other | 7 | argument lists built at runtime, not readable by the scan |

## Requirements

- New `source/timewarp-amuru-tools/gh-commands` following the `dot-net-commands` layout: one
  file per verb, `Options` plumbing, `WithWorkingDirectory`, `WithEnvironmentVariable`,
  `WithNoValidation`, `CaptureAsync` / `RunAsync`, public API files updated.
- Builders for the surface above: `Gh.Run().List()`, `Gh.Run().Download()`, `Gh.Api()`,
  `Gh.Auth().Token()`, `Gh.Auth().Status()`, `Gh.Issue().List()`, `Gh.Issue().Create()`,
  plus `Gh.Pr()` with `View`, `Checks`, `Create`, and `Merge` and `Gh.Release()` with
  `List` and `Create`, because ganda and the dev CLIs reach those today through other paths
  and should land on the same builder. Common flags: `--repo`, `--json` + `--jq`,
  `--limit`. A `WithArgument` escape hatch for a flag the builder does not model.
- Telemetry: each execution opens a span `gh <verb> <subverb>` with tags `gh.command`,
  `gh.repo` when known, `gh.api.endpoint` for `api` (path only, no query values), the exit
  code, and the elapsed time. Follow `dot-net.build-telemetry.cs` for where tags land.
  Never tag or log a token, the `auth token` stdout, request bodies, or `-f` field values.
- `TRACEPARENT` propagation comes from task 149 for free; nothing gh-specific needed beyond
  not overwriting it.
- Typed results where the JSON shape is stable: `run list --json` to a record list, `auth
  status` to a signed-in flag plus host, `pr view --json` to a record. Everything else returns
  the raw `CommandOutput`.
- Tests: Jaribu runfiles asserting the argument string per builder, that the span name and
  tags are set, and that the token never appears in tags. No live GitHub calls.
- Readme section and version bump.
- Add `gh` to the task 150 analyzer table (AMURU0001 error for the verbs covered here) in the
  same change. Task 150 is merged before this task starts.

## Out of scope

- Moving the 51 call sites. Each repo does that on its Amuru bump under its own task.
- A GitHub REST client that bypasses `gh`. The point is to wrap the CLI the repos already rely
  on for auth.

## Depends on

- 150

## Goal (implementer restatement)

Give Amuru a typed `Gh` builder in TimeWarp.Amuru.Tools so a `gh` call shows up in SigNoz as its
own span, child of the caller's span, with `TRACEPARENT` pointing at that span.

Acceptance:

- `source/timewarp-amuru-tools/gh-commands/` exists with one file per verb, the usual builder
  plumbing (`WithWorkingDirectory`, `WithEnvironmentVariable`, `WithNoValidation`,
  `CaptureAsync`, `RunAsync`, ...), a `WithArgument` escape hatch, and the public API files list it.
- Builders: `Gh.Run().List()/Download()`, `Gh.Api()`, `Gh.Auth().Token()/Status()`,
  `Gh.Issue().List()/Create()`, `Gh.Pr().View()/Checks()/Create()/Merge()`,
  `Gh.Release().List()/Create()`, with `--repo`, `--json`, `--jq`, `--limit` where gh supports them.
- Every execution opens a span `gh <verb> <subverb>` on an Amuru `ActivitySource`, tagged
  `gh.command`, `gh.repo` (when set), `gh.api.endpoint` (path only, no query), the exit code, and
  the elapsed time. No token, `auth token` stdout, request body, or `-f`/`-F` value is ever tagged.
  `TRACEPARENT` comes from task 149 unchanged.
- Typed results for `run list --json`, `auth status`, and `pr view --json`; everything else
  returns `CommandOutput`.
- Jaribu runfiles cover argument strings per builder, span name and tags, and the no-token rule.
  No live GitHub calls.
- Readme section, version bump, and `gh` rows in the analyzer table so `Shell.Builder("gh")` for
  a covered verb is AMURU0001.

## Checklist

- [x] gh-commands project layout and public API files
- [x] Run, Api, Auth, Issue, Pr, Release builders
- [x] Span per call with safe tags
- [x] Typed results for stable JSON shapes
- [x] Tests
- [x] Readme and version bump
- [x] Analyzer table entry for gh

## Session

- Created: 533fd3ed-9595-4328-8934-d2075db5b31f (2026-11-18), from a timewarp-jaribu session
- Groomed: 93a23df6-1180-44ac-9a03-f537cd480249 (2026-11-18), prose dependency on 150 made a Depends on id
- Implemented (2026-11-18, ganda task work implement oracle, Claude Opus 5.5)
- Boy-scout: `ganda repo audit` reported one advisory (`vscode-window-icon`, `.vscode/settings.json`
  missing). `ganda repo audit --fix` wrote the ignored local file; no tracked change. Rerun passed.

## Results

### What changed

- `source/timewarp-amuru-tools/gh-commands/` (one file per verb, `gh.md` reference):
  - `gh.cs` `Gh` entry + `Gh.ActivitySourceName` (`TimeWarp.Amuru.Gh`).
  - `gh.command-builder.cs` bases: `GhCommandBuilder<T>` (options, `WithArgument(s)`,
    `ToArguments`, `Build`, span-wrapped `RunAsync`/`CaptureAsync`/`RunAndCaptureAsync`/
    `PassthroughAsync`), `GhRepoCommandBuilder<T>` (`--repo`), `GhJsonCommandBuilder<T>`
    (`--json`, `--jq`), `GhListCommandBuilder<T>` (`--limit`).
  - `gh.telemetry.cs` opens `gh <verb> <sub-verb>` (Client) with `gh.command`, `gh.repo`,
    `gh.api.endpoint` (path only; query, fragment, scheme, host, user info cut), `gh.exit_code`,
    `gh.elapsed_ms`; Error status `exit code N` or the exception type name. Nothing else is tagged.
  - Builders: run list/download, api, auth token/status, issue list/create, pr
    view/checks/create/merge, release list/create.
  - Typed: `ListRunsAsync` → `GhRunListResult`/`GhWorkflowRun`, `ViewPullRequestAsync` →
    `GhPullRequestViewResult`/`GhPullRequest`, `GetStatusAsync` (`--json hosts`, no token) →
    `GhAuthStatus`. AOT via `GhJsonContext` source generation.
  - `PublicAPI.Unshipped.txt` updated with the RS0016 fixer.
- Analyzer: `TypedBuilderTable.GhRows` + `FindAtSecond` (sub-verb must be the 2nd argument);
  `gh` matches report AMURU0001 naming the `Gh` builder; uncovered gh commands are not reported.
  Title/description/AnalyzerReleases note now say "DotNet or Gh".
- `tools/dev-cli/endpoints/workflow-command.cs`: its four `Shell.Builder("gh")` calls moved to
  `Gh.Run().List()`, `Gh.Api()` (x2), `Gh.Run().Download()` (they became AMURU0001 errors).
- `tools/dev-cli/Directory.Build.targets`: task 150's scoped AMURU0001 → suggestion target for
  TimeWarp.Nuru.DevCli package content now also lists `github-packages-credential`,
  `release-command`, `release-command.guards` (raw `gh` calls in the NuGet-cache content, not
  reachable by repo config). Fix belongs in timewarp-nuru once an Amuru release ships `Gh`;
  follow-up: move those calls there and drop the names from `_DevCliRawShellFiles`.
- Tests: `analyzers/shell-builder-analyzer.cs` went over the 500-line cap (TWA0038), so its
  `VerbTable_Given_` surface class moved to `analyzers/typed-builder-table.cs` (plus new Gh
  surface cases). `OtherExecutable_Should_NotReport` now uses `docker ps` (gh is covered now).
- Readme "GitHub CLI Spans" section, Tool Builders example, AMURU0001 row; AGENTS.md package
  layout; csproj description/tags; version `3.0.0-beta.8` → `3.0.0-beta.9`;
  `documentation/release-notes/3.0.0-beta.9.md`.

### Gates

- `./bin/dev build` — pass (0 warnings, 0 errors)
- `dotnet run --file tests/timewarp-amuru/single-file-tests/gh-commands/gh.arguments.cs` — pass 18/18
- `dotnet run --file tests/timewarp-amuru/single-file-tests/gh-commands/gh.telemetry.cs` — pass 11/11
- `dotnet run --file tests/timewarp-amuru/single-file-tests/gh-commands/gh.typed-results.cs` — pass 8/8
- `dotnet run --file tests/timewarp-amuru/single-file-tests/analyzers/shell-builder-analyzer.cs` — pass 24/24
- `dotnet run --file tests/timewarp-amuru/single-file-tests/analyzers/typed-builder-table.cs` — pass 7/7
- `dotnet run --file tools/dev-cli/dev.cs -- self-install` (dev-cli changed) — pass (AOT publish)
- `ganda repo audit` — pass with 1 advisory (`vscode-window-icon`); `--fix` applied; rerun — pass

### How to validate

Smoke:

```bash
./bin/dev build
(cd tests/timewarp-amuru/single-file-tests/gh-commands && dotnet run --file gh.telemetry.cs)
(cd tests/timewarp-amuru/single-file-tests/analyzers && dotnet run --file shell-builder-analyzer.cs)
```

Expect: build 0 errors; `gh.telemetry.cs` 11/11 passed, including
`AuthToken_Should_NeverTagTheToken` (span tag keys exactly `gh.command`, `gh.exit_code`,
`gh.elapsed_ms`) and `Api_Should_TagThePathWithoutQueryOrFieldValues`
(`gh.api.endpoint` = `repos/o/r/dispatches`); analyzer 24/24 passed, including
`GhCoveredVerbs_Should_ReportTheGhBuilder` (`Use Gh.Run().List() (TimeWarp.Amuru.Tools) instead
of Shell.Builder("gh") with "run list"`).

## Notes

Related: 149 (TRACEPARENT from Amuru), 150 (analyzer), 151 (missing dotnet builders).
Steve: "I don't know if the GitHub command itself has telemetry, but if we make an explicit
typed builder for it, then we can add whatever we want."
